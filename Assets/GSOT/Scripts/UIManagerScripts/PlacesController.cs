using ARLocation;
using Assets.GSOT.Scripts.ApiScripts;
using Assets.GSOT.Scripts.ApiScripts.EventLog;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.SceneScripts;
using Assets.GSOT.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.GSOT.Scripts.UIManagerScripts
{
    public class PlacesController : MonoBehaviour
    {
        private bool initialized = false;
        private Vector3 basePosition;
        private List<Button> scenesButtons = new List<Button>();
        private List<Button> allPlacesButtons = new List<Button>();
        private List<Button> demoPlacesButtons = new List<Button>();
        public ARLocationProvider arLocationProvider;
        public Button LanguageButton;
        public Sprite plFlag;
        public Sprite usFlag;
        public Button AllPlacesBtn;
        public Button CloseBtn;
        public Button AddLicenseBtn;
        public Button MyPlacesBtn;
        public Button DemoBtn;

        public GameObject btnPrefab;
        public Canvas MenuCanvas;
        public Text buildVersion;
        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Init()
        {
            basePosition = new Vector3(0, AllPlacesBtn.gameObject.transform.position.y - 120, 0);
            buildVersion.gameObject.transform.position = 
                new Vector3(MyPlacesBtn.gameObject.transform.position.x, MyPlacesBtn.gameObject.transform.position.y + 180, 0 );
            foreach (var place in ModelsQueue.Places.Where(x => x.GSOrderProductLicenseId.HasValue))
            {
                CreateScenesBtn(place.Name, ButtonType.Scenes);
            }
            basePosition = new Vector3(0, AllPlacesBtn.gameObject.transform.position.y - 120, 0);
            foreach (var place in ModelsQueue.AllPlaces)
            {
                CreateScenesBtn(place.Name, ButtonType.Url, place.Id);
            }
            basePosition = new Vector3(0, AllPlacesBtn.gameObject.transform.position.y - 120, 0);
            foreach (var place in ModelsQueue.Places.Where(x => !x.GSOrderProductLicenseId.HasValue))
            {
                CreateScenesBtn(place.Name, ButtonType.Demo);
            }
            SwitchMenu(ButtonType.Demo);
            AllPlacesBtn.onClick.AddListener(() => SwitchMenu(ButtonType.Url));
            MyPlacesBtn.onClick.AddListener(() => SwitchMenu(ButtonType.Scenes));
            DemoBtn.onClick.AddListener(() => SwitchMenu(ButtonType.Demo));
        }

        private void SwitchMenu(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Demo:
                    allPlacesButtons.ForEach(x => x.gameObject.SetActive(false));
                    scenesButtons.ForEach(x => x.gameObject.SetActive(false));
                    demoPlacesButtons.ForEach(x => x.gameObject.SetActive(true));
                    break;
                case ButtonType.Scenes:
                    allPlacesButtons.ForEach(x => x.gameObject.SetActive(false));
                    scenesButtons.ForEach(x => x.gameObject.SetActive(true));
                    demoPlacesButtons.ForEach(x => x.gameObject.SetActive(false));
                    break;
                case ButtonType.Url:
                    allPlacesButtons.ForEach(x => x.gameObject.SetActive(true));
                    scenesButtons.ForEach(x => x.gameObject.SetActive(false));
                    demoPlacesButtons.ForEach(x => x.gameObject.SetActive(false));
                    break;
            }
        }

        private void OnGUI()
        {
            if (!initialized)
            {
                Init();
                ChangeLanguage(false);
                initialized = true;
            }
        }

        private void CreateScenesBtn(string text, ButtonType type, long? placeId = null)
        {
            var btn = Instantiate(btnPrefab);
            btn.transform.position = basePosition;
            var txt = btn.GetComponentInChildren<Text>();
            txt.text = text;
            btn.transform.SetParent(MenuCanvas.transform);
            btn.transform.localScale = new Vector3(1, 1, 1);
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 45);
            var btnComponent = btn.GetComponent<Button>();

            if (type == ButtonType.Scenes)
            {
                btnComponent.onClick.AddListener(() => SelectPlaceClick(btnComponent));
                scenesButtons.Add(btnComponent);
            }
            else if(type == ButtonType.Url)
            {
                btnComponent.onClick.AddListener(() => SelectPlaceClick(placeId.Value));
                allPlacesButtons.Add(btnComponent);
            }
            else
            {
                btnComponent.onClick.AddListener(() => SelectPlaceClick(btnComponent));
                demoPlacesButtons.Add(btnComponent);
            }

            basePosition = new Vector3(basePosition.x, basePosition.y - 70, 0);
        }

        public void SelectPlaceClick(long placeId)
        {
            new MobileAppShoppingEventLogService(placeId, Input.location.lastData.ToLocation()).Add();
            Application.OpenURL(TemporaryDatabase.WebPortalURL);
        }

        public void SelectPlaceClick(Button button)
        {
            //ModelsQueue.ActivePlace = button.gameObject.GetComponentInChildren<Text>().text;
            SceneManager.LoadScene("ScenesScene");
        }

        public void AddLicenseClick()
        {
            SceneManager.LoadScene("LicenseScene");
        }

        public void TapSceneClick()
        {
            SceneManager.LoadScene("ModelOnTapScene");
        }

        public void CloseAppClick()
        {
            Application.Quit();
        }

        //public void AllPlacesClick()
        //{
        //    SceneManager.LoadScene("AllPlacesScene");
        //}

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                new MobileAppUsingEventLogService(TemporaryDatabase.AppStartDate.Value, arLocationProvider.CurrentLocation.ToLocation()).Add();
            }
            else
            {
                TemporaryDatabase.AppStartDate = DateTime.Now;
            }
        }

        void OnApplicationQuit()
        {
            new MobileAppUsingEventLogService(TemporaryDatabase.AppStartDate.Value, arLocationProvider.CurrentLocation.ToLocation()).Add();
        }

        private void ChangeLanguage(bool changed = true)
        {
            if (changed)
            {
                if (ModelsQueue.Language == "pl-PL")
                {
                    PlayerPrefs.SetString("lang", "en-US");
                    ModelsQueue.Language = "en-US";
                }
                else
                {
                    PlayerPrefs.SetString("lang", "pl-PL");
                    ModelsQueue.Language = "pl-PL";
                }
            }
            PlayerPrefs.Save();

            switch (PlayerPrefs.GetString("lang"))
            {
                case "pl-PL":
                    LanguageButton.GetComponent<Image>().sprite = plFlag;
                    break;
                case "en-US":
                    LanguageButton.GetComponent<Image>().sprite = usFlag;
                    break;
            }
            //AllPlacesBtn.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("AllPlaces");
            //AddLicenseBtn.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("AddLicense");
            //CloseBtn.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("Close");
            //MyPlacesBtn.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("MyPlaces");
        }

        private enum ButtonType
        {
            Scenes = 1,
            Url = 2,
            Demo = 3
        }
    }
}
