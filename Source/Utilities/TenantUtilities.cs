using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Verse;
using Verse.AI.Group;
using Tenants.Controllers;

namespace Tenants.Utilities {
    public static class TenantUtilities {
        public static void TenantTick(Pawn tenant, TenantComp comp) {
            if (tenant.IsColonist) {
                CleanComp(comp);
                return;
            }
            switch (comp.Tenancy) {
                case TenancyType.None:
                    break;
                case TenancyType.Tenant:
                    TenantController.Tick(tenant, comp);
                    break;
                case TenancyType.Wanted:
                    WantedController.Tick(tenant, comp);
                    break;
                case TenancyType.Envoy:
                    //EnvoyController.Tick(tenant, comp);
                    break;
                default:
                    break;
            }

        }
        public static Pawn CreateContractedPawn(out TenantComp comp, Faction faction = null) {
            comp = null;
            Pawn tenant;
            if (faction != null)
                tenant = FindRandomPawn(faction);
            else
                tenant = FindRandomPawn();
            if (tenant == null)
                return null;
            tenant.relations.everSeenByPlayer = true;
            comp = ThingCompUtility.TryGetComp<TenantComp>(tenant);
            comp.Contract = ContractUtility.GenerateContract();
            //if (faction != null) // Is envoy
            //    comp.Contract.Payment = 0;
            return tenant;
        }
        public static void Leave(Pawn pawn, TenantComp comp) {
            CleanComp(comp);
            pawn.jobs.ClearQueuedJobs();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void Theft(Pawn pawn, TenantComp comp) {
            CleanComp(comp);
            pawn.jobs.ClearQueuedJobs();
            LordMaker.MakeNewLord(pawn.Faction, new LordJobs.LordJob_TenantTheft(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantDeath(Pawn pawn, TenantComp comp) {
            string label = "Death".Translate() + ": " + pawn.LabelShortCap;
            string text = "TenantDeath".Translate(pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            if (pawn.Faction.def != FactionDefOf.Ancients) {
                int val = FactionUtilities.ChangeRelations(pawn.Faction, true);
                val += FactionUtilities.ChangeRelations(pawn.Faction, true);
                Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Death, pawn);
            CleanComp(comp);
        }
        public static void TenantCaptured(Pawn pawn, Pawn byPawn,TenantComp comp) {
            if (pawn.HostileTo(Find.FactionManager.OfPlayer)) {
                return;
            }
            string text = "TenantCaptured".Translate(pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            string label = "Captured".Translate() + ": " + pawn.LabelShortCap;
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, pawn);
            if (pawn.Faction.def != FactionDefOf.Ancients) {
                int val = FactionUtilities.ChangeRelations(pawn.Faction, true);
                Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }
            CleanComp(comp);
        }
        public static void SpawnTenant(Pawn pawn, Map map, IntVec3 spawnSpot) {
            pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
            GenSpawn.Spawn(pawn, spawnSpot, map);
            pawn.needs.SetInitialLevels();
            CameraJumper.TryJump(pawn);
        }
        private static Pawn FindRandomPawn() {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where Settings.Settings.AvailableRaces.Contains(p.kindDef.race.defName) && !p.Faction.HostileTo(Find.FactionManager.OfPlayer) && !p.Dead && !p.Spawned && !p.Discarded && !PawnUtility.IsFactionLeader(p)
                                select p).ToList();
            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns.RandomElement();
        }
        private static Pawn FindRandomPawn(Faction faction) {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.Faction == faction && Settings.Settings.AvailableRaces.Contains(p.kindDef.race.defName) && !p.Faction.HostileTo(Find.FactionManager.OfPlayer) && !p.Dead && !p.Spawned && !p.Discarded && !PawnUtility.IsFactionLeader(p)
                                select p).ToList();
            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns.RandomElement();
        }
        public static void CleanComp(TenantComp comp) {
            comp.Tenancy = TenancyType.None;
            comp.Contract = null;
        }
    }
}
