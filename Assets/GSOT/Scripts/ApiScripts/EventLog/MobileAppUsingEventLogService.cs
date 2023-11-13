using System;
using Assets.GSOT.Scripts.Enums.ApiEnums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Dtos;
using ARLocation;

namespace Assets.GSOT.Scripts.ApiScripts.EventLog
{
    public class MobileAppUsingEventLogService : EventLogService
    {
        private DateTime _startDate;

        public MobileAppUsingEventLogService(DateTime startDate, Location location)
            : base(EventLogType.MobileAppUsing, location)
        {
            _startDate = startDate;
        }

        protected override EventLogBaseDto GetItemToSend()
        {
            return new EventLogBaseDto()
            {
                StartDate = _startDate,
                EndDate = DateTime.Now,
                Type = EventLogType
            };
        }
    }
}
