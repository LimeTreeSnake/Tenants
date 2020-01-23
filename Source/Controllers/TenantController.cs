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
        public static void Tick(Pawn pawn, TenantComp comp) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (contract == null)
                return;
            if (comp != null) {
                //If tenant belongs to player faction
                if (pawn.IsColonist) {
                    pawn.AllComps.Remove(comp);
                    pawn.AllComps.Remove(contract);
                    return;
                }
                //Tenant alone with no colonist
                if (pawn.Map.mapPawns.FreeColonists.FirstOrDefault(x => ThingCompUtility.TryGetComp<TenantComp>(x) == null) == null) {
                    ContractConclusion(pawn, 0.75f);
                    return;
                }
                //If tenancy is to be terminated
                if (contract.IsTerminated) {
                    TerminateContract(pawn);
                    return;
                }
                //Tenant contract is out
                if (Find.TickManager.TicksGame >= contract.ContractEndTick) {
                    ContractConclusion(pawn);
                    return;
                }

                //Tenancy tick 1/10 per day
                if (Find.TickManager.TicksGame % 6000 == 0) {
                    //Mole
                    MoleComp moleComp = ThingCompUtility.TryGetComp<MoleComp>(pawn);
                    //if (moleComp.MoleMessage) {
                    //    tenantComp.MoleMessage = false;
                    //    Messages.Message("TenantMoleMessage".Translate(), MessageTypeDefOf.NegativeEvent);
                    //}
                    if (moleComp != null && !moleComp.Activated) {
                        if (CalculateMood(comp) == Mood.Sad && comp.NeutralMoodCount > 2) {
                            Building building = pawn.Map.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x.def.defName.ToLower().Contains("commsconsole"));
                            if (building != null) {
                                Job job = new Job(Defs.JobDefOf.JobUseCommsConsoleMole, building);
                                pawn.jobs.TryTakeOrderedJob(job);
                            }
                        }
                    }
                    //Join
                    float mood = pawn.needs.mood.CurInstantLevel;
                    if (!comp.IsEnvoy && mood > 0.8f) {
                        TenantWantToJoin(pawn);
                    }
                    //Calculate mood
                    if (mood > Settings.SettingsHelper.LatestVersion.LevelOfHappiness) {
                        comp.HappyMoodCount++;
                    }
                    else if (mood < pawn.mindState.mentalBreaker.BreakThresholdMinor) {
                        comp.SadMoodCount++;
                        if (comp.RecentBadMoodsCount > 5) {
                            ContractConclusion(pawn, true);
                        }
                    }
                    else {
                        comp.NeutralMoodCount++;
                    }
                }


            }
        }
        public static void ContractConclusion(Pawn pawn, float stealChance = 0.5f) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            TenantComp comp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (contract == null || comp == null)
                return;
            //In case the tenant decides to terminate the contract
            if (contract.IsTerminated) {
                if (Rand.Value > stealChance) {
                    ContractController.ContractPayment(contract);
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTerminated".Translate(contract.Payment * contract.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                    Leave(pawn, Find.FactionManager.OfAncients);
                }
                else {
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTheft".Translate(pawn.Named("PAWN")), LetterDefOf.NegativeEvent);
                    Theft(pawn, comp.HiddenFaction);
                }
                return;
            }
            Mood mood = CalculateMood(comp);
            switch (mood) {
                case Mood.Neutral: {
                        ContractController.ContractPayment(contract);
                        Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDone".Translate(contract.Payment * contract.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.PositiveEvent);
                        ContractProlong(pawn, Settings.SettingsHelper.LatestVersion.StayChanceNeutral / 100f);
                        break;
                    }
                case Mood.Happy: {
                        ContractController.ContractPayment(contract);
                        Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneHappy".Translate(contract.Payment * contract.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                        ContractProlong(pawn, Settings.SettingsHelper.LatestVersion.StayChanceHappy / 100f);
                        break;
                    }
                case Mood.Sad: {
                        ContractController.ContractPayment(contract);
                        Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneSad".Translate(contract.Payment * contract.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                        Leave(pawn, Find.FactionManager.OfAncients);
                        break;
                    }
            }
        }


        public static void ContractProlong(Pawn pawn, float chance) {
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (Rand.Value < chance) {
                if (tenantComp.IsEnvoy) {
                    tenantComp.Contract.ContractDate = Find.TickManager.TicksGame;
                    tenantComp.Contract.ContractEndDate = Find.TickManager.TicksAbs + (tenantComp.Contract.ContractLength * 2) + 60000;
                    tenantComp.ResetMood();
                    string letterLabel = "Envoy".Translate(tenantComp.HiddenFaction);
                    string letterText = "EnvoyStays".Translate(tenantComp.HiddenFaction, pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent);
                    return;
                }
                else {
                    if (tenantComp.Contract.AutoRenew) {
                        tenantComp.Contract.ContractDate = Find.TickManager.TicksGame;
                        tenantComp.Contract.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.Contract.ContractLength + 60000;
                        tenantComp.ResetMood();
                        string letterLabel = "ContractNew".Translate();
                        string letterText = "ContractRenewedMessage".Translate(pawn.Named("PAWN"));
                        Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                        return;
                    }
                    DiaNode diaNode = new DiaNode(ProlongContractMessage(pawn));
                    //Accepted offer.
                    DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                        action = delegate {
                            tenantComp.Contract.ContractDate = Find.TickManager.TicksGame;
                            tenantComp.Contract.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.Contract.ContractLength + 60000;
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
                            Leave(pawn, Find.FactionManager.OfAncients);
                        },
                        link = diaNode2
                    };
                    diaNode.options.Add(diaOption3);
                    string title = "RequestForTenancyTitle".Translate(pawn.Map.Parent.Label);
                    Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
                    Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
                }
            }
            else if (tenantComp.IsEnvoy) {
                Leave(pawn, tenantComp.HiddenFaction);
            }
            else {
                Leave(pawn, Find.FactionManager.OfAncients);
            }
        }
        public static bool Contract(Map map) {
            if (!MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return false;
            }
            Pawn pawn = FindRandomTenant();
            if (pawn == null)
                return false;
            pawn.relations.everSeenByPlayer = true;
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            MapUtilities.GenerateBasicContract(tenantComp, Rand.Range(Settings.SettingsHelper.LatestVersion.MinDailyCost, Settings.SettingsHelper.LatestVersion.MaxDailyCost));
            StringBuilder stringBuilder = new StringBuilder("");
            //Wanted
            if (Rand.Value < 0.2f) {
                GenerateWanted(pawn);
            }
            //Mole
            if (Rand.Value < 0.33f && tenantComp.HiddenFaction.HostileTo(Find.FactionManager.OfPlayer)) {
                MoleComp mole = new MoleComp();
                pawn.AllComps.Add(mole);
            }
            WantedComp wanted = ThingCompUtility.TryGetComp<WantedComp>(pawn);
            if (wanted != null) {
                stringBuilder.Append("RequestForTenancyWantedInitial".Translate(wanted.WantedBy, pawn.Named("PAWN")));
                tenantComp.Contract.Payment = tenantComp.Contract.Payment * 2;
                GenerateContractDialogue("RequestForTenancyWantedTitle".Translate(map.Parent.Label), AppendContractDetails(stringBuilder.ToString(), pawn), pawn, map, spawnSpot, tenantComp);
            }
            //Broadcasted
            else if (TenantsMapComp.GetComponent(map).Broadcast) {
                stringBuilder.Append("RequestForTenancyOpportunity".Translate(pawn.Named("PAWN")));
                TenantsMapComp.GetComponent(map).Broadcast = false;
                GenerateContractDialogue("RequestForTenancyTitle".Translate(map.Parent.Label), AppendContractDetails(stringBuilder.ToString(), pawn), pawn, map, spawnSpot, tenantComp);
            }
            //Normal
            else {
                stringBuilder.Append("RequestForTenancyInitial".Translate(pawn.Named("PAWN")));
                GenerateContractDialogue("RequestForTenancyTitle".Translate(map.Parent.Label), AppendContractDetails(stringBuilder.ToString(), pawn), pawn, map, spawnSpot, tenantComp);
            }
            return true;
        }
        public static void Leave(Pawn pawn, Faction faction) {
            MakePayment(pawn);
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(faction);
            ThingCompUtility.TryGetComp<TenantComp>(pawn).CleanTenancy();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
            if (TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Remove(pawn);
            }
        }
        public static void Theft(Pawn pawn, Faction faction) {
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(faction);
            ThingCompUtility.TryGetComp<TenantComp>(pawn).CleanTenancy();
            LordMaker.MakeNewLord(pawn.Faction, new LordJobs.LordJob_TenantTheft(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TerminateContract(Pawn pawn) {
            pawn.jobs.ClearQueuedJobs();
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (tenantComp.IsEnvoy) {
                int val = Rand.Range(Settings.SettingsHelper.LatestVersion.MinRelation, Settings.SettingsHelper.LatestVersion.MaxRelation + 1);
                Messages.Message("ContractDoneEnvoyPlayerTerminated".Translate(tenantComp.HiddenFaction, val, pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
                pawn.SetFaction(tenantComp.HiddenFaction);
                tenantComp.HiddenFaction.RelationWith(Find.FactionManager.OfPlayer).goodwill -= val;
                Find.FactionManager.OfPlayer.RelationWith(tenantComp.HiddenFaction).goodwill -= val;
            }
            else {
                Messages.Message("ContractDonePlayerTerminated".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
                pawn.SetFaction(Faction.OfAncients);
            }
            tenantComp.CleanTenancy();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TenantDeath(Pawn pawn) {
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            string label = "Death".Translate() + ": " + pawn.LabelShortCap;
            pawn.SetFaction(tenantComp.HiddenFaction);
            string text;
            if (tenantComp.IsEnvoy) {
                int val = MapUtilities.ChangeRelations(tenantComp.HiddenFaction, true);
                val += MapUtilities.ChangeRelations(tenantComp.HiddenFaction, true);
                text = "EnvoyDeath".Translate(tenantComp.HiddenFaction, val, pawn.Named("PAWN"));
                text = text.AdjustedFor(pawn);
            }
            else {
                text = "TenantDeath".Translate(pawn.Named("PAWN"));
                text = text.AdjustedFor(pawn);
                if (pawn.Faction.HostileTo(Find.FactionManager.OfPlayer)) {
                    if (Rand.Value < 0.5f) {
                        if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                            int val = MapUtilities.ChangeRelations(tenantComp.HiddenFaction, true);
                            Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                            TenantsMapComp.GetComponent(pawn.Map).DeadTenantsToAvenge.Add(pawn);
                            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                            parms.raidStrategy = Defs.RaidStrategyDefOf.Retribution;
                            parms.forced = true;
                            Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.RetributionForDead, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                        }
                    }
                }
                else {
                    if (Rand.Value < 0.66f) {
                        if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                            int val = MapUtilities.ChangeRelations(tenantComp.HiddenFaction, true);
                            val += MapUtilities.ChangeRelations(tenantComp.HiddenFaction, true);
                            Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                        }
                    }

                }
            }
            tenantComp.CleanTenancy();
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
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            WantedComp wanted = ThingCompUtility.TryGetComp<WantedComp>(pawn);
            ThingCompUtility.TryGetComp<TenantComp>(pawn).CapturedTenant = true;
            pawn.SetFaction(tenantComp.HiddenFaction);

            if (pawn.Faction.HostileTo(Find.FactionManager.OfPlayer)) {
                if (Rand.Value < 0.25f || wanted != null) {
                    int val = MapUtilities.ChangeRelations(tenantComp.HiddenFaction, true);
                    Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                    TenantsMapComp.GetComponent(byPawn.Map).CapturedTenantsToAvenge.Add(pawn);
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, byPawn.Map);
                    parms.raidStrategy = Defs.RaidStrategyDefOf.Retribution;
                    parms.forced = true;
                    Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.RetributionForCaptured, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                }
            }
            else {
                if (Rand.Value < 0.66f || wanted != null) {
                    if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                        int val = MapUtilities.ChangeRelations(tenantComp.HiddenFaction, true);
                        Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
        }
        public static void TenantMoleCaptured(Pawn pawn) {
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            pawn.SetFaction(tenantComp.HiddenFaction);
            string text = "MoleCaptured".Translate(pawn.Named("PAWN"));
            text = text.AdjustedFor(pawn);
            string label = "Captured".Translate() + ": " + pawn.LabelShortCap;
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, pawn);
        }
        public static void TenantWantToJoin(Pawn pawn) {
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (tenantComp.MayJoin && Rand.Value < 0.02f && tenantComp.HappyMoodCount > 7) {

                string text = "RequestWantToJoin".Translate(pawn.Named("PAWN"));

                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        MakePayment(pawn);
                        Messages.Message("ContractDone".Translate(pawn.Name.ToStringFull, tenantComp.Contract.Payment * tenantComp.Contract.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
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
            if (tenantComp.HiddenFaction != null && tenantComp.HiddenFaction.HostileTo(Find.FactionManager.OfPlayer)) {
                tenantComp.HiddenFaction = null;
            }
        }
        public static void TenantMole(Pawn pawn) {
            MoleComp mole = ThingCompUtility.TryGetComp<MoleComp>(pawn);
            mole.Activated = true;
            TenantsMapComp.GetComponent(pawn.Map).Moles.Add(pawn);
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
            parms.raidStrategy = Defs.RaidStrategyDefOf.MoleRaid;
            parms.forced = true;
            Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.MoleRaid, Find.TickManager.TicksGame + Rand.Range(5000, 30000), parms, 90000);
        }

        public static void TenantInvite(Building_CommsConsole comms, Pawn pawn) {
            Messages.Message("InviteTenantMessage".Translate(), MessageTypeDefOf.NeutralEvent);
            TenantsMapComp.GetComponent(pawn.Map).Broadcast = true;
            if (Rand.Value < 0.20f) {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = Defs.RaidStrategyDefOf.Retribution;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.Opportunists, Find.TickManager.TicksGame + Rand.Range(25000, 150000), parms, 240000);
            }
            else {
                IncidentParms parms = new IncidentParms() { target = pawn.Map, forced = true };
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.RequestForTenancy, Find.TickManager.TicksGame + Rand.Range(15000, 120000), parms, 240000);
            }

        }
        public static Mood CalculateMood(TenantComp tenant) {
            float count = tenant.HappyMoodCount + tenant.NeutralMoodCount + tenant.SadMoodCount;
            if (tenant.NeutralMoodCount / count >= 0.5f)
                return Mood.Neutral;
            else if (tenant.HappyMoodCount > tenant.SadMoodCount)
                return Mood.Happy;
            else
                return Mood.Sad;
        }

        public static Pawn FindRandomTenant() {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where ThingCompUtility.TryGetComp<TenantComp>(p) != null && !p.Dead && !p.Spawned && !p.Discarded
                                select p).ToList();
            if (pawns.Count < 20)
                for (int i = 0; i < 3; i++)
                    pawns.Add(GenerateNewTenant());

            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns[0];
        }
        public static void GenerateWanted(Pawn pawn) {
            TenantComp tenantComp = pawn.TryGetComp<TenantComp>();
            WantedComp wanted = new WantedComp();
            if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                List<FactionRelation> entries = Traverse.Create(tenantComp.HiddenFaction).Field("relations").GetValue<List<FactionRelation>>().Where(p => p.kind == FactionRelationKind.Hostile).ToList();
                if (entries.Count > 0) {
                    int count = 0;
                    while (wanted.WantedBy == null && count < 10) {
                        count++;
                        entries.Shuffle();
                        if (entries[0].other.def.pawnGroupMakers != null && !entries[0].other.IsPlayer)
                            wanted.WantedBy = entries[0].other;
                    }
                }
                if (wanted.WantedBy == null) {
                    pawn.AllComps.Remove(wanted);
                }
            }
        }
        public static Pawn GenerateNewTenant() {
            bool generation = true;
            Pawn newTenant = null;
            while (generation) {
                string race = Settings.SettingsHelper.LatestVersion.AvailableRaces.RandomElement();
                PawnKindDef random = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.defName == race).RandomElement();
                if (random == null)
                    return null;
                Faction faction = RimWorld.FactionUtility.DefaultFactionFrom(random.defaultFactionType);
                newTenant = PawnGenerator.GeneratePawn(random, faction);
                if (newTenant != null && !newTenant.Dead && !newTenant.IsDessicated() && !newTenant.AnimalOrWildMan()) {
                    {
                        TenantComp tenantComp = new TenantComp();
                        tenantComp.HiddenFaction = faction;
                        newTenant.AllComps.Add(tenantComp);
                        if (Settings.SettingsHelper.LatestVersion.SimpleClothing) {
                            FloatRange range = newTenant.kindDef.apparelMoney;
                            newTenant.kindDef.apparelMoney = new FloatRange(0f, Settings.SettingsHelper.LatestVersion.SimpleClothingMax);
                            PawnApparelGenerator.GenerateStartingApparelFor(newTenant, new PawnGenerationRequest(random));
                            newTenant.kindDef.apparelMoney = range;
                        }
                        newTenant.SetFaction(Faction.OfAncients);
                        if (Settings.SettingsHelper.LatestVersion.Weapons) {
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
        public static bool GenerateContractDialogue(string title, string text, Pawn pawn, Map map, IntVec3 spawnSpot, TenantComp tenantComp) {
            DiaNode diaNode = new DiaNode(text);
            //Accepted offer, generating tenant.
            DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                action = delegate {
                    SpawnTenant(pawn, map, spawnSpot);
                    CameraJumper.TryJump(pawn);
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOption);
            //Denied tenant offer
            string text2 = tenantComp.Wanted ? "RequestForTenancyWantedRejected".Translate(pawn.LabelShort, pawn, pawn.Named("PAWN")) : "RequestForTenancyRejected".Translate(pawn.LabelShort, pawn, pawn.Named("PAWN"));
            DiaNode diaNode2 = new DiaNode(text2);
            DiaOption diaOption2 = new DiaOption("OK".Translate()) {
                resolveTree = true
            };
            diaNode2.options.Add(diaOption2);
            DiaOption diaOption3 = new DiaOption("ContractReject".Translate()) {
                action = delegate {
                    ThingCompUtility.TryGetComp<TenantComp>(pawn).CleanTenancy();
                },
                link = diaNode2
            };
            diaNode.options.Add(diaOption3);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            return true;
        }

        public static string AppendPawnDescription(string text, Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder(text);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("ContractTenantDescription".Translate(pawn.ageTracker.AgeBiologicalYears, pawn.def.label, pawn.Named("PAWN")));
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
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
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
        public static void SpawnTenant(Pawn pawn, Map map, IntVec3 spawnSpot) {
            ThingCompUtility.TryGetComp<TenantComp>(pawn).Contracted = true;
            pawn.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(pawn, spawnSpot, map);
            pawn.needs.SetInitialLevels();
            pawn.playerSettings.AreaRestriction = map.areaManager.Home;
            MapUtilities.UpdateAllRestrictions(pawn);
            MapUtilities.UpdateTimeManagement(pawn);
            if (Settings.SettingsHelper.LatestVersion.Weapons) {
                pawn.equipment.DestroyAllEquipment();
            }
        }

    }
}
