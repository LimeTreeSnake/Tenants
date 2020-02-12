using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Tenants.Harmony {
    [StaticConstructorOnStartup]
    internal static class HarmonyTenants {
        static HarmonyTenants() {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("rimworld.limetreesnake.tenants");
        }
    }
}
