using Harmony;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.Controllers {
    public static class TenantController {

        public static void TenantTick(Pawn pawn) {
            if (pawn.IsColonist) {
                RemoveAllComp(pawn);
                return;
            }
        }
        public static Pawn GetContractedPawn(Faction faction = null) {
            Pawn tenant;
            if (faction != null)
                tenant = FindRandomPawn(faction);
            else
                tenant = FindRandomPawn();
            if (tenant == null)
                return null;
            tenant.relations.everSeenByPlayer = true;
            ContractComp comp = ContractController.GenerateContract(tenant);
            if (faction != null)
                comp.Payment = 0;
            return tenant;
        }
        public static void Leave(Pawn pawn) {
            RemoveAllComp(pawn);
            pawn.jobs.ClearQueuedJobs();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
            if (TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Remove(pawn);
            }
        }
        public static void Theft(Pawn pawn) {
            RemoveAllComp(pawn);
            pawn.jobs.ClearQueuedJobs();
            ThingCompUtility.TryGetComp<WandererComp>(pawn);
            LordMaker.MakeNewLord(pawn.Faction, new LordJobs.LordJob_TenantTheft(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantDeath(Pawn pawn) {
            string label = "Death".Translate() + ": " + pawn.LabelShortCap;
            string text = "TenantDeath".Translate(pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            if (pawn.Faction.def != FactionDefOf.Ancients) {
                int val = Utilities.FactionUtilities.ChangeRelations(pawn.Faction, true);
                val += Utilities.FactionUtilities.ChangeRelations(pawn.Faction, true);
                Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Death, pawn);
        }
        public static void TenantCaptured(Pawn pawn, Pawn byPawn) {
            if (pawn.HostileTo(Find.FactionManager.OfPlayer)) {
                return;
            }
            string text = "TenantCaptured".Translate(pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            string label = "Captured".Translate() + ": " + pawn.LabelShortCap;
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, pawn);
            if (Rand.Value < 0.66f) {
                int val = Utilities.FactionUtilities.ChangeRelations(pawn.Faction, true);
                Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }
            RemoveAllComp(pawn);
        }
        public static void TenantWantToJoin(Pawn pawn) {
            WandererComp tenantComp = ThingCompUtility.TryGetComp<WandererComp>(pawn);
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (tenantComp.MayJoin) {
                string text = "RequestWantToJoin".Translate(pawn.Named("PAWN"));

                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        ContractController.ContractPayment(pawn);
                        Messages.Message("ContractDone".Translate(pawn.Name.ToStringFull, contract.Payment * contract.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                        Find.ColonistBar.MarkColonistsDirty();
                    },
                    resolveTree = true,
                };
                diaNode.options.Add(diaOption);
                //Denied offer
                string text2 = "RequestWantToJoinRejected".Translate(pawn.Named("PAWN"));
                DiaNode diaNode2 = new DiaNode(text2);
                DiaOption diaOption2 = new DiaOption("OK".Translate()) {
                    resolveTree = true
                };
                diaNode2.options.Add(diaOption2);
                DiaOption diaOption3 = new DiaOption("ContractReject".Translate()) {
                    action = delegate {
                    },
                    link = diaNode2
                };
                diaNode.options.Add(diaOption3);
                string title = "RequestFromTenant".Translate(pawn.Map.Parent.Label);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
                Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            }
        }
        public static void SpawnTenant(Pawn pawn, Map map, IntVec3 spawnSpot) {
            pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
            GenSpawn.Spawn(pawn, spawnSpot, map);
            pawn.needs.SetInitialLevels();
            CameraJumper.TryJump(pawn);
        }
        public static void TenantInvite(Building_CommsConsole comms, Pawn pawn) {
            Messages.Message("InviteTenantMessage".Translate(), MessageTypeDefOf.NeutralEvent);
            TenantsMapComp.GetComponent(pawn.Map).Broadcast = true;
            if (Rand.Value < 0.20f) {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.Opportunists, Find.TickManager.TicksGame + Rand.Range(25000, 150000), parms, 240000);
            }
            else {
                IncidentParms parms = new IncidentParms() { target = pawn.Map, forced = true };
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.WandererProposition, Find.TickManager.TicksGame + Rand.Range(15000, 120000), parms, 240000);
            }

        }
        public static Pawn FindRandomPawn() {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where Settings.Settings.AvailableRaces.Contains(p.kindDef.race.defName) && !p.Faction.HostileTo(Find.FactionManager.OfPlayer) && !p.Dead && !p.Spawned && !p.Discarded && !PawnUtility.IsFactionLeader(p)
                                select p).ToList();
            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns.RandomElement();
        }
        public static Pawn FindRandomPawn(Faction faction) {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.Faction == faction && Settings.Settings.AvailableRaces.Contains(p.kindDef.race.defName) && !p.Faction.HostileTo(Find.FactionManager.OfPlayer) && !p.Dead && !p.Spawned && !p.Discarded && !PawnUtility.IsFactionLeader(p)
                                select p).ToList();
            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns.RandomElement();
        }
        public static void RemoveAllComp(Pawn pawn) {
            pawn.AllComps.Remove(ThingCompUtility.TryGetComp<ContractComp>(pawn));
            pawn.AllComps.Remove(ThingCompUtility.TryGetComp<WantedComp>(pawn));
            pawn.AllComps.Remove(ThingCompUtility.TryGetComp<TenantComp>(pawn));
            pawn.AllComps.Remove(ThingCompUtility.TryGetComp<EnvoyComp>(pawn));
        }
    }
}
