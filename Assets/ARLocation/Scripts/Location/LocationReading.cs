using Assets.GSOT.Scripts.LoadingScripts;
using System;
using UnityEngine;

namespace ARLocation
{

    public struct LocationReading
    {
        public double latitude;
        public double longitude;
        public double altitude;
        public double accuracy;
        public int floor;

        public Location test;
        /// <summary>
        /// Epoch time in ms
        /// </summary>
        public long timestamp;

        public void ChangePosition()
        {
            ModelsQueue.LastLocation.Latitude += 0.001f;
            ModelsQueue.LastLocation.Longitude += 0.001f;
            //test = new Location(test.Latitude + 0.001f, test.Longitude + 0.001f, test.Altitude);
        }
        public Location ToLocation()
        {
            //return new Location(53.121125, 23.15036, 0); //bia³ystok
            if (Application.isEditor)
            {
                if(test == null)
                {
                    //test = new Location(53.712475, 20.511878, 0); // bialystok
                    test = new Location(53.734952, 20.500535, 0);//open trasa
                }

                if (latitude != 0 && longitude != 0 && ModelsQueue.LastLocation == null)
                {
                    ModelsQueue.LastLocation = test;// new Location(latitude, longitude, 0);
                    //return ModelsQueue.LastLocation;
                }
                return test;
            }


            if (latitude != 0 && longitude != 0)
            {
                ModelsQueue.LastLocation = new Location(latitude, longitude, 0);
            }

            var loc = ModelsQueue.LastLocation == null ? new Location() : ModelsQueue.LastLocation;

            //Console.WriteLine($"AAAAA Returned GPS Position: {loc}");
            return loc;
        }

        public static double HorizontalDistance(LocationReading a, LocationReading b)
        {
            return Location.HorizontalDistance(a.ToLocation(), b.ToLocation());
        }

        public override string ToString()
        {
            return
                "LocationReading { \n" +
                "  latitude = " + latitude + "\n" +
                "  longitude = " + longitude + "\n" +
                "  altitude = " + altitude + "\n" +
                "  accuracy = " + accuracy + "\n" +
                "  floor = " + floor + "\n" +
                "  timestamp = " + timestamp + "\n" +
                "}";
        }
    }
}
