using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Tenants.Controllers;
using Verse;
using Verse.AI;

namespace Tenants.Comps {
    public class Building_MessageBox : Building_WorkTable {
        private GraphicAlternatorComp GraphicComp => GetComp<GraphicAlternatorComp>();
        private EraAlternatorComp EraAlternatorComp => GetComp<EraAlternatorComp>();
        public override Graphic Graphic {
            get {
                return Utilities.TextureUtility.GraphicFinder(GraphicComp, EraAlternatorComp, ThingCompUtility.TryGetComp<MessageBoxComp>(this).IncomingLetters.Count < 1, this);
            }
        }
    }
    public class MessageBoxComp : ThingComp {
        public List<Thing> Items = new List<Thing>();
        public List<Thing> OutgoingLetters = new List<Thing>();
        public List<Thing> IncomingLetters = new List<Thing>();
        public override void PostExposeData() {
            Scribe_Collections.Look(ref Items, "Items", LookMode.Deep);
            Scribe_Collections.Look(ref OutgoingLetters, "OutgoingLetters", LookMode.Deep);
            Scribe_Collections.Look(ref IncomingLetters, "IncomingLetters", LookMode.Deep);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn) {
            MessageBoxComp messageBoxComp = ThingCompUtility.TryGetComp<MessageBoxComp>(parent);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            //Check inventory
            if (Items.Count > 0 || IncomingLetters.Count > 0) {
                void CheckInventory() {
                    Job job = new Job(Defs.JobDefOf.JobCheckLetters, parent);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
                FloatMenuOption checkMailBox = new FloatMenuOption("CheckMessageBox".Translate(), CheckInventory, MenuOptionPriority.High);
                list.Add(checkMailBox);
            }
            IEnumerable<Faction> factions = Find.FactionManager.AllFactions.Where(x => x.defeated == false && x.def.hidden == false && x.def.humanlikeFaction);
            //Diplomatic Letters
            List<Thing> letters = pawn.Map.listerThings.ThingsOfDef(Defs.ThingDefOf.Tenant_LetterDiplomatic);
            if (letters.Count > 0) {
                foreach (Faction faction in factions) {
                    if (messageBoxComp.OutgoingLetters.FirstOrDefault(x => ThingCompUtility.TryGetComp<LetterComp>(x).TypeValue == (int)LetterType.Diplomatic && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(Defs.ThingDefOf.Tenant_LetterDiplomatic), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<LetterComp>(letter).Faction = faction;
                            ThingCompUtility.TryGetComp<LetterComp>(letter).TypeValue = (int)LetterType.Diplomatic;
                            Job job = new Job(Defs.JobDefOf.JobSendLetter, parent, letter) {
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
            letters = pawn.Map.listerThings.ThingsOfDef(Defs.ThingDefOf.Tenant_LetterAngry);
            if (letters.Count > 0) {
                foreach (Faction faction in factions) {
                    if (messageBoxComp.OutgoingLetters.FirstOrDefault(x => ThingCompUtility.TryGetComp<LetterComp>(x).TypeValue == (int)LetterType.Angry && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(Defs.ThingDefOf.Tenant_LetterAngry), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<LetterComp>(letter).Faction = faction;
                            ThingCompUtility.TryGetComp<LetterComp>(letter).TypeValue = (int)LetterType.Angry;
                            Job job = new Job(Defs.JobDefOf.JobSendLetter, parent, letter) {
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
            letters = pawn.Map.listerThings.ThingsOfDef(Defs.ThingDefOf.Tenant_LetterInvite);
            if (letters.Count > 0) {
                foreach (Faction faction in factions.Where(x => (int)x.RelationKindWith(Find.FactionManager.OfPlayer) != 0)) {
                    if (messageBoxComp.OutgoingLetters.FirstOrDefault(x => ThingCompUtility.TryGetComp<LetterComp>(x).TypeValue == (int)LetterType.Invite && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(Defs.ThingDefOf.Tenant_LetterInvite), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<LetterComp>(letter).Faction = faction;
                            ThingCompUtility.TryGetComp<LetterComp>(letter).TypeValue = (int)LetterType.Invite;
                            Job job = new Job(Defs.JobDefOf.JobSendLetter, parent, letter) {
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
    public class CompProps_MessageBox : CompProperties {
        public CompProps_MessageBox() {
            compClass = typeof(MessageBoxComp);
        }
    }
}

