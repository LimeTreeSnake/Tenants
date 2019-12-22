using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Tenants {
    public class Building_MessageBox : Building_WorkTable {
        private Graphic cachedGraphicFull;
        public override Graphic Graphic {
            get {
                if (this.GetMessageBoxComponent().IncomingLetters.Count > 0) {

                    if (def.building.fullGraveGraphicData == null) {
                        return base.Graphic;
                    }
                    if (cachedGraphicFull == null) {
                        cachedGraphicFull = def.building.fullGraveGraphicData.GraphicColoredFor(this);
                    }
                    return cachedGraphicFull;
                }
                return base.Graphic;
            }
        }
    }
    public class MessageBox : ThingComp {
        public List<Thing> Items = new List<Thing>();
        public List<Letter> OutgoingLetters = new List<Letter>();
        public List<Letter> IncomingLetters = new List<Letter>();
        public override void PostExposeData() {
            Scribe_Collections.Look(ref Items, "Items", LookMode.Deep);
            Scribe_Collections.Look(ref OutgoingLetters, "OutgoingLetters", LookMode.Deep);
            Scribe_Collections.Look(ref IncomingLetters, "IncomingLetters", LookMode.Deep);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn) {
            MessageBox messageBoxComp = parent.GetMessageBoxComponent();
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            //Check inventory
            if (Items.Count > 0) {
                void CheckInventory() {
                    Job job = new Job(JobDefOf.JobCheckLetters, parent);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
                FloatMenuOption checkMailBox = new FloatMenuOption("CheckMessageBox".Translate(), CheckInventory, MenuOptionPriority.High);
                list.Add(checkMailBox);
            }
            IEnumerable<Faction> factions = Find.FactionManager.AllFactions.Where(x => x.defeated == false && x.def.hidden == false && x.def.humanlikeFaction);
            //Diplomatic Letters
            List<Thing> letters = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Tenant_LetterDiplomatic);
            if (letters.Count > 0) {
                foreach (Faction faction in factions) {
                    if (messageBoxComp.OutgoingLetters.FirstOrDefault(x => x.Props.letter == LetterType.Diplomatic && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(ThingDefOf.Tenant_LetterDiplomatic), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<Letter>(letter).Faction = faction;
                            Job job = new Job(JobDefOf.JobSendLetter, parent, letter) {
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
            letters = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Tenant_LetterAngry);
            if (letters.Count > 0) {
                foreach (Faction faction in factions) {
                    if (messageBoxComp.OutgoingLetters.FirstOrDefault(x => x.Props.letter == LetterType.Angry && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(ThingDefOf.Tenant_LetterAngry), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<Letter>(letter).Faction = faction;
                            Job job = new Job(JobDefOf.JobSendLetter, parent, letter) {
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
            letters = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Tenant_LetterInvite);
            if (letters.Count > 0) {
                foreach (Faction faction in factions.Where(x => (int)x.RelationKindWith(Find.FactionManager.OfPlayer) != 0)) {
                    if (messageBoxComp.OutgoingLetters.FirstOrDefault(x => x.Props.letter == LetterType.Invite && x.Faction == faction) == null) {
                        void SendMail() {
                            Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(ThingDefOf.Tenant_LetterInvite), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                            ThingCompUtility.TryGetComp<Letter>(letter).Faction = faction;
                            Job job = new Job(JobDefOf.JobSendLetter, parent, letter) {
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
            compClass = typeof(MessageBox);
        }
    }
}

