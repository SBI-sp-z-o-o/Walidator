using ARLocation;
using Assets.GSOT.Scripts.Enums.ApiEnums;
using Assets.GSOT.Scripts.Models.ApiModels;
using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.ApiScripts.EventLog
{
    public class MobileAppSceneUsingEventLogService : EventLogService
    {
        private readonly DateTime _startDate;
        private readonly SceneUsingMode _sceneUsingMode;
        private readonly long _gsSceneId;
        private readonly long _gsPlaceId;
        private readonly long? _gsOrderProductLicenseId;

        public MobileAppSceneUsingEventLogService(DateTime startDate, SceneUsingMode sceneUsingMode, 
            long gsSceneId, long gsPlaceId, long? gsOrderProductLicenseId, Location location)
            : base(EventLogType.MobileAppSceneUsing, location)
        {
            _startDate = startDate;
            _sceneUsingMode = sceneUsingMode;
            _gsSceneId = gsSceneId;
            _gsPlaceId = gsPlaceId;
            _gsOrderProductLicenseId = gsOrderProductLicenseId;
        }

        protected override EventLogBaseDto GetItemToSend()
        {
            return new EventLogBaseDto()
            {
                StartDate = _startDate,
                EndDate = DateTime.Now,
                Type = EventLogType,
                GSSceneId = _gsSceneId,
                GSPlaceId = _gsPlaceId,
                SceneUsingMode = _sceneUsingMode,
                GSOrderProductLicenseId = _gsOrderProductLicenseId
            };
        }
    }
}
