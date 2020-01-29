using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.Controllers {
    public static class EnvoyController {

        public static void Tick(Pawn pawn, EnvoyComp comp) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (contract == null || pawn.IsColonist) {
                TenantController.RemoveAllComp(pawn);
                return;
            }
            //Tenant alone with no colonist
            if (pawn.Map.mapPawns.FreeColonists.FirstOrDefault(x => ThingCompUtility.TryGetComp<WandererComp>(x) == null) == null) {
                contract.IsTerminated = true;
                ContractConclusion(pawn);
                return;
            }
            //Tenant contract is out
            if (Find.TickManager.TicksGame >= contract.ContractEndTick) {
                ContractConclusion(pawn);
                return;
            }

            //Tenancy tick 1/10 per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
            }
        }
        public static void ContractConclusion(Pawn pawn) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            EnvoyComp comp = ThingCompUtility.TryGetComp<EnvoyComp>(pawn);
            if (contract == null || comp == null)
                return;
            if (contract.IsTerminated) {
            }
        }
        public static bool Contract(Map map, Faction faction) {
            if (!Utilities.MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return false;
            }
            Pawn pawn = TenantController.FindRandomPawn(faction);
            if (pawn == null) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return false;
            }
            EnvoyComp comp = Generate(pawn);
            if (comp == null) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return false;
            }
            pawn.relations.everSeenByPlayer = true;
            ContractComp contract = ContractController.GenerateContract(pawn);
            contract.Payment = 0;
            TenantController.SpawnTenant(pawn, map, spawnSpot);
            Messages.Message("EnvoyArriveSuccess".Translate(faction, pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            CameraJumper.TryJump(pawn);
            return true;
        }
        public static EnvoyComp Generate(Pawn pawn) {
            EnvoyComp comp = new EnvoyComp();
            pawn.AllComps.Add(comp);
            return comp;
        }
    }
}
