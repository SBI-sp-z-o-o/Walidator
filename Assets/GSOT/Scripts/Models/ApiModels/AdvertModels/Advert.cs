using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels.AdvertModels
{
    public class Advert
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("file")]
        public File File { get; set; }

        [JsonProperty("isAvailableOnStartupScreen")]
        public bool IsAvailableOnStartupScreen { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("displayStartTime")]
        public DateTime DisplayStartTime { get; set; }

        [JsonProperty("displayEndTime")]
        public DateTime DisplayEndTime { get; set; }

        [JsonProperty("type")]
        public AdvertType Type { get; set; }

        [JsonProperty("localizationEPSG4326")]
        public LocalizationDto LocalizationEPSG4326 { get; set; }

        [JsonProperty("radiusInMeters")]
        public int RadiusInMeters { get; set; }

        [JsonProperty("durationInSeconds")]
        public int DurationInSeconds { get; set; }
    }

    public enum AdvertType
    {
        Standard = 1,
        Alert = 2
    }
}
