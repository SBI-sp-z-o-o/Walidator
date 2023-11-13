using ARLocation;
using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GSOT.Scripts.Enums.ApiEnums;

namespace Assets.GSOT.Scripts.ApiScripts.EventLog
{
    public class MobileAppShoppingEventLogService : EventLogService
    {
        private readonly long _gsPlaceId;

        public MobileAppShoppingEventLogService(long gsPlaceId, Location location)
            : base(EventLogType.MobileAppShopping, location)
        {
            _gsPlaceId = gsPlaceId;
        }

        protected override EventLogBaseDto GetItemToSend()
        {
            return new EventLogBaseDto()
            {
                StartDate = DateTime.Now,
                Type = EventLogType,
                GSPlaceId = _gsPlaceId
            };
        }
    }
}
