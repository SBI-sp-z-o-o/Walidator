using Assets.GSOT.Scripts.Enums.ApiEnums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class PlaceWithSceneDto : PlaceBaseDto
    {
        [JsonProperty("scenes")]
        public List<Scene> Scenes { get; set; }

        [JsonProperty("licensePrice")]
        public double LicensePrice { get; set; }

        [JsonProperty("isAvailableInDemoVersion")]
        public bool IsAvailableInDemoVersion { get; set; }
        public SceneUsingMode SceneUsingMode { get; set; }
        public long? GSOrderProductLicenseId { get; set; }
        public string SceneGroup1Name { get; set; }
        public string SceneGroup2Name { get; set; }
        public LocalizationDto rectanglePoint1EPSG4326 { get; set; }
        public LocalizationDto rectanglePoint2EPSG4326 { get; set; }
    }
}
