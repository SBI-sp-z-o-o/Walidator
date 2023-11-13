using ARLocation;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using Assets.GSOT.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnTap : MonoBehaviour
{
    private ARRaycastManager _arRaycastManager;
    private Vector2 touchPosition;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private LineRenderer line;
    private List<GameObject> placedObjects = new List<GameObject>();
    public Button StartSceneButton;
    public ARLocationProvider _arLocationProvider;
    public GameObject locationRoot;
    public Button StartTableSceneButton;
    public Text DebugText;
    private ApplicationScene Scene;
    public Vector3 PointA;
    public Vector3 PointB;
    public Vector3 PointC;
    public Vector3 PointD;
    public float tableBC = 0;
    public Text TipText;
    public GameObject TipTextBg;
    public bool AddFakePoint;
    public GameObject miecz;

    void Start()
    {
        Debug.Log("Metoda PlaceOnTap Start1");
        if (!ModelsQueue.IsTableScene)
        {
            Debug.Log("Metoda PlaceOnTap Start2");
            ModelsQueue.TableScale = null;
            TipTextBg.gameObject.SetActive(false);
            Debug.Log("Metoda PlaceOnTap Start3");
            DestroyAR();
            Debug.Log("Metoda PlaceOnTap Start4");
            DestroyImmediate(this);
            Debug.Log("Metoda PlaceOnTap Start5");
        }
        else
        {
            Debug.Log("Metoda PlaceOnTap Start6");
            TipText.text = Translator.Instance().GetString("PlaceTableOrientationPoints");
            Debug.Log("Metoda PlaceOnTap Start7");
            Scene = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault()?
                .Scenes.Where(x => x.Id == ModelsQueue.ActiveSceneId).FirstOrDefault();
            Debug.Log("Metoda PlaceOnTap Start8");
            if (Scene.Scale.HasValue && Scene.Scale.Value > 0)
            {
                Debug.Log("Metoda PlaceOnTap Start9");
                ModelsQueue.TableScale = Scene.Scale;
            }
            Debug.Log("Metoda PlaceOnTap Start10");
        }
        Debug.Log("Metoda PlaceOnTap Start11");
        if (Application.isEditor)
        {
            Debug.Log("Metoda PlaceOnTap Start12");
            AddFakePoint = true;
        }
        Debug.Log("Metoda PlaceOnTap Start3");
    }

    private void CalculateAdditionalPoints()
    {
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints1");
        var allModels = ModelsQueue.SceneQueue[ModelsQueue.ActiveSceneId];
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints2");
        allModels.AddRange(ModelsQueue.Rendered[ModelsQueue.ActiveSceneId]);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints3");
        var rlPoints = allModels.SelectMany(x => x.path).Where(x => x.Latitude != 0 && x.Longitude != 0).ToList();
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints4");

        ModelsQueue.TableTransformPosition = allModels.First().path[0];
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints5");
        ModelsQueue.TableTransformPosition = new Location(rlPoints.Min(x => x.Latitude), rlPoints.Min(x => x.Longitude),0);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints6");
        Vector3 temp;
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints7");
        temp = Location.GetGameObjectPositionForLocation(locationRoot.transform, Camera.main.transform.position, 
            ModelsQueue.TableTransformPosition, new ObjectLocation(new Location(rlPoints.Min(x => x.Latitude), rlPoints.Min(x => x.Longitude))), false);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints8");
        Vector3 pA_3d = new Vector3(temp.x, temp.y, temp.z);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints9");
        temp = Location.GetGameObjectPositionForLocation(locationRoot.transform, Camera.main.transform.position, 
            ModelsQueue.TableTransformPosition, new ObjectLocation(new Location(rlPoints.Max(x => x.Latitude), rlPoints.Min(x => x.Longitude))), false);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints10");
        Vector3 pB_3d = new Vector3(temp.x, temp.y, temp.z);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints11");
        temp = Location.GetGameObjectPositionForLocation(locationRoot.transform, Camera.main.transform.position, 
            ModelsQueue.TableTransformPosition, new ObjectLocation(new Location(rlPoints.Max(x => x.Latitude), rlPoints.Max(x => x.Longitude))), false);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints12");
        Vector3 pC_3d = new Vector3(temp.x, temp.y, temp.z);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints13");
        ModelsQueue.ApiPoint1 = pA_3d;
        ModelsQueue.ApiPoint2 = pB_3d;
        var apiAB = Vector3.Distance(pA_3d, pB_3d);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints14");
        var apiBC = Vector3.Distance(pB_3d, pC_3d);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints15");
        PointA = placedObjects[0].transform.position;
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints16");
        if (Scene.FirstSideLength.HasValue && Scene.FirstSideLength.Value > 0)
        {
            Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints17");
            PointB = placedObjects[0].transform.position + (placedObjects[1].transform.position - placedObjects[0].transform.position).normalized * Scene.FirstSideLength.Value;
            Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints18");
            placedObjects[1].transform.position = PointB;
            Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints19");
        }
        else
        {
            Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints20");
            PointB = placedObjects[1].transform.position;
            Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints21");
        }
        var tableAB = Vector3.Distance(placedObjects[0].transform.position, placedObjects[1].transform.position);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints22");
        if (Scene.SecondSideLength.HasValue && Scene.SecondSideLength.Value > 0)
        {
            Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints23");
            tableBC = Scene.SecondSideLength.Value;
        }
        else
        {
            Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints24");
            tableBC = apiBC * (tableAB / apiAB);
        }
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints25");
        PointC = LocationUtils.GetRightPoint(PointA, PointB, tableBC, true);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints26");
        PointD = LocationUtils.GetRightPoint(PointB, PointA, tableBC, false);
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints27");
        PointD.y = PointA.y;
        PointC.y = PointD.y;
        PointB.y = PointC.y;
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints28");
        var debugController = FindObjectOfType<DebugController>();
        Debug.Log("Metoda PlaceOnTap CalculateAdditionalPoints29");
        //debugController.Push("AB Dist", Vector3.Distance(PointA, PointB));
        //debugController.Push("BC Dist", Vector3.Distance(PointB, PointC));
    }

    private void Awake()
    {
        Debug.Log("Metoda PlaceOnTap Awake1");
        _arRaycastManager = GetComponent<ARRaycastManager>();
        Debug.Log("Metoda PlaceOnTap Awake2");
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        Debug.Log("Metoda PlaceOnTap TryGetTouchPosition1");
        if (Input.touchCount > 0)
        {
            Debug.Log("Metoda PlaceOnTap TryGetTouchPosition2");
            touchPosition = Input.GetTouch(0).position;
            Debug.Log("Metoda PlaceOnTap TryGetTouchPosition3");
            return true;
        }
        Debug.Log("Metoda PlaceOnTap TryGetTouchPosition4");
        touchPosition = default;
        Debug.Log("Metoda PlaceOnTap TryGetTouchPosition5");
        return false;
    }

    private void InitLine()
    {
        Debug.Log("Metoda PlaceOnTap InitLine1");
        line = gameObject.AddComponent<LineRenderer>();
        Debug.Log("Metoda PlaceOnTap InitLine2");
        line.material = new Material(Shader.Find("Sprites/Default"));
        Debug.Log("Metoda PlaceOnTap InitLine3");
#pragma warning disable CS0618 // Type or member is obsolete
        line.SetColors(Color.red, Color.red);
        Debug.Log("Metoda PlaceOnTap InitLine4");
        line.SetWidth(0.05f, 0.05f);
        Debug.Log("Metoda PlaceOnTap InitLine5");
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private void ConnectObjects()
    {
        Debug.Log("Metoda PlaceOnTap ConnectObjects1");
        DestroyImmediate(line);
        Debug.Log("Metoda PlaceOnTap ConnectObjects2");
        InitLine();
        Debug.Log("Metoda PlaceOnTap ConnectObjects3");
        CalculateAdditionalPoints();
        Debug.Log("Metoda PlaceOnTap ConnectObjects4");

        var positions = new List<Vector3>();
        positions.Add(PointA);
        positions.Add(PointB);
        positions.Add(PointC);
        positions.Add(PointD);
        positions.Add(PointA);
        Debug.Log("Metoda PlaceOnTap ConnectObjects5");
        line.positionCount = positions.Count;
        Debug.Log("Metoda PlaceOnTap ConnectObjects6");
        line.SetPositions(positions.ToArray());
        Debug.Log("Metoda PlaceOnTap ConnectObjects7");
    }

    void Update()
    {
        Debug.Log("Metoda PlaceOnTap Update1");
        if (ModelsQueue.TableSceneStart)
        {
            Debug.Log("Metoda PlaceOnTap Update2");
            return;
        }
        Debug.Log("Metoda PlaceOnTap Update3");
        if (AddFakePoint)
        {
            Debug.Log("Metoda PlaceOnTap Update4");
            //var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            var obj = (GameObject)Instantiate(miecz, new Vector3(0, 0, 0), Quaternion.identity);
            Debug.Log("Metoda PlaceOnTap Update5");
            obj.transform.localScale = new Vector3(0.10f, 0.05f, 0.10f);
            obj.transform.position = new Vector3(0f, 0.1f, 0f);
            Debug.Log("Metoda PlaceOnTap Update6");
            //var spawnedObject = Instantiate(obj, hitPose.position, new Quaternion());
            obj.AddComponent<TouchMovement>();
            Debug.Log("Metoda PlaceOnTap Update7");
            obj.layer = 8;
            placedObjects.Add(obj);
            Debug.Log("Metoda PlaceOnTap Update8");


            //var obj2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            var obj2 = (GameObject)Instantiate(miecz, new Vector3(0, 0, 0), Quaternion.identity);
            Debug.Log("Metoda PlaceOnTap Update9");
            obj2.transform.localScale = new Vector3(0.10f, 0.05f, 0.10f);
            obj2.transform.position = new Vector3(0f, 0.1f, 10f);
            Debug.Log("Metoda PlaceOnTap Update10");
            //var spawnedObject = Instantiate(obj, hitPose.position, new Quaternion());
            obj2.AddComponent<TouchMovement>();
            Debug.Log("Metoda PlaceOnTap Update11");
            obj2.layer = 8;
            placedObjects.Add(obj2);
            Debug.Log("Metoda PlaceOnTap Update12");

            if (placedObjects.Count == 2)
            {
                Debug.Log("Metoda PlaceOnTap Update13");
                //StartTableSceneButton.gameObject.SetActive(true);
                //ConnectObjects();
                //StartScene();
            }
            Debug.Log("Metoda PlaceOnTap Update14");
            AddFakePoint = false;
            Debug.Log("Metoda PlaceOnTap Update15");
        }

        if (placedObjects.Count == 2)
        {
            Debug.Log("Metoda PlaceOnTap Update16");
            ConnectObjects();
            Debug.Log("Metoda PlaceOnTap Update17");
            StartScene();
            Debug.Log("Metoda PlaceOnTap Update18");
            //DebugText.gameObject.SetActive(true);
            //PrintDebug();
        }
        if (!TryGetTouchPosition(out Vector2 touchPosition) || !(Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Debug.Log("Metoda PlaceOnTap Update19");
            return;
        }
        Debug.Log("Metoda PlaceOnTap Update20");
        if (placedObjects.Count < 2)
        {
            Debug.Log("Metoda PlaceOnTap Update21");
            if (_arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                Debug.Log("Metoda PlaceOnTap Update22");
                var hitPose = hits[0].pose;
                hitPose.position.y += 0.1f;
                Debug.Log("Metoda PlaceOnTap Update23");

                var obj = (GameObject)Instantiate(miecz, new Vector3(0, 0, 0), Quaternion.identity);
                Debug.Log("Metoda PlaceOnTap Update24");
                obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                Debug.Log("Metoda PlaceOnTap Update25");
                obj.transform.position = hitPose.position;
                Debug.Log("Metoda PlaceOnTap Update26");
                //var spawnedObject = Instantiate(obj, hitPose.position, new Quaternion());
                obj.AddComponent<TouchMovement>();
                Debug.Log("Metoda PlaceOnTap Update27");
                obj.layer = 8;
                Debug.Log("Metoda PlaceOnTap Update28");
                placedObjects.Add(obj);
                Debug.Log("Metoda PlaceOnTap Update29");
                if (placedObjects.Count == 2)
                {
                    Debug.Log("Metoda PlaceOnTap Update30");
                    CalculateAdditionalPoints();
                    Debug.Log("Metoda PlaceOnTap Update31");
                    StartScene();
                    Debug.Log("Metoda PlaceOnTap Update32");
                }
            }
        }
    }

    public void StartScene()
    {
        Debug.Log("Metoda PlaceOnTap StartScene1");
        ModelsQueue.TablePoint1 = placedObjects[0].transform.position;
        Debug.Log("Metoda PlaceOnTap StartScene2");
        ModelsQueue.TablePoint2 = placedObjects[1].transform.position;
        Debug.Log("Metoda PlaceOnTap StartScene3");
        TipTextBg.gameObject.SetActive(false);
        Debug.Log("Metoda PlaceOnTap StartScene4");

        ModelsQueue.TableSceneCenter = new Vector3((PointA.x + PointC.x) / 2, ModelsQueue.TablePoint1.y - 0.05f, (PointA.z + PointC.z) / 2);
        Debug.Log("Metoda PlaceOnTap StartScene5");
        ModelsQueue.TablePoint3 = PointC;
        ModelsQueue.TablePoint4 = PointD;
        ModelsQueue.TableBCDistance = tableBC;
        Debug.Log("Metoda PlaceOnTap StartScene6");
        ModelsQueue.PlaygroundSceneObjectScale = null;
        ModelsQueue.PlaygroundSceneObjectTimelineScale = null;
        Debug.Log("Metoda PlaceOnTap StartScene7");
        DestroyImmediate(line);
        Debug.Log("Metoda PlaceOnTap StartScene8");
        DestroyAR();
        Debug.Log("Metoda PlaceOnTap StartScene9");
        StartTableSceneButton.gameObject.SetActive(false);
        Debug.Log("Metoda PlaceOnTap StartScene10");
        ModelsQueue.TableSceneStart = true;
        Debug.Log("Metoda PlaceOnTap StartScene11");
    }

    void DestroyAR()
    {
        Debug.Log("Metoda PlaceOnTap DestroyAR1");
        var pointClouds = FindObjectsOfType<ARPointCloud>();
        Debug.Log("Metoda PlaceOnTap DestroyAR2");
        var arPlane = FindObjectsOfType<ARPlane>();
        Debug.Log("Metoda PlaceOnTap DestroyAR3");
        var ARPointCloudManager = FindObjectOfType<ARPointCloudManager>();
        Debug.Log("Metoda PlaceOnTap DestroyAR4");
        var ARPlaneManager = FindObjectOfType<ARPlaneManager>();
        Debug.Log("Metoda PlaceOnTap DestroyAR5");

        if (ARPointCloudManager)
        {
            Debug.Log("Metoda PlaceOnTap DestroyAR6");
            DestroyImmediate(ARPointCloudManager);
            Debug.Log("Metoda PlaceOnTap DestroyAR7");
        }
        if (ARPlaneManager)
        {
            Debug.Log("Metoda PlaceOnTap DestroyAR8");
            ARPlaneManager.detectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
            //DestroyImmediate(ARPlaneManager);
            Debug.Log("Metoda PlaceOnTap DestroyAR9");
        }

        if (arPlane != null && arPlane.Any())
        {
            Debug.Log("Metoda PlaceOnTap DestroyAR10");
            foreach (var plane in arPlane)
            {
                Debug.Log("Metoda PlaceOnTap DestroyAR11");
                DestroyImmediate(plane.gameObject);
                Debug.Log("Metoda PlaceOnTap DestroyAR12");
            }
        }
        Debug.Log("Metoda PlaceOnTap DestroyAR13");
        if (pointClouds != null && pointClouds.Any())
        {
            Debug.Log("Metoda PlaceOnTap DestroyAR14");
            foreach (var pointCloud in pointClouds)
            {
                Debug.Log("Metoda PlaceOnTap DestroyAR15");
                DestroyImmediate(pointCloud.gameObject);
                Debug.Log("Metoda PlaceOnTap DestroyAR16");
            }
        }
        Debug.Log("Metoda PlaceOnTap DestroyAR17");
        foreach (var go in placedObjects)
        {
            Debug.Log("Metoda PlaceOnTap DestroyAR18");
            DestroyImmediate(go);
            Debug.Log("Metoda PlaceOnTap DestroyAR19");
        }
    }
}
