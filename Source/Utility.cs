using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace Tenants {
    public static class Utility {
        #region PropertiesFinder
        public static List<PawnKindDef> PawnKindDefList { get; } = new List<PawnKindDef>() {
                PawnKindDefOf.SpaceRefugee,
                PawnKindDefOf.Colonist,
                PawnKindDefOf.Villager };
        public static bool IsTenant(this Pawn pawn) {
            try {
                if (!pawn.AnimalOrWildMan())
                    if (ThingCompUtility.TryGetComp<Tenant>(pawn) != null) {
                        return ThingCompUtility.TryGetComp<Tenant>(pawn).IsTenant;
                    }
                return false;
            }
            catch (Exception ex) {
                Log.Warning(pawn.Name.ToStringShort + ": \n" + ex.Message);
                return false;
            }
        }
        public static int ContractLength(this Pawn pawn) {
            try {
                if (ThingCompUtility.TryGetComp<Tenant>(pawn) != null) {
                    return ThingCompUtility.TryGetComp<Tenant>(pawn).ContractLength;
                }
                return 0;
            }
            catch (Exception ex) {
                Log.Warning(pawn.Name.ToStringShort + ": \n" + ex.Message);
                return 0;
            }
        }
        public static int Payment(this Pawn pawn) {
            try {
                if (ThingCompUtility.TryGetComp<Tenant>(pawn) != null) {
                    return ThingCompUtility.TryGetComp<Tenant>(pawn).Payment;
                }
                return 0;
            }
            catch (Exception ex) {
                Log.Warning(pawn.Name.ToStringShort + ": \n" + ex.Message);
                return 0;
            }
        }
        #endregion PropertiesFinder

        public static void TenantKIA(Pawn pawn) {
            //Do something
        }
        public static bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot) {
            bool validator(IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map);
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }
        public static bool TryFindReachableSpot(Map map, Area area, IntVec3 startPos, out IntVec3 spot) {
            List<IntVec3> list = new List<IntVec3>();
            list.AddRange(area.ActiveCells);
            if (list.Any()) {
                spot = list.FirstOrDefault(x => x.IsValid && map.reachability.CanReach(startPos, x, Verse.AI.PathEndMode.OnCell, TraverseMode.PassDoors));
                return true;
            }
            spot = IntVec3.Invalid;
            return false;
        }
        public static Tenant.Mood CalculateMood(List<Tenant.Mood> moods) {
            float x = 0f, y = 0f, z = 0f;
            foreach (Tenant.Mood mood in moods) {
                if (mood == Tenant.Mood.happy)
                    x++;
                else if (mood == Tenant.Mood.neutral)
                    y++;
                else if (mood == Tenant.Mood.sad)
                    z++;
            }
            if (y / moods.Count >= 0.5f)
                return Tenant.Mood.neutral;
            else if (x > z)
                return Tenant.Mood.happy;
            else
                return Tenant.Mood.sad;
        }
        public static bool RecentMood(List<Tenant.Mood> moods) {
            if (moods.Count > 7) {
                for (int i = moods.Count - 1; i > moods.Count - 6; i--) {
                    if (moods[i] != Tenant.Mood.sad) {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }
        public static void ContractConclusion(Pawn pawn, bool terminated) {

            if (terminated) {
                Messages.Message("ContractDoneTerminated".Translate(pawn.Name.ToStringFull, pawn.TryGetComp<Tenant>().Payment, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                PawnLeave(pawn);
            }
            else {
                Tenant.Mood mood = CalculateMood(pawn.TryGetComp<Tenant>().Moods);
                if (mood == Tenant.Mood.happy) {
                    Messages.Message("ContractDoneHappy".Translate(pawn.Name.ToStringFull, pawn.TryGetComp<Tenant>().Payment * pawn.TryGetComp<Tenant>().ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                    ProlongContract(pawn, SettingsHelper.LatestVersion.StayChanceHappy / 100);
                }
                else if (mood == Tenant.Mood.sad) {
                    Messages.Message("ContractDoneSad".Translate(pawn.Name.ToStringFull, pawn.TryGetComp<Tenant>().Payment * pawn.TryGetComp<Tenant>().ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                    ProlongContract(pawn, SettingsHelper.LatestVersion.StayChanceSad/100);
                }
                else {
                    Messages.Message("ContractDone".Translate(pawn.Name.ToStringFull, pawn.TryGetComp<Tenant>().Payment * pawn.TryGetComp<Tenant>().ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                    ProlongContract(pawn, SettingsHelper.LatestVersion.StayChanceNeutral / 100);
                }
            }
            DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, pawn.Position, pawn.TryGetComp<Tenant>().ContractLength / 60000 * pawn.TryGetComp<Tenant>().Payment);
        }
        public static void ProlongContract(Pawn pawn, float chance) {
            if (GenMath.RoundRandom(chance) > 0) {
                Tenant tenantComp = pawn.TryGetComp<Tenant>();
                string text = "RequestForTenancyContinued".Translate(pawn.Name.ToStringFull, pawn.story.Title, pawn.ageTracker.AgeBiologicalYears, tenantComp.Payment, tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                text = text.AdjustedFor(pawn);
                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);

                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("RequestForTenancy_Accept".Translate()) {
                    action = delegate {
                        //Accepted offer.
                        tenantComp.ContractDate = Find.TickManager.TicksGame;
                        tenantComp.Moods.Clear();
                    },
                    resolveTree = true,
                };
                diaNode.options.Add(diaOption);
                //Denied tenant offer
                string text2 = "RequestForTenancyContinuedRejected".Translate(pawn.LabelShort, pawn, pawn.Named("PAWN"));
                DiaNode diaNode2 = new DiaNode(text2);
                DiaOption diaOption2 = new DiaOption("OK".Translate()) {
                    resolveTree = true
                };
                diaNode2.options.Add(diaOption2);
                DiaOption diaOption3 = new DiaOption("RequestForTenancy_Reject".Translate()) {
                    action = delegate {
                        PawnLeave(pawn);
                    },
                    link = diaNode2
                };
                diaNode.options.Add(diaOption3);
                string title = "RequestForTenancyTitle".Translate(pawn.Map.Parent.Label);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
                Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            }
            else {
                PawnLeave(pawn);
            }
        }
        public static bool GenerateNewContract(Map map) {
            if (!TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return false;
            }
            //Finds all generated tenants except those already spawned, creates and selects one for the event.
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.IsTenant() && !p.Dead && !p.Spawned && !p.Discarded
                                select p).ToList();
            if (pawns.Count < 10) {
                //GENERATION CONTEXT
                PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefList[Rand.Range(0, PawnKindDefList.Count)], null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 10f);
                Pawn newTenant = PawnGenerator.GeneratePawn(request);
                newTenant.TryGetComp<Tenant>().IsTenant = true;
                pawns.Add(newTenant);
            }
            pawns.Shuffle();
            Pawn pawn = pawns[0];
            pawn.relations.everSeenByPlayer = true;
            Tenant tenantComp = pawn.TryGetComp<Tenant>();

            tenantComp.Payment = Rand.Range(SettingsHelper.LatestVersion.MinDailyCost, SettingsHelper.LatestVersion.MaxDailyCost);
            tenantComp.ContractDate = Find.TickManager.TicksGame;
            tenantComp.ContractLength = Rand.Range(SettingsHelper.LatestVersion.MinContractTime, SettingsHelper.LatestVersion.MaxContractTime) * 60000;
            tenantComp.IsTenant = true;
            tenantComp.Moods.Clear();

            //Generates event
            string text = "RequestForTenancyInitial".Translate(pawn.Name.ToStringFull, pawn.story.Title, pawn.ageTracker.AgeBiologicalYears, tenantComp.Payment, tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("RequestForTenancy_Accept".Translate()) {
                action = delegate {
                    //Accepted offer, generating tenant.
                    pawn.SetFaction(Faction.OfPlayer);
                    GenSpawn.Spawn(pawn, spawnSpot, map);
                    pawn.needs.SetInitialLevels();
                    pawn.playerSettings.AreaRestriction = map.areaManager.Home;
                    pawn.workSettings.DisableAll();
                    pawn.timetable = new Pawn_TimetableTracker(pawn) {
                        times = new List<TimeAssignmentDef>(24)
                    };
                    for (int i = 0; i < 24; i++) {
                        pawn.timetable.times.Add(TimeAssignmentDefOf.Joy);
                    }
                    CameraJumper.TryJump(pawn);
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOption);
            //Denied tenant offer
            string text2 = "RequestForTenancyRejected".Translate(pawn.LabelShort, pawn, pawn.Named("PAWN"));
            DiaNode diaNode2 = new DiaNode(text2);
            DiaOption diaOption2 = new DiaOption("OK".Translate()) {
                resolveTree = true
            };
            diaNode2.options.Add(diaOption2);
            DiaOption diaOption3 = new DiaOption("RequestForTenancy_Reject".Translate()) {
                action = delegate {
                    Find.WorldPawns.PassToWorld(pawn);
                    tenantComp.Payment = 0;
                    tenantComp.ContractLength = 0;
                },
                link = diaNode2
            };
            diaNode.options.Add(diaOption3);
            string title = "RequestForTenancyTitle".Translate(map.Parent.Label);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));

            return true;
        }
        public static void PawnLeave(Pawn pawn) {
            pawn.SetFaction(null);
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static List<Pawn> RemoveTenantsFromList(List<Pawn> pawns) {
            List<Pawn> tenants = new List<Pawn>();
            foreach (Pawn pawn in pawns) {
                if (pawn.IsTenant())
                    tenants.Add(pawn);
            }
            foreach (Pawn pawn in tenants) {
                pawns.Remove(pawn);
            }
            return pawns;
        }
    }
}
