using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using static RimWorld.ColonistBar;

namespace Tenants {
    [StaticConstructorOnStartup]
    internal static class HarmonyTenants {
        static HarmonyTenants() {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("rimworld.limetreesnake.tenants");
            //Removes ability to control tenant
            harmonyInstance.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "CanTakeOrder"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CanTakeOrder_PreFix")), null);
            //Removes tenant gizmo
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("TenantGetGizmos_PostFix")));
            //Calculates mood and checks contract time.
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), "AgeTick"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("AgeTick_PostFix")));
            //What happens when you capture a tenant 
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "CapturedBy"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CapturedBy_PreFix")), null);
            //Tenant dies
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "Kill"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("Kill_PreFix")), null);
            //Tenant works
            harmonyInstance.Patch(AccessTools.Method(typeof(JobGiver_Work), "PawnCanUseWorkGiver"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("PawnCanUseWorkGiver_PreFix")), null);
            //Remove tenants from caravan list.
            harmonyInstance.Patch(AccessTools.Method(typeof(Dialog_FormCaravan), "AllSendablePawns"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("AllSendablePawns_PostFix")));
            //Removes tenants from from pawn table 
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnTable_PlayerPawns), "RecachePawns"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("RecachePawns_PostFix")));
            //Removes tenants from from colonist bar 
            harmonyInstance.Patch(typeof(ColonistBarDrawLocsFinder).GetMethods().FirstOrDefault(x => x.Name == "CalculateDrawLocs" && x.GetParameters().Count() == 2), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CalculateDrawLocs_PreFix")), null);
            //Removes check for idle tenants
            harmonyInstance.Patch(AccessTools.Method(typeof(Alert_ColonistsIdle), "GetReport"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetReport_PostFix")));
            //Removes check for idle tenants
            harmonyInstance.Patch(AccessTools.Method(typeof(Alert_ColonistsIdle), "GetExplanation"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetExplanation_PreFix")), null);
            //Removes check for idle tenants
            harmonyInstance.Patch(AccessTools.Method(typeof(Alert_ColonistsIdle), "GetLabel"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetLabel_PreFix")), null);
            //Pawn name color patch
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnNameColorUtility), "PawnNameColorOf"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("PawnNameColorOf_PreFix")), null);


            //Comms Console Float Menu Option
            harmonyInstance.Patch(AccessTools.Method(typeof(Building_CommsConsole), "GetFloatMenuOptions"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetFloatMenuOptions_PostFix")));


            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs) {
                if (def.race != null) {
                    def.comps.Add(new CompProps_Tenant());
                }
            }
        }
        public static bool CanTakeOrder_PreFix(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp != null && tenantComp.IsTenant) {
                return false;
            }
            return true;
        }
        public static void TenantGetGizmos_PostFix(ref IEnumerable<Gizmo> __result, ref Pawn __instance) {
            Tenant tenantComp = __instance.GetTenantComponent();
            if (tenantComp != null && tenantComp.IsTenant) {
                List<Gizmo> gizmos = __result.ToList();
                if (gizmos != null) {
                    foreach (Gizmo giz in gizmos.ToList()) {
                        if ((giz as Command).defaultLabel == "Draft")
                            gizmos.Remove(giz);
                    }
                    __result = gizmos.AsEnumerable();
                }
            }
        }
        public static void AgeTick_PostFix(Pawn_AgeTracker __instance) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp != null && tenantComp.IsTenant && pawn.IsColonist) {
                if (tenantComp.ContractEndDate == 0) {
                    tenantComp.Reset();
                }
                if (tenantComp.IsTerminated ) {
                    if (!pawn.health.Downed) {
                        Messages.Message("ContractTerminateFail".Translate(), MessageTypeDefOf.NeutralEvent);
                    }
                    else {
                        Utility.TenantCancelContract(pawn);
                    }
                    tenantComp.IsTerminated = false;
                }
                Pawn colonist = pawn.Map.mapPawns.FreeColonists.FirstOrDefault(x => x.GetTenantComponent().IsTenant == false);
                if (colonist == null) {
                    Utility.ContractConclusion(pawn, true, 1f);
                }
                long ageBiologicalTicksInt = Traverse.Create(__instance).Field("ageBiologicalTicksInt").GetValue<long>();
                if (ageBiologicalTicksInt % 6000 == 0) {
                    if (pawn.needs.mood.CurInstantLevel > 0.8f) {
                        Utility.TenantWantToJoin(pawn);
                    }
                    if (pawn.needs.mood.CurInstantLevel > 0.66f) {
                        tenantComp.HappyMoodCount++;
                        tenantComp.RecentBadMoodsCount = 0;
                    }
                    else if (pawn.needs.mood.CurInstantLevel < pawn.mindState.mentalBreaker.BreakThresholdMinor) {
                        tenantComp.SadMoodCount++;
                        tenantComp.RecentBadMoodsCount++;
                        if (tenantComp.RecentBadMoodsCount > 5) {
                            Utility.ContractConclusion(pawn, true);
                        }
                    }
                    else {
                        tenantComp.NeutralMoodCount++;
                        tenantComp.RecentBadMoodsCount = 0;
                    }
                }
                if (Find.TickManager.TicksGame >= tenantComp.ContractEndTick) {
                    Utility.ContractConclusion(pawn, false);
                }
            }
        }
        public static void CapturedBy_PreFix(Pawn_GuestTracker __instance, Faction by, Pawn byPawn) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp != null && tenantComp.IsTenant) {
                tenantComp.IsTenant = false;
                tenantComp.WasTenant = true;
                pawn.SetFaction(Faction.OfAncients);

                string text = "TenantCaptured".Translate(pawn.Named("PAWN"));
                text = text.AdjustedFor(pawn);
                string label = "Captured".Translate() + ": " + pawn.LabelShortCap;
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, pawn);

                Utility.TenantCaptured(pawn, byPawn);
            }
        }
        public static void Kill_PreFix(Pawn __instance, DamageInfo? dinfo) {
            Tenant tenantComp = __instance.GetTenantComponent();
            if (tenantComp != null)
                if ((tenantComp.IsTenant && __instance.IsColonist) || tenantComp.WasTenant) {
                    tenantComp.IsTenant = false;
                    tenantComp.WasTenant = false;
                    __instance.SetFaction(Faction.OfAncients);

                    string text = "TenantDeath".Translate(__instance.Named("PAWN"));
                    text = text.AdjustedFor(__instance);
                    string label = "Death".Translate() + ": " + __instance.LabelShortCap;
                    Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Death, __instance);

                    Utility.TenantDeath(__instance);
                }
        }
        public static bool PawnCanUseWorkGiver_PreFix(Pawn pawn, WorkGiver giver) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp != null && tenantComp.IsTenant && pawn.IsColonist) {
                if (SettingsHelper.LatestVersion.WorkIsDirty) {
                    Utility.UpdateTenantsWork();
                }
                if (pawn.needs.mood.CurLevel > SettingsHelper.LatestVersion.LevelOfHappinessToWork / 100f || Utility.EmergencyWork(giver)) {
                    return true;
                }
                else {
                    return false;
                }
            }
            return true;
        }
        public static void AllSendablePawns_PostFix(ref List<Pawn> __result) {
            Utility.RemoveTenantsFromList(__result);
        }
        public static void RecachePawns_PostFix(PawnTable __instance) {
            if (__instance is PawnTable_Tenants) {
                return;
            }
            List<Pawn> pawns = Traverse.Create(__instance).Field("cachedPawns").GetValue<List<Pawn>>();
            if (pawns != null || pawns.Count > 0)
                Utility.RemoveTenantsFromList(pawns);
        }
        public static void CalculateDrawLocs_PreFix(List<Vector2> outDrawLocs, out float scale) {
            scale = 1f;
            List<Entry> entries = Traverse.Create(Find.ColonistBar).Field("cachedEntries").GetValue<List<Entry>>();
            if (entries != null && entries.Count > 0) {
                List<Entry> newentries = new List<Entry>();
                foreach (Entry entry in entries) {
                    if (entry.pawn != null) {
                        Tenant tenantComp = entry.pawn.GetTenantComponent();
                        if (tenantComp != null && tenantComp.IsTenant)
                            newentries.Add(entry);
                    }
                }
                foreach (Entry entry in newentries) {
                    entries.Remove(entry);
                }
            }
        }


        public static void GetReport_PostFix(ref AlertReport __result, Alert_ColonistsIdle __instance) {
            if (__result.culprits != null) {
                Utility.RemoveTenantsFromList(ref __result.culprits);
                __result.active = __result.AnyCulpritValid;
            }
        }
        public static bool GetExplanation_PreFix(ref string __result, Alert_ColonistsIdle __instance) {

            StringBuilder stringBuilder = new StringBuilder();
            IEnumerable<Pawn> IdleColonists = Traverse.Create(__instance).Property("IdleColonists").GetValue<IEnumerable<Pawn>>();

            foreach (Pawn idleColonist in IdleColonists) {
                Tenant tenantComp = idleColonist.GetTenantComponent();
                if (tenantComp != null && !tenantComp.IsTenant)
                    stringBuilder.AppendLine("    " + idleColonist.LabelShort.CapitalizeFirst());
            }
            __result = "ColonistsIdleDesc".Translate(stringBuilder.ToString());

            return false;
        }
        public static bool GetLabel_PreFix(ref string __result, Alert_ColonistsIdle __instance) {
            IEnumerable<Pawn> IdleColonists = Traverse.Create(__instance).Property("IdleColonists").GetValue<IEnumerable<Pawn>>();
            int x = 0;
            foreach (Pawn idleColonist in IdleColonists) {
                Tenant tenantComp = idleColonist.GetTenantComponent();
                if (tenantComp != null && !tenantComp.IsTenant)
                    x++;
            }
            __result = "ColonistsIdle".Translate(x.ToStringCached());
            return false;
        }
        public static bool PawnNameColorOf_PreFix(ref Color __result, Pawn pawn) {
            if (pawn.IsColonist) {
                Tenant tenantComp = pawn.GetTenantComponent();
                if (tenantComp != null && tenantComp.IsTenant) {
                    __result = SettingsHelper.LatestVersion.Color;
                    return false;
                }
            }
            return true;
        }
        public static void GetFloatMenuOptions_PostFix(Building_CommsConsole __instance,ref IEnumerable<FloatMenuOption> __result, Pawn myPawn ) {

            if (!MapComponent_Tenants.GetComponent(myPawn.Map).Broadcast) {

                void inviteTenant() {
                    Job job = new Job(JobDefOf.JobUseCommsConsoleTenants, __instance);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }
                FloatMenuOption option = new FloatMenuOption("InviteTenant".Translate(), inviteTenant, MenuOptionPriority.InitiateSocial);
                List<FloatMenuOption> list = __result.ToList();
                list.Add(option);
                __result = list.AsEnumerable();
            }
        }

    }
}