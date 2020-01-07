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
    public static class Events {
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
                int mood = Utility.CalculateMood(tenantComp);
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
                    Utility.MakePayment(pawn);

                    string letterLabel = "ContractNew".Translate();
                    string letterText = "ContractRenewedMessage".Translate(pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                    return;
                }
                string text = Utility.ProlongContractMessage(pawn);
                DiaNode diaNode = new DiaNode(text);
                //Accepted offer.
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        Utility.MakePayment(pawn);
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
            if (!Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return false;
            }
            string title = "", text = "";
            Pawn pawn = Utility.FindRandomTenant();
            if (pawn == null)
                return false;
            pawn.relations.everSeenByPlayer = true;
            Tenant tenantComp = pawn.TryGetComp<Tenant>();
            Utility.GenerateBasicTenancyContract(tenantComp);
            StringBuilder stringBuilder = new StringBuilder("");
            //Check if pawn is special
            //Wanted
            if (Rand.Value < 0.2f) {
                Utility.GenerateWanted(pawn);
            }
            //Mole
            if (Rand.Value < 0.33f && tenantComp.HiddenFaction.HostileTo(Find.FactionManager.OfPlayer)) {
                tenantComp.Mole = true;
            }
            if (pawn.GetTenantComponent().Wanted) {
                stringBuilder.Append("RequestForTenancyHiding".Translate(tenantComp.WantedBy, pawn.Named("PAWN")));
                title = "RequestForTenancyHidingTitle".Translate(map.Parent.Label);
                tenantComp.Payment = tenantComp.Payment * 2;
                text = Utility.AppendContractDetails(stringBuilder.ToString(), pawn);
            }
            //Broadcasted
            else if (MapComponent_Tenants.GetComponent(map).Broadcast) {
                stringBuilder.Append("RequestForTenancyOpportunity".Translate(pawn.Named("PAWN")));
                title = "RequestForTenancyTitle".Translate(map.Parent.Label);
                MapComponent_Tenants.GetComponent(map).Broadcast = false;
                text = Utility.AppendContractDetails(stringBuilder.ToString(), pawn);
            }
            //Normal
            else {
                stringBuilder.Append("RequestForTenancyInitial".Translate(pawn.Named("PAWN")));
                title = "RequestForTenancyTitle".Translate(map.Parent.Label);
                text = Utility.AppendContractDetails(stringBuilder.ToString(), pawn);

            }
            if (Utility.GenerateContractDialogue(title, text, pawn, map, spawnSpot)) {

            }
            return true;
        }
        public static bool Courier(Map map, Building box) {
            try {
                if (!Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                    return false;
                }
                if (MapComponent_Tenants.GetComponent(map).BroadcastCourier == true) {
                    MapComponent_Tenants.GetComponent(map).BroadcastCourier = false;
                }
                if (MapComponent_Tenants.GetComponent(map).KilledCourier > 0) {
                    MapComponent_Tenants.GetComponent(map).KilledCourier--;
                    string courierDeniedLabel = "CourierDeniedTitle".Translate(map.Parent.Label);
                    string courierDeniedText = "CourierDeniedMessage".Translate();
                    Find.LetterStack.ReceiveLetter(courierDeniedLabel, courierDeniedText, LetterDefOf.NegativeEvent);
                    return true;
                }
                Pawn pawn = Utility.FindRandomCourier();
                if (pawn == null)
                    return false;
                GenSpawn.Spawn(pawn, spawnSpot, map);
                pawn.SetFaction(Faction.OfAncients);
                pawn.relations.everSeenByPlayer = true;
                Utility.CourierDress(pawn, map);
                Utility.CourierInventory(pawn, map);
                string letterLabel = "CourierArrivedTitle".Translate(map.Parent.Label);
                string letterText = "CourierArrivedMessage".Translate(pawn.Named("PAWN"));
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, pawn);
                LordMaker.MakeNewLord(pawn.Faction, new LordJob_CourierDeliver(map.listerThings.ThingsOfDef(Defs.ThingDefOf.Tenants_MailBox).RandomElement()), pawn.Map, new List<Pawn> { pawn });
                return true;
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
                return false;
            }
        }
        public static void TenantLeave(Pawn pawn) {
            Utility.MakePayment(pawn);
            pawn.jobs.ClearQueuedJobs();
            pawn.SetFaction(Faction.OfAncients);
            pawn.GetTenantComponent().CleanTenancy();
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
            if (MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Remove(pawn);
            }
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
                    if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {

                        FactionRelation relation = tenantComp.HiddenFaction.RelationWith(Find.FactionManager.OfPlayer);
                        relation.goodwill = relation.goodwill - SettingsHelper.LatestVersion.OutragePenalty * 2;
                        Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                        MapComponent_Tenants.GetComponent(pawn.Map).DeadTenantsToAvenge.Add(pawn);
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
                        FactionRelation relation = tenantComp.HiddenFaction.RelationWith(Find.FactionManager.OfPlayer);
                        relation.goodwill = relation.goodwill - SettingsHelper.LatestVersion.OutragePenalty * 2;
                        Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty * 2, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                    }
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
                    parms.raidStrategy = Defs.RaidStrategyDefOf.Retribution;
                    parms.forced = true;
                    Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.RetributionForCaptured, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                }
            }
            else {
                if (Rand.Value < 0.66f || tenantComp.Wanted) {
                    if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                        FactionRelation relation = pawn.Faction.RelationWith(Find.FactionManager.OfPlayer);
                        relation.goodwill = relation.goodwill - SettingsHelper.LatestVersion.OutragePenalty;
                        Messages.Message("TenantFactionOutrage".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
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

                string text = "RequestTenantWantToJoin".Translate(pawn.Named("PAWN"));

                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        Utility.MakePayment(pawn);
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
            parms.raidStrategy = Defs.RaidStrategyDefOf.MoleRaid;
            parms.forced = true;
            Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.MoleRaid, Find.TickManager.TicksGame + Rand.Range(5000, 30000), parms, 90000);
        }
        public static void TenantWanted(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (Rand.Value < 0.66 && tenantComp.WantedBy.HostileTo(Find.FactionManager.OfPlayer) && !MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Add(pawn);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = Defs.RaidStrategyDefOf.WantedRaid;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.WantedRaid, Find.TickManager.TicksGame + Rand.Range(100000, 300000), parms, 60000);
            }
            else if (Rand.Value < 0.5) {
                tenantComp.WantedBy.RelationWith(Find.FactionManager.OfPlayer).goodwill -= SettingsHelper.LatestVersion.HarborPenalty;
                Find.FactionManager.OfPlayer.RelationWith(tenantComp.WantedBy).goodwill -= SettingsHelper.LatestVersion.HarborPenalty;

                Messages.Message("HarboringWantedTenant".Translate(pawn.GetTenantComponent().WantedBy, SettingsHelper.LatestVersion.HarborPenalty, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }

        }
        public static void TenantInvite(Building_CommsConsole comms, Pawn pawn) {
            Messages.Message("InviteTenantMessage".Translate(), MessageTypeDefOf.NeutralEvent);
            MapComponent_Tenants.GetComponent(pawn.Map).Broadcast = true;
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

        public static void CourierInvite(Building_CommsConsole comms, Pawn pawn) {
            if (MapComponent_Tenants.GetComponent(pawn.Map).KilledCourier > 0) {
                string courierDeniedLabel = "CourierDeniedTitle".Translate(pawn.Map.Parent.Label);
                string courierDeniedText = "CourierDeniedRadioMessage".Translate();
                Find.LetterStack.ReceiveLetter(courierDeniedLabel, courierDeniedText, LetterDefOf.NegativeEvent);
            }
            else {
                Messages.Message("CourierInvited".Translate(SettingsHelper.LatestVersion.CourierCost), MessageTypeDefOf.NeutralEvent);
                MapComponent_Tenants.GetComponent(pawn.Map).BroadcastCourier = true;
                IncidentParms parms = new IncidentParms() { target = pawn.Map, forced = true };
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.TenantCourier, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                Thing silver = ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver);
                silver.stackCount = (int) SettingsHelper.LatestVersion.CourierCost;
                MapComponent_Tenants.GetComponent(pawn.Map).CourierCost.Add(silver);

            }
        }

    }
}
