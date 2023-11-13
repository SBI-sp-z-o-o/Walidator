using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class PlaceGetAvailableResult
    {
        [JsonProperty("places")]
        public List<PlaceWithSceneDto> Places { get; set; }

        [JsonProperty("allPlaces")]
        public List<PlaceBaseDto> AllPlaces { get; set; }

        [JsonProperty("objects")]
        public List<Object> Objects { get; set; }

        public MobileAppConfigurationDto Configuration { get; set; }
        [JsonProperty("application")]
        public ApplicationDto Application { get; set; }
    }

}
