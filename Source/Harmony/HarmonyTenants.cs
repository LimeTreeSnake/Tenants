using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Tenants.Comps;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Tenants {
    [StaticConstructorOnStartup]
    internal static class HarmonyTenants {
        static HarmonyTenants() {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("rimworld.limetreesnake.tenants");
            #region Ticks
            //Tenant Tick
            //harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "Tick"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("Tick_PostFix")));
            //Tenant TickRare
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "TickRare"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("TickRare_PostFix")));
            #endregion Ticks
            #region Functionality
            //What happens when you capture a tenant 
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "CapturedBy"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CapturedBy_PreFix")), null);
            //Tenant dies
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "Kill"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("Kill_PreFix")), null);
            #endregion Functionality
            #region GUI
            //Pawn name color patch
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnNameColorUtility), "PawnNameColorOf"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("PawnNameColorOf_PostFix")));
            //Comms Console Float Menu Option
            harmonyInstance.Patch(AccessTools.Method(typeof(Building_CommsConsole), "GetFloatMenuOptions"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetFloatMenuOptions_PostFix")));
            #endregion GUI

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs) {
                if (def.race != null) {
                    def.comps.Add(new CompProps_Tenant());
                    def.comps.Add(new CompProps_Courier());
                }
            }
        }
        #region Ticks
        public static void TickRare_PostFix(ref Pawn __instance) {
            if (__instance.Spawned && !__instance.NonHumanlikeOrWildMan()) {
                TenantComp comp = ThingCompUtility.TryGetComp<TenantComp>(__instance);
                if (comp.Contract != null) {
                    Utilities.TenantUtilities.TenantTick(__instance, comp);                   
                }
            }
        }
        #endregion Ticks
        #region Functionality
        public static void CapturedBy_PreFix(ref Pawn_GuestTracker __instance, ref Faction by, ref Pawn byPawn) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            TenantComp comp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (comp.Contract != null) {
                Utilities.TenantUtilities.TenantCaptured(pawn, byPawn, comp);
            }
        }
        public static void Kill_PreFix(ref Pawn __instance, ref DamageInfo? dinfo) {
            TenantComp comp = ThingCompUtility.TryGetComp<TenantComp>(__instance);
            if (comp.Contract != null) {
                if (__instance.IsPrisoner && !__instance.guest.Released && __instance.Spawned) {
                    Utilities.TenantUtilities.TenantDeath(__instance, comp);
                }
            }
        }
        #endregion Functionality
        #region GUI
        public static void PawnNameColorOf_PostFix(ref Color __result, ref Pawn pawn) {
            TenantComp comp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (comp.Contract != null) {
                __result = Settings.Settings.Color;
            }
        }
        public static void GetFloatMenuOptions_PostFix(Building_CommsConsole __instance, ref IEnumerable<FloatMenuOption> __result, Pawn myPawn) {
            List<FloatMenuOption> list = __result.ToList();
            if (!TenantsMapComp.GetComponent(myPawn.Map).Broadcast) {
                void inviteTenant() {
                    Job job = new Job(Defs.JobDefOf.JobUseCommsConsoleTenants);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }
                FloatMenuOption inviteTenants = new FloatMenuOption("TenantInvite".Translate(), inviteTenant, MenuOptionPriority.InitiateSocial);
                list.Add(inviteTenants);
            }
            if (!TenantsMapComp.GetComponent(myPawn.Map).BroadcastCourier) {
                void inviteCourier() {
                    Job job = new Job(Defs.JobDefOf.JobUseCommsConsoleInviteCourier, __instance);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }
                FloatMenuOption inviteCouriers = new FloatMenuOption("CourierInvite".Translate(Settings.Settings.CourierCost), inviteCourier, MenuOptionPriority.InitiateSocial);
                list.Add(inviteCouriers);
            }
            __result = list.AsEnumerable();
        }
        #endregion GUI
    }
}