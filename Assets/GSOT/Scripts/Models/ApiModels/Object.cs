using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class Object
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("type")]
        public Type Type { get; set; }

        [JsonProperty("file")]
        public File File { get; set; }
    }

    public enum Type : int
    {
        Model = 1,
        Sound = 2,
        Graphic = 3,
        Video = 4
    }
}
