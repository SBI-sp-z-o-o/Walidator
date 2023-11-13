using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Dtos;
using Assets.GSOT.Scripts.Enums.ApiEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARLocation;

namespace Assets.GSOT.Scripts.ApiScripts.EventLog
{
    public class MobileAppRegisterLicenseCodeEventLogService : EventLogService
    {
        private ApiResultStatus _requestApiResultStatus;

        public MobileAppRegisterLicenseCodeEventLogService(ApiResultStatus requestApiResultStatus, Location location)
            : base(EventLogType.MobileAppRegisterLicenseCode, location)
        {
            _requestApiResultStatus = requestApiResultStatus;
        }

        protected override EventLogBaseDto GetItemToSend()
        {
            return new EventLogBaseDto()
            {
                StartDate = DateTime.Now,
                Type = EventLogType,
                RequestApiResultStatus = _requestApiResultStatus
            };
        }
    }
}
