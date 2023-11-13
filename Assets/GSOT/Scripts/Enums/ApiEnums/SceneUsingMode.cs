using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Enums.ApiEnums
{
    public enum SceneUsingMode : byte
    {
        [Description("Teren")]
        OriginalArea = 1,

        [Description("Boisko")]
        Playground = 2,

        [Description("Stół")]
        Table = 3
    }
}
