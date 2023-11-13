using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApplicationModels
{
    public class Audio
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public long StartTimeSeconds { get; set; }
        public long SceneId { get; set; }
        public DateTime StartTime { get; set; }
        public List<AudioTimeline> Timelines { get; set; }
    }
}
