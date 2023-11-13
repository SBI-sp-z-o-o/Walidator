using ARLocation;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GuideSceneController : MonoBehaviour
{
    private OnlineMaps OM;
    private OnlineMapsMarkerManager markerManager;
    public Texture2D marker;
    public Texture2D soundMarker;
    private int samples = 128;
    public Image image;
    public Text text;
    public GameObject Panel;
    public GameObject MenuCanvas;
    public GameObject TopMenu;
    public Button OpenButton;
    public AudioSource AudioSource;
    public AudioSource ThemeSource;
    public List<ApplicationScene> Scenes = new List<ApplicationScene>();
    private OnlineMapsLocationService locationService;
    public Toggle Toggle;
    public Button CloseButton;
    public Image DirectionArrow;
    public Text DistanceText;
    public List<Tuple<Vector2, ApplicationScene>> Centers = new List<Tuple<Vector2, ApplicationScene>>();
    public Vector2 ClosestCenter;
    public Button SoundOnBtn;
    public Button SoundOffBtn;
    public Button PauseThemeButton;
    public Button PlayThemeButton;
    public Button ReplayThemeButton;

    private DebugController debugController;
    public AudioClip sceneClipTest;

    public GameObject ExitSceneInfo;
    bool ExitScene = false;
    /// <summary>
    /// The thickness of the line.
    /// </summary>
    public float size = 50;

    /// <summary>
    /// Scale UV.
    /// </summary>
    public Vector2 uvScale = new Vector2(2, 1);

    /// <summary>
    /// The material used for line drawing.
    /// </summary>
    public Material material;

    private Vector2[] coords;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private float _size;



    void Start()
    {
        //InitLine();
        TopMenu.GetComponent<RectTransform>().sizeDelta = MenuCanvas.GetComponent<RectTransform>().sizeDelta;
        markerManager = FindObjectOfType<OnlineMapsMarkerManager>();
        OM = FindObjectOfType<OnlineMaps>();
        Panel.gameObject.SetActive(false);
        OpenButton.gameObject.GetComponentInChildren<Text>().text = "Otwórz";// Translator.Instance().GetString("Open");
        locationService = FindObjectOfType<OnlineMapsLocationService>();
        debugController = FindObjectOfType<DebugController>();
        ExitSceneInfo.gameObject.SetActive(false);

        var imgConverter = FindObjectOfType<IMG2Sprite>();
        var cl = imgConverter.LoadNewSprite(ModelsQueue.ButtonCloseFilePath);
        if (cl != null)
        {
            CloseButton.image.sprite = cl;
            CloseButton.transition = Selectable.Transition.None;
        }
        SoundControlActive(false);


        var place = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
        OnlineMaps map = OnlineMaps.instance;
        foreach (var scene in place.Scenes)
        {
            var center = SceneCenter(scene.Id);
            if (center == null)
            {
                continue;
            }
            if (scene.GroupType != Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide)
            {

            }
            Centers.Add(new Tuple<Vector2, ApplicationScene>(center.Value, scene));
            Scenes.Add(scene);
            Texture2D markerToPlace;
            if (scene.GroupType == Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide)
            {
                markerToPlace = soundMarker;
            }
            else
            {
                markerToPlace = marker;
            }
            var m = markerManager.Create(center.Value, markerToPlace, scene.Name);
            m["data"] = scene.Id;
            m.scale = 1;
            m.isDraggable = false;
            m.OnClick += OnMarkerClick;
            DrawCircle(center.Value.y, center.Value.x, scene.RLengthInMeters / 1000, scene.Name);
        }
        ClosestCenter = GetClosestCenter();
        SoundOffClick();
        DrawLine();
        //OnlineMaps.instance.OnChangePosition += DrawLine;
        //OnlineMaps.instance.OnChangeZoom += DrawLine;
    }
    OnlineMapsDrawingLine lineDrawing;
    void DrawLine()
    {
        if (locationService.position == Vector2.zero) return;
        List<Vector2> line = new List<Vector2>
        {
            locationService.position,
            GetClosestCenter()
        };
        var ile = markerManager.items.Count;
        if (ile > 0)
        {
            var dist = (int)(OnlineMapsUtils.DistanceBetweenPoints(line[0], line[1]).magnitude * 1000f); //from km to m
            markerManager.items[ile - 1].label = "Dystans do sceny: " + dist.ToString() + "m";
        }
        if (lineDrawing != null)
        {
            OnlineMapsDrawingElementManager.RemoveItem(lineDrawing);
        }
        lineDrawing = new OnlineMapsDrawingLine(line, Color.green, 5);
        OnlineMapsDrawingElementManager.AddItem(lineDrawing);
    }

    private Vector3 GetClosestCenter()
    {
        var closest = Centers.OrderBy(x => OnlineMapsUtils.DistanceBetweenPoints(locationService.position, x.Item1).magnitude * 1000f).FirstOrDefault();
        return closest.Item1;
    }

    public Tuple<Vector2, ApplicationScene> GetClosestScene()
    {
        var closest = Centers.OrderBy(x => OnlineMapsUtils.DistanceBetweenPoints(locationService.position, x.Item1).magnitude * 1000f).FirstOrDefault();
        return closest;
    }

    //public void UpdateCompass(Vector3 center)
    //{
    //    var targetPosLocal = Camera.main.transform.InverseTransformPoint(center);
    //    var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
    //    DirectionArrow.transform.eulerAngles = new Vector3(0, 0, targetAngle);
    //    var distance = Vector3.Distance(locationService.position, center);
    //    DistanceText.text = distance.ToString("0.0") + "m";
    //}

    long sceneId;
    private void OnMarkerClick(OnlineMapsMarkerBase marker)
    {
        RectTransform rt = Panel.GetComponent<RectTransform>();
        //rt = new RectTransform
        rt.sizeDelta = new Vector2(Screen.width / 1.5f, Screen.height / 2f);
        sceneId = (long)marker["data"];
        var place = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
        var scene = place.Scenes.Where(x => x.Id == sceneId).FirstOrDefault();
        if (scene.GroupType == Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide)
        {
            return;
        }
        var imgConverter = FindObjectOfType<IMG2Sprite>();
        var df = imgConverter.LoadNewSprite(scene.ButtonImage);
        image.sprite = df;
        text.text = scene.Description;
        Panel.gameObject.SetActive(true);
    }

    public void CloseModal()
    {
        Panel.gameObject.SetActive(false);
    }

    public void SelectScene()
    {
        var place = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
        var scene = place.Scenes.Where(x => x.Id == sceneId).FirstOrDefault();
        ModelsQueue.ActiveScene = scene.Name;
        var activePlace = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
        ModelsQueue.ActiveSceneId = sceneId;
        ModelsQueue.Restart(ModelsQueue.ActiveSceneId);
        ModelsQueue.BackToScenesType = Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide;
        SceneManager.LoadScene("ModelsLoadingScene");
    }


    public void SoundOnClick()
    {
        SoundOnBtn.gameObject.SetActive(false);
        SoundOffBtn.gameObject.SetActive(true);
        Toggle.isOn = false;
    }

    public void SoundOffClick()
    {
        SoundOffBtn.gameObject.SetActive(false);
        SoundOnBtn.gameObject.SetActive(true);
        Toggle.isOn = true;
    }

    // Update is called once per frame
    float delta = 10;
    void Update()
    {
        //UpdateCompass(ClosestCenter);
        DrawLine();
        debugController.Push("LoadState", ThemeSource.clip?.loadState);
        if (ThemeSource.clip?.loadState == AudioDataLoadState.Unloaded)
            ThemeSource.clip.LoadAudioData();
        debugController.Push("IsPlaying", ThemeSource.isPlaying);
        debugController.Push("Enabled", ThemeSource.enabled);
        debugController.Push("Name", ThemeSource.clip?.name);
        //if (Math.Abs(size - _size) > float.Epsilon) DrawLine();

        //if (themeStarted && !ThemeSource.isPlaying)
        //{
        //    ThemeSource.clip = null;
        //    themeStarted = false;
        //}

        if (Toggle.isOn)
        {
            if (InRadius(Scenes))
            {
                if (delta >= 10)
                {
                    //AudioSource.Play();
                    delta = 0;
                }
                else
                {
                    delta += Time.deltaTime;
                }
            }
        }
        else
        {
            //AudioSource.Pause();
        }
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitSceneInfo.gameObject.SetActive(true);
                ExitScene = true;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ModelsQueue.BackToScenesType = Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide;
                SceneManager.LoadScene("ScenesScene");
            }
        }
    }
    bool themeStarted = false;
    long? musicSceneId = null;
    private bool InRadius(List<ApplicationScene> Scenes)
    {
        bool inRadius = false;
        foreach (var scene in Scenes)
        {
            var c = Centers.Where(x => x.Item2.Id == scene.Id).FirstOrDefault();
            if (scene.RLengthInMeters >= GetDistance(c.Item1.y, c.Item1.x, locationService.position.y, locationService.position.x))
            {
                if (string.IsNullOrEmpty(scene.ThemeMusic)) continue;
                inRadius = true;
                musicSceneId = scene.Id;
                if ((ThemeSource.clip == null && !themeStarted) || (ThemeSource.clip?.name != scene.ThemeMusic && !ThemeSource.isPlaying))
                {
                    StartCoroutine(LoadAudio(scene.ThemeMusic));
                    //debugController.Push("Load music", scene.ThemeMusic);
                    themeStarted = true;
                    SoundControlActive(true);
                }
                if (!ThemeSource.isPlaying && ThemeSource.clip != null && !manuallyPaused)
                {
                    ThemeSource.Play();
                    //debugController.Push("Play music", scene.ThemeMusic);
                }
            }
        }
        //if (!inRadius)
        //{
        //    themeStarted = false;
        //    ThemeSource.Stop();
        //    musicSceneId = null;
        //}

        return inRadius;
    }


    //var closest = GetClosestScene();
    //if (Dist(locationService.position, closest.Item1) <= closest.Item2.RLengthInMeters && !string.IsNullOrEmpty(closest.Item2.ThemeMusic))
    //{
    //    if (ThemeSource.clip == null && !themeStarted)
    //    {
    //        StartCoroutine(LoadAudio(closest.Item2.ThemeMusic));
    //        themeStarted = true;
    //    }
    //    if (!ThemeSource.isPlaying && ThemeSource.clip != null)
    //    {
    //        ThemeSource.Play();
    //    }
    //}
    //else
    //{
    //    themeStarted = false;
    //    ThemeSource.Stop();
    //    var center = SceneCenter(scene.Id);
    //    if (center == null)
    //    {
    //        continue;
    //    }
    //    var dist = Dist(locationService.position, center.Value);
    //    if (dist <= scene.RLengthInMeters)
    //    {
    //        return true;
    //    }
    //}
    double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371; // Radius of the earth in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c; // Distance in km
        return d * 1000;
    }

    double ToRadians(double deg)
    {
        return deg * (Math.PI / 180);
    }

    private float Dist(Vector2 userCoordinares, Vector2 markerCoordinates)
    {
        return OnlineMapsUtils.DistanceBetweenPoints(userCoordinares, markerCoordinates).magnitude * 1000f; //from km to m
    }

    private Vector2? SceneCenter(long sceneId)
    {
        List<GSOT.DataEntry> allModels = new List<GSOT.DataEntry>();
        if (ModelsQueue.SceneQueue.ContainsKey(sceneId))
        {
            allModels = ModelsQueue.SceneQueue[sceneId];
        }
        if (ModelsQueue.Rendered.ContainsKey(sceneId))
        {
            allModels.AddRange(ModelsQueue.Rendered[sceneId]);
        }
        if (!allModels.Any())
        {
            return null;
        }
        //var allModels = ModelsQueue.SceneQueue[sceneId];
        var realPoints = allModels.Select(x => x.path[0]).ToList();


        float totalX = 0, totalY = 0;
        foreach (var p in realPoints)
        {
            totalX += (float)p.Longitude;
            totalY += (float)p.Latitude;
        }
        float centerX = totalX / realPoints.Count;
        float centerY = totalY / realPoints.Count;

        return new Vector2(centerX, centerY);
    }

    private void DrawCircle(double lat, double lng, float radiusKM, string name)
    {
        //double lng, lat;
        //OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

        // Create a new marker under cursor
        //OnlineMapsMarkerManager.CreateItem(lng, lat, "Marker " + OnlineMapsMarkerManager.CountItems);

        OnlineMaps map = OnlineMaps.instance;

        // Get the coordinate at the desired distance
        double nlng, nlat;
        OnlineMapsUtils.GetCoordinateInDistance(lng, lat, radiusKM, 90, out nlng, out nlat);

        double tx1, ty1, tx2, ty2;

        // Convert the coordinate under cursor to tile position
        map.projection.CoordinatesToTile(lng, lat, map.zoom, out tx1, out ty1);

        // Convert remote coordinate to tile position
        map.projection.CoordinatesToTile(nlng, nlat, map.zoom, out tx2, out ty2);

        // Calculate radius in tiles
        double r = tx2 - tx1;

        // Create a new array for points
        OnlineMapsVector2d[] points = new OnlineMapsVector2d[samples];

        // Calculate a step
        double step = 360d / samples;

        // Calculate each point of circle
        for (int i = 0; i < samples; i++)
        {
            double px = tx1 + Math.Cos(step * i * OnlineMapsUtils.Deg2Rad) * r;
            double py = ty1 + Math.Sin(step * i * OnlineMapsUtils.Deg2Rad) * r;
            map.projection.TileToCoordinates(px, py, map.zoom, out lng, out lat);
            points[i] = new OnlineMapsVector2d(lng, lat);
        }

        // Create a new polygon to draw a circle
        OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingPoly(points, Color.red, 3, name));
    }


#pragma warning disable CS0618 // Type or member is obsolete
    private IEnumerator LoadAudio(string path)
    {
        WWW request = GetAudioFromFile(path);
        yield return request;
        //var audioClip = sceneClipTest;// request.GetAudioClip();
        //debugController.Push("Audio file loaded", path);
        var audioClip = request.GetAudioClip();
        audioClip.name = path;
        ThemeSource.clip = audioClip;
        ThemeSource.loop = false;
        //PlayAudioFile(audioClip, path);
    }

    private WWW GetAudioFromFile(string path)
    {
        string audioToLoad = string.Format(path);
        WWW request = new WWW("file://" + audioToLoad);
        return request;
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public void centerMapToGPSLocation()
    {

        OM.SetPosition(locationService.position.x, locationService.position.y);
    }


    bool manuallyPaused = false;
    public void SoundStop()
    {
        ThemeSource.Pause();
        PauseThemeButton.gameObject.SetActive(false);
        PlayThemeButton.gameObject.SetActive(true);
        manuallyPaused = true;
    }

    public void SoundPlay()
    {
        manuallyPaused = false;
        PauseThemeButton.gameObject.SetActive(true);
        PlayThemeButton.gameObject.SetActive(false);
        ThemeSource.Play();
    }

    public void SoundReplay()
    {
        manuallyPaused = false;
        ThemeSource.Stop();
        ThemeSource.Play();
    }

    private void SoundControlActive(bool active)
    {
        PauseThemeButton.gameObject.SetActive(active);
        PlayThemeButton.gameObject.SetActive(false);
        ReplayThemeButton.gameObject.SetActive(active);
    }
}
