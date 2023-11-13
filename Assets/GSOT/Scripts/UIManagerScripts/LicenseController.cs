using ARLocation;
using Assets.GSOT.Scripts.ApiScripts;
using Assets.GSOT.Scripts.ApiScripts.EventLog;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApiModels;
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
    public class LicenseController : MonoBehaviour
    {
        public InputField InputText;
        private bool atLeastOneGoodCode = false;
        public Text TypeCodeText;
        public Button AddBtn;
        public Text LicenseCodePlaceholder;
        public Image Background;
        public Image Logo;

        public void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            var imgConverter = FindObjectOfType<IMG2Sprite>();
            var bg = imgConverter.LoadNewSprite(ModelsQueue.BackgroundFilePath);
            if (bg != null)
            {
                Background.sprite = bg;
            }

            var lo = imgConverter.LoadNewSprite(ModelsQueue.LogoFilePath);
            if (lo != null)
            {
                Logo.sprite = lo;
            }

            InputText.onValidateInput += delegate (string input, int charIndex, char addedChar) {
                return characterValidation(addedChar);
            };

            TypeCodeText.text = Translator.Instance().GetString("LicenseCode");
            LicenseCodePlaceholder.text = Translator.Instance().GetString("TypeLicenseKey");
            //AddBtn.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("Add");
        }

        private char characterValidation(char c)
        {
            return c.ToString().ToUpper()[0];
        }

        public void Update()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (atLeastOneGoodCode)
                    {
                        var isOk = new GetAllDataService().GetAllData();
                        if (!isOk)
                        {
                            AndroidMessageService.ShowAndroidToastMessage(Translator.Instance().GetString("DataDownloadError"));
                        }
                        GUIAnimSystemFREE.Instance.LoadLevel("LoadingScreen", 1.5f);
                    }
                    else
                    {
                        SceneManager.LoadScene("ScenesScene");
                    }
                }
            }
        }

        public void AddButtonClick()
        {
            string licenseCode = InputText.text;
    
            if (string.IsNullOrEmpty(licenseCode))
            {
                AndroidMessageService.ShowAndroidToastMessage(Translator.Instance().GetString("NoLicenseCode"));
            }
            else
            {
                var response = MobileApiService.RegisterCode(licenseCode);
                if(response.Status > 0)
                {
                    new MobileAppRegisterLicenseCodeEventLogService((ApiResultStatus)response.Status, Input.location.lastData.ToLocation()).Add();
                }
                CodeIsValid(response);
            }
        }


        public void PasteClipBoarad()
        {
            InputText.text = GUIUtility.systemCopyBuffer;
        }
        private void CodeIsValid(SendLicenseModel model)
        {
            if (model.Status == 1)
            {
                atLeastOneGoodCode = true;
                ModelsQueue.IsDemo = false;
                try
                {
                    AndroidMessageService.ShowAndroidToastMessage(Translator.Instance().GetString("LicenseAdded"));
                }
                catch (Exception)
                {

                }
                SceneManager.LoadScene("ScenesScene");
            }
            else
            {
                AndroidMessageService.ShowAndroidToastMessage(model.StatusMessage);
            }
        }

        public void BtnGoToStoreClick()
        {
            Application.OpenURL(TemporaryDatabase.WebPortalURL);
        }

        public void CancelButtonClick()
        {
            if (atLeastOneGoodCode)
            {
                var isOk = new GetAllDataService().GetAllData();
                if (!isOk)
                {
                    AndroidMessageService.ShowAndroidToastMessage(Translator.Instance().GetString("DataDownloadError"));
                }
            }
            //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            SceneManager.LoadScene("ScenesScene");
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
