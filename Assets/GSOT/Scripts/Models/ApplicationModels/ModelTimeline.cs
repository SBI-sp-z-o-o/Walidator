using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApplicationModels
{
    public class ModelTimeline
    {
        public ModelTimeline()
        {
            if(StartLocalization != null && EndLocalization != null)
            {
                this.Distance = GetDistance(StartLocalization.Latitude, 
                    StartLocalization.Longitude, 
                    EndLocalization.Latitude, 
                    EndLocalization.Longitude, 
                    DistanceUnit.M);
            }
        }

        public string GsSceneObjectName { get; set; }
        //public long Id { get; set; }
        public long GsSceneObjectId { get; set; }
        public long StartTimeInSeconds { get; set; }
        public long EndTimeInSeconds { get; set; }
        public ObjectLocation StartLocalization { get; set; }
        public ObjectLocation EndLocalization { get; set; }
        public double Distance { get; set; }


        private static double GetDistance(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(Deg2rad(lat1)) * Math.Sin(Deg2rad(lat2)) + Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) * Math.Cos(Deg2rad(theta));
                dist = Math.Acos(dist);
                dist = Rad2deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == DistanceUnit.KM)
                {
                    dist *= 1.609344;
                }
                else if (unit == DistanceUnit.M)
                {
                    dist = (dist * 1.609344) * 1000;
                }
                return (dist);
            }
        }

        public enum DistanceUnit
        {
            KM = 1,
            M = 2
        }

        private static double Deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double Rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }
}
