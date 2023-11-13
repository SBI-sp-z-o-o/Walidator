using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class RootObject
    {
        [JsonProperty("data")]
        public PlaceGetAvailableResult Data { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }
    }
}
