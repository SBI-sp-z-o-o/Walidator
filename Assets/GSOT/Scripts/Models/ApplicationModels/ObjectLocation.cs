using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.GSOT.Scripts.Models.ApplicationModels
{
    [Serializable]
    public class ObjectLocation : ARLocation.Location
    {
        public ObjectLocation(double? latitude = 0.0, double? longitude = 0.0, double? altitude = 0.0)
            : base(latitude ?? 0, longitude ?? 0, altitude ?? 0)
        {

        }
        [SerializeField]
        public int TimeToReach { get; set; }
        public bool Destroy { get; set; }
        [SerializeField]
        public int? AnimationIndex { get; set; }

        public ObjectLocation() { }
        public ObjectLocation(ARLocation.Location location)
            : base(location.Latitude, location.Longitude, location.Altitude)
        {

        }

        public ObjectLocation(ObjectLocation location)
            : base(location.Latitude, location.Longitude, location.Altitude)
        {
            TimeToReach = location.TimeToReach;
            Destroy = location.Destroy;
            AnimationIndex = location.AnimationIndex;
        }
    }
}
