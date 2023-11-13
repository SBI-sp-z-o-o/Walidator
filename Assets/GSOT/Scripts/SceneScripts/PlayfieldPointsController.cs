using ARLocation;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using Assets.GSOT.Scripts.SceneScripts;
using Assets.GSOT.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayfieldPointsController : MonoBehaviour
{
    public Texture2D marker;
    public Text text;
    public GameObject MenuCanvas;
    public GameObject TopMenu;
    public Button OpenButton;
    public List<ApplicationScene> Scenes = new List<ApplicationScene>();
    private OnlineMapsLocationService locationService;
    public Toggle Toggle;
    public GameObject locationRoot;
    private static Dictionary<string, bool> Points;
    private List<OnlineMapsMarkerBase> markers = new List<OnlineMapsMarkerBase>();
    public ARLocationProvider _ARLocationProvider;
    public GameObject TipText;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartLocation());
        TopMenu.GetComponent<RectTransform>().sizeDelta = MenuCanvas.GetComponent<RectTransform>().sizeDelta;
        OpenButton.interactable = false;
        TipText.gameObject.SetActive(true);
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;
        OpenButton.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("Open");
        locationService = FindObjectOfType<OnlineMapsLocationService>();

        var place = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
        OnlineMaps map = OnlineMaps.instance;
        Points = new Dictionary<string, bool>()
        {
            {"A", false },
            {"B", false }
        };
        markers = new List<OnlineMapsMarkerBase>();
        //TryLoadMarkers();
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

    private void OnMapClick()
    {
        if (stopNextClick)
        {
            stopNextClick = false;
            return;
        }
        if (OnlineMapsMarkerManager.CountItems > 1)
        {
            return;
        }
        // Get the coordinates under the cursor.
        double lng, lat;
        OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

        var p = Points.Where(x => !x.Value).FirstOrDefault();
        // Create a label for the marker.
        string label = p.Key;
        Points[p.Key] = true;
        // Create a new marker.
        var marker = OnlineMapsMarkerManager.CreateItem(lng, lat, label);
        marker.OnClick += OnMarkerClick;
        markers.Add(marker);
        if (OnlineMapsMarkerManager.CountItems > 1)
        {
            OpenButton.interactable = true;
            TipText.gameObject.SetActive(false);
        }
        else
        {
            TipText.gameObject.SetActive(true);
        }
    }
    bool stopNextClick = false;
    private void OnMarkerClick(OnlineMapsMarkerBase marker)
    {
        var p = Points.Where(x => x.Key == marker.label).FirstOrDefault();
        Points[p.Key] = false;
        //marker.enabled = false;
        //OnlineMapsMarkerManager.DestroyImmediate(marker);
        markers.Remove(marker);
        OnlineMapsMarkerManager.RemoveItem(marker as OnlineMapsMarker);
        OpenButton.interactable = false;
        TipText.gameObject.SetActive(true);
    }

    public void ResetMarkers()
    {
        Points["A"] = false;
        Points["B"] = false;
        if (markers.Count > 0)
            OnlineMapsMarkerManager.RemoveItem(markers[0] as OnlineMapsMarker);
        if (markers.Count > 1)
            OnlineMapsMarkerManager.RemoveItem(markers[1] as OnlineMapsMarker);
        OpenButton.interactable = false;
        TipText.gameObject.SetActive(true);
        markers = new List<OnlineMapsMarkerBase>();
        stopNextClick = true;
    }

    bool switchScene = false;
    //ta metoda otrzymuje juz wsp. terenowe 2 punktów boiskowych użytkownika
    public void SelectScene()
    {
        //OnlineMapsMarkerManager.RemoveAllItems();
        switchScene = true;
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("ScenesScene");
            }
        }

        if (ARLocationProvider.Instance.CurrentLocation.ToLocation().Latitude == 0 || ARLocationProvider.Instance.CurrentLocation.ToLocation().Longitude == 0)
        {
            return;
        }
        else if (switchScene)
        {
            switchScene = false;
            //SaveMarkers();
            var place = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
            Vector3 api1 = new Vector3(locationService.position.y, 0, locationService.position.x);// new Vector3(markers[0].position.y, 0, markers[0].position.x);
            Vector3 api2 = new Vector3(markers[0].position.y, 0, markers[0].position.x);
            LocationUtils.CalculateRelativePoints(locationRoot, api1, api2, _ARLocationProvider);
            ModelsQueue.TableScale = null;
            ModelsQueue.IsTableScene = false;
            ModelsQueue.IsPlaygroundScene = true;

            var scene = place.Scenes.Where(x => x.Id == ModelsQueue.ActiveSceneId).FirstOrDefault();
            if(scene.PlaygroundSceneObjectTimelineScale != 0)
            {
                ModelsQueue.PlaygroundSceneObjectTimelineScale = scene.PlaygroundSceneObjectTimelineScale;
            }
            if(scene.PlaygroundObjectScale != 0)
            {
                ModelsQueue.PlaygroundSceneObjectScale = scene.PlaygroundObjectScale;
            }

            //ModelsQueue.PushDebugMessage("Api P1", api1.ToString("0.00000"));
            //ModelsQueue.PushDebugMessage("Api P2", api2.ToString("0.00000"));



            TemporaryDatabase.ActiveSceneStartDate = DateTime.Now;
            TemporaryDatabase.ActiveSceneId = ModelsQueue.ActiveSceneId;
            TemporaryDatabase.ActiveScenePlaceId = place.Id;
            TemporaryDatabase.ActiveSceneUsingMode = Assets.GSOT.Scripts.Enums.ApiEnums.SceneUsingMode.Playground;
            TemporaryDatabase.ActiveSceneOrderProductLicenseId = place.GSOrderProductLicenseId;
            SceneManager.LoadScene("ModelScene");
        }
    }

    #region legacy
    //public void SaveMarkers()
    //{
    //    if (markers.Count < 2)
    //    {
    //        return;
    //    }
    //    // Create XMLDocument and first child
    //    OnlineMapsXML xml = new OnlineMapsXML("Markers");

    //    // Save markers data
    //    foreach (OnlineMapsMarker marker in OnlineMapsMarkerManager.instance)
    //    {
    //        if (string.IsNullOrEmpty(marker.label))
    //        {
    //            continue;
    //        }
    //        // Create marker node
    //        OnlineMapsXML markerNode = xml.Create("Marker");
    //        markerNode.Create("Position", marker.position);
    //        markerNode.Create("Label", marker.label);
    //    }

    //    // Save xml string
    //    PlayerPrefs.SetString(prefsKey + ModelsQueue.ActiveSceneId.ToString(), xml.outerXml);
    //    PlayerPrefs.Save();
    //}
    //private void TryLoadMarkers()
    //{
    //    // If the key does not exist, returns.
    //    if (!PlayerPrefs.HasKey(prefsKey + ModelsQueue.ActiveSceneId.ToString())) return;

    //    // Load xml string from PlayerPrefs
    //    string xmlData = PlayerPrefs.GetString(prefsKey + ModelsQueue.ActiveSceneId.ToString());

    //    // Load xml document
    //    OnlineMapsXML xml = OnlineMapsXML.Load(xmlData);

    //    // Load markers
    //    foreach (OnlineMapsXML node in xml)
    //    {
    //        // Gets coordinates and label
    //        Vector2 position = node.Get<Vector2>("Position");
    //        string label = node.Get<string>("Label");

    //        // Create marker
    //        var marker = OnlineMapsMarkerManager.CreateItem(position, label);
    //        markers.Add(marker);
    //        Points["A"] = true;
    //        Points["B"] = true;
    //    }
    //    if (markers.Count == 2)
    //    {
    //        OpenButton.interactable = true;
    //        TipText.gameObject.SetActive(false);
    //    }
    //    else
    //    {
    //        TipText.gameObject.SetActive(true);
    //    }
    //}

    #endregion
    private float Dist(Vector2 userCoordinares, Vector2 markerCoordinates)
    {
        return OnlineMapsUtils.DistanceBetweenPoints(userCoordinares, markerCoordinates).magnitude * 1000f; //from km to m
    }
}
