using ARLocation;
using ARLocation.UI;
using ARLocation.Utils;
using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using Assets.GSOT.Scripts.Utils;
using GSOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static Assets.GSOT.Scripts.Models.ApiModels.Timeline;

public class ObjectMovingController : MonoBehaviour
{
    [Serializable]
    public class PathSettingsData
    {
        public LocationPath LocationPath;
        public int SplineSampleCount = 500;
        public LineRenderer LineRenderer;
    }

    [Serializable]
    public class Path
    {
        public List<PathLine> Lines;

        public Path()
        {
            Lines = new List<PathLine>();
        }

        public bool IsFirst(PathLine current)
        {
            return Lines.IndexOf(current) == 0;
        }

        public PathLine Next(PathLine current)
        {
            try
            {
                return Lines[Lines.IndexOf(current) + 1];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    [Serializable]
    public class PathLine
    {
        public int Index;
        public float? Speed;
        public double Distance;
        public bool Destroy;
        public int? Animation;
        public Vector3 PointA;
        public Vector3 PointB;
        public ObjectLocation A;
        public ObjectLocation B;
        public int TimeToTravel;
        public string Caption;
        public double? CaptionLocalizationAltitude;


        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Vx { get; set; }
        public double? Vy { get; set; }
        public double? Vz { get; set; }

        public Vector3? VideoLookAt
        {
            get
            {
                if (!Vx.HasValue || !Vy.HasValue || !Vz.HasValue) return null;
                return new Vector3((float)Vx, (float)Vy, (float)Vz);
            }
        }

        public SceneObjectTimelineGraphicVideoPosition? GraphicVideoPosition { get; set; }

        //helper
        [SerializeField]
        public int TimeToB => B.TimeToReach;

        public override string ToString()
        {
            return $"Index:{Index} Distance:{Distance} Speed:{Speed}";
        }

        public PathLine()
        {

        }
        public PathLine(PathLine l)
        {
            this.A = l.A;
            this.B = l.B;
            this.PointA = l.PointA;
            this.PointB = l.PointB;
            this.TimeToTravel = l.TimeToTravel;
            this.Animation = l.Animation;
            this.Destroy = l.Destroy;
            this.Speed = l.Speed;
            this.Index = l.Index;
            this.Distance = l.Distance;
            this.Caption = l.Caption;
            this.CaptionLocalizationAltitude = l.CaptionLocalizationAltitude;
            this.Vx = l.Vx;
            this.Vy = l.Vy;
            this.Vz = l.Vz;
            this.Width = l.Width;
            this.Height = l.Height;
            this.GraphicVideoPosition = l.GraphicVideoPosition;
        }
    }

    public PathSettingsData PathSettings = new PathSettingsData();

    public Assets.GSOT.Scripts.Models.ApiModels.Type Type = Assets.GSOT.Scripts.Models.ApiModels.Type.Model;
    public bool DebugMode { get; set; }
    private bool play;
    public Spline Spline;
    public bool Play
    {
        get
        {
            return play;
        }
        set
        {
            Current = LinesPath.Lines[0];
            play = value;
        }
    }
    private double u;
    public Transform mainCameraTransform;
    private bool useLineRenderer;
    private bool hasInitialized;
    private GroundHeight groundHeight;
    private List<PathLine> SpeedAtParts;
    public float Scale { get; set; } = 1;
    public float NormalizedScale => Scale;
    public Path LinesPath;

    //[SerializeReference]
    public PathLine Current;// { get; set; }
    public List<Vector3> localPoints { get; set; }
    int pointsCount => localPoints.Count;
    public Vector3 InitialOrientation { get; set; }
    public Vector3 LocalInitialOrientation { get; set; }
    public bool FacingToCamera { get; set; }

    public void SetLocationPath(LocationPath path)
    {
        PathSettings.LocationPath = path;
        hasInitialized = true;
        mainCameraTransform = ARLocationManager.Instance.MainCamera.transform;
        localPoints = new List<Vector3>();
        ARLocationProvider.Instance.OnLocationUpdatedEvent(LocationUpdated);

        Initialize();
        BuildSpline();
    }

    public void CalculatePartsSpeed()
    {
        if (ModelsQueue.IsTableScene || ModelsQueue.IsPlaygroundScene)
        {
            //if (ModelsQueue.PlaygroundSceneObjectTimelineScale.HasValue)
            //{
            //    Scale = ModelsQueue.PlaygroundSceneObjectTimelineScale.Value;
            //}
            //else if (ModelsQueue.TableScale.HasValue)
            //{
            //    Scale = ModelsQueue.TableScale.Value;
            //}
            //else
            //{
            //    Scale = Vector3.Distance(tablePoint1, tablePoint2) / Vector3.Distance(apiPoint1, apiPoint2);
            //}
        }
        int totalTime = 0;
        foreach (var line in LinesPath.Lines)
        {
            Location myLocation = new Location();
            if (ModelsQueue.IsTableScene || ModelsQueue.IsPlaygroundScene)
            {
                myLocation = ModelsQueue.TableTransformPosition;
            }
            else
            {
                myLocation = ARLocationProvider.Instance.CurrentLocation.ToLocation();
            }

            line.PointA = Location.GetGameObjectPositionForLocation(ARLocationManager.Instance.gameObject.transform,
                mainCameraTransform, myLocation, line.A, true);
            if (line.B != null)// != Vector3.zero)
                line.PointB = Location.GetGameObjectPositionForLocation(ARLocationManager.Instance.gameObject.transform,
                    mainCameraTransform, myLocation, line.B, true);
            if (ModelsQueue.IsTableScene || ModelsQueue.IsPlaygroundScene)
            {
                line.PointA = LocationUtils.CalculateTablePoint(line.PointA);
                if (line.B != null)
                    line.PointB = LocationUtils.CalculateTablePoint(line.PointB);
            }
            if (line.B != null)
                line.Distance = Vector3.Distance(line.PointA, line.PointB);
            else
            {
                line.Distance = 0;
            }


            //if (ModelsQueue.IsTableScene)
            //{
            //    line.Distance *= Scale;
            //}
            var timeDelta = line.TimeToTravel - totalTime;
            line.Speed = (float)line.Distance / timeDelta;
            line.TimeToTravel = timeDelta;
            totalTime += timeDelta;
        }

        if (LinesPath.Lines[0].VideoLookAt.HasValue)
        {
            Location myLocation = new Location();
            if (ModelsQueue.IsTableScene || ModelsQueue.IsPlaygroundScene)
            {
                myLocation = ModelsQueue.TableTransformPosition;
            }
            else
            {
                myLocation = ARLocationProvider.Instance.CurrentLocation.ToLocation();
            }
            var videoLookAtPoint = LinesPath.Lines[0].VideoLookAt;
            videoLookAtPoint = Location.GetGameObjectPositionForLocation(ARLocationManager.Instance.gameObject.transform,
                mainCameraTransform, myLocation, new Location(videoLookAtPoint.Value.z, videoLookAtPoint.Value.x, videoLookAtPoint.Value.y), true);

            if (ModelsQueue.IsTableScene || ModelsQueue.IsPlaygroundScene)
            {
                videoLookAtPoint = LocationUtils.CalculateTablePoint(videoLookAtPoint.Value);
            }
            LinesPath.Lines[0].Vx = videoLookAtPoint.Value.x;
            LinesPath.Lines[0].Vy = videoLookAtPoint.Value.y;
            LinesPath.Lines[0].Vz = videoLookAtPoint.Value.z;
        }
        return;
    }

    private void Initialize()
    {
        if (DebugMode)
        {
            gameObject.AddComponent<ModelDebugDistance>();
        }
        localPoints = new List<Vector3>();
        useLineRenderer = PathSettings.LineRenderer != null;
        transform.SetParent(ARLocationManager.Instance.gameObject.transform);
        groundHeight = GetComponent<GroundHeight>();
        if (groundHeight)
        {
            Destroy(groundHeight);
            groundHeight = null;
        }
        if (ARLocationProvider.Instance.IsEnabled)
        {
            LocationUpdated(ARLocationProvider.Instance.CurrentLocation, ARLocationProvider.Instance.LastLocation);
        }
    }

    public void BuildSpline()
    {
        var location = ARLocationProvider.Instance.CurrentLocation.ToLocation();
        if (ModelsQueue.IsTableScene || ModelsQueue.IsPlaygroundScene)
        {
            location = ModelsQueue.TableTransformPosition;
        }
        PathSettings.LocationPath.Locations = PathSettings.LocationPath.Locations.Where(x => x.Latitude != 0 && x.Longitude != 0).OrderBy(x => (x as ObjectLocation).TimeToReach).ToArray();
        location.Altitude = 0;
        if (PathSettings.LocationPath.Locations.Length == 0)
        {
            return;
        }
        foreach (var loc in PathSettings.LocationPath.Locations)
        {
            //var loc = PathSettings.LocationPath.Locations[i];

            localPoints.Add(Location.GetGameObjectPositionForLocation(ARLocationManager.Instance.gameObject.transform,
                mainCameraTransform, location, loc, true));
        }

        LocalInitialOrientation = Location.GetGameObjectPositionForLocation(ARLocationManager.Instance.gameObject.transform,
                mainCameraTransform, location, new Location()
                {
                    Latitude = InitialOrientation.x,
                    Altitude = InitialOrientation.y,
                    Longitude = InitialOrientation.z

                }, true);

        if (ModelsQueue.IsTableScene || ModelsQueue.IsPlaygroundScene)
        {
            for (var i = 0; i < pointsCount; i++)
            {
                var loc = PathSettings.LocationPath.Locations[i];

                localPoints[i] = Location.GetGameObjectPositionForLocation(ARLocationManager.Instance.gameObject.transform,
                    mainCameraTransform, ModelsQueue.TableTransformPosition, loc, true);
            }

            LocalInitialOrientation = Location.GetGameObjectPositionForLocation(ARLocationManager.Instance.gameObject.transform,
                    mainCameraTransform, ModelsQueue.TableTransformPosition, new Location()
                    {
                        Latitude = InitialOrientation.x,
                        Altitude = InitialOrientation.y,
                        Longitude = InitialOrientation.z

                    }, true);

            var debugController = FindObjectOfType<DebugController>();
            if (ModelsQueue.TableScale.HasValue || ModelsQueue.PlaygroundSceneObjectScale.HasValue)
            {
                Scale = ModelsQueue.PlaygroundSceneObjectScale.HasValue ? ModelsQueue.PlaygroundSceneObjectScale.Value
                    : ModelsQueue.TableScale.Value;
                //debugController.Push("Scale api", Scale);
            }
            else if (LocationUtils.apiPoint1 == LocationUtils.apiPoint2)
            {
                Scale = 1;
            }
            else
            {
                Scale = Vector3.Distance(LocationUtils.tablePoint1, LocationUtils.tablePoint2)
                    / Vector3.Distance(LocationUtils.apiPoint1, LocationUtils.apiPoint2);
                //debugController.Push("Scale calculated", Scale);
            }

            var newPoints = LocationUtils.CalculateTablePoints(localPoints.ToList());
            debugController.Push("Pos to calc points:", location);


            //debugController.Push("TA", ModelsQueue.TablePoint1);
            //debugController.Push("TB", ModelsQueue.TablePoint2);
            //debugController.Push("TC", ModelsQueue.TablePoint3);
            //debugController.Push("TD", ModelsQueue.TablePoint4);
            localPoints = newPoints.Distinct().ToList();

            var localScale = this.gameObject.transform.localScale;
            if (Type == Assets.GSOT.Scripts.Models.ApiModels.Type.Video && LinesPath.Lines[0].GraphicVideoPosition == SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
            {
                localScale.x = 0.01f;
            }
            else
            {
                localScale.x = Scale;
            }
            localScale.y = Scale;
            localScale.z = Scale;
            this.gameObject.transform.localScale = localScale;
        }

        if (PathSettings.LocationPath.Locations.Length > 1)
        {
            Spline = Misc.BuildSpline(SplineType.CatmullromSpline, localPoints.ToArray(), 500, 0.5f);
        }

        transform.position = localPoints[0];
        var pos = transform.position;
        if (LinesPath.Lines[0].Height.HasValue)
            pos.y = pos.y + (float)(LinesPath.Lines[0].Height / 2);
        transform.position = pos;
        transform.LookAt(LocalInitialOrientation);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    private void LocationUpdated(LocationReading location, LocationReading _)
    {
        //if (locationUpdatesCount > 1)
        //{
        //    return;
        //}
        //BuildSpline();

        //locationUpdatesCount++;
    }

    float timer = 0;
    bool startedMoving = false;
    Vector3 oldTan;

    private void Update()
    {
        if (Current == null || u > 1)
        {
            return;
        }
        if (!ARLocationProvider.Instance.IsEnabled)
        {
            ARLocationProvider.Instance.Resume();
            return;
        }
        if (!ModelsQueue.SceneStarted)
        {
            ModelsQueue.SceneStarted = true;
        }
        var txt = gameObject.GetComponentInChildren<TextMesh>();
        if (txt)
        {
            if (!string.IsNullOrEmpty(Current.Caption))
            {
                txt.text = Current.Caption;
                var v = txt.transform.localPosition;
                v.y = (float?)Current.CaptionLocalizationAltitude ?? 0f;
                txt.transform.localPosition = v;
                txt.gameObject.transform.LookAt(Camera.main.transform);
                txt.gameObject.transform.localEulerAngles = new Vector3(0, txt.gameObject.transform.localEulerAngles.y + 180, 0);
            }
            else
            {
                txt.text = "";
            }
        }

        if (Current.Width.HasValue && Current.Height.HasValue && ((float)Current.Width * Scale != gameObject.transform.localScale.x
            || (float)Current.Height * Scale != gameObject.transform.localScale.y))
        {
            var entry = JsonUtility.FromJson<DataEntry>(gameObject.name);
            if (entry.Type == Assets.GSOT.Scripts.Models.ApiModels.Type.Video)
            {
                if (LinesPath.Lines[0].GraphicVideoPosition == SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
                {
                    gameObject.transform.localScale = new Vector3(0.01f, (float)Current.Height * Scale, (float)Current.Width * Scale);
                }
                else
                {
                    gameObject.transform.localScale = new Vector3((float)Current.Width * Scale, 0f, (float)Current.Height * Scale);
                    //if (gameObject.transform.rotation.eulerAngles.x != 90f)
                    //gameObject.transform.Rotate(90, 0, 0);
                }
            }
            else
            {
                gameObject.transform.localScale = new Vector3((float)Current.Width * Scale, (float)Current.Height * Scale, 0f);
                //gameObject.transform.localScale = new Vector3(1, 1, 0);
            }
        }

        if (Current.TimeToTravel <= timer)
        {
            timer = 0f;
            if (Current.Destroy)
            {
                DestroyImmediate(gameObject);
            }
            Current = LinesPath.Next(Current);
            if (Current != null && Current.Speed > 0)
            {
                startedMoving = true;
            }
        }
        else
        {
            timer += Time.deltaTime;
        }
        if (Current == null)
        {
            return;
        }
        var animation = gameObject.GetComponentInChildren<AnimationListComponent>();
        if (animation != null)
        {
            animation.PlayAnimation(Current.Animation.ToString());
        }
        if (!Play || Spline == null)
        {
            if (LinesPath.Lines[0].GraphicVideoPosition == SceneObjectTimelineGraphicVideoPosition.OneSideFollow)
            {
                transform.LookAt(Camera.main.transform);
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }
            else if (LinesPath.Lines[0].VideoLookAt.HasValue)
            {
                transform.LookAt(LinesPath.Lines[0].VideoLookAt.Value);
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }

            if(Type == Assets.GSOT.Scripts.Models.ApiModels.Type.Video)
            {
                if(LinesPath.Lines[0].GraphicVideoPosition != SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
                {
                    transform.Rotate(90, 0, 0);
                }
            }
            return;
        }
        var s = Spline.Length * u;
        var data = Spline.GetPointAndTangentAtArcLength((float)s);
        var tan = ARLocationManager.Instance.gameObject.transform.InverseTransformVector(data.tangent);
        //tan.y = 0;
        if (!(float.IsNaN(data.point.magnitude) || float.IsNaN(data.point.x) || float.IsNaN(data.point.y) || float.IsNaN(data.point.z)))
        {
            //if (Current.Height.HasValue)
            //{
            //    data.point.y += (float)((Current.Height.Value * Scale) / 2);
            //}
            transform.position = data.point;
        }

        var groundY = 0.0f;
        if (groundHeight)
        {
            var position = transform.position;
            groundY = groundHeight.CurrentGroundY;
            position = MathUtils.SetY(position, position.y + groundY);
            transform.position = position;
        }

        if (LinesPath.Lines[0].GraphicVideoPosition.HasValue)
        {
            if (LinesPath.Lines[0].GraphicVideoPosition == SceneObjectTimelineGraphicVideoPosition.OneSideFollow
                || LinesPath.Lines[0].VideoLookAt.HasValue)
            {
                FacingToCamera = true;
            }
            else
            {
                FacingToCamera = false;
            }
        }

        if (FacingToCamera)
        {
            if (LinesPath.Lines[0].VideoLookAt.HasValue && LinesPath.Lines[0].GraphicVideoPosition != SceneObjectTimelineGraphicVideoPosition.OneSideFollow)
            {
                transform.LookAt(LinesPath.Lines[0].VideoLookAt.Value);
            }
            else
            {
                transform.LookAt(Camera.main.transform);
            }
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

            if (LinesPath.Lines[0].GraphicVideoPosition.HasValue)
            {
                if (LinesPath.Lines[0].GraphicVideoPosition.Value == SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
                {
                    //transform.Rotate(0, 90, 0);
                }
                else
                {
                    //transform.Rotate(90, 0, 0);
                }
            }
        }
        else
        {
            var a = Vector3.Angle(tan, oldTan);
            if (!startedMoving && InitialOrientation != Vector3.zero)
            {
                transform.LookAt(LocalInitialOrientation);
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }
            else if (tan != Vector3.zero && tan != oldTan && a < 150)
            {
                transform.localRotation = Quaternion.LookRotation(tan, Vector3.up);
                oldTan = tan;
            }
        }
        if (Type == Assets.GSOT.Scripts.Models.ApiModels.Type.Video)
        {
            if (LinesPath.Lines[0].GraphicVideoPosition != SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
            {
                transform.Rotate(90, 0, 0);
            }
        }

        u += (Current.Speed.Value * Time.deltaTime) / Spline.Length;

        if (u >= 1)
        {
            u = 0;
            Play = false;
        }

        useLineRenderer = Application.isEditor;// && false;
        if (useLineRenderer && Application.isEditor && Spline != null)
        {
            LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (!lineRenderer)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.useWorldSpace = true;
            var t = ARLocationManager.Instance.gameObject.transform;
            Spline.DrawCurveWithLineRenderer(lineRenderer,
                p => MathUtils.SetY(p, p.y + groundY));
        }
    }

    private void OnDestroy()
    {
        try
        {
            ARLocationProvider.Instance.OnLocationUpdatedDelegate -= LocationUpdated;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Ex: {ex.Message}");
        }
    }
}
