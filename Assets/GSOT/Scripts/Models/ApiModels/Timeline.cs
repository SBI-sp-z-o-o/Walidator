using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class Timeline
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("gsSceneObjectId")]
        public long GsSceneObjectId { get; set; }

        [JsonProperty("startTimeInSeconds")]
        public long StartTimeInSeconds { get; set; }

        [JsonProperty("endTimeInSeconds")]
        public long EndTimeInSeconds { get; set; }

        [JsonProperty("startLocalization")]
        public LocalizationDto StartLocalization { get; set; }

        [JsonProperty("endLocalization")]
        public LocalizationDto EndLocalization { get; set; }

 	    [JsonProperty("animationIndex")]
        public int? AnimationIndex { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }
        [JsonProperty("captionLocalizationAltitude")]

        public double? CaptionLocalizationAltitude { get; set; }

        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Vx { get; set; }
        public double? Vy { get; set; }
        public double? Vz { get; set; }

        public SceneObjectTimelineGraphicVideoPosition? GraphicVideoPosition { get; set; }

        public override string ToString()
        {
            return $"Lat1:{StartLocalization?.Latitude} Lon1:{StartLocalization?.Longitude} Lat2:{EndLocalization?.Latitude} Lon2:{EndLocalization?.Longitude}";
        }

        public enum SceneObjectTimelineGraphicVideoPosition
        {
            OneSideNoFollow = 1,
            OneSideFollow = 2,
            TwoSidesNoFollow = 3
        }
    }
}
