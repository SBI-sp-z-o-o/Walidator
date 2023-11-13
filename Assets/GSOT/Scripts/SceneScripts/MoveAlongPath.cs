using System;
using UnityEngine;
using UnityEngine.Serialization;
using Assets.GSOT.Scripts.Models.ApplicationModels;
// ReSharper disable UnusedMember.Global

namespace GSOT
{
    using System.Collections.Generic;
    using System.Linq;
    using ARLocation;
    using ARLocation.UI;
    using ARLocation.Utils;
    using Assets.GSOT;
    using Assets.GSOT.Scripts.LoadingScripts;
    using Assets.GSOT.Scripts.Utils;

    /// <summary>
    /// This component, when attached to a GameObject, makes it traverse a
    /// path that interpolates a given set of geographical locations.
    /// </summary>
    [AddComponentMenu("AR+GPS/Move Along Path")]
    [HelpURL("https://http://docs.unity-ar-gps-location.com/guide/#movealongpath")]
    [DisallowMultipleComponent]
    public class MoveAlongPath : MonoBehaviour
    {
        [Serializable]
        public class PathSettingsData
        {
            /// <summary>
            /// The LocationPath describing the path to be traversed.
            /// </summary>
            [Tooltip("The LocationPath describing the path to be traversed.")]
            public LocationPath LocationPath;

            /// <summary>
            /// The number of points-per-segment used to calculate the spline.
            /// </summary>
            [Tooltip("The number of points-per-segment used to calculate the spline.")]
            public int SplineSampleCount = 500;

            /// <summary>
            /// If present, renders the spline in the scene using the given line renderer.
            /// </summary>
            [FormerlySerializedAs("lineRenderer")]
            [Tooltip("If present, renders the spline in the scene using the given line renderer.")]
            public LineRenderer LineRenderer;
        }

        [Serializable]
        public class PlaybackSettingsData
        {
            /// <summary>
            /// The speed along the path.
            /// </summary>
            [Tooltip("The speed along the path.")]
            public float Speed = 1.0f;

            /// <summary>
            /// The up direction to be used for orientation along the path.
            /// </summary>
            [Tooltip("The up direction to be used for orientation along the path.")]
            public Vector3 Up = Vector3.up;

            /// <summary>
            /// If true, play the path traversal in a loop.
            /// </summary>
            [Tooltip("If true, play the path traversal in a loop.")]
            public bool Loop = false;

            /// <summary>
            /// If true, start playing automatically.
            /// </summary>
            [Tooltip("If true, start playing automatically.")]
            public bool AutoPlay = true;

            [FormerlySerializedAs("offset")]
            [Tooltip("The parameters offset; marks the initial position of the object along the curve.")]
            public float Offset;
        }

        [Serializable]
        public class PlacementSettingsData
        {
            [Tooltip("The altitude mode. The altitude modes of the individual path locations are ignored, and this will be used instead.")]
            public AltitudeMode AltitudeMode = AltitudeMode.DeviceRelative;

            [Tooltip(
                "The maximum number of times this object will be affected by GPS location updates. Zero means no limits are imposed.")]
            public uint MaxNumberOfLocationUpdates = 4;
        }

        [Serializable]
        public class StateData
        {
            public uint UpdateCount;
            public Vector3[] Points;
            public int PointCount;
            public bool Playing;
            public Spline Spline;
            public Vector3 Translation;
            public float Speed;
            public float TraveledDist;
        }

        private class PathPart
        {
            public int Index;
            public float? Speed;
            public double Distance;
            public bool Destroy;
            public int? Animation;
            public Vector3 PointA;
            public Vector3 PointB;

            public override string ToString()
            {
                return $"Index:{Index} Distance:{Distance} Speed:{Speed}";
            }
        }

        public PathSettingsData PathSettings = new PathSettingsData();
        public PlaybackSettingsData PlaybackSettings = new PlaybackSettingsData();
        public PlacementSettingsData PlacementSettings = new PlacementSettingsData();

        public float Speed
        {
            get => state.Speed;
            set => state.Speed = value;
        }

        [Space(4.0f)]

        [Header("Debug")]
        [Tooltip("When debug mode is enabled, this component will print relevant messages to the console. Filter by 'MoveAlongPath' in the log output to see the messages.")]
        public bool DebugMode;

        [Space(4.0f)]

        public StateData state = new StateData();
        public ARLocationProvider locationProvider;
        private double u;
        public GameObject arLocationRoot;
        public Transform mainCameraTransform;
        private bool useLineRenderer;
        private bool hasInitialized;
        private GroundHeight groundHeight;
        private List<PathPart> SpeedAtParts;
        public float Scale { get; set; } = 1;

        private bool HeightRelativeToDevice => PlacementSettings.AltitudeMode == AltitudeMode.DeviceRelative;
        private bool HeightGroundRelative => PlacementSettings.AltitudeMode == AltitudeMode.GroundRelative;

        /// <summary>
        /// Change the `LocationPath` the GameObject will traverse.
        /// </summary>
        /// <param name="path"></param>
        public void SetLocationPath(LocationPath path)
        {
            PathSettings.LocationPath = path;

            locationProvider = ARLocationProvider.Instance;

            mainCameraTransform = ARLocationManager.Instance.MainCamera.transform;
            arLocationRoot = ARLocationManager.Instance.gameObject;
            state.PointCount = PathSettings.LocationPath.Locations.Length;
            state.Points = new Vector3[state.PointCount];

            BuildSpline();
        }

        void Start()
        {
            if (transform.childCount < 1)
            {
                return;
            }

            if (PathSettings.LocationPath == null)
            {
                throw new NullReferenceException("[AR+GPS][MoveAlongPath]: Null Path! Please set the 'LocationPath' property!");
            }
            CalculatePartsSpeed();

            locationProvider.OnLocationUpdatedEvent(LocationUpdated);

            Initialize();
            hasInitialized = true;
        }

        public void CalculatePartsSpeed()
        {
            SpeedAtParts = new List<PathPart>();
            int currentTime = 0;
            int currentPart = 1;
            var loc = PathSettings.LocationPath.Locations[0] as ObjectLocation; //starting point
            while (true)
            {
                var destination = GetNext(currentPart);
                if (destination == null)
                {
                    break;
                }
                if (destination.Destroy)
                {
                    SpeedAtParts.Add(new PathPart()
                    {
                        Index = currentPart,
                        Speed = -1,
                        Destroy = true
                    });
                    break;
                }

                Vector3 pointA = new Vector3();
                Vector3 pointB = new Vector3();

                if (ModelsQueue.IsTableScene)
                {

                    //pointA = CalculateTablePoint(Location.GetGameObjectPositionForLocation(arLocationRoot.transform,
                    //    mainCameraTransform, ModelsQueue.TableTransformPosition, loc, HeightRelativeToDevice || HeightGroundRelative));
                    //pointB = CalculateTablePoint(Location.GetGameObjectPositionForLocation(arLocationRoot.transform,
                    //    mainCameraTransform, ModelsQueue.TableTransformPosition, destination, HeightRelativeToDevice || HeightGroundRelative));
                }
                else
                {
                    var location = locationProvider.CurrentLocation.ToLocation();
                    pointA = Location.GetGameObjectPositionForLocation(arLocationRoot.transform,
                        mainCameraTransform, location, loc, HeightRelativeToDevice || HeightGroundRelative);
                    pointB = Location.GetGameObjectPositionForLocation(arLocationRoot.transform,
                        mainCameraTransform, location, destination, HeightRelativeToDevice || HeightGroundRelative);
                }

                var dist = Vector3.Distance(pointA, pointB);

                var distance = dist;// LocationUtils.Distance(loc, destination);
                if (distance == 0) distance = 0.01f;
                //time to reach location - time i have already spent
                var timeToSpend = destination.TimeToReach - currentTime;
                float speed = 0;
                if (timeToSpend != 0)
                {
                    speed = (float)(distance / timeToSpend);
                }
                SpeedAtParts.Add(new PathPart()
                {
                    Index = currentPart,
                    Distance = distance,
                    Speed = speed,
                    Animation = (PathSettings.LocationPath.Locations[currentPart] as ObjectLocation).AnimationIndex,
                    PointA = pointA,
                    PointB = pointB
                });

                currentPart++;
                currentTime += timeToSpend;
                //destination becomes first location, so we calculated speed needed to reach next location
                loc = destination;
            }
        }

        private PathPart WhatShouldBeMySpeed(double traveledDistance)
        {
            int index = 0;
            var current = SpeedAtParts.ElementAtOrDefault(index);
            if (current != null)
            {
                double distanceSum = 0d;
                while (true)
                {
                    if (current.Destroy)
                    {
                        var line = GameObject.Find(gameObject.GetInstanceID() + "_text");
                        if (line != null)
                        {
                            Destroy(line);
                        }
                        Destroy(gameObject);
                        DebugMode = false;
                    }
                    AnimationListComponent animation = null;
                    distanceSum += current.Distance;
                    if (traveledDistance <= distanceSum)
                    {
                        animation = gameObject.GetComponentInChildren<AnimationListComponent>();
                        if (animation != null)
                        {
                            animation.PlayAnimation(current.Animation.ToString());
                        }
                        return current;
                    }
                    index++;
                    if (SpeedAtParts.ElementAtOrDefault(index) != null)
                    {
                        current = SpeedAtParts[index];
                        continue;
                    }
                    animation = gameObject.GetComponentInChildren<AnimationListComponent>();
                    if (animation != null)
                    {
                        animation.PlayAnimation(current.Animation.ToString());
                    }
                    return current;
                }
            }
            return null;
        }

        private ObjectLocation GetNext(int current)
        {
            try
            {
                return PathSettings.LocationPath.Locations[current] as ObjectLocation;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Ex: {ex.Message}");
                return null;
            }
        }

        private void Initialize()
        {
            if (DebugMode)
            {
                gameObject.AddComponent<ModelDebugDistance>();
            }
            state.PointCount = PathSettings.LocationPath.Locations.Length;
            state.Points = new Vector3[state.PointCount];
            state.Speed = PlaybackSettings.Speed;

            useLineRenderer = PathSettings.LineRenderer != null;

            transform.SetParent(ARLocationManager.Instance.gameObject.transform);

            state.Playing = PlaybackSettings.AutoPlay;

            u += PlaybackSettings.Offset;

            groundHeight = GetComponent<GroundHeight>();
            if (PlacementSettings.AltitudeMode == AltitudeMode.GroundRelative)
            {
                if (!groundHeight)
                {
                    groundHeight = gameObject.AddComponent<GroundHeight>();
                    groundHeight.Settings.DisableUpdate = true;
                }
            }
            else
            {
                if (groundHeight)
                {
                    Destroy(groundHeight);
                    groundHeight = null;
                }
            }

            if (!hasInitialized)
            {
                locationProvider.OnProviderRestartEvent(ProviderRestarted);
            }

            if (locationProvider.IsEnabled)
            {
                LocationUpdated(locationProvider.CurrentLocation, locationProvider.LastLocation);
            }
        }

        private void ProviderRestarted()
        {
            state.UpdateCount = 0;
        }

        public void BuildSpline()
        {
            var location = locationProvider.CurrentLocation.ToLocation();
            location.Altitude = 0;
            if (state.PointCount == 0)
            {
                state.PointCount = PathSettings.LocationPath.Locations.Length;
                state.Points = new Vector3[state.PointCount];
            }
            for (var i = 0; i < state.PointCount; i++)
            {
                var loc = PathSettings.LocationPath.Locations[i];

                state.Points[i] = Location.GetGameObjectPositionForLocation(arLocationRoot.transform,
                    mainCameraTransform, location, loc, HeightRelativeToDevice || HeightGroundRelative);

                state.Points[i].x += 0.01f * i;
                state.Points[i].z += 0.01f * i;

                Logger.LogFromMethod("MoveAlongPath", "BuildSpline", $"({gameObject.name}): Points[{i}] = {state.Points[i]},  geo-location = {loc}", DebugMode);
            }
            if (ModelsQueue.IsTableScene)
            {
                for (var i = 0; i < state.PointCount; i++)
                {
                    var loc = PathSettings.LocationPath.Locations[i];

                    state.Points[i] = Location.GetGameObjectPositionForLocation(arLocationRoot.transform,
                        mainCameraTransform, ModelsQueue.TableTransformPosition, loc, HeightRelativeToDevice || HeightGroundRelative);

                    state.Points[i].x += 0.01f * i;
                    state.Points[i].z += 0.01f * i;
                }
                //var newPoints = CalculateTablePoints(state.Points.ToList(), Vector3.Distance(tablePoint1, tablePoint2) / Vector3.Distance(apiPoint1, apiPoint2));
                //foreach(var p in newPoints)
                //{
                //    p.y = tablePoint1.y;
                //}
                //state.Points = newPoints.ToArray();
                Scale = Vector3.Distance(tablePoint1, tablePoint2) / Vector3.Distance(apiPoint1, apiPoint2);
                var localScale = this.gameObject.transform.localScale;
                localScale.x *= Scale < 0.3f? 0.3f : Scale;
                localScale.y *= Scale < 0.3f ? 0.3f : Scale;
                localScale.z *= Scale < 0.3f ? 0.3f : Scale;
                this.gameObject.transform.localScale = localScale;
            }
            state.Spline = Misc.BuildSpline(PathSettings.LocationPath.SplineType, state.Points, PathSettings.SplineSampleCount, PathSettings.LocationPath.Alpha);
            //InitLine();
        }

        Vector3 tablePoint1
        {
            get
            {
                return ModelsQueue.TablePoint1;
            }
        }
        Vector3 tablePoint2
        {
            get
            {
                return ModelsQueue.TablePoint2;
            }
        }
        Vector3 apiPoint1
        {
            get
            {
                return ModelsQueue.ApiPoint1;
            }
        }
        Vector3 apiPoint2
        {
            get
            {
                return ModelsQueue.ApiPoint2;
            }
        }

        //public List<Vector3> CalculateTablePoints(List<Vector3> points, float scale)
        //{
        //    var result = new List<Vector3>();
        //    var a = LocationUtils.A(tablePoint1.x, tablePoint1.z, tablePoint2.x, tablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
        //    var b = LocationUtils.B(tablePoint1.x, tablePoint1.z, tablePoint2.x, tablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
        //    var c = LocationUtils.C(tablePoint1.x, tablePoint1.z, tablePoint2.x, tablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
        //    var d = LocationUtils.D(tablePoint1.x, tablePoint1.z, tablePoint2.x, tablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);

        //    foreach (var point in points)
        //    {
        //        var newX = c + b * point.x - a * point.z;
        //        var newZ = d + a * point.x + b * point.z;

        //        result.Add(new Vector3((float)newX, tablePoint1.y + (point.y * scale), (float)newZ));
        //    }

        //    return result;
        //}

        //public Vector3 CalculateTablePoint(Vector3 point)
        //{
        //    var a = LocationUtils.A(tablePoint1.x, tablePoint1.z, tablePoint2.x, tablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
        //    var b = LocationUtils.B(tablePoint1.x, tablePoint1.z, tablePoint2.x, tablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
        //    var c = LocationUtils.C(tablePoint1.x, tablePoint1.z, tablePoint2.x, tablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
        //    var d = LocationUtils.D(tablePoint1.x, tablePoint1.z, tablePoint2.x, tablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);

        //    var newX = c + b * point.x - a * point.z;
        //    var newZ = d + a * point.x + b * point.z;

        //    return new Vector3((float)newX, tablePoint1.y, (float)newZ);
        //}

        public static double A(double xx1, double yy1, double xx2, double yy2, double x1, double y1, double x2, double y2)
        {
            var a = -(x1 * (yy2 - yy1) - x2 * yy2 + x2 * yy1 + xx2 * y2 - xx1 * y2 + (xx1 - xx2) * y1)
                / (Math.Pow(y2, 2d) - 2 * y1 * y2 + Math.Pow(y1, 2) + Math.Pow(x2, 2) - 2 * x1 * x2 + Math.Pow(x1, 2));

            return a;
        }
        public static double B(double xx1, double yy1, double xx2, double yy2, double x1, double y1, double x2, double y2)
        {
            var b = (y2 * yy2 + y1 * (yy1 - yy2) - y2 * yy1 + x2 * xx2 - x1 * xx2 + (x1 - x2) * xx1)
                / (Math.Pow(y2, 2) - 2 * y1 * y2 + Math.Pow(y1, 2) + Math.Pow(x2, 2) - 2 * x1 * x2 + Math.Pow(x1, 2));
            return b;
        }
        public static double C(double xx1, double yy1, double xx2, double yy2, double x1, double y1, double x2, double y2)
        {
            var c = (x1 * (-y2 * yy2 + y2 * yy1 - x2 * xx2) + y1 * (x2 * yy2 - x2 * yy1 - xx2 * y2 - xx1 * y2) + xx1 * (Math.Pow(y2, 2) + Math.Pow(x2, 2) - x1 * x2) + xx2 * Math.Pow(y1, 2) + Math.Pow(x1, 2) * xx2)
                / (Math.Pow(y2, 2) - 2 * y1 * y2 + Math.Pow(y1, 2) + Math.Pow(x2, 2) - 2 * x1 * x2 + Math.Pow(x1, 2));
            return c;
        }
        public static double D(double xx1, double yy1, double xx2, double yy2, double x1, double y1, double x2, double y2)
        {
            var d = (y1 * (-y2 * yy2 - y2 * yy1 - x2 * xx2 + x2 * xx1) + x1 * (-x2 * yy2 - x2 * yy1 + xx2 * y2) + Math.Pow(y1, 2) * yy2 + Math.Pow(x1, 2) * yy2 + (Math.Pow(y2, 2) + Math.Pow(x2, 2)) * yy1 - x1 * xx1 * y2) / (Math.Pow(y2, 2) - 2 * y1 * y2 + Math.Pow(y1, 2) + Math.Pow(x2, 2) - 2 * x1 * x2 + Math.Pow(x1, 2));
            return d;
        }

        private void LocationUpdated(LocationReading location, LocationReading _)
        {
            Logger.LogFromMethod("MoveAlongPath", "LocationUpdated", $"({gameObject.name}): New device location {location}", DebugMode);

            if (PlacementSettings.MaxNumberOfLocationUpdates > 0 && state.UpdateCount > PlacementSettings.MaxNumberOfLocationUpdates)
            {
                Logger.LogFromMethod("MoveAlongPath", "LocationUpdated", $"({gameObject.name}): Max number of updates reached! returning", DebugMode);
                return;
            }
            BuildSpline();
            state.Translation = new Vector3(0, 0, 0);

            state.UpdateCount++;
        }
        private Quaternion LookAt = new Quaternion();
        private void Update()
        {
            if (!state.Playing)
            {
                return;
            }

            // If there is no location provider, or spline, do nothing
            if (state.Spline == null || !locationProvider.IsEnabled)
            {
                locationProvider.Resume();
                return;
            }
            if (!Assets.GSOT.Scripts.LoadingScripts.ModelsQueue.SceneStarted)
            {
                Assets.GSOT.Scripts.LoadingScripts.ModelsQueue.SceneStarted = true;
            }

            if (!SpeedAtParts.Any())
            {
                return;
            }
            // Get spline point at current parameter
            var s = state.Spline.Length * u;

            var data = state.Spline.GetPointAndTangentAtArcLength((float)s);
            data.tangent.y = 0;
            var tan = arLocationRoot.transform.InverseTransformVector(data.tangent);
            tan.y = 0;
            var speed = WhatShouldBeMySpeed(s);
            if (!speed.Speed.HasValue)
            {
                return;
            }

            if (float.IsNaN(data.point.magnitude) || float.IsNaN(data.point.x) || float.IsNaN(data.point.y) || float.IsNaN(data.point.z))
            {
                transform.position = transform.position;
            }
            else
            {
                if (Vector3.Distance(speed.PointA, speed.PointB) < 0.1f)
                {
                    transform.position = new Vector3(speed.PointA.x, data.point.y, speed.PointA.z);
                }
                else
                {
                    transform.position = data.point;
                    LookAt = new Quaternion();
                }
            }

            var groundY = 0.0f;
            if (groundHeight)
            {
                var position = transform.position;
                groundY = groundHeight.CurrentGroundY;
                position = MathUtils.SetY(position, position.y + groundY);
                transform.position = position;
            }

            // Set orientation
            float timeDelta = Time.deltaTime;

            if (LookAt.x == 0 && LookAt.y == 0)//itd
            {
                var cel = speed.PointB;
                cel.y = transform.position.y;
                transform.LookAt(cel);
                LookAt = transform.rotation;
            }
            else
            {
                transform.localRotation = Quaternion.LookRotation(tan, PlaybackSettings.Up);
            }

            state.Speed = speed.Speed.Value;
            u += (speed.Speed.Value * timeDelta) / state.Spline.Length;

            //
            //var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            //var type = assembly.GetType("UnityEditor.LogEntries");
            //var method = type.GetMethod("Clear");
            //method.Invoke(new object(), null);
            //Logger.LogFromMethod("MoveAlongPath", "Update", $"Punkt B: {speed.PointB.ToString()}", true);

            if (u >= 1 && !PlaybackSettings.Loop)
            {
                u = 0;
                state.Playing = false;
            }
            else
            {
                u = u % 1.0f;
            }

            // If there is a line renderer, render the path
            //useLineRenderer = true;
            if (useLineRenderer && Application.isEditor)
            {
                if (PathSettings.LineRenderer == null)
                {
                    PathSettings.LineRenderer = gameObject.AddComponent<LineRenderer>();
                }
                PathSettings.LineRenderer.startWidth = 0.01f;
                PathSettings.LineRenderer.endWidth = 0.01f;
                PathSettings.LineRenderer.useWorldSpace = true;
                var t = arLocationRoot.transform;
                state.Spline.DrawCurveWithLineRenderer(PathSettings.LineRenderer,
                    p => MathUtils.SetY(p, p.y + groundY)); //t.TransformVector(p - state.Translation));
            }
        }

        private void OnDestroy()
        {
            try
            {
                locationProvider.OnLocationUpdatedDelegate -= LocationUpdated;
                locationProvider.OnRestartDelegate -= ProviderRestarted;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Ex: {ex.Message}");
            }
        }
    }
}
