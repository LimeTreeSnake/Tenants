using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Tenants
{
    public class MailBox : ThingComp
    {
        public List<Thing> Items = new List<Thing>();
        public List<Thing> Letters = new List<Thing>();
        public override void PostExposeData() {
            Scribe_Collections.Look(ref Items, "Items", LookMode.Deep);
            Scribe_Collections.Look(ref Letters, "Letters", LookMode.Deep);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn) {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            //Check inventory
            if (Items.Count > 0) {
                void CheckInventory() {
                    Job job = new Job(JobDefOf.JobCheckMailBox, parent);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
                FloatMenuOption checkMailBox = new FloatMenuOption("CheckMailBox".Translate(), CheckInventory, MenuOptionPriority.High);
                list.Add(checkMailBox);
            }
            //Diplomatic Letters
            List<Thing> letters = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Tenant_LetterDiplomatic);
            if (letters.Count > 0) {
                foreach (Faction faction in Find.FactionManager.AllFactions) {
                    void SendMail() {
                        Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(ThingDefOf.Tenant_LetterDiplomatic), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                        ThingCompUtility.TryGetComp<Letter>(letter).faction = faction;
                        Job job = new Job(JobDefOf.JobSendMail, parent, letter);
                        pawn.jobs.TryTakeOrderedJob(job);
                        Log.Message(faction.Name);
                    }
                    FloatMenuOption sendMail = new FloatMenuOption("SendLetterDiplomatic".Translate(faction), SendMail);
                    list.Add(sendMail);
                }
            }
            //Angry Letters
            letters = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Tenant_LetterAngry);
            if (letters.Count > 0) {
                foreach (Faction faction in Find.FactionManager.AllFactions) {
                    void SendMail() {
                        Thing letter = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(parent.Position, parent.Map, ThingRequest.ForDef(ThingDefOf.Tenant_LetterAngry), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Unspecified));
                        ThingCompUtility.TryGetComp<Letter>(letter).faction = faction;
                        Job job = new Job(JobDefOf.JobSendMail, parent, letter);
                        pawn.jobs.TryTakeOrderedJob(job);
                        Log.Message(faction.Name);
                    }
                    FloatMenuOption sendMail = new FloatMenuOption("SendLetterAngry".Translate(faction), SendMail);
                    list.Add(sendMail);
                }
            }

            return list.AsEnumerable();
        }
        public void EmptyMailBox() {
            if (Items.Count > 0) {
                foreach (Thing thing in Items) {
                    DebugThingPlaceHelper.DebugSpawn(thing.def, parent.Position, thing.stackCount);
                }
                Items.Clear();
            }
        }
    }


    public class CompProps_MailBox : CompProperties
    {
        public CompProps_MailBox() {
            compClass = typeof(MailBox);
        }
    }
}

