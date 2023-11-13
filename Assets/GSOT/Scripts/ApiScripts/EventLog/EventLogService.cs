using ARLocation;
using Assets.GSOT.Scripts.Enums.ApiEnums;
using Assets.GSOT.Scripts.Models.ApiModels;
using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Dtos;
using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Models;
using Assets.GSOT.Scripts.SceneScripts;
using Assets.GSOT.Scripts.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriLib;
using UnityEngine;

namespace Assets.GSOT.Scripts.ApiScripts.EventLog
{
    public abstract class EventLogService
    {
        private readonly string _notSendItemsPath;

        protected EventLogType EventLogType { get; private set; }
        private Location _location;

        protected EventLogService(EventLogType eventLogType, Location location)
        {
            EventLogType = eventLogType;
            _location = location;
            _notSendItemsPath = Path.Combine(FilesUtils.DatabasePath, "EventLogs.json");
        }

        protected abstract EventLogBaseDto GetItemToSend();

        public void Add()
        {
            if (TemporaryDatabase.EventLogEnabled)
            {
                var itemToSend = GetItemToSend();

                if(_location != null && _location.Latitude != 0)
                {
                    itemToSend.LocalizationEPSG4326 = new LocalizationDto()
                    {
                        Altitude = _location.Altitude,
                        Latitude = _location.Latitude,
                        Longitude = _location.Longitude
                    };
                }

                var eventLogList = new List<EventLogBaseDto>()
                {
                    itemToSend
                };

                eventLogList.AddRange(GetNotSendItemsList());

                var isOk = MobileApiService.EventLogAdd(new EventLogAddModel()
                {
                    EventLogList = eventLogList
                });

                if (!isOk)
                {
                    AddNotSendItem(itemToSend);
                }
                else
                {
                    ClearNotSendItemsList();
                }
            }
        }

        #region Private
        private void AddNotSendItem(EventLogBaseDto item)
        {
            List<EventLogBaseDto> eventLogs = GetNotSendItemsList();
            eventLogs.Add(item);

            System.IO.File.WriteAllText(_notSendItemsPath, JsonConvert.SerializeObject(eventLogs));
        }

        private List<EventLogBaseDto> GetNotSendItemsList()
        {
            List<EventLogBaseDto> eventLogs = System.IO.File.Exists(_notSendItemsPath)
                ? JsonConvert.DeserializeObject<List<EventLogBaseDto>>(System.IO.File.ReadAllText(_notSendItemsPath))
                : new List<EventLogBaseDto>();
            return eventLogs;
        }

        private void ClearNotSendItemsList()
        {
            if (System.IO.File.Exists(_notSendItemsPath))
            {
                System.IO.File.Delete(_notSendItemsPath);
            }
        }
        #endregion
    }
}
