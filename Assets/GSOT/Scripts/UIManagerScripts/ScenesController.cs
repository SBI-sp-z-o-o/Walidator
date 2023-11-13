using ARLocation;
using Assets.GSOT.Scripts.ApiScripts;
using Assets.GSOT.Scripts.ApiScripts.EventLog;
using Assets.GSOT.Scripts.Enums.ApiEnums;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApiModels;
using Assets.GSOT.Scripts.SceneScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.GSOT.Scripts.UIManagerScripts
{
    public class ScenesController : MonoBehaviour
    {
        //        private Vector3 basePosition;
        public static List<GameObject> BattleButtons = new List<GameObject>();
        public static List<GameObject> ModelButtons = new List<GameObject>();
        public ARLocationProvider arLocationProvider;
        public Text titleText;
        public GameObject btnPrefab;
        public Canvas MenuCanvas;
        public Button CloseBtn;
        public Button QuestionmarkBtn;
        public Button AddLicenseBtn;
        public Button AddLicenseBtn2;
        public Button LanguageButton;
        public Button BattleBtn;
        public Button ModelBtn;
        public Button GuideBtn;
        public Sprite plFlag;
        public Sprite usFlag;
        public ScrollRect scrollView;
        public Image Background;
        public Image Logo;

        public Text IsDemoText;

        private IMG2Sprite imgConverter;

        private List<GameObject> currentScenes = new List<GameObject>();


        private void Start()
        {
            scrollView.gameObject.SetActive(false);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            imgConverter = FindObjectOfType<IMG2Sprite>();
            ModelBtn.onClick.AddListener(ModelBtnClick);
            BattleBtn.onClick.AddListener(BattleBtnClick);
            GuideBtn.onClick.AddListener(GuideBtnClick);
            AddLicenseBtn.onClick.AddListener(AddLicenseClick);
            AddLicenseBtn2.onClick.AddListener(AddLicenseClick);


            InitMenuBtns();
            Init();
            AddLicenseBtn2.gameObject.SetActive(false);
            var location = arLocationProvider.CurrentLocation.ToLocation();


            var activePlace = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
            bool isDemo = ModelsQueue.IsDemo.HasValue ? ModelsQueue.IsDemo.Value : !activePlace.GSOrderProductLicenseId.HasValue;

            var place = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();

            ModelButtons.RemoveAll(x => x == null);
            BattleButtons.RemoveAll(x => x == null);
            if (!BattleButtons.Any())
            {
                foreach (var scene in place.Scenes.Where(x => x.GroupType == SceneGroupType.Battle).OrderBy(x => x.Name))
                {
                    StartCoroutine(CreateScenesBtn(scene.Name, scene.ButtonImage, isDemo));
                }
                BattleButtons.AddRange(currentScenes);
            }
            currentScenes = new List<GameObject>();

            if (!ModelButtons.Any())
            {
                foreach (var scene in place.Scenes.Where(x => x.GroupType == SceneGroupType.Model).OrderBy(x => x.Name))
                {
                    StartCoroutine(CreateScenesBtn(scene.Name, scene.ButtonImage, isDemo));
                }
                ModelButtons.AddRange(currentScenes);
            }

            if (ModelsQueue.BackToScenesType.HasValue && ModelsQueue.BackToScenesType.Value != SceneGroupType.Guide)
            {
                StartCoroutine(CreateScenesWithType(ModelsQueue.BackToScenesType.Value));
            }
        }

        void InitMenuBtns()
        {
            Debug.Log("Metoda przyciski InitMenuBtns1");
      //      var df = imgConverter.LoadNewSprite(ModelsQueue.ButtonDefaultFilePath);
            Debug.Log("Metoda przyciski InitMenuBtns2");
            //if (df != null)
            //{
            //    Debug.Log("Metoda przyciski InitMenuBtns3");
            //    ModelBtn.image.sprite = df;
            //    ModelBtn.transition = Selectable.Transition.None;
            //    BattleBtn.image.sprite = df;
            //    BattleBtn.transition = Selectable.Transition.None;
            //    GuideBtn.image.sprite = df;
            //    BattleBtn.transition = Selectable.Transition.None;
            //    CloseBtn.image.sprite = df;
            //    CloseBtn.transition = Selectable.Transition.None;
            //    AddLicenseBtn.image.sprite = df;
            //    AddLicenseBtn.transition = Selectable.Transition.None;
            //    AddLicenseBtn2.image.sprite = df;
            //    AddLicenseBtn2.transition = Selectable.Transition.None;
            //    Debug.Log("Metoda przyciski InitMenuBtns4");
            //}
            //yield return null;
            Debug.Log("Metoda przyciski InitMenuBtns5");
        //    var bg = imgConverter.LoadNewSprite(ModelsQueue.BackgroundFilePath);
            Debug.Log("Metoda przyciski InitMenuBtns6");
         //   if (bg != null)
            {
                Debug.Log("Metoda przyciski InitMenuBtns7");
           //     Background.sprite = bg;
                Debug.Log("Metoda przyciski InitMenuBtns8");
            }
            //yield return null;
      //      var grA = imgConverter.LoadNewSprite(ModelsQueue.ButtonGroupAFilePath);
            Debug.Log("Metoda przyciski InitMenuBtns9");
       //     if (grA != null)
            {
                Debug.Log("Metoda przyciski InitMenuBtns10");
              //  ModelBtn.image.sprite = grA;
             //   ModelBtn.transition = Selectable.Transition.None;
                Debug.Log("Metoda przyciski InitMenuBtns11");
            }
            //yield return null;
            Debug.Log("Metoda przyciski InitMenuBtns12");
        //    var grB = imgConverter.LoadNewSprite(ModelsQueue.ButtonGroupBFilePath);
            Debug.Log("Metoda przyciski InitMenuBtns13");
       //     if (grB != null)
            {
                Debug.Log("Metoda przyciski InitMenuBtns14");
         //       BattleBtn.image.sprite = grB;
                Debug.Log("Metoda przyciski InitMenuBtns15");
                BattleBtn.transition = Selectable.Transition.None;
                Debug.Log("Metoda przyciski InitMenuBtns16");
            }
            Debug.Log("Metoda przyciski InitMenuBtns17");
            //yield return null;
            var gu = imgConverter.LoadNewSprite(ModelsQueue.ButtonGuideFilePath);
            Debug.Log("Metoda przyciski InitMenuBtns18");
            if (gu != null)
            {
                Debug.Log("Metoda przyciski InitMenuBtns19");
                GuideBtn.image.sprite = gu;
                Debug.Log("Metoda przyciski InitMenuBtns20");
                GuideBtn.transition = Selectable.Transition.None;
                Debug.Log("Metoda przyciski InitMenuBtns21");
            }
            //yield return null;
            Debug.Log("Metoda przyciski InitMenuBtns22");
            var cl = imgConverter.LoadNewSprite(ModelsQueue.ButtonCloseFilePath);
            Debug.Log("Metoda przyciski InitMenuBtns23");
            //var cl = await Task.Run(() => LoadButton(ModelsQueue.ButtonCloseFilePath));
            if (cl != null)
            {
                Debug.Log("Metoda przyciski InitMenuBtns24");
                CloseBtn.image.sprite = cl;
                Debug.Log("Metoda przyciski InitMenuBtns25");
                CloseBtn.transition = Selectable.Transition.None;
                Debug.Log("Metoda przyciski InitMenuBtns26");
            }
            //yield return null;
            Debug.Log("Metoda przyciski InitMenuBtns27");

            //var li = await Task.Run(() => LoadButton(ModelsQueue.ButtonLicenseFilePath));
            var li = imgConverter.LoadNewSprite(ModelsQueue.ButtonLicenseFilePath);
            Debug.Log("Metoda przyciski InitMenuBtns28");
            if (li != null)
            {
                Debug.Log("Metoda przyciski InitMenuBtns29");
                AddLicenseBtn.image.sprite = li;
                AddLicenseBtn.transition = Selectable.Transition.None;
                AddLicenseBtn2.image.sprite = li;
                AddLicenseBtn2.transition = Selectable.Transition.None;
                Debug.Log("Metoda przyciski InitMenuBtns30");
            }
            //yield return null;
            Debug.Log("Metoda przyciski InitMenuBtns31");

            var lo = imgConverter.LoadNewSprite(ModelsQueue.LogoFilePath);
            Debug.Log("Metoda przyciski InitMenuBtns31");
            if (lo != null)
            {
                Debug.Log("Metoda przyciski InitMenuBtns32");
                Logo.sprite = lo;
                Debug.Log("Metoda przyciski InitMenuBtns33");
            }
            //yield return null;
            Debug.Log("Metoda przyciski InitMenuBtns34");
            bool? isDemo = ModelsQueue.IsDemo.HasValue ? ModelsQueue.IsDemo.Value : !ModelsQueue.Places.FirstOrDefault().GSOrderProductLicenseId.HasValue;
            Debug.Log("Metoda przyciski InitMenuBtns35");
            IsDemoText.gameObject.SetActive(isDemo ?? true);
            Debug.Log("Metoda przyciski InitMenuBtns36");
        }

        private void Init()
        {
            ModelsQueue.ActivePlace = ModelsQueue.Places.FirstOrDefault()?.Name;
            //ModelBtn.gameObject.GetComponentInChildren<Text>().text = ModelsQueue.Places.FirstOrDefault()?.SceneGroup1Name;
            //BattleBtn.gameObject.GetComponentInChildren<Text>().text = ModelsQueue.Places.FirstOrDefault()?.SceneGroup2Name;
            //MenuButtonsActive(true);
        }

        public void ModelBtnClick()
        {
            StartCoroutine(CreateScenesWithType(SceneGroupType.Model));
        }

        public void BattleBtnClick()
        {
            StartCoroutine(CreateScenesWithType(SceneGroupType.Battle));
        }

        public void GuideBtnClick()
        {
            StartCoroutine(LoadSceneAsync("GuideScene"));
        }

        IEnumerator LoadSceneAsync(string name)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        public void QuestionMarkClick()
        {
            Application.OpenURL(TemporaryDatabase.CompetitionURL);
        }

        IEnumerator CreateScenesWithType(SceneGroupType type)
        {
            ModelsQueue.BackToScenesType = type;
            scrollView.content.gameObject.transform.localPosition = new Vector3();
            //basePosition = new Vector3(Screen.height / 20, -Screen.height / 4.5f, 0);
            var place = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
            if (place == null)
            {
                yield return null;
            }
            scrollView.verticalNormalizedPosition = 1;
            ModelButtons.RemoveAll(x => x == null);
            BattleButtons.RemoveAll(x => x == null);
            ModelButtons.ForEach(x => x.SetActive(false));
            BattleButtons.ForEach(x => x.SetActive(false));

            var activePlace = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
            bool isDemo = ModelsQueue.IsDemo.HasValue ? ModelsQueue.IsDemo.Value : !activePlace.GSOrderProductLicenseId.HasValue;

            IsDemoText.gameObject.SetActive(isDemo);

            if (type == SceneGroupType.Model)
            {
                if (!ModelButtons.Any())
                {
                    currentScenes = new List<GameObject>();
                    foreach (var scene in place.Scenes.Where(x => x.GroupType == type).OrderBy(x => x.Name))
                    {
                        StartCoroutine(CreateScenesBtn(scene.Name, scene.ButtonImage, isDemo));
                        yield return null;
                    }
                    ModelButtons.AddRange(currentScenes);
                }
                else
                {
                    ModelButtons.ForEach(x => x.SetActive(true));
                    BattleButtons.ForEach(x => x.SetActive(false));
                    yield return null;
                }
            }
            else if (type == SceneGroupType.Battle)
            {
                if (!BattleButtons.Any())
                {
                    currentScenes = new List<GameObject>();
                    foreach (var scene in place.Scenes.Where(x => x.GroupType == type).OrderBy(x => x.Name))
                    {
                        StartCoroutine(CreateScenesBtn(scene.Name, scene.ButtonImage, isDemo));
                        yield return null;
                    }
                    BattleButtons.AddRange(currentScenes);
                }
                else
                {
                    ModelButtons.ForEach(x => x.SetActive(false));
                    BattleButtons.ForEach(x => x.SetActive(true));
                    yield return null;
                }
            }
            MenuButtonsActive(false);
        }

        private void MenuButtonsActive(bool active)
        {
            ModelBtn.gameObject.SetActive(active);
            BattleBtn.gameObject.SetActive(active);
            GuideBtn.gameObject.SetActive(active);
            CloseBtn.gameObject.SetActive(active);
            QuestionmarkBtn.gameObject.SetActive(active);
            AddLicenseBtn.gameObject.SetActive(active);

            AddLicenseBtn2.gameObject.SetActive(!active);
            scrollView.gameObject.SetActive(!active);
            var svc = FindObjectOfType<ScrollViewController>();
            if (svc)
            {
                svc.Enabled = !scrollView.gameObject.activeSelf;
            }
        }

        IEnumerator CreateScenesBtn(string text, string button = null, bool demo = false)
        {

            Debug.Log("Metoda przyciski CreateScenesBtn1");
            var btn = Instantiate(btnPrefab);
            Debug.Log("Metoda przyciski CreateScenesBtn2");
            DontDestroyOnLoad(btn.gameObject);
            Debug.Log("Metoda przyciski CreateScenesBtn3");
            var txt = btn.GetComponentInChildren<Text>();
            Debug.Log("Metoda przyciski CreateScenesBtn4");
            txt.text = demo ? "DEMO" : "";
            Debug.Log("Metoda przyciski CreateScenesBtn5");
            txt.name = text;
            Debug.Log("Metoda przyciski CreateScenesBtn6");
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 270);
            Debug.Log("Metoda przyciski CreateScenesBtn7");
            btn.transform.SetParent(scrollView.content.gameObject.transform, false);
            Debug.Log("Metoda przyciski CreateScenesBtn8");
            txt.alignment = TextAnchor.LowerRight;
            Debug.Log("Metoda przyciski CreateScenesBtn9");
            txt.color = Color.red;
            Debug.Log("Metoda przyciski CreateScenesBtn10");
            var btnComponent = btn.GetComponent<Button>();
            Debug.Log("Metoda przyciski CreateScenesBtn11");

            //if (!string.IsNullOrEmpty(button))
            //{
            //    Debug.Log("Metoda przyciski CreateScenesBtn111");
            //    var image = imgConverter.LoadNewSprite(button);
            //    Debug.Log("Metoda przyciski CreateScenesBtn112");
            //    if (image != null)
            //    {
            //        Debug.Log("Metoda przyciski CreateScenesBtn113");
            //        btnComponent.image.sprite = image;
            //        Debug.Log("Metoda przyciski CreateScenesBtn114");
            //        btnComponent.transition = Selectable.Transition.None;
            //        Debug.Log("Metoda przyciski CreateScenesBtn115");
            //    }
            //}
            //else
            //{
            //    Debug.Log("Metoda przyciski CreateScenesBtn116");
            //    var df = imgConverter.LoadNewSprite(ModelsQueue.ButtonDefaultFilePath);
            //    Debug.Log("Metoda przyciski CreateScenesBtn117");
            //    if (df != null)
            //    {
            //        Debug.Log("Metoda przyciski CreateScenesBtn118");
            //        btnComponent.image.sprite = df;
            //        Debug.Log("Metoda przyciski CreateScenesBtn119");
            //        btnComponent.transition = Selectable.Transition.None;
            //        Debug.Log("Metoda przyciski CreateScenesBtn120");
            //    }
            //    Debug.Log("Metoda przyciski CreateScenesBtn121");
            //}
            Debug.Log("Metoda przyciski CreateScenesBtn122");
            btnComponent.onClick.AddListener(() => SelectSceneClick(btnComponent));
            Debug.Log("Metoda przyciski CreateScenesBtn123");
            //basePosition = new Vector3(basePosition.x, basePosition.y - Screen.height / 5f, 0);
            currentScenes.Add(btn);

            Debug.Log("Metoda przyciski CreateScenesBtn13");
            yield return null;
        }

        private void Update()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ModelButtons.ForEach(x => x.SetActive(false));
                    BattleButtons.ForEach(x => x.SetActive(false));
                    MenuButtonsActive(true);
                }
            }
        }

        public void ChangeLanguage(bool changed = true)
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
                PlayerPrefs.Save();
                GUIAnimSystemFREE.Instance.LoadLevel("LoadingScreen", 1.5f);
            }

            switch (PlayerPrefs.GetString("lang"))
            {
                case "pl-PL":
                    LanguageButton.GetComponent<Image>().sprite = plFlag;
                    break;
                case "en-US":
                    LanguageButton.GetComponent<Image>().sprite = usFlag;
                    break;
            }
            //AddLicenseBtn.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("AddLicense");
            //AddLicenseBtn2.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("AddLicense");
            //CloseBtn.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("Close");
            //GuideBtn.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("Guide");
        }

        public void AddLicenseClick()
        {
            SceneManager.LoadScene("LicenseScene");
        }

        public void SelectSceneClick(Button button)
        {
            string scene = button.gameObject.GetComponentInChildren<Text>().name;
            ModelsQueue.ActiveScene = scene;
            var activePlace = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
            ModelsQueue.ActiveSceneId = activePlace.Scenes.Where(x => x.Name == scene).First().Id;
            ModelsQueue.Restart(ModelsQueue.ActiveSceneId);
            ModelsQueue.Messages.Clear();

            var buttons = GetComponents<Button>();
            foreach(var b in buttons)
            {
                Debug.Log($"Niszcze button {b}");

                DestroyImmediate(b.gameObject);
                Debug.Log($"Po Niszcze button {b}");

            }
            foreach (var t in ModelsQueue.Textures2D)
            {
                Debug.Log($"Niszcze teksture {t.Value}");
                DestroyImmediate(t.Value);
                Debug.Log($"Po Niszcze teksture {t.Value}");
            }
            foreach(var m in ModelsQueue.Materials)
            {

                try
                {
                    Debug.Log($"Niszcze material {m}");
                    DestroyImmediate(m);
                    Debug.Log($"Po Niszcze material {m}");

                }
                catch (Exception ex) { }
            }
            ModelsQueue.Materials.Clear();
            ModelsQueue.Textures2D.Clear();

            SceneManager.LoadScene("ModelsLoadingScene");
        }

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

        private void OnGUI()
        {
            ChangeLanguage(false);
        }


        void OnApplicationQuit()
        {
            new MobileAppUsingEventLogService(TemporaryDatabase.AppStartDate.Value, arLocationProvider.CurrentLocation.ToLocation()).Add();
        }
        public void CloseAppClick()
        {
            Application.Quit();
        }
    }
}
