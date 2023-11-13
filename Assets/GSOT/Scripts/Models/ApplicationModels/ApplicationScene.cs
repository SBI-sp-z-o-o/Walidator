using Assets.GSOT.Scripts.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApplicationModels
{
    public class ApplicationScene
    {
        public List<ApplicationSceneObject> SceneObjects { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public long GsPlaceId { get; set; }

        public long? DemoVersionTimeInSeconds { get; set; }
        public SceneGroupType GroupType { get; set; }
        public string GroundColor { get; set; }
        public int GroundColorAlpha { get; set; }
        public string ButtonImage { get; set; }
        public bool IsAvailableInTableSceneUsingMode { get; set; }
        public bool IsAvailableInPlaygroundScene { get; set; }
        public float? FirstSideLength { get; set; }
        public float? SecondSideLength { get; set; }
        public float? Scale { get; set; }
        public float RLengthInMeters { get; set; }
        public float PlaygroundSceneObjectTimelineScale { get; set; }
        public float PlaygroundObjectScale { get; set; }
        public string ThemeMusic { get; set; }
        public string RectangleFile { get; set; }

        public override string ToString()
        {
            return $"Name: {Name} Objects: {SceneObjects.Count}";
        }
    }
}
