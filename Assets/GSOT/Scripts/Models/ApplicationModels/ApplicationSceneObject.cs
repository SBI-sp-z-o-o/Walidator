using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApplicationModels
{
    public class ApplicationSceneObject
    {
        public List<ModelTimeline> Timelines { get; set; }

        public long Id { get; set; }

        public long GsSceneId { get; set; }

        public long GsObjectId { get; set; }

        public bool IsAvailableInDemoVersion { get; set; }
    }
}
