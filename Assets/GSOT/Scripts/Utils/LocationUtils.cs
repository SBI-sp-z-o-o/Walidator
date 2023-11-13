using ARLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using UnityEngine;
using Assets.GSOT.Scripts.LoadingScripts;

namespace Assets.GSOT.Scripts.Utils
{

    public static class LocationUtils
    {
        public static Vector3 tablePoint1
        {
            get
            {
                return ModelsQueue.TablePoint1;
            }
        }
        public static Vector3 tablePoint2
        {
            get
            {
                return ModelsQueue.TablePoint2;
            }
        }
        public static Vector3 tablePoint3
        {
            get
            {
                return ModelsQueue.TablePoint3;
            }
        }
        public static Vector3 apiPoint1
        {
            get
            {
                return ModelsQueue.ApiPoint1;
            }
        }
        public static Vector3 apiPoint2
        {
            get
            {
                return ModelsQueue.ApiPoint2;
            }
        }

        public static Location ToLocation(this LocationInfo locationInfo)
        {
            return new Location()
            {
                Latitude = locationInfo.latitude,
                Longitude = locationInfo.longitude,
                Altitude = locationInfo.altitude
            };
        }

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

        public static ObjectLocation RotatedLocation(Location l1, Location l2, double poprawkaKierunku)
        {
            double rad = Math.PI / 180;
            double a = Math.PI / 2 - l1.Latitude * rad;
            double gamma = Azymut(l1, l2) + poprawkaKierunku * rad;
            double b = Dzeta(l1, l2);
            double szer = Math.Acos(Math.Cos(a) * Math.Cos(b) + Math.Sin(a) * Math.Sin(b) * Math.Cos(gamma));
            double dlug = l1.Longitude;
            if (szer != 0)
                dlug = dlug + Math.Asin(Math.Sin(b) * Math.Sin(gamma) / Math.Sin(szer)) / rad;
            return (new ObjectLocation(90 - szer / rad, dlug, l2.Altitude));
        }

        public static double Dzeta(Location l1, Location l2)
        {
            double rad = Math.PI / 180;
            double dLat = (l2.Latitude - l1.Latitude) * rad;
            double dLon = (l2.Longitude - l1.Longitude) * rad;
            double lat1 = l1.Latitude * rad;
            double lat2 = l2.Latitude * rad;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        }

        public static double Azymut(Location l1, Location l2)
        {
            double rad = Math.PI / 180;
            double dLat = (l2.Latitude - l1.Latitude) * rad;
            double dLon = (l2.Longitude - l1.Longitude) * rad;
            double lat1 = l1.Latitude * rad;
            double lat2 = l2.Latitude * rad;
            return Math.Atan2(Math.Sin(dLon) * Math.Cos(lat2), (Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon)));
        }

        public static void CalculateRelativePoints(GameObject locationRoot, Vector3 pointA, Vector3 pointB, ARLocationProvider arprovider)
        {
            var allModels = ModelsQueue.SceneQueue[ModelsQueue.ActiveSceneId];
            allModels.AddRange(ModelsQueue.Rendered[ModelsQueue.ActiveSceneId]);
            var realPoints = allModels.SelectMany(x => x.path).Where(x => x.Latitude != 0 && x.Longitude != 0).ToList();
            var position = arprovider.CurrentLocation.ToLocation();
            Vector3 kamera = new Vector3(Camera.main.transform.position.x, -1.8f, Camera.main.transform.position.z);
            ModelsQueue.TableTransformPosition = position;
            ModelsQueue.TablePoint1 = Location.GetGameObjectPositionForLocation(locationRoot.transform, kamera,
                ModelsQueue.TableTransformPosition, new ObjectLocation(new Location(pointA.x, pointA.z)), false);

            
            ModelsQueue.TablePoint2 = Location.GetGameObjectPositionForLocation(locationRoot.transform, kamera,
                ModelsQueue.TableTransformPosition, new ObjectLocation(new Location(pointB.x, pointB.z)), false);

            Vector3 temp;
            temp = Location.GetGameObjectPositionForLocation(locationRoot.transform, kamera,
                ModelsQueue.TableTransformPosition, new ObjectLocation(new Location(realPoints.Min(x => x.Latitude), realPoints.Min(x => x.Longitude))), false);
            Vector3 pA_3d = new Vector3(temp.x, temp.y, temp.z);
            temp = Location.GetGameObjectPositionForLocation(locationRoot.transform, kamera,
                ModelsQueue.TableTransformPosition, new ObjectLocation(new Location(realPoints.Max(x => x.Latitude), realPoints.Min(x => x.Longitude))), false);
            Vector3 pB_3d = new Vector3(temp.x, temp.y, temp.z);
            temp = Location.GetGameObjectPositionForLocation(locationRoot.transform, kamera,
                ModelsQueue.TableTransformPosition, new ObjectLocation(new Location(realPoints.Max(x => x.Latitude), realPoints.Max(x => x.Longitude))), false);
            Vector3 pC_3d = new Vector3(temp.x, temp.y, temp.z);

            ModelsQueue.ApiPoint1 = pA_3d;
            ModelsQueue.ApiPoint2 = pB_3d;
            var apiAB = Vector3.Distance(pA_3d, pB_3d);
            var apiBC = Vector3.Distance(pB_3d, pC_3d);
            var tableAB = Vector3.Distance(ModelsQueue.TablePoint1, ModelsQueue.TablePoint2);
            var tableBC = apiBC * (tableAB / apiAB);

            ModelsQueue.TablePoint3 = GetRightPoint(ModelsQueue.TablePoint1, ModelsQueue.TablePoint2, tableBC, true);
            ModelsQueue.TablePoint4 = GetRightPoint(ModelsQueue.TablePoint2, ModelsQueue.TablePoint1, tableBC, false);
            ModelsQueue.TablePoint4.y = ModelsQueue.TablePoint1.y;
            ModelsQueue.TablePoint3.y = ModelsQueue.TablePoint1.y;
            ModelsQueue.TablePoint2.y = ModelsQueue.TablePoint1.y;
            ModelsQueue.TableSceneCenter = new Vector3((ModelsQueue.TablePoint1.x + ModelsQueue.TablePoint3.x) / 2, ModelsQueue.TablePoint1.y - 0.05f, (ModelsQueue.TablePoint1.z + ModelsQueue.TablePoint3.z) / 2);
            ModelsQueue.TableBCDistance = tableBC;
        }

        public static Vector3 GetRightPoint(Vector3 point1, Vector3 point2, float length, bool rightPosition = true)
        {
            double azimuth = Math.Atan2(point2.x - point1.x, point2.z - point1.z);
            azimuth = rightPosition ? azimuth + Math.PI / 2 : azimuth - Math.PI / 2;
            Vector3 newPoint = new Vector3(point2.x + (length * (float)Math.Sin(azimuth)), 0 /*<- y*/, point2.z + (length * (float)Math.Cos(azimuth)));

            return newPoint;
        }

        public static List<Vector3> CalculateTablePoints(List<Vector3> points)
        {
            if (apiPoint1 == apiPoint2)
            {
                if (ModelsQueue.IsTableScene)
                {
                    return new List<Vector3>()
                    {
                        new Vector3((tablePoint1.x + tablePoint3.x) / 2, (tablePoint1.y + tablePoint3.y) / 2, (tablePoint1.z + tablePoint3.z) / 2)
                    };
                }
                else if (ModelsQueue.IsPlaygroundScene)
                {
                    return new List<Vector3>()
                    {
                        new Vector3((tablePoint1.x + tablePoint2.x) / 2, (tablePoint1.y + tablePoint2.y) / 2, (tablePoint1.z + tablePoint2.z) / 2)
                    };
                }
            }

            float scale = 1;
            Vector3 newTablePoint2 = new Vector3(tablePoint2.x, tablePoint2.y, tablePoint2.z);

            //jezeli scena stolowa albo boiskowa
            if (ModelsQueue.IsPlaygroundScene || ModelsQueue.IsTableScene)
            {
                double terrainDistance =
                Math.Sqrt((apiPoint1.x - apiPoint2.x) * (apiPoint1.x - apiPoint2.x) + (apiPoint1.z - apiPoint2.z) * (apiPoint1.z - apiPoint2.z));

                //dla scen stolowych jest zawsze wyliczane
                if (ModelsQueue.IsTableScene)
                {
                    scale = (float)Vector3.Distance(ModelsQueue.TablePoint1, ModelsQueue.TablePoint2) / (float)terrainDistance;
                }
                //else - scena jest boiskowa
                else
                {
                    //jesli na webie jest uzupelnione to przepisujemy
                    if(ModelsQueue.PlaygroundSceneObjectTimelineScale.HasValue)
                    {
                        scale = ModelsQueue.PlaygroundSceneObjectTimelineScale.Value;
                    }
                    //jak nie jest uzupelnione to wyliczamy tak samo jak dla stolu
                    else
                    {
                        scale = (float)Vector3.Distance(ModelsQueue.TablePoint1, ModelsQueue.TablePoint2) / (float)terrainDistance;
                    }
                }

                double dx = tablePoint2.x - tablePoint1.x;
                double dy = tablePoint2.z - tablePoint1.z;
                double Az = Math.Atan2(dy, dx);
                newTablePoint2 = new Vector3(
                  (float)(tablePoint1.x + terrainDistance * scale * Math.Cos(Az)),
                  0,
                  (float)(tablePoint1.z + terrainDistance * scale * Math.Sin(Az))
                  );
            }

            var result = new List<Vector3>();
            var a = A(tablePoint1.x, tablePoint1.z, newTablePoint2.x, newTablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
            var b = B(tablePoint1.x, tablePoint1.z, newTablePoint2.x, newTablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
            var c = C(tablePoint1.x, tablePoint1.z, newTablePoint2.x, newTablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
            var d = D(tablePoint1.x, tablePoint1.z, newTablePoint2.x, newTablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);

            foreach (var point in points)
            {
                var newX = c + b * point.x - a * point.z;
                var newZ = d + a * point.x + b * point.z;

                result.Add(new Vector3((float)newX, tablePoint1.y + (point.y * scale), (float)newZ));
            }
            return result;
        }

        public static Vector3 CalculateTablePoint(Vector3 point)
        {
            if (apiPoint1 == apiPoint2)
            {
                if (ModelsQueue.IsTableScene)
                {
                    return new Vector3((tablePoint1.x + tablePoint3.x) / 2, (tablePoint1.y + tablePoint3.y) / 2, (tablePoint1.z + tablePoint3.z) / 2);
                }
                else if (ModelsQueue.IsPlaygroundScene)
                {
                    return new Vector3((tablePoint1.x + tablePoint2.x) / 2, (tablePoint1.y + tablePoint2.y) / 2, (tablePoint1.z + tablePoint2.z) / 2);
                }
            }

            Vector3 newTablePoint2 = new Vector3(tablePoint2.x, tablePoint2.y, tablePoint2.z);

            float scale = 1;
            if (ModelsQueue.PlaygroundSceneObjectTimelineScale.HasValue || ModelsQueue.TableScale.HasValue)
            {
                scale = ModelsQueue.PlaygroundSceneObjectTimelineScale.HasValue ? ModelsQueue.PlaygroundSceneObjectTimelineScale.Value
                    : ModelsQueue.TableScale.Value;
                double terrainDistance =
                    Math.Sqrt((apiPoint1.x - apiPoint2.x) * (apiPoint1.x - apiPoint2.x) + (apiPoint1.z - apiPoint2.z) * (apiPoint1.z - apiPoint2.z));
                
                //dla scen stolowych jest zawsze wyliczane
                if (ModelsQueue.IsTableScene)
                {
                    scale = (float)Vector3.Distance(ModelsQueue.TablePoint1, ModelsQueue.TablePoint2) / (float)terrainDistance;
                }
                //else - scena jest boiskowa
                else
                {
                    //jesli na webie jest uzupelnione to przepisujemy
                    if (ModelsQueue.PlaygroundSceneObjectTimelineScale.HasValue)
                    {
                        scale = ModelsQueue.PlaygroundSceneObjectTimelineScale.Value;
                    }
                    //jak nie jest uzupelnione to wyliczamy tak samo jak dla stolu
                    else
                    {
                        scale = (float)Vector3.Distance(ModelsQueue.TablePoint1, ModelsQueue.TablePoint2) / (float)terrainDistance;
                    }
                }

                double dx = tablePoint2.x - tablePoint1.x;
                double dy = tablePoint2.z - tablePoint1.z;
                double Az = Math.Atan2(dy, dx);
                newTablePoint2 = new Vector3(
                  (float)(tablePoint1.x + terrainDistance * scale * Math.Cos(Az)),
                  0,
                  (float)(tablePoint1.z + terrainDistance * scale * Math.Sin(Az))
                  );
            }

            var a = A(tablePoint1.x, tablePoint1.z, newTablePoint2.x, newTablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
            var b = B(tablePoint1.x, tablePoint1.z, newTablePoint2.x, newTablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
            var c = C(tablePoint1.x, tablePoint1.z, newTablePoint2.x, newTablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);
            var d = D(tablePoint1.x, tablePoint1.z, newTablePoint2.x, newTablePoint2.z, apiPoint1.x, apiPoint1.z, apiPoint2.x, apiPoint2.z);

            var newX = c + b * point.x - a * point.z;
            var newZ = d + a * point.x + b * point.z;

            return new Vector3((float)newX, tablePoint1.y + (point.y * scale), (float)newZ);
        }
    }
}
