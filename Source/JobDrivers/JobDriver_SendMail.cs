using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Tenants.Comps;

namespace Tenants.JobDrivers {
    public class JobDriver_SendMail : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.A);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, false);
            Toil checkMailBox = new Toil();
            checkMailBox.initAction = delegate {
                Thing building_MailBox = checkMailBox.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                ScrollComp scroll = ThingCompUtility.TryGetComp<Comps.ScrollComp>(TargetThingB);
                scroll.Skill = pawn.skills.skills.FirstOrDefault(x => x.def.defName.ToLower() == "social").levelInt;
                ThingCompUtility.TryGetComp<MailBoxComp>(building_MailBox).OutgoingLetters.Add(TargetThingB);
                TargetThingB.Destroy();
            };
            yield return checkMailBox;
        }
    }
}
