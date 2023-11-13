using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Android;
using Assets.GSOT.Scripts.Models.Wrappers;
using Assets.GSOT.Scripts.LoadingScripts;
using System.Linq;
using System;
using System.Collections.Generic;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using Assets.GSOT.Scripts.ApiScripts;
using Assets.GSOT.Scripts.ApiScripts.EventLog;
using Assets.GSOT.Scripts.SceneScripts;
using ARLocation;
using Assets.GSOT.Scripts.Utils;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.GSOT.Scripts.UIManagerScripts
{
    public class IntroController : MonoBehaviour
    {
        GameObject dialog = null;
        public Text Intro1;
        public Text Intro2;
        public Text Intro3;
        public Text AskForCamera1;
        public Text AskForCamera2;
        public Text Ready;
        public Text AskForLocation1;
        public Text AskForLocation2;
        public Text DoWeStart;
        public Button LanguageButton;
        public Sprite plFlag;
        public Sprite usFlag;

        public Button askLocationButton;

        bool cameraAsked = false;
        bool locationAsked = false;

        void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SetTranslations();
        }

        private void SetTranslations()
        {
            if (LanguageButton)
            {
                switch (PlayerPrefs.GetString("lang"))
                {
                    case "pl-PL":
                        LanguageButton.GetComponent<Image>().sprite = plFlag;
                        break;
                    case "en-US":
                        LanguageButton.GetComponent<Image>().sprite = usFlag;
                        break;
                }
            }
            var translator = Translator.Instance();
            if (Intro1 != null)
            {
                Intro1.text = translator.GetString("Intro1");
                Intro2.text = translator.GetString("Intro2");
                Intro3.text = translator.GetString("Intro3");
            }

            if (AskForCamera1 != null)
            {
                AskForCamera1.text = translator.GetString("AskForCamera1");
                AskForCamera2.text = translator.GetString("AskForCamera2");
                Ready.text = translator.GetString("Ready");
            }

            if (AskForLocation1 != null)
            {
                AskForLocation1.text = translator.GetString("AskForLocation1");
                AskForLocation2.text = translator.GetString("AskForLocation2");
                DoWeStart.text = translator.GetString("DoWeStart");
            }
        }

        public void ChangeLanguage()
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
            SetTranslations();
        }

        void Update() { }

        #region Buttons
        public void BtnOpenLocationScene()
        {
            AskForCamera();
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                SceneManager.LoadScene("Ask For Location");
            }
            else
            {
                SceneManager.LoadScene("ScenesScene");
            }
        }
        public void BtnOpenCameraScene()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                SceneManager.LoadScene("Ask For Camera");
            }
            else
            {
                SceneManager.LoadScene("Ask For Location");
            }
        }
        public void BtnOpenIntroScene()
        {
            askLocationButton.gameObject.SetActive(false);
            AskForLocalization();

            SceneManager.LoadScene("ScenesScene");
        }
        #endregion

        public void AskForCamera()
        {
#if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                cameraAsked = true;
                Permission.RequestUserPermission(Permission.Camera);
                dialog = new GameObject();
            }
#endif
        }

        public void AskForLocalization()
        {
#if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                locationAsked = true;
                Permission.RequestUserPermission(Permission.FineLocation);
                dialog = new GameObject();
            }
#endif
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                if (!Permission.HasUserAuthorizedPermission(Permission.Camera) && cameraAsked)
                {
                    Application.Quit();
                }
                if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && locationAsked)
                {
                    Application.Quit();
                }
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