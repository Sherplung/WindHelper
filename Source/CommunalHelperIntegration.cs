using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.WindHelper
{
    [ModImportName("CommunalHelper.DashStates")]
    internal static class CommunalHelperIntegration
    {
        public static Func<int> GetDreamTunnelDashState;
    }
}
