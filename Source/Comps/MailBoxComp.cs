using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Tenants.Controllers;
using Verse;
using Verse.AI;

namespace Tenants.Comps {
    public class Building_MailBox : Building_WorkTable {
      
    }
    public class MailBoxComp : ThingComp {
        public List<Thing> Items = new List<Thing>();
        public List<Thing> OutgoingLetters = new List<Thing>();
        public List<Thing> IncomingLetters = new List<Thing>();
        public override void PostExposeData() {
            Scribe_Collections.Look(ref Items, "Items", LookMode.Deep);
            Scribe_Collections.Look(ref OutgoingLetters, "OutgoingLetters", LookMode.Deep);
            Scribe_Collections.Look(ref IncomingLetters, "IncomingLetters", LookMode.Deep);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn) {
            MailBoxComp mailBoxComp = ThingCompUtility.TryGetComp<MailBoxComp>(parent);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            //Check inventory
            if (Items.Count > 0 || IncomingLetters.Count > 0) {
                void CheckInventory() {
                    Job job = new Job(Defs.JobDefOf.JobCheckMail, parent);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
                FloatMenuOption checkMailBox = new FloatMenuOption("CheckMailBox".Translate(), CheckInventory, MenuOptionPriority.High);
                list.Add(checkMailBox);
            }
            IEnumerable<Faction> factions = Find.FactionManager.AllFactions.Where(x => x.defeated == false && x.def.hidden == false && x.def.humanlikeFaction);
            //Diplomatic Letters
            List<Thing> letters = pawn.Map.listerThings.ThingsOfDef(Defs.ThingDefOf.Tenant_ScrollDiplomatic);
            if (letters.Count > 0) {
                foreach (Faction faction in factions) {
                    if (mailBoxComp.OutgoingLetters.FirstOrDefault(x => ThingCompUtility.TryGetComp<ScrollComp>(x).TypeValue == (int)ScrollType.Diplomatic && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(Defs.ThingDefOf.Tenant_ScrollDiplomatic), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<ScrollComp>(letter).Faction = faction;
                            ThingCompUtility.TryGetComp<ScrollComp>(letter).TypeValue = (int)ScrollType.Diplomatic;
                            Job job = new Job(Defs.JobDefOf.JobSendMail, parent, letter) {
                                count = 1
                            };
                            pawn.jobs.TryTakeOrderedJob(job);
                        }
                        FloatMenuOption sendMail = new FloatMenuOption("SendLetterDiplomatic".Translate(faction), SendMail);
                        list.Add(sendMail);
                    }
                }
            }
            //Angry Letters
            letters = pawn.Map.listerThings.ThingsOfDef(Defs.ThingDefOf.Tenant_ScrollMean);
            if (letters.Count > 0) {
                foreach (Faction faction in factions) {
                    if (mailBoxComp.OutgoingLetters.FirstOrDefault(x => ThingCompUtility.TryGetComp<ScrollComp>(x).TypeValue == (int)ScrollType.Angry && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(Defs.ThingDefOf.Tenant_ScrollMean), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<ScrollComp>(letter).Faction = faction;
                            ThingCompUtility.TryGetComp<ScrollComp>(letter).TypeValue = (int)ScrollType.Angry;
                            Job job = new Job(Defs.JobDefOf.JobSendMail, parent, letter) {
                                count = 1
                            };
                            pawn.jobs.TryTakeOrderedJob(job);
                        }
                        FloatMenuOption sendMail = new FloatMenuOption("SendLetterAngry".Translate(faction), SendMail);
                        list.Add(sendMail);
                    }
                }
            }
            //Invite Letters
            letters = pawn.Map.listerThings.ThingsOfDef(Defs.ThingDefOf.Tenant_ScrollInvite);
            if (letters.Count > 0) {
                foreach (Faction faction in factions.Where(x => (int)x.RelationKindWith(Find.FactionManager.OfPlayer) != 0)) {
                    if (mailBoxComp.OutgoingLetters.FirstOrDefault(x => ThingCompUtility.TryGetComp<ScrollComp>(x).TypeValue == (int)ScrollType.Invite && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(Defs.ThingDefOf.Tenant_ScrollInvite), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<ScrollComp>(letter).Faction = faction;
                            ThingCompUtility.TryGetComp<ScrollComp>(letter).TypeValue = (int)ScrollType.Invite;
                            Job job = new Job(Defs.JobDefOf.JobSendMail, parent, letter) {
                                count = 1
                            };
                            pawn.jobs.TryTakeOrderedJob(job);
                        }
                        FloatMenuOption sendMail = new FloatMenuOption("SendLetterInvite".Translate(faction), SendMail);
                        list.Add(sendMail);
                    }
                }
            }
            return list.AsEnumerable();
        }

    }
    public class CompProps_MailBox : CompProperties {
        public CompProps_MailBox() {
            compClass = typeof(MailBoxComp);
        }
    }
}

