using ARLocation;
using Assets.GSOT.Scripts.Enums.ApiEnums;
using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.ApiScripts.EventLog
{
    public class MobileAppLoginEventLogService : EventLogService
    {
        public MobileAppLoginEventLogService(Location location)
            :base(EventLogType.MobileAppLogin, location)
        {

        }

        protected override EventLogBaseDto GetItemToSend()
        {
            return new EventLogBaseDto()
            {
                StartDate = DateTime.Now,
                Type = EventLogType
            };
        }
    }
}
