using Harmony;
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
        #endregion Fields
        #region PropertiesFinder

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
            string letterLabel, letterText;
            LetterDef def;

            if (!tenantComp.IsTenant && !pawn.IsColonist)
                return;
            if (terminated) {
                if (Rand.Value < stealChance) {
                    letterLabel = "ContractEnd".Translate();
                    letterText = "ContractDoneTerminated".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                    def = LetterDefOf.NeutralEvent;
                    TenantLeave(pawn);
                }
                else {
                    letterLabel = "ContractBreach".Translate();
                    letterText = "ContractDoneTheft".Translate(pawn.Named("PAWN"));
                    def = LetterDefOf.NegativeEvent;
                    TenantTheft(pawn);
                }
            }
            else {
                int mood = CalculateMood(tenantComp);
                if (mood == 1) {
                    letterLabel = "ContractEnd".Translate();
                    letterText = "ContractDoneHappy".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                    def = LetterDefOf.PositiveEvent;
                    ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceHappy / 100f);
                }
                else if (mood == -1) {
                    letterLabel = "ContractEnd".Translate();
                    letterText = "ContractDoneSad".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                    def = LetterDefOf.NeutralEvent;
                    ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceSad / 100f);
                }
                else {
                    letterLabel = "ContractEnd".Translate();
                    letterText = "ContractDone".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN"));
                    def = LetterDefOf.NeutralEvent;
                    ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceNeutral / 100f);
                }
            }
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, def);
        }
        public static void ContractProlong(Pawn pawn, float chance) {
            if (Rand.Value < chance) {
                Tenant tenantComp = pawn.TryGetComp<Tenant>();
                if (tenantComp.AutoRenew) {
                    tenantComp.ContractDate = Find.TickManager.TicksGame;
                    tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
                    tenantComp.ResetMood();
                    SpawnPayment(pawn);

                    string letterLabel = "ContractNew".Translate();
                    string letterText = "ContractRenewedMessage".Translate(pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                    return;
                }
                string text = ProlongContractMessage(pawn);
                DiaNode diaNode = new DiaNode(text);
                //Accepted offer.
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        SpawnPayment(pawn);
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
        public static bool ContractTenancy(Map map) {
            string title = "";
            Pawn pawn = FindRandomTenant();
            pawn.relations.everSeenByPlayer = true;
            Tenant tenantComp = pawn.TryGetComp<Tenant>();
            GenerateBasicTenancyContract(tenantComp);
            bool special = false;
            StringBuilder stringBuilder = new StringBuilder("");
            //Check if pawn is special
            //Wanted
            if (Rand.Value < 0.2f) {
                special = GenerateWanted(pawn);
                if (special) {
                    stringBuilder.Append("RequestForTenancyHiding".Translate(tenantComp.WantedBy, pawn.Named("PAWN")));
                    title = "RequestForTenancyHidingTitle".Translate(map.Parent.Label);
                    tenantComp.Payment = tenantComp.Payment * 2;
                    string text = AppendContractDetails(stringBuilder.ToString(), pawn);
                    if (GenerateContractDialogue(title, text, pawn, map)) {

                    }
                }
            }
            //Mole
            else if (Rand.Value < 0.33f && tenantComp.HiddenFaction.HostileTo(Find.FactionManager.OfPlayer)) {
                tenantComp.Mole = true;
            }
            if (!special) {
                //Broadcasted
                if (MapComponent_Tenants.GetComponent(map).Broadcast) {
                    stringBuilder.Append("RequestForTenancyOpportunity".Translate(pawn.Named("PAWN")));
                    title = "RequestForTenancyTitle".Translate(map.Parent.Label);
                    MapComponent_Tenants.GetComponent(map).Broadcast = false;
                    string text = AppendContractDetails(stringBuilder.ToString(), pawn);
                    if (GenerateContractDialogue(title, text, pawn, map)) {

                    }
                }
                //Normal
                else {
                    stringBuilder.Append("RequestForTenancyInitial".Translate(pawn.Named("PAWN")));
                    title = "RequestForTenancyTitle".Translate(map.Parent.Label);
                    string text = AppendContractDetails(stringBuilder.ToString(), pawn);
                    if (GenerateContractDialogue(title, text, pawn, map)) {

                    }
                }
            }
            return true;
        }
        public static void TenantLeave(Pawn pawn) {
            SpawnPayment(pawn);
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            pawn.GetTenantComponent().CleanTenancy();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantCancelContract(Pawn pawn) {
            Messages.Message("ContractDonePlayerTerminated".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            pawn.GetTenantComponent().CleanTenancy();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantTheft(Pawn pawn) {
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            pawn.GetTenantComponent().IsTenant = false;
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_TenantTheft(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantDeath(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            tenantComp.IsTenant = false;
            string text = "TenantDeath".Translate(pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            string label = "Death".Translate() + ": " + pawn.LabelShortCap;
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Death, pawn);
            pawn.SetFaction(tenantComp.HiddenFaction);
            if (pawn.Faction.HostileTo(Find.FactionManager.OfPlayer)) {
                if (Rand.Value < 0.5f) {
                    FactionRelation relation = tenantComp.WantedBy.RelationWith(Find.FactionManager.OfPlayer);
                    relation.goodwill = relation.goodwill - SettingsHelper.LatestVersion.OutragePenalty;
                    Messages.Message("TenantDeathRelative".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                    MapComponent_Tenants.GetComponent(pawn.Map).DeadTenantsToAvenge.Add(pawn);
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                    parms.raidStrategy = RaidStrategyDefOf.Retribution;
                    parms.forced = true;
                    Find.Storyteller.incidentQueue.Add(IncidentDefOf.RetributionForDead, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                }
            }
            else {
                if (Rand.Value < 0.66f) {
                    FactionRelation relation = tenantComp.WantedBy.RelationWith(Find.FactionManager.OfPlayer);
                    relation.goodwill = relation.goodwill - SettingsHelper.LatestVersion.OutragePenalty * 2;
                    Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty * 2, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }

            }
        }
        public static void TenantCaptured(Pawn pawn, Pawn byPawn) {
            if (pawn.HostileTo(Find.FactionManager.OfPlayer)) {
                return;
            }
            string text = "TenantCaptured".Translate(pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            string label = "Captured".Translate() + ": " + pawn.LabelShortCap;
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, pawn);
            Tenant tenantComp = pawn.GetTenantComponent();
            tenantComp.IsTenant = false;
            pawn.GetTenantComponent().CapturedTenant = true;
            pawn.SetFaction(tenantComp.HiddenFaction);

            if (pawn.Faction.HostileTo(Find.FactionManager.OfPlayer)) {
                if (Rand.Value < 0.25f || tenantComp.Wanted) {
                    MapComponent_Tenants.GetComponent(byPawn.Map).CapturedTenantsToAvenge.Add(pawn);
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, byPawn.Map);
                    parms.raidStrategy = RaidStrategyDefOf.Retribution;
                    parms.forced = true;
                    Find.Storyteller.incidentQueue.Add(IncidentDefOf.RetributionForCaptured, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                }
            }
            else {
                if (Rand.Value < 0.66f || tenantComp.Wanted) {
                    FactionRelation relation = pawn.Faction.RelationWith(Find.FactionManager.OfPlayer);
                    relation.goodwill = relation.goodwill - SettingsHelper.LatestVersion.OutragePenalty;
                    Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }
            }
        }
        public static void TenantMoleCaptured(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            tenantComp.IsTenant = false;
            pawn.SetFaction(tenantComp.HiddenFaction);
            string text = "MoleCaptured".Translate(pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            string label = "Captured".Translate() + ": " + pawn.LabelShortCap;
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, pawn);
        }
        public static void TenantWantToJoin(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp.MayJoin && Rand.Value < 0.02f && tenantComp.HappyMoodCount > 7) {

                string text = "RequestTenantWantToJoin".Translate(pawn.Named("PAWN"));

                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        SpawnPayment(pawn);
                        tenantComp.IsTenant = false;
                        Messages.Message("ContractDone".Translate(pawn.Name.ToStringFull, tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
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
            if (tenantComp.HiddenFaction != null && tenantComp.HiddenFaction.HostileTo(Find.FactionManager.OfPlayer)) {
                tenantComp.HiddenFaction = null;
            }
        }
        public static void TenantMole(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            tenantComp.MoleActivated = true;
            tenantComp.MoleMessage = true;
            MapComponent_Tenants.GetComponent(pawn.Map).Moles.Add(pawn);
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
            parms.raidStrategy = RaidStrategyDefOf.MoleRaid;
            parms.forced = true;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.MoleRaid, Find.TickManager.TicksGame + Rand.Range(5000, 30000), parms, 90000);
        }
        public static void TenantWanted(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp.WantedBy.HostileTo(Find.FactionManager.OfPlayer) && !MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Add(pawn);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = RaidStrategyDefOf.WantedRaid;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.WantedRaid, Find.TickManager.TicksGame + Rand.Range(30000, 150000), parms, 60000);
            }
            else if (Rand.Value < 0.5) {
                FactionRelation relation = tenantComp.WantedBy.RelationWith(Find.FactionManager.OfPlayer);
                relation.goodwill = relation.goodwill - SettingsHelper.LatestVersion.HarborPenalty;
                Messages.Message("HarboringWantedTenant".Translate(pawn.GetTenantComponent().WantedBy, SettingsHelper.LatestVersion.HarborPenalty, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }

        }
        public static void InviteTenant(Building_CommsConsole comms, Pawn pawn) {
            Messages.Message("InviteTenantMessage".Translate(), MessageTypeDefOf.NeutralEvent);
            MapComponent_Tenants.GetComponent(pawn.Map).Broadcast = true;
            if (Rand.Value < 0.20f) {
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


        public static Pawn FindRandomTenant() {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.GetTenantComponent() != null && p.GetTenantComponent().IsTenant && !p.Dead && !p.Spawned && !p.Discarded
                                select p).ToList();
            if (pawns.Count < 20)
                for (int i = 0; i < 3; i++)
                    pawns.Add(GenerateNewTenant());
            pawns.Shuffle();
            return pawns[0];
        }
        public static bool GenerateWanted(Pawn pawn) {
            Tenant tenantComp = pawn.TryGetComp<Tenant>();
            if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                List<FactionRelation> entries = Traverse.Create(tenantComp.HiddenFaction).Field("relations").GetValue<List<FactionRelation>>().Where(p => p.kind == FactionRelationKind.Hostile).ToList();
                if (entries.Count > 0) {
                    entries.Shuffle();
                    tenantComp.WantedBy = entries[0].other;
                    tenantComp.Wanted = true;
                    return true;
                }
            }
            return false;
        }
        public static Pawn GenerateNewTenant() {
            bool generation = true;
            Pawn newTenant = null;
            while (generation) {
                PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
                Faction faction = FactionUtility.DefaultFactionFrom(random.defaultFactionType);
                newTenant = PawnGenerator.GeneratePawn(random, faction);
                if (newTenant != null && !newTenant.Dead && !newTenant.IsDessicated() && !newTenant.AnimalOrWildMan() && newTenant.RaceProps.Humanlike && newTenant.RaceProps.IsFlesh && newTenant.RaceProps.ResolvedDietCategory != DietCategory.NeverEats) {
                    {
                        if (SettingsHelper.LatestVersion.SimpleClothing) {
                            FloatRange range = newTenant.kindDef.apparelMoney;
                            newTenant.kindDef.apparelMoney = new FloatRange(SettingsHelper.LatestVersion.SimpleClothingMin, SettingsHelper.LatestVersion.SimpleClothingMax);
                            PawnApparelGenerator.GenerateStartingApparelFor(newTenant, new PawnGenerationRequest(random));
                            newTenant.kindDef.apparelMoney = range;
                        }
                        newTenant.GetTenantComponent().IsTenant = true;
                        newTenant.GetTenantComponent().HiddenFaction = faction;
                        newTenant.SetFaction(Faction.OfAncients);
                        if (SettingsHelper.LatestVersion.Weapons) {
                            List<Thing> ammo = newTenant.inventory.innerContainer.Where(x => x.def.defName.Contains("Ammunition")).ToList();
                            foreach (Thing thing in ammo)
                                newTenant.inventory.innerContainer.Remove(thing);
                        }
                        newTenant.DestroyOrPassToWorld();
                        generation = false;
                    }
                }
            }
            return newTenant;
        }
        public static bool GenerateContractDialogue(string title, string text, Pawn pawn, Map map) {
            if (!TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return false;
            }
            DiaNode diaNode = new DiaNode(text);
            //Accepted offer, generating tenant.
            DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                action = delegate {
                    pawn.SetFaction(Faction.OfPlayer);
                    pawn.GetTenantComponent().Contracted = true;
                    GenSpawn.Spawn(pawn, spawnSpot, map);
                    pawn.needs.SetInitialLevels();
                    pawn.playerSettings.AreaRestriction = map.areaManager.Home;
                    UpdateAllRestrictions(pawn);
                    TraitDef nightOwl = DefDatabase<TraitDef>.GetNamedSilentFail("NightOwl");
                    if (nightOwl != null && pawn.story.traits.HasTrait(nightOwl)) {
                        UpdateNightOwl(pawn);
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
                    pawn.GetTenantComponent().CleanTenancy();
                },
                link = diaNode2
            };
            diaNode.options.Add(diaOption3);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            return true;
        }
        public static void GenerateBasicTenancyContract(Tenant tenantComp) {
            tenantComp.Payment = Rand.Range(SettingsHelper.LatestVersion.MinDailyCost, SettingsHelper.LatestVersion.MaxDailyCost);
            tenantComp.ContractLength = Rand.Range(SettingsHelper.LatestVersion.MinContractTime, SettingsHelper.LatestVersion.MaxContractTime) * 60000;
            tenantComp.ContractDate = Find.TickManager.TicksGame;
            tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
            tenantComp.ResetMood();
        }
        public static void UpdateAllRestrictions(Pawn pawn) {
            UpdateWork(pawn);
            UpdateOutfitManagement(pawn);
            UpdateFoodManagement(pawn);
            UpdateDrugManagement(pawn);
        }
        public static void UpdateWork(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs) {

                if (def.defName == "Patient") {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "PatientBedRest") {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (!pawn.story.WorkTagIsDisabled(WorkTags.Firefighting) && def.defName == "Firefighter") {
                    pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Firefighter"), 3);
                    tenantComp.MayFirefight = true;
                }
                else if (!pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) && def.defName == "BasicWorker") {
                    pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "BasicWorker"), 3);
                    tenantComp.MayBasic = true;
                }
                else if (!(pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) || pawn.story.WorkTagIsDisabled(WorkTags.Hauling)) && def.defName == "Hauling") {
                    pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Hauling"), 3);
                    tenantComp.MayHaul = true;
                }
                else if (!(pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) || pawn.story.WorkTagIsDisabled(WorkTags.Cleaning)) && def.defName == "Cleaning") {
                    pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Cleaning"), 3);
                    tenantComp.MayClean = true;
                }
                else
                    pawn.workSettings.Disable(def);
            }
        }
        public static bool UpdateEmergencyWork(WorkGiver giver) {
            if (giver is WorkGiver_PatientGoToBedEmergencyTreatment
                || giver is WorkGiver_PatientGoToBedTreatment
                || giver is WorkGiver_PatientGoToBedRecuperate
                || giver.def.workTags == WorkTags.Firefighting) {
                return true;
            }
            return false;
        }
        public static void UpdateOutfitManagement(Pawn pawn) {
            Outfit restriction = Current.Game.outfitDatabase.AllOutfits.FirstOrDefault(x => x.label == "Tenants".Translate());
            if (restriction == null) {
                int uniqueId = (!Current.Game.outfitDatabase.AllOutfits.Any()) ? 1 : (Current.Game.outfitDatabase.AllOutfits.Max((Outfit o) => o.uniqueId) + 1);
                restriction = new Outfit(uniqueId, "Tenants".Translate());
                restriction.filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
                Current.Game.outfitDatabase.AllOutfits.Add(restriction);
            }
            pawn.outfits.CurrentOutfit = restriction;
        }
        public static void UpdateFoodManagement(Pawn pawn) {
            FoodRestriction restriction = Current.Game.foodRestrictionDatabase.AllFoodRestrictions.FirstOrDefault(x => x.label == "Tenants".Translate());
            if (restriction == null) {
                int uniqueId = (!Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Any()) ? 1 : (Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Max((FoodRestriction o) => o.id) + 1);
                restriction = new FoodRestriction(uniqueId, "Tenants".Translate());
                restriction.filter.SetAllow(ThingCategoryDefOf.FoodMeals, allow: true);
                restriction.filter.SetAllow(ThingCategoryDefOf.Foods, allow: true);
                Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Add(restriction);
            }
            pawn.foodRestriction.CurrentFoodRestriction = restriction;
        }
        public static void UpdateDrugManagement(Pawn pawn) {
            DrugPolicy restriction = Current.Game.drugPolicyDatabase.AllPolicies.FirstOrDefault(x => x.label == "Tenants".Translate());
            if (restriction == null) {
                int uniqueId = (!Current.Game.drugPolicyDatabase.AllPolicies.Any()) ? 1 : (Current.Game.drugPolicyDatabase.AllPolicies.Max((DrugPolicy o) => o.uniqueId) + 1);
                restriction = new DrugPolicy(uniqueId, "Tenants".Translate());
                Current.Game.drugPolicyDatabase.AllPolicies.Add(restriction);
            }
            pawn.drugs.CurrentPolicy = restriction;
        }
        public static void UpdateNightOwl(Pawn pawn) {
            pawn.timetable.times = new List<TimeAssignmentDef>(24);
            for (int i = 0; i < 24; i++) {
                TimeAssignmentDef item = (i >= 10 && i <= 17) ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                pawn.timetable.times.Add(item);
            }
        }
        public static void SpawnPayment(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            int payment = (tenantComp.ContractLength / 60000) * tenantComp.Payment;
            while (payment > 500) {
                DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, pawn.Position, 500);
                payment = payment - 500;
            }
            DebugThingPlaceHelper.DebugSpawn(ThingDefOf.Silver, pawn.Position, payment);
        }


        public static string AppendPawnDescription(string text, Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder(text);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("TenantDescription".Translate(pawn.ageTracker.AgeBiologicalYears, pawn.def.label, pawn.Named("PAWN")));
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
        public static string ProlongContractMessage(Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append("RequestForTenancyContinued".Translate(pawn.Named("PAWN")));
            return AppendContractDetails(stringBuilder.ToString(), pawn);
        }
        public static string NewBasicRaidMessage(IncidentParms parms, List<Pawn> pawns) {
            Log.Message("Couldn't spawn correct letter for retribution.");
            string basic = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
            basic += "\n\n";
            basic += parms.raidStrategy.arrivalTextEnemy;
            Pawn leader = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (leader != null) {
                basic += "\n\n";
                basic += "EnemyRaidLeaderPresent".Translate(leader.Faction.def.pawnsPlural, leader.LabelShort, leader.Named("LEADER"));
            }
            return basic;
        }

        #endregion Methods
    }
}
