using Assets.GSOT.Scripts.Enums.ApiEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.SceneScripts
{
    public static class TemporaryDatabase
    {
        public const string WebPortalURL = "https://portal.gsot.pl";
        public const string CompetitionURL = "http://gsot.pl/pl/index.html#competition_area";
        public const string FaqURL = "http://gsot.pl/#faq";


        public static bool EventLogEnabled { get; set; } =  true;
        public static DateTime? AppStartDate { get; set; }
        public static DateTime? ActiveSceneStartDate { get; set; }
        public static long? ActiveSceneId { get; set; }
        public static long? ActiveScenePlaceId { get; set; }
        public static long? ActiveSceneOrderProductLicenseId { get; set; }
        public static SceneUsingMode? ActiveSceneUsingMode { get; set; }
    }
}
