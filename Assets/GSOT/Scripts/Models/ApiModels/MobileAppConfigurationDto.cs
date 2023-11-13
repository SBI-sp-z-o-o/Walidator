using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class MobileAppConfigurationDto
    {
        public short MobileAppMaxOfflineHours { get; set; }
        public Mode AdvertMode { get; set; }
        public string GoogleAndroidAdUnitId { get; set; }
        public int MobileAppAdvertRefreshTimeInSeconds { get; set; }


        public enum Mode
        {
            Picture = 1,
            Google = 2
        }
    }
}
