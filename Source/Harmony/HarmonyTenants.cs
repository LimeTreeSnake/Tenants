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
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_NeedsTracker), "ShouldHaveNeed"), new HarmonyMethod(typeof(HarmonyTenants).GetMethod("ShouldHaveNeed_PostFix")), null);
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
            if (__instance.Spawned && !__instance.NonHumanlikeOrWildMan()) {
                ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(__instance);
                if (contract != null) {
                    Controllers.TenantController.TenantTick(__instance);
                    WantedComp wanted = ThingCompUtility.TryGetComp<WantedComp>(__instance);
                    if (wanted != null) {
                        Controllers.WantedController.Tick(__instance, wanted, contract);
                        return;
                    }
                    WandererComp wanderer = ThingCompUtility.TryGetComp<WandererComp>(__instance);
                    if (wanderer != null) {
                        Controllers.WandererController.Tick(__instance, wanderer, contract);
                        return;
                    }
                    EnvoyComp envoy = ThingCompUtility.TryGetComp<EnvoyComp>(__instance);
                    if (envoy != null) {
                        Controllers.EnvoyController.Tick(__instance, envoy, contract);
                        return;
                    }
                }
            }
        }
        #endregion Ticks
        #region Functionality
        public static void CapturedBy_PreFix(ref Pawn_GuestTracker __instance, ref Faction by, ref Pawn byPawn) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            ContractComp contractComp = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (contractComp != null) {
                Controllers.TenantController.TenantCaptured(pawn, byPawn);
            }
        }
        public static void Kill_PreFix(ref Pawn __instance, ref DamageInfo? dinfo) {
            ContractComp contractComp = ThingCompUtility.TryGetComp<ContractComp>(__instance);
            if (contractComp != null) {
                if (__instance.IsPrisoner && !__instance.guest.Released && __instance.Spawned) {
                    Controllers.TenantController.TenantDeath(__instance);
                }
            }
        }
        #endregion Functionality
        #region GUI
        public static bool PawnNameColorOf_PreFix(ref Color __result, ref Pawn pawn) {
            ContractComp contractComp = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (contractComp != null) {
                __result = Settings.Settings.Color;
                return false;
            }
            return true;
        }
        public static void GetFloatMenuOptions_PostFix(Building_CommsConsole __instance, ref IEnumerable<FloatMenuOption> __result, Pawn myPawn) {
            List<FloatMenuOption> list = __result.ToList();
            if (!TenantsMapComp.GetComponent(myPawn.Map).Broadcast) {
                void inviteTenant() {
                    Job job = new Job(Defs.JobDefOf.JobUseCommsConsoleTenants);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }
                FloatMenuOption inviteTenants = new FloatMenuOption("InviteTenant".Translate(), inviteTenant, MenuOptionPriority.InitiateSocial);
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