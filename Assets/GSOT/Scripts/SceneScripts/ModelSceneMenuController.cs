using ARLocation;
using ARLocation.Utils;
using Assets.GSOT.Scripts.ApiScripts.EventLog;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using Assets.GSOT.Scripts.SceneScripts;
using Assets.GSOT.Scripts.Utils;
using Assets.GSOT.Scripts.Models.ApiModels;
using GSOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

public class ModelSceneMenuController : MonoBehaviour
{


    public GameObject cameraRoot;
    public GameObject menuCamera;
    public Slider heightSlider;
    public Slider azimuthSlider;
    public Slider animationSlider;
    public GameObject RadialAnimationProgressBar;
    public Text debugText;
    public List<GameObject> models = new List<GameObject>();
    public GameObject locationRoot;
    public Button showSliderButton;
    public Button pauseButton;
    public Button playButton;
    public Button refreshButton;
    public Canvas menuCanvas;
    private AnimationComponent animationComponent;
    private ARLocationOrientation orientation;
    private int animationLength = 0;
    private float elapsedTime = 0f;
    private Stopwatch animationTimer;
    private bool descriptionIn = false;
    private float oldSliderData = ModelsQueue.HeightCorrection;
    public ARLocationProvider _arLocationProvider;
    private bool settingsActive = false;
    private bool lockScene = false;
    private bool isDemo = false;
    private long placeId;
    private Vector2 centroid;

    public GameObject DemoEndedPopup;
    public Text DemoEndedMsg;
    public Button DemoEndedBtn;
    public GameObject TopBar;
    public GameObject RotationArrow;
    public Text DistanceToCenter;
    public Button BackToScenesBtn;
    public Button StartTableSceneButton;
    public Button ScreenShotButton;
    public Button ShopButton;
    DebugController debugController;
    bool initialized = false;
    public Button MoreInfoBtn;
    public Button SlideUpBtn;
    public Button SlideDownBtn;
    public Button AzimuthLeftBtn;
    public Button AzimuthRightBtn;
    public Text HeightCorrectionText;
    public Button RefreshBtn;
    private bool initDone = false;
    public GameObject NoGPSText;

    public ButtonPressed AzLeft;
    public ButtonPressed AzRight;
    public ButtonPressed BtnUp;
    public ButtonPressed BtnDown;
    public GameObject DescriptionBgObject;
    public GameObject TableFloor;

    public float azimuthSliderValue = 0f;


    public GameObject ExitSceneInfo;
    bool ExitScene = false;

    void Start()
    {
        Debug.Log("Metoda Menu Update1");
        var activePlace = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
        Debug.Log("Metoda Menu Update2");
        if (activePlace == null) activePlace = new ApplicationPlace();
        Debug.Log("Metoda Menu Update3");
        isDemo = ModelsQueue.IsDemo.HasValue ? ModelsQueue.IsDemo.Value : (activePlace != null ? !activePlace.GSOrderProductLicenseId.HasValue : true);
        Debug.Log("Metoda Menu Update4");
        this.placeId = activePlace != null ? activePlace.Id : 0;
        Debug.Log("Metoda Menu Update5");

        Time.timeScale = 1;
        Debug.Log("Metoda Menu Update6");
        pauseButton.gameObject.SetActive(true);
        Debug.Log("Metoda Menu Update7");
        playButton.gameObject.SetActive(false);
        Debug.Log("Metoda Menu Update8");
        ExitSceneInfo.gameObject.SetActive(false);
        Debug.Log("Metoda Menu Update9");
        DemoEndedMsg.text = Translator.Instance().GetString("DemoEndingMsg");
        Debug.Log("Metoda Menu Update10");
        DemoEndedMsg.text = DemoEndedMsg.text.Replace("#place#", activePlace.Name);
        Debug.Log("Metoda Menu Update11");


        ShopButton.gameObject.SetActive(isDemo);
        Debug.Log("Metoda Menu Update12");
        debugController = FindObjectOfType<DebugController>();
        Debug.Log("Metoda Menu Update13");
        DistanceToCenter.text = "";
        Debug.Log("Metoda Menu Update4");
        if (ModelsQueue.IsTableScene)
        {
            Debug.Log("Metoda Menu Update5");
            RotationArrow.gameObject.SetActive(false);
            DistanceToCenter.gameObject.SetActive(false);
            azimuthSlider.gameObject.SetActive(false);
            oldSliderData = 0f;
            Debug.Log("Metoda Menu Update6");
        }
        Debug.Log("Metoda Menu Update7");
        TopBar.transform.position = new Vector3(Screen.width / 2, Screen.height - 60, 0);
        Debug.Log("Metoda Menu Update8");
        azimuthSlider.transform.position = new Vector3(Screen.width / 2, Screen.height / 5);
        Debug.Log("Metoda Menu Update9");
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Debug.Log("Metoda Menu Update10");
    }

    private bool LocationActive()
    {
        Debug.Log("Metoda Menu LocationActive1");
        if (Application.isEditor) return true;
        Debug.Log("Metoda Menu LocationActive2");
        return _arLocationProvider.CurrentLocation.latitude != 0 && _arLocationProvider.CurrentLocation.longitude != 0;
    }

    private void Init()
    {
        Debug.Log("Metoda Menu Init1");
        BackToScenesBtn.onClick.AddListener(() => BackToScenesBtnClick());
        Debug.Log("Metoda Menu Init2");

        if (Application.isEditor)
        {
            Debug.Log("Metoda Menu Init3");
            Camera.main.clearFlags = CameraClearFlags.Skybox;
            Debug.Log("Metoda Menu Init4");
        }
        Debug.Log("Metoda Menu Init5");
        debugText.gameObject.SetActive(false);//for testing
        Debug.Log("Metoda Menu Init6");

        animationComponent = menuCanvas.gameObject.GetComponentInChildren<AnimationComponent>();
        Debug.Log("Metoda Menu Init7");
        orientation = locationRoot.GetComponentInChildren<ARLocationOrientation>();
        Debug.Log("Metoda Menu Init8");
        #region heightSlider
        if (!ModelsQueue.IsTableScene)
        {
            heightSlider.value = ModelsQueue.HeightCorrection;
        }

        HeightCorrectionText.text = $"{ModelsQueue.HeightCorrection} m";
        var heightText = heightSlider.GetComponentInChildren<Text>();
        heightText.text = Translator.Instance().GetString("ModelLevel");
        #endregion
        #region azimuthSlider
        azimuthSlider.value = ModelsQueue.AzimuthCorrection;
        azimuthSliderValue = ModelsQueue.AzimuthCorrection;
        azimuthSlider.onValueChanged.AddListener((x) => { AzimuthSliderValueChanged(x); });
        azimuthSlider.gameObject.SetActive(false);
        var azimuthText = azimuthSlider.GetComponentInChildren<Text>();
        oldAzimuthData = ModelsQueue.AzimuthCorrection;
        azimuthText.text = Translator.Instance().GetString("ChangeViewDirection");

        #endregion
        #region buttons
        float moveX = Screen.width / 8f;
        showSliderButton.onClick.AddListener(() => ShowSliderButtonClick());

        pauseButton.onClick.AddListener(() => PauseButtonClick());
        playButton.onClick.AddListener(() => PlayButtonClick());
        refreshButton.onClick.AddListener(() => RefreshButtonClick());
        #endregion
        #region animationSlider
        //animationSlider.transform.position = new Vector3(Screen.width - Screen.width / 4, Screen.height - Screen.height / 10);
        if (ModelsQueue.GetSceneAnimationLength() > 0)
        {
            //animationSlider.maxValue = (int)ModelsQueue.GetSceneAnimationLength();
            animationLength = (int)ModelsQueue.GetSceneAnimationLength();

            RadialAnimationProgressBar.GetComponent<ProgressBar>().TotalTime = (int)ModelsQueue.GetSceneAnimationLength();
            RadialAnimationProgressBar.GetComponent<ProgressBar>().currentAmount = 0;// (int)ModelsQueue.GetSceneAnimationLength();
        }
        else
        {
            //animationSlider.gameObject.SetActive(false);
        }
        #endregion

        initialized = true;
        Debug.Log("Metoda Menu Init9");
    }

    private void PlayButtonClick()
    {
        Debug.Log("Metoda Menu PlayButtonClick1");
        if (animationTimer == null) return;
        Debug.Log("Metoda Menu PlayButtonClick2");
        Time.timeScale = 1;
        animationTimer.Start();
        Debug.Log("Metoda Menu PlayButtonClick3");
        pauseButton.gameObject.SetActive(true);
        Debug.Log("Metoda Menu PlayButtonClick4");
        playButton.gameObject.SetActive(false);
        Debug.Log("Metoda Menu PlayButtonClick5");

        var sounds = GameObject.Find("sounds").GetComponentsInChildren<AudioController>();
        Debug.Log("Metoda Menu PlayButtonClick6");
        if (sounds != null)
        {
            Debug.Log("Metoda Menu PlayButtonClick7");
            foreach (var sound in sounds.Where(x => x.enabled))
            {
                Debug.Log("Metoda Menu PlayButtonClick8");
                sound.Play();
                Debug.Log("Metoda Menu PlayButtonClick9");
            }
        }
        Debug.Log("Metoda Menu PlayButtonClick10");
        var videos = FindObjectsOfType<VideoPlayer>();
        Debug.Log("Metoda Menu PlayButtonClick11");
        if (videos != null && videos.Any())
        {
            Debug.Log("Metoda Menu PlayButtonClick12");
            foreach (var video in videos)
            {
                Debug.Log("Metoda Menu PlayButtonClick13");
                video.Play();
                Debug.Log("Metoda Menu PlayButtonClick14");
            }
            Debug.Log("Metoda Menu PlayButtonClick15");
        }
    }

    private void PauseButtonClick()
    {
        Debug.Log("Metoda Menu PauseButtonClick1");
        if (animationTimer == null) return;
        Debug.Log("Metoda Menu PauseButtonClick2");
        Time.timeScale = 0;
        Debug.Log("Metoda Menu PauseButtonClick3");
        animationTimer.Stop();
        Debug.Log("Metoda Menu PauseButtonClick4");
        pauseButton.gameObject.SetActive(false);
        Debug.Log("Metoda Menu PauseButtonClick5");
        playButton.gameObject.SetActive(true);
        Debug.Log("Metoda Menu PauseButtonClick6");

        var sounds = GameObject.Find("sounds").GetComponentsInChildren<AudioController>();
        Debug.Log("Metoda Menu PauseButtonClick7");
        if (sounds != null)
        {
            Debug.Log("Metoda Menu PauseButtonClick8");
            foreach (var sound in sounds.Where(x => x.enabled))
            {
                Debug.Log("Metoda Menu PauseButtonClick9");
                sound.Stop();
                Debug.Log("Metoda Menu PauseButtonClick10");
            }
        }
        Debug.Log("Metoda Menu PauseButtonClick11");
        var videos = FindObjectsOfType<VideoPlayer>();
        Debug.Log("Metoda Menu PauseButtonClick12");
        if (videos != null && videos.Any())
        {
            Debug.Log("Metoda Menu PauseButtonClick13");
            foreach (var video in videos)
            {
                Debug.Log("Metoda Menu PauseButtonClick14");
                video.Pause();
                Debug.Log("Metoda Menu PauseButtonClick15");
            }
        }
    }

    public void Refresh()
    {
        Debug.Log("Metoda Menu Refresh1");
        RefreshButtonClick();
        Debug.Log("Metoda Menu Refresh2");

    }

    private void RefreshButtonClick()
    {
        Debug.Log("Metoda Menu RefreshButtonClick1");
        Time.timeScale = 1;
        Debug.Log("Metoda Menu RefreshButtonClick2");

        var sounds = GameObject.Find("sounds").GetComponentsInChildren<AudioController>().ToList();
        Debug.Log("Metoda Menu RefreshButtonClick3");

        var audioS = GameObject.Find("sounds").GetComponentsInChildren<AudioSource>().ToList();
        Debug.Log("Metoda Menu RefreshButtonClick4");

        sounds.ForEach(x => x.enabled = false);
        Debug.Log("Metoda Menu RefreshButtonClick5");

        audioS.ForEach(x => x.enabled = false);
        Debug.Log("Metoda Menu RefreshButtonClick6");

        StartCoroutine(WaitAndChangeScene());
        Debug.Log("Metoda Menu RefreshButtonClick7");

    }

    IEnumerator WaitAndChangeScene()
    {
        Debug.Log("Metoda Menu WaitAndChangeScene1");
        yield return new WaitForSecondsRealtime(1);
        Debug.Log("Metoda Menu WaitAndChangeScene2");
        ModelsQueue.Restart(ModelsQueue.ActiveSceneId);
        Debug.Log("Metoda Menu WaitAndChangeScene3");
        ModelsQueue.InitScene();
        Debug.Log("Metoda Menu WaitAndChangeScene4");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Metoda Menu WaitAndChangeScene5");
    }

    private void BackToScenesBtnClick()
    {
        Debug.Log("Metoda Menu BackToScenesBtnClick1");
        var sounds = GameObject.Find("sounds").GetComponentsInChildren<AudioController>().ToList();
        var audioS = GameObject.Find("sounds").GetComponentsInChildren<AudioSource>().ToList();
        Debug.Log("Metoda Menu BackToScenesBtnClick2");
        sounds.ForEach(x => x.enabled = false);
        audioS.ForEach(x => x.enabled = false);
        Debug.Log("Metoda Menu BackToScenesBtnClick3");
        new MobileAppSceneUsingEventLogService(TemporaryDatabase.ActiveSceneStartDate.Value,
            TemporaryDatabase.ActiveSceneUsingMode.Value, TemporaryDatabase.ActiveSceneId.Value, TemporaryDatabase.ActiveScenePlaceId.Value,
            TemporaryDatabase.ActiveSceneOrderProductLicenseId, _arLocationProvider.CurrentLocation.ToLocation()).Add();
        Debug.Log("Metoda Menu BackToScenesBtnClick4");
        SceneManager.LoadScene("ScenesScene");
        Debug.Log("Metoda Menu BackToScenesBtnClick5");
    }

    private void ShowSliderButtonClick()
    {
        Debug.Log("Metoda Menu ShowSliderButtonClick1");
        //heightSlider.gameObject.SetActive(!heightSlider.gameObject.activeSelf);
        SlideUpBtn.gameObject.SetActive(!SlideUpBtn.gameObject.activeSelf);
        SlideDownBtn.gameObject.SetActive(!SlideDownBtn.gameObject.activeSelf);
        Debug.Log("Metoda Menu ShowSliderButtonClick2");
        //if (!ModelsQueue.IsTableScene && !ModelsQueue.IsPlaygroundScene)
        //{
        AzimuthLeftBtn.gameObject.SetActive(!AzimuthLeftBtn.gameObject.activeSelf);
        AzimuthRightBtn.gameObject.SetActive(!AzimuthRightBtn.gameObject.activeSelf);
        Debug.Log("Metoda Menu ShowSliderButtonClick3");
        //}
        HeightCorrectionText.gameObject.SetActive(!HeightCorrectionText.gameObject.activeSelf);
        Debug.Log("Metoda Menu ShowSliderButtonClick4");
        if (!ModelsQueue.IsTableScene)
        {
            //azimuthSlider.gameObject.SetActive(!azimuthSlider.gameObject.activeSelf);
        }
        Debug.Log("Metoda Menu ShowSliderButtonClick5");
        //animationSlider.gameObject.SetActive(!azimuthSlider.gameObject.activeSelf);
        settingsActive = !settingsActive;
        Debug.Log("Metoda Menu ShowSliderButtonClick6");
    }

    float oldAzimuthData = 0;

    public void AzimuthRight()
    {
    }
    public void AzimuthLeft()
    {
    }
    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) { return Quaternion.Euler(angles) * (point - pivot) + pivot; }
    private void AzimuthSliderValueChanged(float value)
    {
        Debug.Log("Metoda Menu AzimuthSliderValueChanged1");
        var delta = value - oldAzimuthData;

        if (orientation.TrueNorthOffset != value)
        {
            orientation.TrueNorthOffset = value;
            var currLocation = _arLocationProvider.CurrentLocation.ToLocation();
            foreach (var model in this.models)
            {

                var moveComponent = model.gameObject.GetComponentInChildren<ObjectMovingController>();
                if (moveComponent != null)
                {
                    List<Vector3> newPoints = new List<Vector3>();
                    foreach (var point in moveComponent.localPoints)
                    {
                        Vector3 p;
                        p = RotatePointAroundPivot(point, centroid, new Vector3(0, delta, 0));
                        newPoints.Add(p);
                    }
                    if (ModelsQueue.IsTableScene)
                    {
                        TableFloor.transform.RotateAround(centroid, Vector3.up, delta/2);
                        //var newTableCenter = RotatePointAroundPivot(TableFloor.transform.position, centroid, new Vector3(0, delta/3, 0));
                        //TableFloor.transform.position = newTableCenter;
                    }
                    moveComponent.localPoints = newPoints;
                    if (moveComponent.Spline == null && moveComponent.localPoints.Count() == 1 || PointsTheSame(moveComponent.localPoints))
                    {
                        moveComponent.gameObject.transform.position = newPoints[0];
                    }
                    else
                    {
                        var pos = moveComponent.gameObject.transform.position;
                        moveComponent.gameObject.transform.position = pos;
                        moveComponent.Spline = Misc.BuildSpline(SplineType.CatmullromSpline, newPoints.ToArray(), 500, 0.5f);
                    }
                }
            }
        }

        ModelsQueue.AzimuthCorrection = value;
        oldAzimuthData = value;
    }

    public void SlideUp()
    {
        //ChangeHeight(0.25f);
    }
    public void SlideDown()
    {
        //ChangeHeight(-0.25f);
    }

    public void ChangeHeight(float value)
    {
        Debug.Log("Metoda Menu ChangeHeight1");
        value = (float)(Math.Round(value * 20) / 20);
        if ((ModelsQueue.HeightCorrection >= 10 && value > 0) || (ModelsQueue.HeightCorrection <= -10 && value < 0))
        {
            return;
        }
        foreach (var model in this.models)
        {
            var moveComponent = model.gameObject.GetComponentInChildren<ObjectMovingController>();
            if (moveComponent != null)
            {
                List<Vector3> newPoints = new List<Vector3>();
                foreach (var point in moveComponent.localPoints)
                {
                    Vector3 p;
                    if (ModelsQueue.IsTableScene)
                    {
                        p = new Vector3(point.x, point.y + (value * 0.06f), point.z);
                        //debugController.Push("Height table correction", (delta * 0.06f));
                    }
                    else
                    {
                        p = new Vector3(point.x, point.y + value, point.z);
                    }
                    newPoints.Add(p);
                }
                moveComponent.localPoints = newPoints;
                if (moveComponent.Spline == null && moveComponent.localPoints.Count() == 1 || PointsTheSame(moveComponent.localPoints))
                {
                    moveComponent.gameObject.transform.position = newPoints[0];
                }
                else
                {
                    var pos = moveComponent.gameObject.transform.position;
                    //pos.y += (value * 0.06f);
                    moveComponent.gameObject.transform.position = pos;
                    moveComponent.Spline = Misc.BuildSpline(SplineType.CatmullromSpline, newPoints.ToArray(), 500, 0.5f);
                }
            }
        }
        if (!ModelsQueue.IsTableScene)
        {
            ModelsQueue.HeightCorrection += value;
            HeightCorrectionText.text = $"{ModelsQueue.HeightCorrection:0.00} m";
        }
    }

    private void HeightSliderValueChanged(float value)
    {
        Debug.Log("Metoda Menu HeightSliderValueChanged1");
        var delta = value - oldSliderData;
        foreach (var model in this.models)
        {
            var moveComponent = model.gameObject.GetComponentInChildren<ObjectMovingController>();
            if (moveComponent != null)
            {
                List<Vector3> newPoints = new List<Vector3>();
                foreach (var point in moveComponent.localPoints)
                {
                    Vector3 p;
                    if (ModelsQueue.IsTableScene)
                    {
                        p = new Vector3(point.x, point.y + (delta * 0.06f), point.z);
                        //debugController.Push("Height table correction", (delta * 0.06f));
                    }
                    else
                    {
                        p = new Vector3(point.x, point.y + delta, point.z);
                    }
                    newPoints.Add(p);
                }
                moveComponent.localPoints = newPoints;
                if (moveComponent.Spline == null && moveComponent.localPoints.Count() == 1 || PointsTheSame(moveComponent.localPoints))
                {
                    moveComponent.gameObject.transform.position = newPoints[0];
                }
                else
                {
                    var pos = moveComponent.gameObject.transform.position;
                    pos.y += (delta * 0.06f);
                    moveComponent.gameObject.transform.position = pos;
                    moveComponent.Spline = Misc.BuildSpline(SplineType.CatmullromSpline, newPoints.ToArray(), 500, 0.5f);
                }
            }
        }
        if (!ModelsQueue.IsTableScene)
        {
            ModelsQueue.HeightCorrection += delta;
        }
        oldSliderData = value;
    }

    private bool PointsTheSame(IEnumerable<Vector3> points)
    {
        Debug.Log("Metoda Menu PointsTheSame1");
        return points.GroupBy(x => x).Count() == 1;
    }

    void Update()
    {
        Debug.Log("Metoda Menu Update1");

        debugController.Push(ARLocationManager.Instance.GetARSessionInfoString() ?? "ARSessionState", "true");
        Debug.Log("Metoda Menu Update2");
        if (NoGPSText.activeSelf)
        {
            Debug.Log("Metoda Menu Update3");
            return;
        }
        else
        {
            Debug.Log("Metoda Menu Update4");
            if (!initDone)
            {
                Debug.Log("Metoda Menu Update5");
                Init();
                Debug.Log("Metoda Menu Update6");
                initDone = true;
            }
        }
        Debug.Log("Metoda Menu Update7");
        if (Teleport)
        {
            Debug.Log("Metoda Menu Update8");
            TpToModel();
        }
        if (ChangePosition)
        {
            Debug.Log("Metoda Menu Update9");
            ChangeUserGPSPosition();
            Debug.Log("Metoda Menu Update10");
        }
        if (!initialized)
        {
            Debug.Log("Metoda Menu Update11");
            return;
        }
        Debug.Log("Metoda Menu Update12");
        if (lockScene)
        {
            Debug.Log("Metoda Menu Update13");
            PauseButtonClick();
            Debug.Log("Metoda Menu Update14");
            playButton.interactable = false;
            Debug.Log("Metoda Menu Update15");
            showSliderButton.interactable = false;
            Debug.Log("Metoda Menu Update16");

            if (isDemo)
                refreshButton.interactable = false;
            else
            {
                Debug.Log("Metoda Menu Update17");
                refreshButton.interactable = true;
                RefreshBtn.interactable = true;
                RefreshBtn.gameObject.SetActive(true);
                Debug.Log("Metoda Menu Update18");
            }
            Debug.Log("Metoda Menu Update19");
            if (isDemo)
                DemoEndedPopup.SetActive(true);
            Debug.Log("Metoda Menu Update20");
        }
        while (!LocationActive())
        {
            Debug.Log("Metoda Menu Update21");
            return;
        }
        Debug.Log("Metoda Menu Update22");
        if (centroid.x == 0 && centroid.y == 0)
        {
            Debug.Log("Metoda Menu Update23");
            try
            {
                Debug.Log("Metoda Menu Update24");
                if (!ModelsQueue.IsTableScene || ModelsQueue.TablePoint1 != Vector3.zero)
                    GetCentroid();
                Debug.Log("Metoda Menu Update25");
            }
            catch (Exception) { }
        }
        Debug.Log("Metoda Menu Update26");
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
        {
            Debug.Log("Metoda Menu Update27");
            if (ExitScene)
            {
                Debug.Log("Metoda Menu Update28");

                var sounds = GameObject.Find("sounds").GetComponentsInChildren<AudioController>().ToList();
                var audioS = GameObject.Find("sounds").GetComponentsInChildren<AudioSource>().ToList();
                Debug.Log("Metoda Menu Update29");
                sounds.ForEach(x => x.enabled = false);
                audioS.ForEach(x => x.enabled = false);
                Debug.Log("Metoda Menu Update30");
                new MobileAppSceneUsingEventLogService(TemporaryDatabase.ActiveSceneStartDate.Value,
                    TemporaryDatabase.ActiveSceneUsingMode.Value, TemporaryDatabase.ActiveSceneId.Value, TemporaryDatabase.ActiveScenePlaceId.Value,
                    TemporaryDatabase.ActiveSceneOrderProductLicenseId, _arLocationProvider.CurrentLocation.ToLocation()).Add();
                Debug.Log("Metoda Menu Update31");
                System.GC.Collect();
                Debug.Log("Metoda Menu Update32");
                Resources.UnloadUnusedAssets();
                Debug.Log("Metoda Menu Update33");
                var smr = locationRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
                Debug.Log("Metoda Menu Update34");
                if (smr.Any())
                {
                    Debug.Log("Metoda Menu Update35");
                    foreach (var s in smr)
                    {
                        Debug.Log("Metoda Menu Update36");
                        try
                        {
                            DestroyImmediate(s.material);
                            DestroyImmediate(s);
                            Debug.Log("Metoda Menu Update37");
                        }
                        catch { }
                    }
                }
                Debug.Log("Metoda Menu Update38");
                foreach (var model in models)
                {
                    try
                    {
                        var rs = model.GetComponents<Renderer>();
                        Debug.Log("Metoda Menu Update39");
                        if (rs.Any())
                        {
                            Debug.Log("Metoda Menu Update40");
                            foreach (var r in rs)
                            {
                                try
                                {
                                    DestroyImmediate(r.material);
                                    DestroyImmediate(r);
                                    Debug.Log("Metoda Menu Update41");
                                }
                                catch { }
                            }
                        }
                        Debug.Log("Metoda Menu Update42");
                        var modelsmr = model.GetComponentsInChildren<SkinnedMeshRenderer>();
                        Debug.Log("Metoda Menu Update43");
                        if (modelsmr.Any())
                        {
                            Debug.Log("Metoda Menu Update44");
                            foreach (var s in modelsmr)
                            {
                                Debug.Log("Metoda Menu Update45");
                                try
                                {
                                    DestroyImmediate(s.material);
                                    DestroyImmediate(s);
                                    Debug.Log("Metoda Menu Update46");

                                }
                                catch { }
                            }
                        }
                        Debug.Log("Metoda Menu Update47");
                        DestroyImmediate(model);
                        Debug.Log("Metoda Menu Update48");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in releasing model resources: " + ex.Message);
                    }
                }
                Debug.Log("Metoda Menu Update49");
                if (ModelsQueue.BackToScenesType == SceneGroupType.Guide)
                {
                    Debug.Log("Metoda Menu Update50");
                    SceneManager.LoadScene("GuideScene");
                    Debug.Log("Metoda Menu Update51");
                }
                else
                {
                    Debug.Log("Metoda Menu Update52");
                    SceneManager.LoadScene("ScenesScene");
                    Debug.Log("Metoda Menu Update53");
                }
            }
            Debug.Log("Metoda Menu Update54");
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Metoda Menu Update55");
                ExitSceneInfo.gameObject.SetActive(true);
                ExitScene = true;
                Debug.Log("Metoda Menu Update56");
            }

        }
        Debug.Log("Metoda Menu Update57");
        if (!refreshButton.interactable && !lockScene)
        {
            Debug.Log("Metoda Menu Update58");
            refreshButton.interactable = true;

        }

        if (!pauseButton.interactable)
        {
            Debug.Log("Metoda Menu Update59");
            pauseButton.interactable = true;
        }
        if (!playButton.interactable && !lockScene)
        {
            Debug.Log("Metoda Menu Update60");
            playButton.interactable = true;
        }
        if (!showSliderButton.interactable && Time.timeScale > 0 && !lockScene)
        {
            Debug.Log("Metoda Menu Update61");
            showSliderButton.interactable = true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Metoda Menu Update62");
            TriggerDescription();
            Debug.Log("Metoda Menu Update63");
        }

        if (locationRoot.transform.childCount == 0)
        {
            Debug.Log("Metoda Menu Update64");
            return;
        }

        if (AzLeft.ButtonIsPressed)
        {
            Debug.Log("Metoda Menu Update65");
            if (azimuthSliderValue == -360) azimuthSliderValue = 0;
            azimuthSliderValue -= 1;
            //azimuthSlider.value -= 1f;
            AzimuthSliderValueChanged(azimuthSliderValue);
            Debug.Log("Metoda Menu Update66");
        }
        else if (AzRight.ButtonIsPressed)
        {
            Debug.Log("Metoda Menu Update67");
            if (azimuthSliderValue == 360) azimuthSliderValue = 0;
            azimuthSliderValue += 1;
            //azimuthSlider.value += 1f;
            AzimuthSliderValueChanged(azimuthSliderValue);
            Debug.Log("Metoda Menu Update68");
        }

        if (BtnUp.ButtonIsPressed)
        {
            Debug.Log("Metoda Menu Update69");
            ChangeHeight(0.05f);
            Debug.Log("Metoda Menu Update70");
        }
        else if (BtnDown.ButtonIsPressed)
        {
            Debug.Log("Metoda Menu Update71");
            ChangeHeight(-0.05f);
            Debug.Log("Metoda Menu Update72");
        }

        try
        {
            Debug.Log("Metoda Menu Update73");
            UpdateLocations();
            Debug.Log("Metoda Menu Update74");
        }
        catch (Exception) {
            Debug.Log("Metoda Menu Update75");
        }
        Debug.Log("Metoda Menu Update76");
        if (animationLength > 0 && animationTimer != null)
        {
            Debug.Log("Metoda Menu Update77");
            elapsedTime += Time.deltaTime;
            //animationSlider.value = elapsedTime;
            RadialAnimationProgressBar.GetComponent<ProgressBar>().currentAmount = elapsedTime;
            Debug.Log("Metoda Menu Update78");
            if ((int)animationTimer.Elapsed.TotalSeconds >= animationLength)
            {
                Debug.Log("Metoda Menu Update79");
                //if (isDemo)
                //{
                lockScene = true;
                animationTimer = null;
                //}
                //else
                //{
                //    elapsedTime = 0f;
                //    animationTimer.Restart();
                //}
            }
            Debug.Log("Metoda Menu Update80");
        }

        Debug.Log("Metoda Menu Update81");
        UpdateCompass();
        Debug.Log("Metoda Menu Update81");
    }

    public void UpdateCompass()
    {
        Debug.Log("Metoda Menu UpdateCompass1");
    
    var center = new Vector3(centroid.x, 0, centroid.y);
        var targetPosLocal = Camera.main.transform.InverseTransformPoint(center);
        var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
        RotationArrow.transform.eulerAngles = new Vector3(0, 0, targetAngle);

        //debugController.Push("Compass angle", targetAngle);

        var cameraPosition = Camera.main.transform.position;
        var distance = Vector3.Distance(cameraPosition, center);
        //if (distance > 5000)
        //{
        //    DistanceToCenter.text = @"¯\_(ツ)_/¯";
        //    DistanceToCenter.fontSize = 45;

        //}
        //else
        //{
        DistanceToCenter.text = distance.ToString("0.0") + "m";
        DistanceToCenter.fontSize = 35;
        //}
        Debug.Log("Metoda Menu UpdateCompass2");
    }

    public void GetCentroid()
    {
        Debug.Log("Metoda Menu GetCentroid1");
        //var allModels = ModelsQueue.SceneQueue[ModelsQueue.ActiveSceneId];
        //allModels.AddRange(ModelsQueue.Rendered[ModelsQueue.ActiveSceneId]);
        //var realPoints = allModels.SelectMany(x => x.path).ToList();
        List<Vector3> localPoints = new List<Vector3>();

        var location = _arLocationProvider.CurrentLocation.ToLocation();
        var allModels = FindObjectsOfType<ObjectMovingController>();

        Debug.Log("Metoda Menu GetCentroid2");
        if (allModels.Count() == 0) return;

        Debug.Log("Metoda Menu GetCentroid3");
        foreach (var model in allModels)
        {

            Debug.Log("Metoda Menu GetCentroid4");
            var moveComponent = model.gameObject.GetComponentInChildren<ObjectMovingController>();
            if (moveComponent != null)
            {
                foreach (var point in moveComponent.localPoints)
                {
                    localPoints.Add(point);
                }
            }

            Debug.Log("Metoda Menu GetCentroid5");
        }
        //foreach (var point in realPoints)
        //{
        //    var localPoint = Location.GetGameObjectPositionForLocation(locationRoot.transform,
        //        Camera.main.transform.position, location, point, false);
        //    Vector3 flatPoint = new Vector3(localPoint.x, 0, localPoint.z);
        //    localPoints.Add(flatPoint);
        //    //debugController.Push("Flat point:", flatPoint);
        //    Console.WriteLine($"AAAAA Flat point: {flatPoint}");
        //}
        //Console.WriteLine($"AAAAA Pos to calc centroid: {location}");

        //if (ModelsQueue.IsPlaygroundScene || ModelsQueue.IsTableScene)
        //{
        //var newPoints = new List<Vector3>();
        //foreach (var p in localPoints)
        //{
        //    var newP = LocationUtils.CalculateTablePoint(p);
        //    newPoints.Add(newP);
        //}
        //localPoints = newPoints;
        if (localPoints.Count == 1)
        {
            Debug.Log("Metoda Menu GetCentroid6");
            centroid = new Vector2(-localPoints[0].x, -localPoints[0].z);
            return;
        }
        Debug.Log("Metoda Menu GetCentroid7");
        //}
        float totalX = 0, totalY = 0;
        foreach (Vector3 p in localPoints)
        {
            totalX += p.x;
            totalY += p.z;
        }
        Debug.Log("Metoda Menu GetCentroid9");
        float centerX = totalX / localPoints.Count;
        float centerY = totalY / localPoints.Count;

        centroid = new Vector2(centerX, centerY);

        //debugController.Push("Centroid:", centroid);
        Debug.Log("Metoda Menu GetCentroid10");
    }
    bool tableRotated = false;
    private void UpdateLocations()
    {
        Debug.Log("Metoda Menu UpdateLocations1");
        var currLocation = _arLocationProvider.CurrentLocation.ToLocation();
        if (animationTimer == null)
        {
            Debug.Log("Metoda Menu UpdateLocations2");
            if (ModelsQueue.GetSceneAnimationLength() > 0 && ModelsQueue.SceneStarted)
            {
                animationTimer = new Stopwatch();
                animationTimer.Start();
            }
            Debug.Log("Metoda Menu UpdateLocations3");
        }
        if (ModelsQueue.IsTableScene && !tableRotated && centroid != Vector2.zero)
        {
            Debug.Log("Metoda Menu UpdateLocations4");
            TableFloor.transform.RotateAround(centroid, Vector3.up, ModelsQueue.AzimuthCorrection);
            tableRotated = true;
            Debug.Log("Metoda Menu UpdateLocations5");
        }
        Debug.Log("Metoda Menu UpdateLocations6");
        for (int i = 0; i < locationRoot.transform.childCount; i++)
        {
            Debug.Log("Metoda Menu UpdateLocations7");
            var model = locationRoot.transform.GetChild(i).gameObject;
            if (!this.models.Contains(model))
            {
                Debug.Log("Metoda Menu UpdateLocations9");
                this.models.Add(model);
                var moveComponent = model.gameObject.GetComponentInChildren<ObjectMovingController>();
                if (moveComponent != null)
                {
                    List<Vector3> newPoints = new List<Vector3>();
                    foreach (var point in moveComponent.localPoints)
                    {
                        Vector3 p;
                        p = RotatePointAroundPivot(point, centroid, new Vector3(0, ModelsQueue.AzimuthCorrection, 0));
                        p.y += ModelsQueue.IsTableScene ? ModelsQueue.HeightCorrection * 0.06f : ModelsQueue.HeightCorrection;
                        newPoints.Add(p);
                    }
                    moveComponent.localPoints = newPoints;
                    if (moveComponent.Spline == null && moveComponent.localPoints.Count() == 1 || PointsTheSame(moveComponent.localPoints))
                    {
                        moveComponent.gameObject.transform.position = newPoints[0];
                    }
                    else
                    {
                        var pos = moveComponent.gameObject.transform.position;
                        moveComponent.gameObject.transform.position = pos;
                        moveComponent.Spline = Misc.BuildSpline(SplineType.CatmullromSpline, newPoints.ToArray(), 500, 0.5f);
                    }
                }
                Debug.Log("Metoda Menu UpdateLocations10");
            }
        }
    }
    string url;
    private void TriggerDescription()
    {
        Debug.Log("Metoda Menu TriggerDescription1");
        if (Input.touchCount == 2)
        {
            return;
        }
        float height = Screen.height;
        if ((height - Input.mousePosition.y) / height < 0.1f || lockScene)
        {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.gameObject != null)
            {
                var description = hit.transform.gameObject.GetComponentInChildren<DescriptionComponent>();
                if (description == null || string.IsNullOrEmpty(description.Description))
                {
                    return;
                }
                if (animationComponent != null && !descriptionIn)
                {
                    var descriptionComp = DescriptionBgObject.gameObject.GetComponentsInChildren<Text>()
                        .Where(x => x.name == "ModelDescription").FirstOrDefault();
                    var text = description.Description;
                    int pFrom = text.IndexOf("#") + 1;
                    int pTo = text.LastIndexOf("#");
                    if (pTo > 0)
                    {
                        url = text.Substring(pFrom, pTo - pFrom);
                        text = Regex.Replace(text, "#.*?#", string.Empty);
                        MoreInfoBtn.gameObject.SetActive(true);
                    }
                    else
                    {
                        MoreInfoBtn.gameObject.SetActive(false);
                    }
                    descriptionComp.text = text;
                    //animationComponent.gameObject.SetActive(true);
                    animationComponent.PlayInAnimation();
                    descriptionIn = true;
                    PauseButtonClick();
                }
            }
        }
        else if (descriptionIn)
        {
            if (animationComponent != null && descriptionIn)
            {
                animationComponent.PlayOutAnimation();
                //animationComponent.gameObject.SetActive(false);
                descriptionIn = false;
                PlayButtonClick();
            }
        }
    }

    public void openUrl()
    {
        Application.OpenURL(url);
    }

    public void BtnGoToStoreClick()
    {
        new MobileAppShoppingEventLogService(placeId, Input.location.lastData.ToLocation()).Add();
        Application.OpenURL(TemporaryDatabase.WebPortalURL);
    }
    public void BtnGoToManual()
    {
        Debug.Log("Metoda Menu BtnGoToManual1");
        new MobileAppShoppingEventLogService(placeId, Input.location.lastData.ToLocation()).Add();
        Application.OpenURL(TemporaryDatabase.FaqURL);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log("Metoda Menu OnApplicationPause1");
        if (pauseStatus)
        {
            new MobileAppSceneUsingEventLogService(TemporaryDatabase.ActiveSceneStartDate.Value,
                    TemporaryDatabase.ActiveSceneUsingMode.Value, TemporaryDatabase.ActiveSceneId.Value, TemporaryDatabase.ActiveScenePlaceId.Value,
                    TemporaryDatabase.ActiveSceneOrderProductLicenseId, _arLocationProvider.CurrentLocation.ToLocation()).Add();
            new MobileAppUsingEventLogService(TemporaryDatabase.AppStartDate.Value, _arLocationProvider.CurrentLocation.ToLocation()).Add();
        }
        else
        {
            TemporaryDatabase.ActiveSceneStartDate = DateTime.Now;
            TemporaryDatabase.AppStartDate = DateTime.Now;
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Metoda Menu OnApplicationQuit1");
        new MobileAppSceneUsingEventLogService(TemporaryDatabase.ActiveSceneStartDate.Value,
                    TemporaryDatabase.ActiveSceneUsingMode.Value, TemporaryDatabase.ActiveSceneId.Value, TemporaryDatabase.ActiveScenePlaceId.Value,
                    TemporaryDatabase.ActiveSceneOrderProductLicenseId, _arLocationProvider.CurrentLocation.ToLocation()).Add();
        new MobileAppUsingEventLogService(TemporaryDatabase.AppStartDate.Value, _arLocationProvider.CurrentLocation.ToLocation()).Add();
    }

    public bool Teleport = false;
    public bool ChangePosition = false;

    public void TpToModel()
    {
               Debug.Log("Metoda Menu TpToModel1");
        Camera.main.transform.position = models[0].transform.position;
        Teleport = false;
    }
    public void ChangeUserGPSPosition()
    {
        Debug.Log("Metoda Menu ChangeUserGPSPosition1");
        ARLocationProvider.Instance.CurrentLocation.ChangePosition();
        ChangePosition = false;
    }
}
