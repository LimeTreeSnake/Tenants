using Harmony;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants {
    public static class UtilityTenant {
        public static void TenancyCheck(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp != null) {
                //If tenant belongs to player faction
                if (pawn.IsColonist && tenantComp.IsTenant) {
                    //If a tenant has joined but has no contract -> regular colonist.
                    if (!tenantComp.Contracted) {
                        tenantComp.IsTenant = false;
                    }
                    //Tenant alone with no colonist
                    if (pawn.Map.mapPawns.FreeColonists.FirstOrDefault(x => x.GetTenantComponent().IsTenant == false) == null) {
                        ContractConclusion(pawn, true, 0.75f);
                        return;
                    }
                    //Tenant contract is out
                    if (Find.TickManager.TicksGame >= tenantComp.ContractEndTick) {
                        ContractConclusion(pawn);
                        return;
                    }
                    //If tenancy is to be terminated
                    if (tenantComp.IsTerminated) {
                        if (pawn.health.Downed) {
                            Messages.Message("ContractTerminateFail".Translate(), MessageTypeDefOf.NeutralEvent);
                        }
                        else {
                            TerminateContract(pawn);
                            return;
                        }
                        tenantComp.IsTerminated = false;
                    }
                    //Surgery
                    if (pawn.BillStack.Count > 0) {
                        if ((pawn.BillStack.Bills.Where(x => x.recipe.isViolation == true).Count() > 0)) {
                            pawn.BillStack.Clear();
                            tenantComp.SurgeryQueue++;
                            if (tenantComp.SurgeryQueue < 2) {
                                Messages.Message("TenantSurgeryWarning".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
                            }
                            else {
                                Messages.Message("TenantSurgeryLeave".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                                Leave(pawn, Find.FactionManager.OfAncients);
                            }
                        }
                    }
                    //Tenancy tick per day
                    if (Find.TickManager.TicksGame % 60000 == 0) {
                        if (tenantComp.Wanted) {
                            if (!MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                                TenantWanted(pawn);
                            }
                        }
                    }
                    //Tenancy tick 1/10 per day
                    if (Find.TickManager.TicksGame % 6000 == 0) {
                        //Mole
                        if (tenantComp.MoleMessage) {
                            tenantComp.MoleMessage = false;
                            Messages.Message("TenantMoleMessage".Translate(), MessageTypeDefOf.NegativeEvent);
                        }
                        if (tenantComp.Mole && !tenantComp.MoleActivated) {
                            if (CalculateMood(tenantComp) == Mood.Sad && tenantComp.NeutralMoodCount > 2) {
                                Building building = pawn.Map.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x.def.defName.ToLower().Contains("commsconsole"));
                                if (building != null) {
                                    Job job = new Job(JobDefOf.JobUseCommsConsoleMole, building);
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }
                            }
                        }
                        //Join
                        float mood = pawn.needs.mood.CurInstantLevel;
                        if (!tenantComp.IsEnvoy && mood > 0.8f) {
                            TenantWantToJoin(pawn);
                        }
                        //Calculate mood
                        if (mood > SettingsHelper.LatestVersion.LevelOfHappiness) {
                            tenantComp.HappyMoodCount++;
                        }
                        else if (mood < pawn.mindState.mentalBreaker.BreakThresholdMinor) {
                            tenantComp.SadMoodCount++;
                            if (tenantComp.RecentBadMoodsCount > 5) {
                                ContractConclusion(pawn, true);
                            }
                        }
                        else {
                            tenantComp.NeutralMoodCount++;
                        }
                    }

                }
            }
        }
        public static void ContractConclusion(Pawn pawn, bool terminated = false, float stealChance = 0.5f) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (!tenantComp.IsTenant && !pawn.IsColonist)
                return;
            //In case the tenant decides to terminate the contract
            if (terminated) {
                if (tenantComp.IsEnvoy) {
                    int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneEnvoyTerminated".Translate(tenantComp.HiddenFaction, val, pawn.Named("PAWN")), LetterDefOf.NegativeEvent);
                    Leave(pawn, tenantComp.HiddenFaction);
                }
                else if (Rand.Value > stealChance) {
                    MakePayment(pawn);
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTerminated".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                    Leave(pawn, Find.FactionManager.OfAncients);
                }
                else {
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTheft".Translate(pawn.Named("PAWN")), LetterDefOf.NegativeEvent);
                    Theft(pawn, tenantComp.HiddenFaction);
                }
                return;
            }
            Mood mood = CalculateMood(tenantComp);
            if (tenantComp.IsEnvoy) {
                switch (mood) {
                    case Mood.Neutral: {
                            int val = Utility.ChangeRelations(tenantComp.HiddenFaction);
                            Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneEnvoy".Translate(tenantComp.HiddenFaction, val, pawn.Named("PAWN")), LetterDefOf.PositiveEvent);
                            ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceNeutral / 100f);
                            break;
                        }
                    case Mood.Happy: {
                            int val = Utility.ChangeRelations(tenantComp.HiddenFaction);
                            val += Utility.ChangeRelations(tenantComp.HiddenFaction);
                            Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneEnvoyHappy".Translate(tenantComp.HiddenFaction, val, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                            ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceHappy / 100f);
                            break;
                        }
                    case Mood.Sad: {
                            int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                            Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneEnvoySad".Translate(tenantComp.HiddenFaction, val, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                            Leave(pawn, tenantComp.HiddenFaction);
                            break;
                        }
                }
            }
            else {
                switch (mood) {
                    case Mood.Neutral: {
                            MakePayment(pawn);
                            Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDone".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.PositiveEvent);
                            ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceNeutral / 100f);
                            break;
                        }
                    case Mood.Happy: {
                            MakePayment(pawn);
                            Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneHappy".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                            ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceHappy / 100f);
                            break;
                        }
                    case Mood.Sad: {
                            MakePayment(pawn);
                            Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneSad".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                            Leave(pawn, Find.FactionManager.OfAncients);
                            break;
                        }
                }
            }
        }
        public static void ContractProlong(Pawn pawn, float chance) {
            Tenant tenantComp = pawn.TryGetComp<Tenant>();
            if (Rand.Value < chance) {
                if (tenantComp.IsEnvoy) {
                    tenantComp.ContractDate = Find.TickManager.TicksGame;
                    tenantComp.ContractEndDate = Find.TickManager.TicksAbs + (tenantComp.ContractLength * 2) + 60000;
                    tenantComp.ResetMood();
                    string letterLabel = "Envoy".Translate(tenantComp.HiddenFaction);
                    string letterText = "EnvoyStays".Translate(tenantComp.HiddenFaction, pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent);
                    return;
                }
                else {
                    if (tenantComp.AutoRenew) {
                        tenantComp.ContractDate = Find.TickManager.TicksGame;
                        tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
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
            if (!Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return false;
            }
            Pawn pawn = FindRandomTenant();
            if (pawn == null)
                return false;
            pawn.relations.everSeenByPlayer = true;
            Tenant tenantComp = pawn.TryGetComp<Tenant>();
            Utility.GenerateBasicContract(tenantComp, Rand.Range(SettingsHelper.LatestVersion.MinDailyCost, SettingsHelper.LatestVersion.MaxDailyCost));
            StringBuilder stringBuilder = new StringBuilder("");
            //Wanted
            if (Rand.Value < 0.2f) {
                GenerateWanted(pawn);
            }
            //Mole
            if (Rand.Value < 0.33f && tenantComp.HiddenFaction.HostileTo(Find.FactionManager.OfPlayer)) {
                tenantComp.Mole = true;
            }
            if (pawn.GetTenantComponent().Wanted) {
                stringBuilder.Append("RequestForTenancyWantedInitial".Translate(tenantComp.WantedBy, pawn.Named("PAWN")));
                tenantComp.Payment = tenantComp.Payment * 2;
                GenerateContractDialogue("RequestForTenancyWantedTitle".Translate(map.Parent.Label), AppendContractDetails(stringBuilder.ToString(), pawn), pawn, map, spawnSpot, tenantComp);
            }
            //Broadcasted
            else if (MapComponent_Tenants.GetComponent(map).Broadcast) {
                stringBuilder.Append("RequestForTenancyOpportunity".Translate(pawn.Named("PAWN")));
                MapComponent_Tenants.GetComponent(map).Broadcast = false;
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
            pawn.GetTenantComponent().CleanTenancy();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
            if (MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Remove(pawn);
            }
        }
        public static void Theft(Pawn pawn, Faction faction) {
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(faction);
            pawn.GetTenantComponent().CleanTenancy();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_TenantTheft(), pawn.Map, new List<Pawn> { pawn });
        }
        public static void TerminateContract(Pawn pawn) {
            pawn.jobs.ClearQueuedJobs();
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp.IsEnvoy) {
                int val = Rand.Range(SettingsHelper.LatestVersion.MinRelation, SettingsHelper.LatestVersion.MaxRelation + 1);
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
            Tenant tenantComp = pawn.GetTenantComponent();
            string label = "Death".Translate() + ": " + pawn.LabelShortCap;
            tenantComp.IsTenant = false;
            pawn.SetFaction(tenantComp.HiddenFaction);
            string text;
            if (tenantComp.IsEnvoy) {
                int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                val += Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                text = "EnvoyDeath".Translate(tenantComp.HiddenFaction, val, pawn.Named("PAWN"));
                text = text.AdjustedFor(pawn);
            }
            else {
                text = "TenantDeath".Translate(pawn.Named("PAWN"));
                text = text.AdjustedFor(pawn);
                if (pawn.Faction.HostileTo(Find.FactionManager.OfPlayer)) {
                    if (Rand.Value < 0.5f) {
                        if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                            int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                            Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                            MapComponent_Tenants.GetComponent(pawn.Map).DeadTenantsToAvenge.Add(pawn);
                            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                            parms.raidStrategy = RaidStrategyDefOf.Retribution;
                            parms.forced = true;
                            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RetributionForDead, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                        }
                    }
                }
                else {
                    if (Rand.Value < 0.66f) {
                        if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                            int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                            val += Utility.ChangeRelations(tenantComp.HiddenFaction, true);
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
            Tenant tenantComp = pawn.GetTenantComponent();
            tenantComp.IsTenant = false;
            pawn.GetTenantComponent().CapturedTenant = true;
            pawn.SetFaction(tenantComp.HiddenFaction);

            if (pawn.Faction.HostileTo(Find.FactionManager.OfPlayer)) {
                if (Rand.Value < 0.25f || tenantComp.Wanted) {
                    int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                    Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                    MapComponent_Tenants.GetComponent(byPawn.Map).CapturedTenantsToAvenge.Add(pawn);
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, byPawn.Map);
                    parms.raidStrategy = RaidStrategyDefOf.Retribution;
                    parms.forced = true;
                    Find.Storyteller.incidentQueue.Add(IncidentDefOf.RetributionForCaptured, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                }
            }
            else {
                if (Rand.Value < 0.66f || tenantComp.Wanted) {
                    if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                        int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                        Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                    }
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

                string text = "RequestWantToJoin".Translate(pawn.Named("PAWN"));

                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        MakePayment(pawn);
                        tenantComp.IsTenant = false;
                        Messages.Message("ContractDone".Translate(pawn.Name.ToStringFull, tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
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
            if (Rand.Value < 0.66 && tenantComp.WantedBy.HostileTo(Find.FactionManager.OfPlayer) && !MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                Messages.Message("HarboringWantedTenant".Translate(pawn.GetTenantComponent().WantedBy, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Add(pawn);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = RaidStrategyDefOf.WantedRaid;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.WantedRaid, Find.TickManager.TicksGame + Rand.Range(100000, 300000), parms, 60000);
            }
            else if (Rand.Value < 0.5) {
                int val = Utility.ChangeRelations(tenantComp.HiddenFaction, true);
                Messages.Message("HarboringWantedTenant".Translate(pawn.GetTenantComponent().WantedBy, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }
        }
        public static void TenantInvite(Building_CommsConsole comms, Pawn pawn) {
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
        public static Mood CalculateMood(Tenant tenant) {
            float count = tenant.HappyMoodCount + tenant.NeutralMoodCount + tenant.SadMoodCount;
            if (tenant.NeutralMoodCount / count >= 0.5f)
                return Mood.Neutral;
            else if (tenant.HappyMoodCount > tenant.SadMoodCount)
                return Mood.Happy;
            else
                return Mood.Sad;
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

            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns[0];
        }
        public static void GenerateWanted(Pawn pawn) {
            Tenant tenantComp = pawn.TryGetComp<Tenant>();
            if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                List<FactionRelation> entries = Traverse.Create(tenantComp.HiddenFaction).Field("relations").GetValue<List<FactionRelation>>().Where(p => p.kind == FactionRelationKind.Hostile).ToList();
                if (entries.Count > 0) {
                    int count = 0;
                    while (tenantComp.WantedBy == null && count < 10) {
                        count++;
                        entries.Shuffle();
                        if (entries[0].other.def.pawnGroupMakers != null && !entries[0].other.IsPlayer)
                            tenantComp.WantedBy = entries[0].other;
                    }
                    if (tenantComp.WantedBy != null)
                        tenantComp.Wanted = true;
                }
            }
        }
        public static Pawn GenerateNewTenant() {
            bool generation = true;
            Pawn newTenant = null;
            while (generation) {
                string race = SettingsHelper.LatestVersion.AvailableRaces.RandomElement();
                PawnKindDef random = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.defName == race).RandomElement();
                if (random == null)
                    return null;
                Faction faction = FactionUtility.DefaultFactionFrom(random.defaultFactionType);
                newTenant = PawnGenerator.GeneratePawn(random, faction);
                if (newTenant != null && !newTenant.Dead && !newTenant.IsDessicated() && !newTenant.AnimalOrWildMan()) {
                    {
                        if (SettingsHelper.LatestVersion.SimpleClothing) {
                            FloatRange range = newTenant.kindDef.apparelMoney;
                            newTenant.kindDef.apparelMoney = new FloatRange(0f, SettingsHelper.LatestVersion.SimpleClothingMax);
                            PawnApparelGenerator.GenerateStartingApparelFor(newTenant, new PawnGenerationRequest(random));
                            newTenant.kindDef.apparelMoney = range;
                        }
                        Utility.RemoveExpensiveItems(newTenant);
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
        public static bool GenerateContractDialogue(string title, string text, Pawn pawn, Map map, IntVec3 spawnSpot, Tenant tenantComp) {
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
                    pawn.GetTenantComponent().CleanTenancy();
                },
                link = diaNode2
            };
            diaNode.options.Add(diaOption3);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            return true;
        }
        public static void MakePayment(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            int payment = (tenantComp.ContractLength / 60000) * tenantComp.Payment;
            while (payment > 500) {
                Thing silver = ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver);
                silver.stackCount = 500;
                MapComponent_Tenants.GetComponent(pawn.Map).IncomingMail.Add(silver);
                payment = payment - 500;
            }
            Thing silverRest = ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver);
            silverRest.stackCount = payment;
            MapComponent_Tenants.GetComponent(pawn.Map).IncomingMail.Add(silverRest);
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
        public static void SpawnTenant(Pawn pawn, Map map, IntVec3 spawnSpot) {
            pawn.GetTenantComponent().Contracted = true;
            pawn.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(pawn, spawnSpot, map);
            pawn.needs.SetInitialLevels();
            pawn.playerSettings.AreaRestriction = map.areaManager.Home;
            Utility.UpdateAllRestrictions(pawn);
            Utility.UpdateTimeManagement(pawn);           
            if (SettingsHelper.LatestVersion.Weapons) {
                pawn.equipment.DestroyAllEquipment();
            }
        }
        public static Pawn FindRandomEnvoy(Faction faction) {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.GetTenantComponent() != null && p.GetTenantComponent().IsTenant && !p.Dead && !p.Spawned && !p.Discarded && p.Faction != null && p.Faction == faction
                                select p).ToList();
            if (pawns.Count < 3)
                for (int i = 0; i < 3; i++)
                    pawns.Add(GenerateEnvoy(faction));
            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns[0];
        }
        public static Pawn GenerateEnvoy(Faction faction) {
            bool generation = true;
            Pawn newTenant = null;
            while (generation) {
                string race = SettingsHelper.LatestVersion.AvailableRaces.RandomElement();
                PawnKindDef random = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.defName == race && x.defaultFactionType == faction.def).RandomElement();
                if (random == null)
                    return null;
                newTenant = PawnGenerator.GeneratePawn(random, faction);
                if (newTenant != null && !newTenant.Dead && !newTenant.IsDessicated() && !newTenant.AnimalOrWildMan()) {
                    {
                        if (SettingsHelper.LatestVersion.SimpleClothing) {
                            FloatRange range = newTenant.kindDef.apparelMoney;
                            newTenant.kindDef.apparelMoney = new FloatRange(0f, SettingsHelper.LatestVersion.SimpleClothingMax);
                            PawnApparelGenerator.GenerateStartingApparelFor(newTenant, new PawnGenerationRequest(random));
                            newTenant.kindDef.apparelMoney = range;
                        }
                        Utility.RemoveExpensiveItems(newTenant);
                        newTenant.GetTenantComponent().IsTenant = true;
                        newTenant.GetTenantComponent().HiddenFaction = faction;
                        newTenant.SetFaction(faction);
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
        public static bool EnvoyTenancy(Map map, Faction faction) {
            if (!Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return false;
            }
            Pawn pawn = FindRandomEnvoy(faction);
            if (pawn == null) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return false;
            }
            pawn.relations.everSeenByPlayer = true;
            Tenant tenantComp = pawn.TryGetComp<Tenant>();
            tenantComp.IsEnvoy = true;
            SpawnTenant(pawn, map, spawnSpot);
            Utility.GenerateBasicContract(tenantComp, 0, 2);
            Messages.Message("EnvoyArriveSuccess".Translate(faction, pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            CameraJumper.TryJump(pawn);
            return true;
        }
    }
}
