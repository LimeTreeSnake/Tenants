using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace Tenants {
    public static class Utility {
        #region Fields
        private static readonly List<PawnKindDef> pawnKindDefList = new List<PawnKindDef>() {
                PawnKindDefOf.SpaceRefugee,
                PawnKindDefOf.Colonist,
                PawnKindDefOf.Villager };
        #endregion Fields
        #region PropertiesFinder
        public static List<PawnKindDef> GetPawnKindDefList() => pawnKindDefList;
        public static bool IsTenant(this Pawn pawn) {
            try {
                if (!pawn.AnimalOrWildMan()) {
                    Tenant tenantComp = GetTenantComponent(pawn);
                    if (tenantComp != null) {
                        return tenantComp.IsTenant;
                    }
                }
                return false;
            }
            catch (Exception ex) {
                Log.Warning(pawn.Name.ToStringShort + ": \n" + ex.Message);
                return false;
            }
        }
        public static bool IsTerminated(this Pawn pawn) {
            try {
                if (!pawn.AnimalOrWildMan()) {
                    Tenant tenantComp = GetTenantComponent(pawn);
                    if (tenantComp != null) {
                        return tenantComp.IsTerminated;
                    }
                }
                return false;
            }
            catch (Exception ex) {
                Log.Warning(pawn.Name.ToStringShort + ": \n" + ex.Message);
                return false;
            }
        }

        public static void SetTenant(this Pawn pawn, bool tenancy) {
            Tenant tenantComp = GetTenantComponent(pawn);
            if (tenantComp != null) {
                tenantComp.IsTenant = tenancy;
            }
        }
        
        public static void SetTerminated(this Pawn pawn, bool terminated) {
            Tenant tenantComp = GetTenantComponent(pawn);
            if (tenantComp != null) {
                tenantComp.IsTerminated = terminated;
            }
        }
        public static Tenant GetTenantComponent(this Pawn pawn) {
            if (ThingCompUtility.TryGetComp<Tenant>(pawn) != null) {
                return ThingCompUtility.TryGetComp<Tenant>(pawn);
            }
            return null;
        }
        public static int ContractLength(this Pawn pawn) {
            try {
                Tenant tenantComp = GetTenantComponent(pawn);
                if (tenantComp != null) {
                    return tenantComp.ContractLength;
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
                Tenant tenantComp = GetTenantComponent(pawn);
                if (tenantComp != null) {
                    return tenantComp.Payment;
                }
                return 0;
            }
            catch (Exception ex) {
                Log.Warning(pawn.Name.ToStringShort + ": \n" + ex.Message);
                return 0;
            }
        }
        #endregion PropertiesFinder
        #region Methods
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
        public static int CalculateMood(Tenant tenant) {
            float count = tenant.HappyMoodCount + tenant.NeutralMoodCount + tenant.SadMoodCount;

            if (tenant.NeutralMoodCount / count >= 0.5f)
                return 0;
            else if (tenant.HappyMoodCount > tenant.SadMoodCount)
                return 1;
            else
                return -1;
        }
        public static void ContractConclusion(Pawn pawn, bool terminated) {
            Tenant tenantComp = GetTenantComponent(pawn);
            if (terminated) {
                if (Rand.Value < 0.6f) {
                    Messages.Message("ContractDoneTerminated".Translate(pawn.Name.ToStringFull, tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
                    TenantLeave(pawn);
                    DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, pawn.Position, tenantComp.ContractLength / 60000 * tenantComp.Payment);
                }
                else {
                    Messages.Message("ContractDoneTheft".Translate(pawn.Name.ToStringFull, tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                    TenantTheft(pawn);
                }
            }
            else {
                int mood = CalculateMood(GetTenantComponent(pawn));
                if (mood == 1) {
                    Messages.Message("ContractDoneHappy".Translate(pawn.Name.ToStringFull, tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                    ProlongContract(pawn, SettingsHelper.LatestVersion.StayChanceHappy / 100);
                }
                else if (mood == -1) {
                    Messages.Message("ContractDoneSad".Translate(pawn.Name.ToStringFull, tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
                    ProlongContract(pawn, SettingsHelper.LatestVersion.StayChanceSad / 100);
                }
                else {
                    Messages.Message("ContractDone".Translate(pawn.Name.ToStringFull, tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                    ProlongContract(pawn, SettingsHelper.LatestVersion.StayChanceNeutral / 100);
                }
                DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, pawn.Position, tenantComp.ContractLength / 60000 * tenantComp.Payment);
            }
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
                        tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
                        tenantComp.ResetMood();
                    },
                    resolveTree = true,
                };
                diaNode.options.Add(diaOption);
                //Denied offer
                string text2 = "RequestForTenancyContinuedRejected".Translate(pawn.LabelShort, pawn, pawn.Named("PAWN"));
                DiaNode diaNode2 = new DiaNode(text2);
                DiaOption diaOption2 = new DiaOption("OK".Translate()) {
                    resolveTree = true
                };
                diaNode2.options.Add(diaOption2);
                DiaOption diaOption3 = new DiaOption("RequestForTenancy_Reject".Translate()) {
                    action = delegate {
                        TenantLeave(pawn);
                    },
                    link = diaNode2
                };
                diaNode.options.Add(diaOption3);
                string title = "RequestForTenancyTitle".Translate(pawn.Map.Parent.Label);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
                Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            }
            else {
                TenantLeave(pawn);
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
                PawnGenerationRequest request = new PawnGenerationRequest(GetPawnKindDefList()[Rand.Range(0, GetPawnKindDefList().Count)], null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 10f);
                Pawn newTenant = PawnGenerator.GeneratePawn(request);
                newTenant.SetFaction(Faction.OfAncients);
                SetTenant(newTenant, true);
                pawns.Add(newTenant);
            }
            pawns.Shuffle();
            Pawn pawn = pawns[0];
            pawn.relations.everSeenByPlayer = true;
            Tenant tenantComp = pawn.TryGetComp<Tenant>();

            tenantComp.Payment = Rand.Range(SettingsHelper.LatestVersion.MinDailyCost, SettingsHelper.LatestVersion.MaxDailyCost); tenantComp.ContractLength = Rand.Range(SettingsHelper.LatestVersion.MinContractTime, SettingsHelper.LatestVersion.MaxContractTime) * 60000;
            tenantComp.ContractDate = Find.TickManager.TicksGame;
            tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
            tenantComp.ResetMood();

            //Generates event
            string text = "RequestForTenancyInitial".Translate(pawn.def.label, pawn.ageTracker.AgeBiologicalYears, tenantComp.Payment, tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            text = AppendPawnDescription(text, pawn);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("RequestForTenancy_Accept".Translate()) {
                action = delegate {
                    //Accepted offer, generating tenant.
                    pawn.SetFaction(Faction.OfPlayer);
                    GenSpawn.Spawn(pawn, spawnSpot, map);
                    pawn.needs.SetInitialLevels();
                    pawn.playerSettings.AreaRestriction = map.areaManager.Home;
                    UpdateTenantWork(pawn);
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
                    if (!Find.WorldPawns.Contains(pawn))
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
        public static void TenantLeave(Pawn pawn) {
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantCancelContract(Pawn pawn) {
            Messages.Message("ContractDonePlayerTerminated".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantTheft(Pawn pawn) {
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            pawn.SetTenant(false);
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_TenantTheft(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantDeath(Pawn pawn) {
            if (Rand.Value < 0.33f) {
                MapComponent_Tenants.GetComponent(pawn.Map).DeadTenantsToAvenge.Add(pawn);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = RaidStrategyDefOf.Retribution;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.Retribution, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
            }
        }
        public static void TenantCaptured(Pawn pawn) {
            //NOTHING ATM
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
        public static IEnumerable<GlobalTargetInfo> RemoveTenantsFromList(ref IEnumerable<GlobalTargetInfo> pawns) {
            List<GlobalTargetInfo> tenants = new List<GlobalTargetInfo>();
            foreach (GlobalTargetInfo pawn in pawns) {
                if (pawn.Thing.TryGetComp<Tenant>().IsTenant) {
                    tenants.Add(pawn);
                }
            }
            List<GlobalTargetInfo> list = pawns.ToList();
            foreach (GlobalTargetInfo pawn in tenants) {
                list.Remove(pawn);
            }
            pawns = list.AsEnumerable();
            return pawns;
        }

        public static bool EmergencyWork(WorkGiver giver) {
            if (giver is WorkGiver_PatientGoToBedEmergencyTreatment || giver is WorkGiver_PatientGoToBedTreatment) {
                if (SettingsHelper.LatestVersion.Patient && !SettingsHelper.LatestVersion.PatientHappy)
                    return true;
            }
            else if (giver is WorkGiver_PatientGoToBedRecuperate) {
                if (SettingsHelper.LatestVersion.PatientBedRest && !SettingsHelper.LatestVersion.PatientBedRestHappy)
                    return true;
            }
            else if (giver.def.workTags == WorkTags.Firefighting) {
                if (SettingsHelper.LatestVersion.Firefighter && !SettingsHelper.LatestVersion.FirefighterHappy)
                    return true;
            }
            else if (giver.def.workTags == WorkTags.Hauling) {
                if (SettingsHelper.LatestVersion.Hauling && !SettingsHelper.LatestVersion.HaulingHappy)
                    return true;
            }
            else if (giver.def.workTags == WorkTags.Cleaning) {
                if (SettingsHelper.LatestVersion.Cleaning && !SettingsHelper.LatestVersion.CleaningHappy)
                    return true;
            }
            else if (giver.def.workTags == WorkTags.ManualDumb) {
                if (SettingsHelper.LatestVersion.BasicWorker && !SettingsHelper.LatestVersion.BasicWorkerHappy)
                    return true;
            }
            return false;
        }
        public static void UpdateTenantsWork() {
            foreach(Map map in Find.Maps) {
                foreach(Pawn pawn in map.mapPawns.FreeColonistsAndPrisoners) {
                    if (pawn.IsTenant()) {
                        UpdateTenantWork(pawn);
                    }
                }
            }
            SettingsHelper.LatestVersion.WorkIsDirty = false;
        }
        public static void UpdateTenantWork(Pawn pawn) {
            foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs) {
                if (def.defName == "Firefighter" && SettingsHelper.LatestVersion.Firefighter) {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "Patient" && SettingsHelper.LatestVersion.Patient) {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "PatientBedRest" && SettingsHelper.LatestVersion.PatientBedRest) {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "BasicWorker" && SettingsHelper.LatestVersion.BasicWorker) {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "Hauling" && SettingsHelper.LatestVersion.Hauling) {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "Cleaning" && SettingsHelper.LatestVersion.Cleaning) {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else
                    pawn.workSettings.Disable(def);
            }
        }
        
       public static string AppendPawnDescription(string text, Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder(text);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("TenantDescription".Translate(pawn.ageTracker.AgeBiologicalYears, pawn.def.defName, pawn.Named("PAWN")));
            stringBuilder.AppendLine();
            stringBuilder.Append("Traits".Translate()+ ": ");
            if (pawn.story.traits.allTraits.Count == 0) {
                stringBuilder.AppendLine();
                stringBuilder.Append("(" + "NoneLower".Translate() + ")");
            }
            else {
                stringBuilder.Append("(");
                for (int i = 0; i < pawn.story.traits.allTraits.Count; i++) {
                    if (i != 0) {
                        stringBuilder.Append(" ,");
                    }
                    stringBuilder.Append(pawn.story.traits.allTraits[i].LabelCap);
                }
                stringBuilder.Append(")");
            }
            return stringBuilder.ToString();
        }


        #endregion Methods
    }
}
