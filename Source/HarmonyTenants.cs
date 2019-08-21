using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using static RimWorld.ColonistBar;

namespace Tenants {
    [StaticConstructorOnStartup]
    internal static class HarmonyTenants {
        static HarmonyTenants() {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("rimworld.limetreesnake.tenants");
            harmonyInstance.Patch(AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DoListingItems_MapTools"), null, new HarmonyMethod(typeof(HarmonyPatch), "DoListingItems_MapTools"));
            harmonyInstance.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "CanTakeOrder"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("TenantCanTakeOrder_PreFix")));
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("TenantGetGizmos_PostFix")));
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_TimetableTracker), "SetAssignment"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("SetAssignment_PreFix")), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_WorkSettings), "SetPriority"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("SetAssignment_PreFix")), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), "AgeTick"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("AgeTick_PostFix")));
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "CapturedBy"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CapturedBy_PostFix")));
            harmonyInstance.Patch(AccessTools.Method(typeof(Dialog_FormCaravan), "AllSendablePawns"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("AllSendablePawns_PostFix")));
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnTable_PlayerPawns), "RecachePawns"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("RecachePawns_PostFix")));
            harmonyInstance.Patch(AccessTools.Method(typeof(ColonistBarDrawLocsFinder), "CalculateColonistsInGroup"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CalculateColonistsInGroup_PreFix")), null);

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs) {
                if (def.race != null && !def.race.Animal && def.race.Humanlike) {
                    def.comps.Add(new CompProps_Tenant());
                }
            }
        }
        static void DoListingItems_MapTools_PostFix(Dialog_DebugActionsMenu __instance) {
            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DoLabel").Invoke(__instance, new object[] { "Tools - Tenants" });
            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DebugToolMap").Invoke(__instance, new object[] {
                "Spawn Contract Event", new Action(()=>
                {
                    IncidentParms parms = new IncidentParms
                    {
                        target = Find.CurrentMap,
                    };
                    IncidentWorker_TenantProposition incident = new IncidentWorker_TenantProposition();
                    incident.TryExecute(parms);
                })
            });
        }
        public static void TenantCanTakeOrder_PostFix(ref bool __result, Pawn pawn) {
            if (pawn.IsTenant()) {
                __result = false;
            }
        }
        public static void TenantGetGizmos_PostFix(ref IEnumerable<Gizmo> __result, ref Pawn __instance) {
            if (__instance.IsTenant()) {
                List<Gizmo> gizmos = __result.ToList();
                foreach (Gizmo giz in gizmos.ToList()) {
                    if ((giz as Command).defaultLabel == "Draft")
                        gizmos.Remove(giz);
                }
                __result = gizmos.AsEnumerable();
            }
        }
        public static bool SetAssignment_PreFix(Pawn_TimetableTracker __instance) {
            return !Traverse.Create(__instance).Field("pawn").GetValue<Pawn>().IsTenant();
        }
        public static bool SetPriority_PreFix(Pawn_WorkSettings __instance) {
            return !Traverse.Create(__instance).Field("pawn").GetValue<Pawn>().IsTenant();
        }
        public static void AgeTick_PostFix(Pawn_AgeTracker __instance) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.IsTenant() && pawn.IsColonist) {
                long ageBiologicalTicksInt = Traverse.Create(__instance).Field("ageBiologicalTicksInt").GetValue<long>();
                if (ageBiologicalTicksInt % 10000 == 0) {
                    if (pawn.needs.mood.CurInstantLevel > 0.65f) {
                        pawn.TryGetComp<Tenant>().Moods.Add(Tenant.Mood.happy);
                    }
                    else if (pawn.needs.mood.CurInstantLevel < pawn.mindState.mentalBreaker.BreakThresholdMinor) {
                        pawn.TryGetComp<Tenant>().Moods.Add(Tenant.Mood.sad);
                        if (!Utility.RecentMood(pawn.TryGetComp<Tenant>().Moods)) {
                            Utility.ContractConclusion(pawn, true);
                        }
                    }
                    else {
                        pawn.TryGetComp<Tenant>().Moods.Add(Tenant.Mood.neutral);
                    }
                }
                if (Find.TickManager.TicksGame >= pawn.TryGetComp<Tenant>().ContractEndDate) {
                    Utility.ContractConclusion(pawn, false);
                }
            }
        }
        public static void CapturedBy_PostFix(Pawn_GuestTracker __instance, Faction by, Pawn byPawn) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.IsTenant() && pawn.IsColonist) {
                Utility.PawnLeave(pawn);    
            }
        }
        public static void AllSendablePawns_PostFix(ref List<Pawn> __result) {
            Utility.RemoveTenantsFromList(__result);
        }
        public static void RecachePawns_PostFix(PawnTable __instance) {
            if (__instance is PawnTable_PlayerPawns) {
                List<Pawn> pawns = Traverse.Create(__instance).Field("cachedPawns").GetValue<List<Pawn>>();
                Utility.RemoveTenantsFromList(pawns);
            }
        }
        public static void CalculateColonistsInGroup_PreFix() {
            List<Entry> entries = Find.ColonistBar.Entries;
            List<Entry> newentries = new List<Entry>();
            foreach (Entry entry in entries) {
                if (entry.pawn.IsTenant())
                    newentries.Add(entry);
            }
            foreach (Entry entry in newentries) {
                entries.Remove(entry);
            }
        }
    }
}
