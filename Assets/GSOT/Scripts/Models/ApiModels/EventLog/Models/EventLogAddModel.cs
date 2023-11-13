using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels.EventLog.Models
{
    public class EventLogAddModel
    {
        public List<EventLogBaseDto> EventLogList { get; set; }
    }
}
