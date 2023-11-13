using Assets.GSOT.Scripts.ApiScripts;
using Assets.GSOT.Scripts.ApiScripts.EventLog;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.SceneScripts;
using Assets.GSOT.Scripts.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.GSOT.Scripts.UIManagerScripts
{
    public class LoadingController : MonoBehaviour
    {
        public Text DownloadText;
        bool downloaded = false;
        bool redirect = true;
        private bool _hasError = false;
        public Slider ProgressSlider;
        public Text ProgressText;
        private GetAllDataService dataService;
        public void Start()
        {
            Screen.fullScreen = false;
            StartCoroutine("StartLocation");
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (!TemporaryDatabase.AppStartDate.HasValue)
            {
                TemporaryDatabase.AppStartDate = DateTime.Now;
            }
            ModelsQueue.DeviceId = SystemInfo.deviceUniqueIdentifier;
            Utils.FilesUtils.Init();

            //if (PlayerPrefs.HasKey("lang"))
            //{
                ModelsQueue.Language = "pl-PL";// PlayerPrefs.GetString("lang");
                                               //}
                                               //else
                                               //{
                                               //    string lang = Application.systemLanguage.ToString();
                                               //    if (lang == "Polish")
                                               //    {
            PlayerPrefs.SetString("lang", "pl-PL");
            //    }
            //    else if (lang == "English")
            //    {
            //        PlayerPrefs.SetString("lang", "en-US");
            //    }
            //    PlayerPrefs.Save();
            //}
            DownloadText.text = Translator.Instance().GetString("DownloadingData");
            dataService = new GetAllDataService();
            new Task(GetData).Start();
        }

        private void UpdateProgress(int loaded, int all)
        {
            if(all == 0)
            {
                return;
            }
            var progress = ((float)loaded / (float)all) * 100;
            ProgressSlider.value = progress;
            ProgressText.text = ((int)progress).ToString() + "%";
        }

        public void GetData()
        {
            try
            {
                var isOk = dataService.GetAllData();
                _hasError = !isOk;
                downloaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Ex: {ex.Message}");
            }
        }

        public void Update()
        {
            if (downloaded && redirect)
            {
                redirect = false;
                new MobileAppLoginEventLogService(Input.location.lastData.ToLocation()).Add();
                if (_hasError)
                {
                    AndroidMessageService.ShowAndroidToastMessage(Translator.Instance().GetString("DataDownloadError"));
                }
                else if (Permission.HasUserAuthorizedPermission(Permission.FineLocation) && Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    SceneManager.LoadScene("ScenesScene");
                    //GUIAnimSystemFREE.Instance.LoadLevel("ScenesScene", 1.5f);
                }
                else
                {
                    SceneManager.LoadScene("Intro");
                    //GUIAnimSystemFREE.Instance.LoadLevel("Intro", 1.5f);
                }
            }
            else
            {
                UpdateProgress(dataService.Downloaded, dataService.ObjectsToDownload);
            }
        }

        System.Collections.IEnumerator StartLocation()
        {
            if (!Input.location.isEnabledByUser)
                yield return new WaitForSeconds(1);

            Input.location.Start();

            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            if (maxWait < 1)
            {
                yield break;
            }
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                yield break;
            }
            else
            {
                //locationActive = true;
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                new MobileAppUsingEventLogService(TemporaryDatabase.AppStartDate.Value, Input.location.lastData.ToLocation()).Add();

            }
            else
            {
                TemporaryDatabase.AppStartDate = DateTime.Now;
            }
        }

        void OnApplicationQuit()
        {
            new MobileAppUsingEventLogService(TemporaryDatabase.AppStartDate.Value, Input.location.lastData.ToLocation()).Add();

        }
    }
}
