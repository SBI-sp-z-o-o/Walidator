using ARLocation;
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
    public class AllPlacesController : MonoBehaviour
    {
        List<Button> buttons;

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Init();
        }

        private void Init()
        {
            var c = GameObject.Find("Canvas");
            buttons = c.gameObject.GetComponentsInChildren<Button>().Where(x => x.name.ToLower().Contains("button")).ToList();
            buttons.ForEach(x => x.gameObject.SetActive(false));
            var places = ModelsQueue.AllPlaces;

            int index = 0;

            foreach (var place in places)
            {
                if(buttons.Count > index)
                {
                    buttons[index].gameObject.SetActive(true);
                    buttons[index].gameObject.GetComponentInChildren<Text>().text = place.Name;
                    buttons[index].onClick.AddListener(() =>
                    {
                        SelectPlaceClick(place.Id);
                    });
                }
                index++;
            }
        }

        public void SelectPlaceClick(long placeId)
        {
            new MobileAppShoppingEventLogService(placeId, Input.location.lastData.ToLocation()).Add();
            Application.OpenURL(TemporaryDatabase.WebPortalURL);
        }

        public void Update()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SceneManager.LoadScene("ScenesScene");
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
