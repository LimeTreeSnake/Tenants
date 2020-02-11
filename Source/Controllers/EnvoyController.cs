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
using Tenants.Utilities;

namespace Tenants.Controllers {
    public static class EnvoyController {

        public static void Tick(Pawn tenant, TenantComp comp) {
            if (comp.Contract.IsTerminated) {
                int damage = Rand.Range(Settings.Settings.MinRelation, Settings.Settings.MaxRelation);
                Find.LetterStack.ReceiveLetter("ContractBreach".Translate(), "ContractDoneEnvoyTerminated".Translate(tenant.Faction, damage, tenant.Named("PAWN")), LetterDefOf.NeutralEvent);
                TenantUtilities.Leave(tenant, comp);
            }
            //Tenant alone with no colonist
            if (tenant.Map.mapPawns.FreeColonists.FirstOrDefault(x => ThingCompUtility.TryGetComp<TenantComp>(x) == null) == null) {
                comp.Contract.IsTerminated = true;
                ContractConclusion(tenant, comp);
                return;
            }
            //Tenant contract is out
            if (Find.TickManager.TicksGame >= comp.Contract.ContractEndTick) {
                ContractConclusion(tenant, comp);
                return;
            }
            //Tenancy tick 1/10 per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
            }
        }
        public static void ContractConclusion(Pawn pawn, TenantComp comp) {
            if (comp.Contract.IsTerminated) {

            }
        }
        public static void Contract(Map map, Faction faction) {
            if (!MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return;
            }
            Pawn tenant = TenantUtilities.CreateContractedPawn(out TenantComp comp, faction);
            if (tenant == null) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return;
            }
            tenant.AllComps.Add(new TenantComp());
            TenantUtilities.SpawnTenant(tenant, map, spawnSpot);
            Messages.Message("EnvoyArriveSuccess".Translate(faction, tenant.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            CameraJumper.TryJump(tenant);
        }
    }
}
