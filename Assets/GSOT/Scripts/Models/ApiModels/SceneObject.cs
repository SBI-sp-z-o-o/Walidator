using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class SceneObject
    {
        [JsonProperty("timelines")]
        public List<Timeline> Timelines { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("gsSceneId")]
        public long GsSceneId { get; set; }

        [JsonProperty("gsObjectId")]
        public long GsObjectId { get; set; }

        [JsonProperty("isAvailableInDemoVersion")]
        public bool IsAvailableInDemoVersion { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("orientationPointEPSG4326")]
        public LocalizationDto OrientationPoint { get; set; }

        [JsonProperty("isFacingSpectator")]
        public bool IsFacingSpectator { get; set; }
        [JsonProperty("isAvailableInTableSceneUsingMode")]
        public bool IsAvailableInTableSceneUsingMode { get; set; }

    }
}
