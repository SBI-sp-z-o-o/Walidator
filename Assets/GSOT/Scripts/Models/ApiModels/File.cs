using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class File
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("originalFileName")]
        public string OriginalFileName { get; set; }

        [JsonProperty("discFileName")]
        public string DiscFileName { get; set; }
    }
}
