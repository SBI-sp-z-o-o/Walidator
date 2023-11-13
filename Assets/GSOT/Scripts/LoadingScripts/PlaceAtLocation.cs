using System;
using ARLocation.UI;
using UnityEngine;
using UnityEngine.Events;
using Assets.GSOT.Scripts.Models.ApplicationModels;

namespace GSOT
{
    using ARLocation;
    using ARLocation.Utils;
    using Assets.GSOT;
    using Assets.GSOT.Scripts.LoadingScripts;
    using Assets.GSOT.Scripts.SceneScripts;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using TriLib;
    using UnityEngine.Networking;
    using static ARLocation.WebMapLoader;

    [Serializable]
    public class OverrideAltitudeData
    {
        [Tooltip("If true, override the LocationData's altitude options.")]
        public bool OverrideAltitude;

        [Tooltip("The override altitude value.")]
        public double Altitude;

        [Tooltip("The override altitude mode.")]
        public AltitudeMode AltitudeMode = AltitudeMode.GroundRelative;
    }

    [Serializable]
    public class LocationPropertyData
    {
        [Serializable]
        public enum LocationPropertyType
        {
            Location,
            LocationData
        }

        [Tooltip("The type of location coordinate input used. Either 'Location' to directly input location " +
                 "coordinates, or 'LocationData' to use a ScriptableObject.")]
        public LocationPropertyType LocationInputType = LocationPropertyType.Location;

        [Tooltip("A LocationData ScriptableObject storing the desired GPS coordinates to place the object.")]
        public LocationData LocationData;

        [Tooltip("Input the desired GPS coordinates here.")]
        public Location Location = new Location();

        [Tooltip("Use this to override the LocationData's altitude options.")]
        public OverrideAltitudeData OverrideAltitudeData = new OverrideAltitudeData();
    }

    /// <summary>
    /// Apply to a GameObject to place it at a specified geographic location.
    /// </summary>
    [AddComponentMenu("AR+GPS/Place At Location")]
    [HelpURL("https://http://docs.unity-ar-gps-location.com/guide/#placeatlocation")]
    [DisallowMultipleComponent]
    public class PlaceAtLocation : MonoBehaviour
    {
        [Serializable]
        public class ObjectUpdatedEvent : UnityEvent<GameObject, Location, int>
        {
        }

        [Serializable]
        public class PlaceAtOptions
        {
            [Tooltip(
                 "The smoothing factor for movement due to GPS location adjustments; if set to zero it is disabled."),
             Range(0, 1)]
            public float MovementSmoothing = 0.05f;

            [Tooltip(
                "The maximum number of times this object will be affected by GPS location updates. Zero means no limits are imposed.")]
            public int MaxNumberOfLocationUpdates = 4;

            [Tooltip("If true, use a moving average filter.")]
            public bool UseMovingAverage;

            [Tooltip(
                "If true, the object will be hidden until the object is placed at the geolocation. If will enable/disable the MeshRenderer or SkinnedMeshRenderer " +
                "when available, and enable/disable all child game objects.")]
            public bool HideObjectUntilItIsPlaced = true;
        }

        [Serializable]
        public class LocationSettingsData
        {
            public LocationPropertyData LocationInput = new LocationPropertyData();

            public Location GetLocation()
            {
                Location location;

                if (LocationInput.LocationInputType ==
                    LocationPropertyData.LocationPropertyType.LocationData)
                {
                    if (LocationInput.LocationData == null)
                    {
                        Debug.LogWarning("[AR+GPS][LocationSettingsData#GetLocation]: " +
                                         "Null LocationData; falling back to Location. When using `Location Input Type = Location Data` " +
                                         "make sure you associate a LocationData ScriptableObject to it.");

                        location = LocationInput.Location.Clone();
                    }
                    else
                    {
                        location = LocationInput.LocationData.Location.Clone();

                        if (LocationInput.OverrideAltitudeData.OverrideAltitude)
                        {
                            location.Altitude = LocationInput.OverrideAltitudeData.Altitude;
                            location.AltitudeMode = LocationInput.OverrideAltitudeData.AltitudeMode;
                        }
                    }
                }
                else
                {
                    location = LocationInput.Location.Clone();
                }

                return location;
            }
        }

        [Serializable]
        public class StateData
        {
            public Location Location;
            public uint LocationUpdatedCount;
            public uint PositionUpdatedCount;
            public bool Paused;
        }

        public LocationSettingsData LocationOptions = new LocationSettingsData();

        [Space(4.0f)] public PlaceAtOptions PlacementOptions = new PlaceAtOptions();

        [Space(4.0f)]

        [Header("Debug")]
        [Tooltip("When debug mode is enabled, this component will print relevant messages to the console. Filter by 'PlateAtLocation' in the log output to see the messages. It will also " +
                 "display the direction from the user to the object on the screen, as well as a line renderer from the camera to the object location. To customize how this line looks, add " +
                 "a Line Renderer component to this game object.")]
        public bool DebugMode;

        [Space(4.0f)]

        [Header("Events")]
        [Space(4.0f)]
        [Tooltip(
            "Event called when the object's location is updated. The arguments are the current GameObject, the location, and the number of location updates received " +
            "by the object so far.")]
        public ObjectUpdatedEvent ObjectLocationUpdated = new ObjectUpdatedEvent();

        [Tooltip(
            "Event called when the object's position is updated after a location update. " +
            "If the Movement Smoothing is larger than 0, this will fire at a later time than the Location Updated event.  The arguments are the current GameObject, the location, and the number of position updates received " +
            "by the object so far.")]
        public ObjectUpdatedEvent ObjectPositionUpdated = new ObjectUpdatedEvent();


        public Location Location
        {
            get => state.Location;

            set
            {
                if (!hasInitialized)
                {
                    LocationOptions.LocationInput.LocationInputType =
                        LocationPropertyData.LocationPropertyType.Location;

                    LocationOptions.LocationInput.LocationData = null;
                    LocationOptions.LocationInput.Location = value.Clone();

                    return;
                }

                if (groundHeight != null)
                {
                    groundHeight.Settings.Altitude = (float)value.Altitude;
                }

                state.Location = value.Clone();
                UpdatePosition(true);
            }
        }

        public float SceneDistance
        {
            get
            {
                var cameraPos = mainCameraTransform.position;

                return Vector3.Distance(cameraPos, transform.position);
            }
        }

        public double RawGpsDistance =>
            Location.HorizontalDistance(locationProvider.Provider.CurrentLocationRaw.ToLocation(),
                state.Location);

        public bool Paused
        {
            get => state.Paused;
            set => state.Paused = value;
        }

        public StateData state = new StateData();

        private ARLocationProvider locationProvider;
        private Transform arLocationRoot;
        private SmoothMove smoothMove;
        private MovingAveragePosition movingAverageFilter;
        private GameObject debugPanel;
        private ARLocationManager arLocationManager;
        public Transform mainCameraTransform;
        private bool hasInitialized;
        private GroundHeight groundHeight;

        // Use this for initialization
        void Start()
        {
            locationProvider = ARLocationProvider.Instance;
            arLocationManager = ARLocationManager.Instance;
            arLocationRoot = arLocationManager.gameObject.transform;
            mainCameraTransform = arLocationManager.MainCamera.transform;

            if (locationProvider == null)
            {
                Debug.LogError("[AR+GPS][PlaceAtLocation]: LocationProvider GameObject or Component not found.");
                return;
            }

            Initialize();

            hasInitialized = true;
        }

        public void Restart()
        {
            Logger.LogFromMethod("PlaceAtLocation", "Restart", $"({gameObject.name}) - Restarting!", DebugMode);

            RemoveLocationProviderListeners();

            state = new StateData();
            Initialize();

            if (locationProvider.IsEnabled)
            {
                locationUpdatedHandler(locationProvider.CurrentLocation, locationProvider.LastLocation);
            }
        }

        void Initialize()
        {
            state.Location = LocationOptions.GetLocation();

            Transform transform1;
            (transform1 = transform).SetParent(arLocationRoot.transform);
            transform1.localPosition = Vector3.zero;

            if (!hasInitialized)
            {
                if (PlacementOptions.HideObjectUntilItIsPlaced)
                {
                    //Misc.HideGameObject(gameObject);
                }

                if (PlacementOptions.MovementSmoothing > 0)
                {
                    smoothMove = SmoothMove.AddSmoothMove(gameObject, PlacementOptions.MovementSmoothing);
                }

                if (PlacementOptions.UseMovingAverage)
                {
                    movingAverageFilter = new MovingAveragePosition
                    {
                        aMax = locationProvider.Provider.Options.AccuracyRadius > 0
                            ? locationProvider.Provider.Options.AccuracyRadius
                            : 20
                    };
                }

                if (DebugMode)
                {
                    gameObject.AddComponent<ModelDebugDistance>();
                }

                RegisterLocationProviderListeners();
            }

            Logger.LogFromMethod("PlaceAtLocation", "Initialize", $"({gameObject.name}) initialized object with geo-location {state.Location}", DebugMode);
        }

        private void RegisterLocationProviderListeners()
        {
            locationProvider.OnLocationUpdatedEvent(locationUpdatedHandler);
            locationProvider.OnProviderRestartEvent(ProviderRestarted);
        }

        private void RemoveLocationProviderListeners()
        {
            locationProvider.OnLocationUpdatedDelegate -= locationUpdatedHandler;
            locationProvider.OnRestartDelegate -= ProviderRestarted;
        }

        private void ProviderRestarted()
        {
            Logger.LogFromMethod("PlaceAtLocation", "ProviderRestarted", $"({gameObject.name})", DebugMode);

            state.LocationUpdatedCount = 0;
            state.PositionUpdatedCount = 0;
        }

        private void locationUpdatedHandler(LocationReading currentLocation, LocationReading lastLocation)
        {
            UpdatePosition();
        }

        public void UpdatePosition(bool forceUpdate = false)
        {
            var deviceLocation = locationProvider.CurrentLocation.ToLocation();
            deviceLocation.Altitude = 0;
            if (state.Paused)
            {
                Logger.LogFromMethod("PlaceAtLocation", "UpdatePosition", $"({gameObject.name}): Updates are paused; returning", DebugMode);
                return;
            }

            Vector3 targetPosition;
            var location = state.Location;
            var useSmoothMove = smoothMove != null;
            var isHeightRelative = location.AltitudeMode == AltitudeMode.DeviceRelative;
            // If we have reached the max number of location updates, do nothing
            if ((PlacementOptions.MaxNumberOfLocationUpdates > 0) &&
                (state.LocationUpdatedCount >= PlacementOptions.MaxNumberOfLocationUpdates) && !forceUpdate)
            {
                return;
            }

            // Calculate the target position where the object will be placed next
            if (movingAverageFilter != null)
            {
                var position = Location.GetGameObjectPositionForLocation(
                    arLocationRoot, mainCameraTransform, deviceLocation, location, isHeightRelative
                );

                var accuracy = locationProvider.CurrentLocation.accuracy;

                movingAverageFilter.AddEntry(new DVector3(position), accuracy);

                targetPosition = movingAverageFilter.CalculateAveragePosition().toVector3();
            }
            else
            {
                targetPosition = Location.GetGameObjectPositionForLocation(
                    arLocationRoot, mainCameraTransform, deviceLocation, location, isHeightRelative
                );
            }

            if (useSmoothMove && state.PositionUpdatedCount > 0)
            {
                smoothMove.Move(targetPosition, PositionUpdated);
            }
            else
            {
                transform.position = targetPosition;
                PositionUpdated();
            }

            state.LocationUpdatedCount++;
            ObjectLocationUpdated?.Invoke(gameObject, location, (int)state.LocationUpdatedCount);
        }

        private void PositionUpdated()
        {
            if (PlacementOptions.HideObjectUntilItIsPlaced && state.PositionUpdatedCount <= 0)
            {
                Misc.ShowGameObject(gameObject);
            }

            state.PositionUpdatedCount++;

            Logger.LogFromMethod("PlaceAtLocation", "PositionUpdated", $"({gameObject.name}): Object position updated! PositionUpdatedCount = {state.PositionUpdatedCount}, transform.position = {transform.position}", DebugMode);

            ObjectPositionUpdated?.Invoke(gameObject, state.Location, (int)state.PositionUpdatedCount);
        }


        private List<KeyValuePair<DateTime, int?>> animations = new List<KeyValuePair<DateTime, int?>>();
        //private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        public void Update()
        {
            if (destroy && DateTime.Now >= destroyTime)
            {
                var line = GameObject.Find(gameObject.GetInstanceID() + "_text");
                if (line != null)
                {
                    Destroy(line);
                }
                Destroy(gameObject);
            }
            else
            {
                animations = animations.OrderBy(x => x.Key).ToList();
                var currentAnimation = animations.FirstOrDefault(x => x.Key >= DateTime.Now);



                if (!currentAnimation.Value.HasValue)
                {
                    return;
                }
                var animation = gameObject.GetComponentInChildren<AnimationListComponent>();
                if (animation != null)
                {
                    animation.PlayAnimation(currentAnimation.Value.ToString());
                }
            }
        }
        private bool destroy = false;
        private DateTime destroyTime;

        public static GameObject CreatePlacedInstance(GSOT.DataEntry entry, GameObject go, Location location, PlaceAtOptions options, bool useDebugMode = false, List<ObjectLocation> path = null)
        {
            var movingComponent = go.AddComponent<ObjectMovingController>();
            movingComponent.Type = entry.Type;
            var locPath = ScriptableObject.CreateInstance<LocationPath>();
            locPath.Locations = path.ToArray();
            movingComponent.LinesPath = new ObjectMovingController.Path()
            {
                Lines = entry.pathLines.Select(x => new ObjectMovingController.PathLine(x)).ToList()
            };
            movingComponent.InitialOrientation = entry.FacingDirection;
            movingComponent.FacingToCamera = entry.IsFacingSpectator;
            movingComponent.SetLocationPath(locPath);
            movingComponent.DebugMode = useDebugMode;
            movingComponent.Play = true;
            movingComponent.CalculatePartsSpeed();
            return go;
        }

        static bool LocationsTheSame(List<ObjectLocation> locations)
        {
            if (locations.Count < 2)
            {
                return true;
            }
            if (locations[locations.Count - 1].Latitude == locations[locations.Count - 1].Longitude && locations[locations.Count - 1].Latitude == 0)
            {
                return true;
            }
            var first = locations.First();
            bool result = true;
            foreach (var location in locations)
            {
                result = location.Latitude == first.Latitude && location.Longitude == first.Longitude && location.Altitude == first.Altitude;
                if (!result)
                {
                    break;
                }
            }
            return result;
        }

        public static PlaceAtLocation AddPlaceAtComponent(GameObject go, Location location, PlaceAtOptions options,
            bool useDebugMode = false)
        {
            var placeAt = go.AddComponent<PlaceAtLocation>();

            placeAt.PlacementOptions = options;
            placeAt.LocationOptions.LocationInput.LocationInputType =
                LocationPropertyData.LocationPropertyType.Location;
            placeAt.LocationOptions.LocationInput.Location = location.Clone();
            placeAt.DebugMode = useDebugMode;

            return placeAt;
        }

        private void OnDestroy()
        {
            RemoveLocationProviderListeners();
        }
    }
}
