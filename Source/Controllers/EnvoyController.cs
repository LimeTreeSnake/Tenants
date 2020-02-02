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

        public static void Tick(Pawn tenant, EnvoyComp comp, ContractComp contract) {
            if (contract.IsTerminated) {
                int damage = Rand.Range(Settings.Settings.MinRelation, Settings.Settings.MaxRelation);
                Find.LetterStack.ReceiveLetter("ContractBreach".Translate(), "ContractDoneEnvoyTerminated".Translate(tenant.Faction, damage, tenant.Named("PAWN")), LetterDefOf.NeutralEvent);
                TenantController.Leave(tenant);
            }
            //Tenant alone with no colonist
            if (tenant.Map.mapPawns.FreeColonists.FirstOrDefault(x => ThingCompUtility.TryGetComp<WandererComp>(x) == null) == null) {
                contract.IsTerminated = true;
                ContractConclusion(tenant, comp, contract);
                return;
            }
            //Tenant contract is out
            if (Find.TickManager.TicksGame >= contract.ContractEndTick) {
                ContractConclusion(tenant, comp, contract);
                return;
            }
            //Tenancy tick 1/10 per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
            }
        }
        public static void ContractConclusion(Pawn pawn, EnvoyComp comp, ContractComp contract) {
            if (contract.IsTerminated) {

            }
        }
        public static void Contract(Map map, Faction faction) {
            if (!Utilities.MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return;
            }
            Pawn tenant = TenantController.GetContractedPawn(faction);
            if (tenant == null) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return;
            }
            tenant.AllComps.Add(new EnvoyComp());
            TenantController.SpawnTenant(tenant, map, spawnSpot);
            Messages.Message("EnvoyArriveSuccess".Translate(faction, tenant.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            CameraJumper.TryJump(tenant);
        }
    }
}
