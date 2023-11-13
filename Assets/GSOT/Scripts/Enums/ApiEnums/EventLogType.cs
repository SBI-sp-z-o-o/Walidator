using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Enums.ApiEnums
{
    public enum EventLogType : byte
    {
        [Description("Logowanie")]
        MobileAppLogin = 1,

        [Description("Użycie aplikacji")]
        MobileAppUsing = 2,

        [Description("Wyświetlenie sceny")]
        MobileAppSceneUsing = 3,

        [Description("Rejestracja kodu licencyjnego")]
        MobileAppRegisterLicenseCode = 4,

        [Description("Przejście do zakupów")]
        MobileAppShopping = 5
    }
}
