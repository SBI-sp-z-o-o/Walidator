//using GoogleMobileAds.Api;
using ARLocation;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Utils;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdsController : MonoBehaviour
{
    public ARLocationProvider _arLocationProvider;
    public Image AdvertImage;
    private IMG2Sprite imgConverter;
    private bool? isDemo = false;
    private bool isGoogle = false;
    #region google
    private InterstitialAd interstitial;
    private BannerView bannerView;
    private bool showed = false;
    private DateTime timeToShow { 
        get
        {
            return ModelsQueue.TimeToShowAd;
        }
        set
        {
            ModelsQueue.TimeToShowAd = value;
        }
    }
    // Start is called before the first frame update
    #endregion
    void Start()
    {
        isDemo = ModelsQueue.IsDemo.HasValue? ModelsQueue.IsDemo.Value : !ModelsQueue.Places.FirstOrDefault()?.GSOrderProductLicenseId.HasValue;
        isDemo = isDemo ?? true;
        #region google
        isGoogle = ModelsQueue.Configuration?.AdvertMode == Assets.GSOT.Scripts.Models.ApiModels.MobileAppConfigurationDto.Mode.Google;
        if (isGoogle)
        {
            //MobileAds.Initialize(initStatus => { });
            //if (SceneManager.GetActiveScene().name == "ScenesScene")
            //RequestBanner();

            //ShowAd();
        }
        #endregion

        //imgConverter = FindObjectOfType<IMG2Sprite>();
        InvokeRepeating("CheckAds", 0, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (advertEnd <= DateTime.Now && AdvertImage.gameObject.activeSelf)
        {
            AdvertImage.sprite = null;
            AdvertImage.gameObject.SetActive(false);

            timeToShow = DateTime.Now.AddSeconds(ModelsQueue.Configuration.MobileAppAdvertRefreshTimeInSeconds);
        }
        #region google
        if (isGoogle && isDemo.Value && !showed)
        {
            ShowAd();
        }
        #endregion
    }

    void CheckAds()
    {
        if(DateTime.Now < timeToShow)
        {
            return;
        }
        if (isGoogle && !showed)
        {
            RequestInterstitial();
            timeToShow = DateTime.MaxValue;
            //ShowAd();
        }
        else if (_arLocationProvider == null || _arLocationProvider.CurrentLocation.ToLocation().Latitude == 0)
        {
            return;
        }
        var currLocation = _arLocationProvider.CurrentLocation.ToLocation();
        var ads = MobileApiService.GetAdverts((float)currLocation.Latitude, (float)currLocation.Longitude, (float)currLocation.Altitude);
        if (ads != null && ads.Data.Adverts != null && ads.Data.Adverts.Any())
        {
            var ad = ads.Data.Adverts
                .OrderByDescending(x => x.IsAvailableOnStartupScreen && SceneManager.GetActiveScene().name == "ScenesScene")
                .ThenByDescending(x => x.Type == Assets.GSOT.Scripts.Models.ApiModels.AdvertModels.AdvertType.Alert)
                .FirstOrDefault();
            if (!ad.IsAvailableOnStartupScreen && SceneManager.GetActiveScene().name == "ScenesScene")
            {
                return;
            }

            if (!isDemo.Value && ad.Type != Assets.GSOT.Scripts.Models.ApiModels.AdvertModels.AdvertType.Alert)
            {
                return;
            }


            string path = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(ad.File.DiscFileName));
            int pFrom = ad.Description.IndexOf("#") + 1;
            int pTo = ad.Description.LastIndexOf("#");
            if (pTo > 0)
            {
                var url = ad.Description.Substring(pFrom, pTo - pFrom);
                var cl = AdvertImage.GetComponent<ClickComponent>();
                cl.Url = url;
            }
            AdvertImage.sprite = imgConverter.LoadNewSprite(path);
            AdvertImage.gameObject.SetActive(true);
            advertEnd = DateTime.Now.AddSeconds(ad.DurationInSeconds);
            timeToShow = DateTime.Now.AddSeconds(ad.DurationInSeconds + 1);

            //StopCoroutine(nameof(ExecuteAfterTime));
            //StartCoroutine(ExecuteAfterTime(ad.DurationInSeconds));
        }
    }
    DateTime advertEnd = DateTime.Now;
    IEnumerator ExecuteAfterTime(int delay)
    {
        if (advertEnd >= DateTime.Now)
        {
            yield return new WaitForSeconds(1);
        }
        else
        {
            AdvertImage.sprite = null;
            AdvertImage.gameObject.SetActive(false);
            yield return null;
        }
    }
    #region google
    private void RequestBanner()
    {
        //#if UNITY_ANDROID
        //        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
        //#endif

        //        // Create a 320x50 banner at the top of the screen.
        //        if(SceneManager.GetActiveScene().name == "ModelScene")
        //        {
        //            this.bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);
        //        }
        //        else
        //        {
        //            this.bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Top);
        //        }

        //        // Create an empty ad request.
        //        AdRequest request = new AdRequest.Builder().Build();

        //        // Load the banner with the request.
        //        this.bannerView.LoadAd(request);
    }

    private void RequestInterstitial()
    {
        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(ModelsQueue.Configuration.GoogleAndroidAdUnitId);


        // Called when an ad request has successfully loaded.
        this.interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitial.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;


        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        Debug.Log("HandleAdOpened event received");
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLeavingApplication event received");
    }

    private void HandleOnAdClosed(object sender, EventArgs e)
    {
        advertEnd = DateTime.Now;
        showed = false;
        timeToShow = DateTime.Now.AddSeconds(ModelsQueue.Configuration.MobileAppAdvertRefreshTimeInSeconds);
        this.interstitial = new InterstitialAd(ModelsQueue.Configuration.GoogleAndroidAdUnitId);
    }

    private void ShowAd()
    {
        if(interstitial == null)
        {
            return;
        }
        if (this.interstitial.IsLoaded())
        {
            this.interstitial.Show();
            showed = true;
            advertEnd = DateTime.Now.AddHours(1);
        }
    }
    #endregion
}
