using Assets.GSOT.Scripts.Enums.ApiEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels.EventLog.Dtos
{
    public class EventLogBaseDto
    {
        public EventLogType Type { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public long? GSSceneId { get; set; }
        public long? GSPlaceId { get; set; }
        public ApiResultStatus? RequestApiResultStatus { get; set; }
        public SceneUsingMode? SceneUsingMode { get; set; }
        public long? GSOrderProductLicenseId { get; set; }
        public LocalizationDto LocalizationEPSG4326 { get; set; }
    }
}
