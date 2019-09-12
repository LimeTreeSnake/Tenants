using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants {
    public static class Utility {
        #region Fields
        private static List<PawnKindDef> pawnKindDefList => DefDatabase<PawnKindDef>.AllDefs.ToList();
        #endregion Fields
        #region PropertiesFinder
        public static List<PawnKindDef> GetPawnKindDefList() => pawnKindDefList;

        public static Tenant GetTenantComponent(this Pawn pawn) {
            if (ThingCompUtility.TryGetComp<Tenant>(pawn) != null) {
                return ThingCompUtility.TryGetComp<Tenant>(pawn);
            }
            return null;
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
        public static void ContractConclusion(Pawn pawn, bool terminated, float stealChance = 0.5f) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (terminated) {
                if (Rand.Value < stealChance) {
                    string letterLabel = "ContractEnd".Translate();
                    string letterText = "ContractDoneTerminated".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent);
                    TenantLeave(pawn);
                    SpawnPayment(pawn);
                }
                else {
                    string letterLabel = "ContractBreach".Translate();
                    string letterText = "ContractDoneTheft".Translate(pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent);
                    TenantTheft(pawn);
                }
            }
            else {
                int mood = CalculateMood(tenantComp);
                if (mood == 1) {
                    string letterLabel = "ContractEnd".Translate();
                    string letterText = "ContractDoneHappy".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                    ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceHappy / 100f);
                }
                else if (mood == -1) {
                    string letterLabel = "ContractEnd".Translate();
                    string letterText = "ContractDoneSad".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent);
                    ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceSad / 100f);
                }
                else {
                    string letterLabel = "ContractEnd".Translate();
                    string letterText = "ContractDone".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent);
                    ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceNeutral / 100f);
                }
                SpawnPayment(pawn);
            }
        }
        public static void ContractProlong(Pawn pawn, float chance) {
            if (Rand.Value < chance) {
                Tenant tenantComp = pawn.TryGetComp<Tenant>();
                if (tenantComp.AutoRenew) {
                    tenantComp.ContractDate = Find.TickManager.TicksGame;
                    tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
                    tenantComp.ResetMood();
                    tenantComp.Paid = false;

                    string letterLabel = "ContractNew".Translate();
                    string letterText = "ContractRenewedMessage".Translate(pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                    return;
                }
                string text = ProlongContractMessage(pawn);
                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        //Accepted offer.
                        tenantComp.ContractDate = Find.TickManager.TicksGame;
                        tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
                        tenantComp.ResetMood();
                        tenantComp.Paid = false;
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
                DiaOption diaOption3 = new DiaOption("ContractReject".Translate()) {
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
        public static bool ContractGenerateNew(Map map) {
            if (!TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return false;
            }
            //Finds all generated tenants except those already spawned, creates and selects one for the event.
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.GetTenantComponent() != null && p.GetTenantComponent().IsTenant && !p.Dead && !p.Spawned && !p.Discarded
                                select p).ToList();
            if (pawns.Count < 20) {
                for (int i = 0; i < 3; i++) {
                    //GENERATION CONTEXT
                    bool loop = true;
                    while (loop) {
                        PawnKindDef def = GetPawnKindDefList()[Rand.Range(0, GetPawnKindDefList().Count)];
                        PawnGenerationRequest request = new PawnGenerationRequest(def, Faction.OfAncients);
                        Pawn newTenant = PawnGenerator.GeneratePawn(request);
                        if (!newTenant.Dead && !newTenant.IsDessicated() && !newTenant.AnimalOrWildMan() && newTenant.RaceProps.Humanlike && newTenant.RaceProps.EatsFood && newTenant.RaceProps.IsFlesh && newTenant.RaceProps.FleshType.defName != "Android") {
                            {
                                if (SettingsHelper.LatestVersion.SimpleClothing) {
                                    FloatRange range = newTenant.kindDef.apparelMoney;
                                    newTenant.kindDef.apparelMoney = new FloatRange(SettingsHelper.LatestVersion.SimpleClothingMin, SettingsHelper.LatestVersion.SimpleClothingMax);
                                    PawnApparelGenerator.GenerateStartingApparelFor(newTenant, request);
                                    newTenant.kindDef.apparelMoney = range;
                                }

                                newTenant.GetTenantComponent().IsTenant = true;
                                pawns.Add(newTenant);
                                loop = false;
                            }
                        }
                    }
                }
            }
            pawns.Shuffle();
            Pawn pawn = pawns[0];
            pawn.relations.everSeenByPlayer = true;
            Tenant tenantComp = pawn.TryGetComp<Tenant>();

            tenantComp.Payment = Rand.Range(SettingsHelper.LatestVersion.MinDailyCost, SettingsHelper.LatestVersion.MaxDailyCost); tenantComp.ContractLength = Rand.Range(SettingsHelper.LatestVersion.MinContractTime, SettingsHelper.LatestVersion.MaxContractTime) * 60000;
            tenantComp.ContractDate = Find.TickManager.TicksGame;
            tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
            tenantComp.ResetMood();
            tenantComp.Paid = false;

            //Generates event
            bool broadcasted = MapComponent_Tenants.GetComponent(map).Broadcast;
            string text = NewContractMessage(pawn, MapComponent_Tenants.GetComponent(map).Broadcast);
            if (broadcasted)
                MapComponent_Tenants.GetComponent(map).Broadcast = false;
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                action = delegate {
                    //Accepted offer, generating tenant.
                    pawn.SetFaction(Faction.OfPlayer);
                    GenSpawn.Spawn(pawn, spawnSpot, map);
                    pawn.needs.SetInitialLevels();
                    pawn.playerSettings.AreaRestriction = map.areaManager.Home;
                    UpdateTenantWork(pawn);
                    TraitDef nightOwl = DefDatabase<TraitDef>.GetNamedSilentFail("NightOwl");
                    if (nightOwl != null && pawn.story.traits.HasTrait(nightOwl)) {
                        UpdateTenantNightOwl(pawn);
                    }
                    if (SettingsHelper.LatestVersion.Weapons) {
                        pawn.equipment.DestroyAllEquipment();
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
            DiaOption diaOption3 = new DiaOption("ContractReject".Translate()) {
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
            pawn.GetTenantComponent().ContractEndDate = 0;
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantCancelContract(Pawn pawn) {
            Messages.Message("ContractDonePlayerTerminated".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            pawn.GetTenantComponent().ContractEndDate = 0;
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantTheft(Pawn pawn) {
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            pawn.GetTenantComponent().Reset();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_TenantTheft(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantDeath(Pawn pawn) {
            if (Rand.Value < 0.5f) {
                MapComponent_Tenants.GetComponent(pawn.Map).DeadTenantsToAvenge.Add(pawn);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = RaidStrategyDefOf.Retribution;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RetributionForDead, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
            }
        }
        public static void TenantCaptured(Pawn pawn, Pawn byPawn) {
            if (Rand.Value < 0.25f) {
                MapComponent_Tenants.GetComponent(byPawn.Map).CapturedTenantsToAvenge.Add(pawn);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, byPawn.Map);
                parms.raidStrategy = RaidStrategyDefOf.Retribution;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RetributionForCaptured, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
            }
        }
        public static void TenantWantToJoin(Pawn pawn) {
            Tenant tenant = pawn.GetTenantComponent();
            if (tenant.MayJoin && Rand.Value < 0.05f && tenant.HappyMoodCount > 7) {

                string text = "RequestTenantWantToJoin".Translate(pawn.Named("PAWN"));

                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        tenant.IsTenant = false;
                        Messages.Message("ContractDone".Translate(pawn.Name.ToStringFull, tenant.Payment * tenant.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                        SpawnPayment(pawn);
                        Find.ColonistBar.MarkColonistsDirty();
                    },
                    resolveTree = true,
                };
                diaNode.options.Add(diaOption);
                //Denied offer
                string text2 = "RequestTenantWantToJoinRejected".Translate(pawn.Named("PAWN"));
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
                string title = "RequestTenantWantToJoinTitle".Translate(pawn.Map.Parent.Label);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
                Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            }
        }
        public static void InviteTenant(Building_CommsConsole comms, Pawn pawn) {
            Messages.Message("InviteTenantMessage".Translate(), MessageTypeDefOf.NeutralEvent);
            MapComponent_Tenants.GetComponent(pawn.Map).Broadcast = true;
            if (Rand.Value < 0.75f) {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = RaidStrategyDefOf.Retribution;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.Opportunists, Find.TickManager.TicksGame + Rand.Range(25000, 150000), parms, 240000);
            }
            else {
                IncidentParms parms = new IncidentParms() { target = pawn.Map, forced = true };
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RequestForTenancy, Find.TickManager.TicksGame + Rand.Range(15000, 120000), parms, 240000);
            }

        }
        public static List<Pawn> RemoveTenantsFromList(List<Pawn> pawns) {
            List<Pawn> tenants = new List<Pawn>();
            foreach (Pawn pawn in pawns) {
                Tenant tenantComp = pawn.GetTenantComponent();
                if (tenantComp != null && tenantComp.IsTenant)
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
            foreach (Map map in Find.Maps) {
                foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisoners) {
                    Tenant tenantComp = pawn.GetTenantComponent();
                    if (tenantComp != null && tenantComp.IsTenant) {
                        UpdateTenantWork(pawn);
                    }
                }
            }
            SettingsHelper.LatestVersion.WorkIsDirty = false;
        }
        public static void UpdateTenantWork(Pawn pawn) {
            foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs) {
                if (def.defName == "Firefighter" && SettingsHelper.LatestVersion.Firefighter) {
                    if (!pawn.story.WorkTagIsDisabled(WorkTags.Firefighting)) {
                        pawn.workSettings.SetPriority(def, 3);
                    }
                }
                else if (def.defName == "Patient" && SettingsHelper.LatestVersion.Patient) {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "PatientBedRest" && SettingsHelper.LatestVersion.PatientBedRest) {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "BasicWorker" && SettingsHelper.LatestVersion.BasicWorker) {
                    if (!pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb)) {
                        pawn.workSettings.SetPriority(def, 3);
                    }
                }
                else if (def.defName == "Hauling" && SettingsHelper.LatestVersion.Hauling) {
                    if (!pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb)) {
                        if (!pawn.story.WorkTagIsDisabled(WorkTags.Hauling))
                            pawn.workSettings.SetPriority(def, 3);
                    }
                }
                else if (def.defName == "Cleaning" && SettingsHelper.LatestVersion.Cleaning) {
                    if (!pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb)) {
                        if (!pawn.story.WorkTagIsDisabled(WorkTags.Cleaning))
                            pawn.workSettings.SetPriority(def, 3);
                    }
                }
                else
                    pawn.workSettings.Disable(def);
            }
        }
        public static void UpdateTenantNightOwl(Pawn pawn) {
            pawn.timetable.times = new List<TimeAssignmentDef>(24);
            for (int i = 0; i < 24; i++) {
                TimeAssignmentDef item = (i >= 10 && i <= 17) ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                pawn.timetable.times.Add(item);
            }
        }
        public static void SpawnPayment(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if(tenantComp.Paid == false) {
                DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, pawn.Position, tenantComp.ContractLength / 60000 * tenantComp.Payment);
                tenantComp.Paid = true;
            }
        }
        public static string AppendPawnDescription(string text, Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder(text);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("TenantDescription".Translate(pawn.ageTracker.AgeBiologicalYears, pawn.def.defName, pawn.Named("PAWN")));
            stringBuilder.AppendLine();
            stringBuilder.Append("Traits".Translate() + ": ");
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
        public static string AppendContractDetails(string text, Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            StringBuilder stringBuilder = new StringBuilder(text);
            stringBuilder.AppendLine();
            stringBuilder.Append("RequestForTenancyContract".Translate(tenantComp.ContractLength / 60000, tenantComp.Payment, pawn.Named("PAWN")));
            text = stringBuilder.ToString();
            text = text.AdjustedFor(pawn);
            text = AppendPawnDescription(text, pawn);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
            return text;
        }
        public static string NewContractMessage(Pawn pawn, bool isRandomEvent) {
            StringBuilder stringBuilder = new StringBuilder("");
            if (isRandomEvent)
                stringBuilder.Append("RequestForTenancyOpportunity".Translate(pawn.Named("PAWN")));
            else {
                stringBuilder.Append("RequestForTenancyInitial".Translate(pawn.Named("PAWN")));
            }
            return AppendContractDetails(stringBuilder.ToString(), pawn);

        }
        public static string ProlongContractMessage(Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append("RequestForTenancyContinued".Translate(pawn.Named("PAWN")));
            return AppendContractDetails(stringBuilder.ToString(), pawn);
        }

        #endregion Methods
    }
}
