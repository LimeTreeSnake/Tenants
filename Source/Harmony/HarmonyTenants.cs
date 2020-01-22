using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using UnityEngine;
using Verse;
using Verse.AI;
using static RimWorld.ColonistBar;

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
            //Tenant Inspiration
            harmonyInstance.Patch(AccessTools.Method(typeof(InspirationWorker), "InspirationCanOccur"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("InspirationCanOccur_PreFix")), null);
            #endregion Functionality
            #region GUI
            //Pawn name color patch
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnNameColorUtility), "PawnNameColorOf"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("PawnNameColorOf_PreFix")), null);
            //Comms Console Float Menu Option
            harmonyInstance.Patch(AccessTools.Method(typeof(Building_CommsConsole), "GetFloatMenuOptions"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetFloatMenuOptions_PostFix")));
            #endregion GUI
        }
        #region Ticks
        public static void TickRare_PostFix(ref Pawn __instance) {
            Utilities.UtilityTenant.TenancyCheck(__instance);
        }
        #endregion Ticks
        #region Functionality
        public static void CapturedBy_PreFix(ref Pawn_GuestTracker __instance,ref Faction by,ref Pawn byPawn) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            Tenant tenantComp = ThingCompUtility.TryGetComp<Tenant>(pawn);
            if (tenantComp != null) {
                Mole mole = ThingCompUtility.TryGetComp<Mole>(pawn);
                if (mole != null && mole.Activated) {
                    Utilities.UtilityTenant.TenantMoleCaptured(pawn);
                }
                else {
                    Utilities.UtilityTenant.TenantCaptured(pawn, byPawn);
                }
            }
        }
        public static void Kill_PreFix(ref Pawn __instance, ref DamageInfo? dinfo) {
            Tenant tenantComp = ThingCompUtility.TryGetComp<Tenant>(__instance);
            if (tenantComp != null)
                if ((tenantComp.Contracted || tenantComp.CapturedTenant && !__instance.guest.Released) && __instance.Spawned) {
                    Utilities.UtilityTenant.TenantDeath(__instance);
                }
        }
        public static bool InspirationCanOccur_PreFix(ref Pawn pawn) {
            Tenant tenantComp = ThingCompUtility.TryGetComp<Tenant>(pawn);
            if (tenantComp != null)
                    return false;
            return true;
        }
        #endregion Functionality
        #region GUI
        public static bool PawnNameColorOf_PreFix(ref Color __result, ref Pawn pawn) {
            if (pawn.IsColonist) {
                Tenant tenantComp = ThingCompUtility.TryGetComp<Tenant>(pawn);
                if (tenantComp != null) {
                    __result = Settings.SettingsHelper.LatestVersion.Color;
                    return false;
                }
            }
            return true;
        }
        public static void GetFloatMenuOptions_PostFix(Building_CommsConsole __instance, ref IEnumerable<FloatMenuOption> __result, Pawn myPawn) {
            List<FloatMenuOption> list = __result.ToList();
            if (!MapComponent_Tenants.GetComponent(myPawn.Map).Broadcast) {
                void inviteTenant() {
                    Job job = new Job(Defs.JobDefOf.JobUseCommsConsoleTenants);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }
                FloatMenuOption inviteTenants = new FloatMenuOption("InviteTenant".Translate(), inviteTenant, MenuOptionPriority.InitiateSocial);
                list.Add(inviteTenants);
            }
            if (!MapComponent_Tenants.GetComponent(myPawn.Map).BroadcastCourier) {
                void inviteCourier() {
                    Job job = new Job(Defs.JobDefOf.JobUseCommsConsoleInviteCourier, __instance);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }
                FloatMenuOption inviteCouriers = new FloatMenuOption("CourierInvite".Translate(Settings.SettingsHelper.LatestVersion.CourierCost), inviteCourier, MenuOptionPriority.InitiateSocial);
                list.Add(inviteCouriers);
            }
            __result = list.AsEnumerable();
        }
        #endregion GUI
    }
}