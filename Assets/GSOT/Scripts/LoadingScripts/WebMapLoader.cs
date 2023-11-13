using ARLocation;
using Assets.GSOT;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using Assets.GSOT.Scripts.SceneScripts;
using UnityEngine.XR.ARFoundation;
using Assets.GSOT.Scripts.ApiScripts.EventLog;
using System.Threading.Tasks;
using static ObjectMovingController;
using UnityEngine.Video;
using System.IO;

namespace GSOT
{
    public class DataEntry
    {
        public int id;
        public long sceneId;
        [NonSerialized]
        public double lat;
        [NonSerialized]
        public double lng;
        [NonSerialized]
        public double altitude;
        [NonSerialized]
        public string altitudeMode;
        public string name;
        public string meshId;
        [NonSerialized]
        public float movementSmoothing;
        [NonSerialized]
        public int maxNumberOfLocationUpdates;
        [NonSerialized]
        public bool useMovingAverage;
        [NonSerialized]
        public bool hideObjectUtilItIsPlaced = true;
        public DateTime timeToRender;
        public string description;
        [NonSerialized]
        public long SecondsToRender;
        [NonSerialized]
        public List<ObjectLocation> path;
        [NonSerialized]
        public List<PathLine> pathLines;

        public Vector3 FacingDirection { get; set; }
        public bool IsFacingSpectator;
        public bool IsAvailableInTableSceneUsingMode;
        public Assets.GSOT.Scripts.Models.ApiModels.Type Type;

        public AltitudeMode getAltitudeMode()
        {
            if (altitudeMode == "GroundRelative")
            {
                return AltitudeMode.GroundRelative;
            }
            else if (altitudeMode == "DeviceRelative")
            {
                return AltitudeMode.DeviceRelative;
            }
            else if (altitudeMode == "Absolute")
            {
                return AltitudeMode.Absolute;
            }
            else
            {
                return AltitudeMode.Ignore;
            }
        }
    }

    public class WebMapLoader : MonoBehaviour
    {
        public GameObject spriteObject;
        public Material floorMaterial;
        public GameObject TwoSidesVideoPrefab;
        public GameObject OneSideVideoPrefab;
        public GameObject OneSideImagePrefab;
        public GameObject TwoSidesImagePrefab;
        public Material VideoMaterial;
        public RenderTexture VideoRenderTexture;
        public bool DebugMode;

        private List<DataEntry> _dataEntries = new List<DataEntry>();
        private List<GameObject> _stages = new List<GameObject>();
        private List<PlaceAtLocation> _placeAtComponents = new List<PlaceAtLocation>();
        private bool initDone = false;
        public GameObject NoGPSText;

        bool SceneInitiated = false;
        void Start()
        {
            Debug.Log("Metoda Start1");
            var sounds = GameObject.Find("sounds");
            Debug.Log("Metoda Start2");
            if (sounds)
            {
                Debug.Log("Metoda Start3");
                var aus = sounds.GetComponents<AudioSource>();
                Debug.Log("Metoda Start4");
                var ac = sounds.GetComponents<AudioController>();
                Debug.Log("Metoda Start4");
                if (aus.Any())
                {
                    Debug.Log("Metoda Start5");
                    foreach (var a in aus)
                    {
                        Debug.Log("Metoda Start6");
                        DestroyImmediate(a);
                    }
                }
                Debug.Log("Metoda Start7");
                if (ac.Any())
                {
                    Debug.Log("Metoda Start8");
                    foreach (var a in ac)
                    {
                        Debug.Log("Metoda Start9");
                        DestroyImmediate(a);
                    }
                }

            }
            Debug.Log("Metoda Start10");
            StartCoroutine(StartLocation());
        }

        System.Collections.IEnumerator StartLocation()
        {
            Debug.Log("Metoda StartLocation1");
            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("Metoda StartLocation2");
                yield return new WaitForSeconds(1);
            }
            Debug.Log("Metoda StartLocation3");
            Input.location.Start();
            Debug.Log("Metoda StartLocation4");
            int maxWait = 200;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                Debug.Log("Metoda StartLocation5");
                yield return new WaitForSeconds(1);
                maxWait--;
                Debug.Log("Metoda StartLocation6");
            }
            if (maxWait < 1)
            {
                Debug.Log("Metoda StartLocation7");
                yield break;
            }
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("Metoda StartLocation8");
                yield break;
            }
            else
            {
                Debug.Log("Metoda StartLocation9");
                //locationActive = true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log("Metoda Update1");
            if ((ARLocationProvider.Instance.CurrentLocation.ToLocation().Latitude == 0 || ARLocationProvider.Instance.CurrentLocation.ToLocation().Longitude == 0)
                && !ModelsQueue.IsTableScene)
            {
                Debug.Log("Metoda Update2");
                NoGPSText.SetActive(true);
                Debug.Log("Metoda Update3");
                return;
            }
            else
            {
                Debug.Log("Metoda Update4");
                NoGPSText.SetActive(false);
                Debug.Log("Metoda Update5");
                if (!initDone)
                {
                    Debug.Log("Metoda Update6");
                    spriteObject.SetActive(false);
                    Debug.Log("Metoda Update7");
                    if (!ModelsQueue.IsTableScene)
                    {
                        Debug.Log("Metoda Update8");
                        ModelsQueue.InitScene();
                        Debug.Log("Metoda Update9");
                        SceneInitiated = true;
                        Debug.Log("Metoda Update10");
                    }

                    if (Application.isEditor)
                    {
                        Debug.Log("Metoda Update11");
                        var spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
                        Debug.Log("Metoda Update12");
                        //spriteObject.SetActive(true);
                        spriteObject.transform.position = Vector3.zero;// ModelsQueue.TableSceneCenter;
                                                                       //spriteObject.transform.localScale = new Vector3(Vector3.Distance(ModelsQueue.TablePoint1, ModelsQueue.TablePoint2), ModelsQueue.TableBCDistance, 0);
                        Debug.Log("Metoda Update13");
                        spriteObject.transform.localScale = new Vector3(1, 2, 2);
                        Debug.Log("Metoda Update14");
                        var scene = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault()?
                            .Scenes.Where(x => x.Id == ModelsQueue.ActiveSceneId).FirstOrDefault();
                        Debug.Log("Metoda Update15");
                        if (scene == null) scene = new ApplicationScene();
                        Debug.Log("Metoda Update16");
                        var color = scene?.GroundColor;
                        Debug.Log("Metoda Update17");
                        if (color == null) color = "#ffffff";
                        Debug.Log("Metoda Update18");
                        Color newCol;
                        Debug.Log("Metoda Update19");
                        if (ColorUtility.TryParseHtmlString(color, out newCol))
                        {
                            Debug.Log("Metoda Update20");
                            newCol.a = (float)((float)scene.GroundColorAlpha / 255f);
                            Debug.Log("Metoda Update21");
                            spriteRenderer.color = newCol;
                            Debug.Log("Metoda Update22");
                        }
                    }
                    Debug.Log("Metoda Update23");
                    var sounds = GameObject.Find("sounds");
                    Debug.Log("Metoda Update24");
                    if (sounds == null)
                    {
                        Debug.Log("Metoda Update25");
                        sounds = new GameObject("sounds");
                        Debug.Log("Metoda Update26");
                        Instantiate(sounds);
                        Debug.Log("Metoda Update27");
                    }
                    initDone = true;
                    Debug.Log("Metoda Update28");
                }
            }

            if ((ModelsQueue.TableSceneStart || !ModelsQueue.IsTableScene) && !SceneInitiated)
            {
                Debug.Log("Metoda Update29");
                ModelsQueue.InitScene();
                Debug.Log("Metoda Update30");

                var scene = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault()?
                    .Scenes.Where(x => x.Id == ModelsQueue.ActiveSceneId).FirstOrDefault();
                Debug.Log("Metoda Update31");
                SceneInitiated = true;
                Debug.Log("Metoda Update32");

                var spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
                Debug.Log("Metoda Update33");
                if (!string.IsNullOrEmpty(scene.RectangleFile))
                {
                    Debug.Log("Metoda Update34");
                    var imgConverter = FindObjectOfType<IMG2Sprite>();
                    Debug.Log("Metoda Update35");
                    var bg = imgConverter.LoadTexture(scene.RectangleFile);
                    Debug.Log("Metoda Update36");
                    var NewSprite = Sprite.Create(bg, new Rect(0, 0, bg.width, bg.height), new Vector2(0.5f, 0.5f), bg.width);
                    Debug.Log("Metoda Update37");
                    if (bg != null)
                    {
                        spriteRenderer.sprite = NewSprite;
                        Debug.Log("Metoda Update38");
                    }
                }
                spriteObject.SetActive(true);
                Debug.Log("Metoda Update39");
                spriteObject.transform.position = ModelsQueue.TableSceneCenter;
                Debug.Log("Metoda Update40");
                spriteObject.transform.localScale = new Vector3(Vector3.Distance(ModelsQueue.TablePoint1, ModelsQueue.TablePoint2), ModelsQueue.TableBCDistance, 0);
                Debug.Log("Metoda Update41");

                var scale = spriteObject.transform.localScale;
                Debug.Log("Metoda Update42");
                scale.x += 0.25f;
                scale.y += 0.25f;
                scale.z += 0.25f;
                Debug.Log("Metoda Update43");
                spriteObject.transform.localScale = scale;
                Debug.Log("Metoda Update44");
                spriteObject.transform.forward = ModelsQueue.TablePoint3 - ModelsQueue.TablePoint2;
                Debug.Log("Metoda Update45");

                var debugController = FindObjectOfType<DebugController>();
                Debug.Log("Metoda Update46");
                var rotation = spriteObject.transform.eulerAngles;
                Debug.Log("Metoda Update47");
                rotation.x = 90f;
                Debug.Log("Metoda Update48");
                spriteObject.transform.eulerAngles = rotation;
                Debug.Log("Metoda Update49");


                if (string.IsNullOrEmpty(scene.RectangleFile))
                {
                    Debug.Log("Metoda Update50");
                    var color = scene?.GroundColor;
                    Debug.Log("Metoda Update52");
                    if (color == null) color = "#ffffff";
                    Debug.Log("Metoda Update52");
                    Color newCol;
                    if (ColorUtility.TryParseHtmlString(color, out newCol))
                    {
                        Debug.Log("Metoda Update53");
                        newCol.a = (float)((float)scene.GroundColorAlpha / 255f);
                        Debug.Log("Metoda Update54");
                        spriteRenderer.color = newCol;
                        Debug.Log("Metoda Update55");
                    }
                }
            }
            if ((ModelsQueue.IsNotEmpty() || ModelsQueue.HasAudioToPlay()) && SceneInitiated)
            {
                Debug.Log("Metoda Update56");
                BuildGameObjects();
                Debug.Log("Metoda Update57");
            }
            Debug.Log("Metoda Update58");
        }


        private void CreateTableMesh(Color color)
        {
            Debug.Log("Metoda Update59");
            Vector2[] vertices2D = new Vector2[] {
            new Vector2(ModelsQueue.TablePoint1.x - spriteObject.transform.position.x,ModelsQueue.TablePoint1.z - ModelsQueue.TableSceneCenter.z),
            new Vector2(ModelsQueue.TablePoint2.x - spriteObject.transform.position.x,ModelsQueue.TablePoint2.z - ModelsQueue.TableSceneCenter.z),
            new Vector2(ModelsQueue.TablePoint3.x - spriteObject.transform.position.x,ModelsQueue.TablePoint3.z - ModelsQueue.TableSceneCenter.z),
            new Vector2(ModelsQueue.TablePoint4.x - spriteObject.transform.position.x,ModelsQueue.TablePoint4.z - ModelsQueue.TableSceneCenter.z)
            };
            Debug.Log("Metoda Update60");
            if (Application.isEditor)
            {
                Debug.Log("Metoda Update61");
                vertices2D = new Vector2[] {
                new Vector2(0 - spriteObject.transform.position.x,0 - spriteObject.transform.position.z),
                new Vector2(0 - spriteObject.transform.position.x,50 - spriteObject.transform.position.z),
                new Vector2(50 - spriteObject.transform.position.x,50 - spriteObject.transform.position.z),
                new Vector2(50 - spriteObject.transform.position.x,100 - spriteObject.transform.position.z),
                new Vector2(0 - spriteObject.transform.position.x,100 - spriteObject.transform.position.z),
                new Vector2(0 - spriteObject.transform.position.x,150 - spriteObject.transform.position.z),
                new Vector2(150 - spriteObject.transform.position.x,150 - spriteObject.transform.position.z),
                new Vector2(150 - spriteObject.transform.position.x,100 - spriteObject.transform.position.z),
                new Vector2(100 - spriteObject.transform.position.x,100 - spriteObject.transform.position.z),
                new Vector2(100 - spriteObject.transform.position.x,50 - spriteObject.transform.position.z),
                new Vector2(150 - spriteObject.transform.position.x,50 - spriteObject.transform.position.z),
                new Vector2(150 - spriteObject.transform.position.x,0 - spriteObject.transform.position.z)
                };
                Debug.Log("Metoda Update62");
            }

            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(vertices2D);
            Debug.Log("Metoda Update63");
            int[] indices = tr.Triangulate();
            Debug.Log("Metoda Update64");

            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[vertices2D.Length];
            Debug.Log("Metoda Update65");
            for (int i = 0; i < vertices.Length; i++)
            {
                Debug.Log("Metoda Update66");
                vertices[i] = transform.InverseTransformPoint(new Vector3(vertices2D[i].x, ModelsQueue.TablePoint4.y - spriteObject.transform.position.y, vertices2D[i].y));
                Debug.Log("Metoda Update67");
            }


            //transform.transformpoin
            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices;
            Debug.Log("Metoda Update68");
            msh.triangles = indices;
            Debug.Log("Metoda Update69");
            msh.RecalculateNormals();
            Debug.Log("Metoda Update70");
            msh.RecalculateBounds();
            Debug.Log("Metoda Update71");
            // Set up game object with mesh;
            var mr = spriteObject.GetComponent<MeshRenderer>();
            Debug.Log("Metoda Update72");
            MeshFilter filter = mr.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            Debug.Log("Metoda Update73");
            //Material material = new Material(Shader.Find("Autodesk Interactive"));
            //mr.material = material;
            mr.material.SetColor("_Color", color);
            Debug.Log("Metoda Update74");
            spriteObject.transform.rotation = Quaternion.FromToRotation(ModelsQueue.TablePoint2, ModelsQueue.TablePoint3);
            Debug.Log("Metoda Update75");
            spriteObject.transform.Rotate(0, Input.GetAxis("Horizontal") * -0.15f, 0, Space.Self);
            Debug.Log("Metoda Update76");
            filter.mesh.Clear();
            Debug.Log("Metoda Update77");
            filter.mesh = msh;
            Debug.Log("Metoda Update78");
        }

        void BuildGameObjects()
        {
            Debug.Log("Metoda BuildGameObjects1");
            if (ModelsQueue.HasAudioToPlay())
            {
                Debug.Log("Metoda BuildGameObjects2");
                var sounds = GameObject.Find("sounds");
                Debug.Log("Metoda BuildGameObjects3");
                var audio = ModelsQueue.GetAudio();
                Debug.Log("Metoda BuildGameObjects3");
                if (audio != null)
                {
                    Debug.Log("Metoda BuildGameObjects4");
                    var ac = sounds.AddComponent<AudioController>();
                    Debug.Log("Metoda BuildGameObjects5");
                    ac.InitAudio(audio);
                    Debug.Log("Metoda BuildGameObjects6");
                }
                Debug.Log("Metoda BuildGameObjects7");
            }
            Debug.Log("Metoda BuildGameObjects8");
            foreach (var entry in ModelsQueue.ModelsToRender())
            {
                Debug.Log("Metoda BuildGameObjects9");
                if (ModelsQueue.IsTableScene)
                {
                    Debug.Log("Metoda BuildGameObjects9");
                    if (!entry.IsAvailableInTableSceneUsingMode)
                    {
                        Debug.Log("Metoda BuildGameObjects10");
                        continue;
                    }
                }
                Debug.Log("Metoda BuildGameObjects10");
                GameObject Prefab = null;
                Debug.Log("Metoda BuildGameObjects11");
                if (entry.Type == Assets.GSOT.Scripts.Models.ApiModels.Type.Model)
                {
                    Debug.Log("Metoda BuildGameObjects12");
                    Prefab = GetDontDestroyOnLoadObjects(entry.meshId);
                    Debug.Log("Metoda BuildGameObjects13");
                }

                if (Prefab == null)
                {
                    Debug.Log("Metoda BuildGameObjects14");
                    try
                    {
                        Debug.Log("Metoda BuildGameObjects15");
                        if (entry.Type != Assets.GSOT.Scripts.Models.ApiModels.Type.Model)
                        {
                            Debug.Log("Metoda BuildGameObjects16");
                            if (entry.Type == Assets.GSOT.Scripts.Models.ApiModels.Type.Graphic)
                            {
                                Debug.Log("Metoda BuildGameObjects17");
                                if (entry.pathLines[0].GraphicVideoPosition == Assets.GSOT.Scripts.Models.ApiModels.Timeline.SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
                                {
                                    Debug.Log("Metoda BuildGameObjects18");
                                    Prefab = FilesUtils.LoadVideo(entry.meshId, TwoSidesImagePrefab, entry);
                                    Debug.Log("Metoda BuildGameObjects19");
                                }
                                else
                                {
                                    Debug.Log("Metoda BuildGameObjects20");
                                    Prefab = FilesUtils.LoadVideo(entry.meshId, OneSideImagePrefab, entry);
                                    Debug.Log("Metoda BuildGameObjects21");
                                }

                                //var imgCont = Prefab.transform.Find("ImageContainer");
                                //SpriteRenderer sr = Prefab.GetComponentInChildren<SpriteRenderer>();
                                //imgCont.gameObject.transform.localPosition = new Vector3(-sr.sprite.texture.width / 200, 0, 0);
                                Debug.Log("Metoda BuildGameObjects22");
                            }
                            else
                            {
                                Debug.Log("Metoda BuildGameObjects23");
                                if (entry.pathLines[0].GraphicVideoPosition == Assets.GSOT.Scripts.Models.ApiModels.Timeline.SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
                                {
                                    Debug.Log("Metoda BuildGameObjects24");
                                    Prefab = FilesUtils.LoadVideo(entry.meshId, TwoSidesVideoPrefab, entry);
                                    Debug.Log("Metoda BuildGameObjects25");
                                }
                                else
                                {
                                    Debug.Log("Metoda BuildGameObjects26");
                                    Prefab = FilesUtils.LoadVideo(entry.meshId, OneSideVideoPrefab, entry);
                                    Debug.Log("Metoda BuildGameObjects27");
                                }
                                Debug.Log("Metoda BuildGameObjects28");
                                //var vp = Prefab.GetComponent<VideoPlayer>();
                                //if(vp != null)
                                //{
                                //    vp.renderMode = VideoRenderMode.CameraNearPlane;
                                //    vp.targetCamera = Camera.main;
                                //    //vp.targe
                                //}
                            }
                            Debug.Log("Metoda BuildGameObjects29");
                        }
                        else
                        {
                            Debug.Log("Metoda BuildGameObjects30");
                            if (File.Exists(entry.meshId) == false)
                            {
                                Debug.Log($"plik: {entry.meshId} nie istnieje");
                                Prefab = FilesUtils.LoadModelFromFile(entry.meshId);
                            }
                            else
                            {
                                Debug.Log($"plik: {entry.meshId}  istnieje");
                                Prefab = FilesUtils.LoadModelFromFile(entry.meshId);
                            }
                            //Prefab = FilesUtils.LoadModelFromFile(entry.meshId);
                            Debug.Log("Metoda BuildGameObjects31");
                            //Prefab.name = entry.meshId;
                            //DontDestroyOnLoad(Prefab);
                        }
                        Debug.Log("Metoda BuildGameObjects32");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ARLocation#WebMapLoader]: Prefab {entry.meshId} not found. Ex: {ex}");
                    }
                }
                if (!Prefab)
                {
                    Debug.Log("Metoda BuildGameObjects33");
                    Debug.LogWarning($"[ARLocation#WebMapLoader]: Prefab {entry.meshId} not found.");
                    return;
                }
                
                else
                {
                    Debug.Log("Metoda BuildGameObjects34");
                    Load(Prefab, entry);
                    Debug.Log("Metoda BuildGameObjects35");
                }
                Debug.Log("Metoda BuildGameObjects36");
            }
            Debug.Log("Metoda BuildGameObjects37");
        }

        public static GameObject GetDontDestroyOnLoadObjects(string name)
        {
            GameObject temp = null;
            try
            {
                Debug.Log("Metoda Update120");
                temp = new GameObject();
                DontDestroyOnLoad(temp);
                Debug.Log("Metoda Update121");
                Scene dontDestroyOnLoad = temp.scene;
                Debug.Log("Metoda Update122");
                DestroyImmediate(temp);
                Debug.Log("Metoda Update123");
                temp = null;
                var dontDestroy = dontDestroyOnLoad.GetRootGameObjects().ToList();
                Debug.Log("Metoda Update124");
                foreach (var dd in dontDestroy)
                {
                    Debug.Log("Metoda Update125");
                    try
                    {
                        Debug.Log("Metoda Update126");
                        if (dd.name[0] != '{') continue;
                        Debug.Log("Metoda Update127");
                        var entry = JsonUtility.FromJson<DataEntry>(dd.name);
                        Debug.Log("Metoda Update128");
                        if (entry != null)
                        {
                            Debug.Log("Metoda Update129");
                            if (entry.meshId == name)
                            {
                                Debug.Log("Metoda Update130");
                                return dd;
                            }
                            Debug.Log("Metoda Update131");
                        }
                        Debug.Log("Metoda Update132");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Ex: {ex.Message}");
                        Debug.Log("Metoda Update132");
                    }
                }
                Debug.Log("Metoda Update133");
                return null;
                //return dontDestroyOnLoad.GetRootGameObjects().ToList().Where(x => JsonUtility.FromJson<DataEntry>(x.name).meshId == name).FirstOrDefault();
            }
            finally
            {
                Debug.Log("Metoda Updat134");
                if (temp != null)
                {
                    Debug.Log("Metoda Update135");
                    DestroyImmediate(temp);
                    Debug.Log("Metoda Update136");
                }
            }
        }

        public void Load(GameObject Prefab, DataEntry entry)
        {
            Debug.Log("Metoda Update137");
            var go = Instantiate(Prefab);
            Debug.Log("Metoda Update138");

            if (entry.Type == Assets.GSOT.Scripts.Models.ApiModels.Type.Graphic)
            {
                Debug.Log("Metoda Update139");
                if (entry.pathLines[0].GraphicVideoPosition != Assets.GSOT.Scripts.Models.ApiModels.Timeline.SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
                {
                    Debug.Log("Metoda Update140");
                    var imgCont = go.transform.Find("ImageContainer");
                    Debug.Log("Metoda Update141");
                    SpriteRenderer sr = imgCont.GetComponent<SpriteRenderer>();
                    Debug.Log("Metoda Update142");
                    imgCont.transform.localScale = new Vector3(1 / ((float)sr.sprite.texture.width / 100), 1 / ((float)sr.sprite.texture.height / 100));
                    Debug.Log("Metoda Update143");
                    //var imgCont2 = go.transform.Find("ImageBehind");
                    //SpriteRenderer sr2 = imgCont2.GetComponent<SpriteRenderer>();
                    //sr2.sprite = sr.sprite;
                    //sr2.size = sr.size;
                    //sr2.sprite.texture.width = sr.sprite.texture.width;
                    //sr2.sprite.texture.height = sr.sprite.texture.height;
                    //imgCont2.transform.localScale = imgCont.transform.localScale;
                }
                else
                {
                    Debug.Log("Metoda Update144");
                    var imgCont = go.transform.Find("ImageContainer");
                    Debug.Log("Metoda Update145");
                    SpriteRenderer sr = imgCont.GetComponent<SpriteRenderer>();
                    Debug.Log("Metoda Update146");
                    imgCont.transform.localScale = new Vector3(1 / ((float)sr.sprite.texture.width / 100), 1 / ((float)sr.sprite.texture.height / 100));
                    Debug.Log("Metoda Update147");
                    //var imgCont2 = go.transform.Find("ImageContainer2");
                    //SpriteRenderer sr2 = imgCont2.GetComponent<SpriteRenderer>();
                    //imgCont2.transform.localScale = new Vector3(1 / ((float)sr2.sprite.texture.width / 100), 1 / ((float)sr2.sprite.texture.height / 100));
                }
            }
            Debug.Log("Metoda Update148");
            go.name = JsonUtility.ToJson(entry);
            Debug.Log("Metoda Update149");
            var location = new Location()
            {
                Latitude = entry.path.FirstOrDefault().Latitude,
                Longitude = entry.path.FirstOrDefault().Longitude,
                Altitude = entry.path.FirstOrDefault().Altitude,
                AltitudeMode = AltitudeMode.DeviceRelative,
                Label = entry.name
            };
            Debug.Log("Metoda Update150");
            var collider = go.AddComponent<CapsuleCollider>();
            Debug.Log("Metoda Update151");
            collider.center = new Vector3(0, 1, 0);
            Debug.Log("Metoda Update152");
            collider.radius = 1;
            collider.height = 3;
            Debug.Log("Metoda Update153");
            var descriptionComponent = go.AddComponent<DescriptionComponent>();
            Debug.Log("Metoda Update154");
            descriptionComponent.Description = entry.description;
            Debug.Log("Metoda Update155");

            var PlacementOptions = new PlaceAtLocation.PlaceAtOptions()
            {
                MovementSmoothing = 0,
                MaxNumberOfLocationUpdates = 0,
                UseMovingAverage = entry.useMovingAverage,
                HideObjectUntilItIsPlaced = entry.hideObjectUtilItIsPlaced
            };
            Debug.Log("Metoda Update160");
            List<ObjectLocation> distinctLocations = new List<ObjectLocation>();
            Debug.Log("Metoda Update161");
            foreach (var l in entry.path)
            {
                Debug.Log("Metoda Update162");
                if (!distinctLocations.Any())
                {
                    Debug.Log("Metoda Update163");
                    distinctLocations.Add(l);
                    Debug.Log("Metoda Update164");
                }
                Debug.Log("Metoda Update165");
                var lastLoc = distinctLocations.Last();
                Debug.Log("Metoda Update166");
                if (lastLoc == null || (l.Latitude != lastLoc.Latitude || l.Longitude != lastLoc.Longitude || l.Altitude != lastLoc.Altitude))
                {
                    Debug.Log("Metoda Update167");
                    distinctLocations.Add(l);
                    Debug.Log("Metoda Update168");
                }
            }

            GameObject txt = new GameObject();
            Debug.Log("Metoda Update169");
            txt.transform.SetParent(go.transform);
            Debug.Log("Metoda Update170");
            var tm = txt.AddComponent<TextMesh>();
            Debug.Log("Metoda Update171");
            txt.transform.rotation = go.transform.rotation;
            Debug.Log("Metoda Update172");
            txt.transform.localPosition = new Vector3(0, 2, 0);
            Debug.Log("Metoda Update173");
            tm.text = "";
            Debug.Log("Metoda Update174");
            tm.anchor = TextAnchor.MiddleCenter;
            Debug.Log("Metoda Update175");
            tm.alignment = TextAlignment.Center;
            Debug.Log("Metoda Update176");
            tm.richText = true;
            Debug.Log("Metoda Update177");
            //tm.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            tm.fontSize = 13;
            Debug.Log("Metoda Update178");
            tm.characterSize = 0.1f;
            Debug.Log("Metoda Update179");
            var rotation = tm.transform.eulerAngles;
            Debug.Log("Metoda Update180");
            rotation.y = 180f;
            Debug.Log("Metoda Update181");
            tm.transform.eulerAngles = rotation;
            Debug.Log("Metoda Update182");



            PlaceAtLocation.CreatePlacedInstance(entry, go, location, PlacementOptions, DebugMode, distinctLocations);
            Debug.Log("Metoda Update183");
        }

        //public float MaxHeight()
        //{
        //    Vector3[] verts = meshFilter.mesh.vertices;
        //    for (int i = 0; i < verts.Length; i++)
        //    {
        //        verts[i] = transform.TransformPoint(verts[i]);
        //    }
        //    float maxHeight = verts[0].y;
        //    for (int i = 1; i < verts.Length; i++)
        //    {
        //        if (verts[i].y > maxHeight)
        //        {
        //            maxHeight = verts[i].y;
        //        }
        //    }
        //}
    }
}
