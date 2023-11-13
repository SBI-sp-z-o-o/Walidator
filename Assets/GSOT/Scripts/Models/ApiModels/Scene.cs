using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public partial class Scene
    {
        [JsonProperty("sceneObjects")]
        public List<SceneObject> SceneObjects { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("gsPlaceId")]
        public long GsPlaceId { get; set; }

        [JsonProperty("demoVersionTimeInSeconds")]
        public long? DemoVersionTimeInSeconds { get; set; }
        [JsonProperty("groupType")]
        public SceneGroupType GroupType { get; set; }
        [JsonProperty("groundColor")]
        public string GroundColor { get; set; }
        [JsonProperty("groundColorAlpha")]
        public int GroundColorAlpha { get; set; }
        [JsonProperty("logoFile")]
        public File LogoFile { get; set; }

        [JsonProperty("isAvailableInTableSceneUsingMode")]
        public bool IsAvailableInTableSceneUsingMode { get; set; }
        [JsonProperty("isAvailableInPlaygroundSceneUsingMode")]
        public bool IsAvailableInPlaygroundScene { get; set; }

        [JsonProperty("rectangleFirstSideLengthInMeters")]
        public float? FirstSideLength { get; set; }
        [JsonProperty("rectangleSecondSideLengthInMeters")]
        public float? SecondSideLength { get; set; }
        [JsonProperty("objectScale")]
        public float? Scale { get; set; }
        [JsonProperty("rLengthInMeters")]
        public float RLengthInMeters { get; set; }
        [JsonProperty("PlaygroundObjectScale")]
        public float PlaygroundObjectScale { get; set; }
        [JsonProperty("PlaygroundSceneObjectTimelineScale")]
        public float PlaygroundSceneObjectTimelineScale { get; set; }
        [JsonProperty("themeMusicFile")]
        public File ThemeMusicFile { get; set; }
        [JsonProperty("rectangleFile")]
        public File RectangleFile { get; set; }

        public override string ToString()
        {
            return $"Name: {Name} Objects: {SceneObjects.Count}";
        }
    }

    public enum SceneGroupType
    {
        Model = 1,
        Battle = 2,
        Guide = 3
    }
}
