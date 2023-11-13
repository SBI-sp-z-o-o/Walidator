using ARLocation;
using Assets.GSOT.Scripts.Models.ApiModels;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GSOT;

namespace Assets.GSOT.Scripts.Models.Wrappers
{
    public class SceneObjectConverter
    {
        public SceneObject SceneObject { get; set; }
        public SceneObjectConverter(SceneObject obj)
        {
            SceneObject = obj;
        }

        public DataEntry ToDataEntry(Assets.GSOT.Scripts.Models.ApiModels.Type type)
        {
            Vector3 direction = new Vector3();
            if(SceneObject.OrientationPoint != null)
            {
                direction = new UnityEngine.Vector3((float)SceneObject.OrientationPoint?.Latitude, (float)SceneObject.OrientationPoint?.Altitude, (float)SceneObject.OrientationPoint?.Longitude);
            }
            return new DataEntry()
            {
                id = (int)SceneObject.Id,
                meshId = SceneObject.GsObjectId.ToString(),
                sceneId = SceneObject.GsSceneId,
                Type = type,
                path = this.GetPath(),
                FacingDirection = direction,
                IsFacingSpectator = SceneObject.IsFacingSpectator,
                IsAvailableInTableSceneUsingMode = SceneObject.IsAvailableInTableSceneUsingMode,
                description = SceneObject.description,
                pathLines = SceneObject.Timelines.OrderBy(x => x.StartTimeInSeconds).Select(x => new ObjectMovingController.PathLine()
                {
                    A = new ObjectLocation(x.StartLocalization.Latitude, x.StartLocalization.Longitude, x.StartLocalization.Altitude),
                    B = x.EndLocalization != null? new ObjectLocation(x.EndLocalization?.Latitude, x.EndLocalization?.Longitude, x.EndLocalization?.Altitude) : null,
                    Animation = x.AnimationIndex,
                    TimeToTravel = (int)x.EndTimeInSeconds,
                    Destroy = x.EndLocalization == null,
                    Caption = x.Caption,
                    CaptionLocalizationAltitude = x.CaptionLocalizationAltitude,
                    Vx = x.Vx,
                    Vy = x.Vy,
                    Vz = x.Vz,
                    Width = x.Width,
                    Height = x.Height,
                    GraphicVideoPosition = x.GraphicVideoPosition
                }).ToList()
            };
        }
        public List<ObjectLocation> GetPath()
        {
            var path = new List<ObjectLocation>();
            SceneObject.Timelines = SceneObject.Timelines.OrderBy(x => x.StartTimeInSeconds).ToList();
            try
            {
                path.Add(new ObjectLocation()
                {
                    Altitude = SceneObject.Timelines[0].StartLocalization.Altitude,
                    Longitude = SceneObject.Timelines[0].StartLocalization.Longitude,
                    TimeToReach = 0,
                    Latitude = SceneObject.Timelines[0].StartLocalization.Latitude,
                    AltitudeMode = AltitudeMode.GroundRelative,
                    AnimationIndex = SceneObject.Timelines[0].AnimationIndex
                });
                int timeToReach = 0;
                foreach (var item in SceneObject.Timelines)
                {
                    timeToReach = (int)item.EndTimeInSeconds;
                    if (item.EndLocalization != null)
                    {
                        path.Add(new ObjectLocation()
                        {
                            Altitude = item.EndLocalization.Altitude,
                            Latitude = item.EndLocalization.Latitude,
                            Longitude = item.EndLocalization.Longitude,
                            TimeToReach = timeToReach,
                            AltitudeMode = AltitudeMode.DeviceRelative,
                            AnimationIndex = item.AnimationIndex
                        });
                    }
                    else
                    {
                        path.Add(new ObjectLocation()
                        {
                            TimeToReach = timeToReach,
                            Destroy = true
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogWarning($"Ex: {ex.Message}");
            }

            return path;
        }
    }
}
