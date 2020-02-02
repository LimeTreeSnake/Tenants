using System.Collections.Generic;
using Verse;
using Verse.AI;
using Tenants.Comps;

namespace Tenants.JobDrivers {
    public class JobDriver_CheckMail : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
            Toil CheckMail = new Toil();
            CheckMail.initAction = delegate {
                Thing building_MailBox = CheckMail.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                MailBoxComp comp = ThingCompUtility.TryGetComp<MailBoxComp>(building_MailBox);
                Controllers.CourierController.EmptyMailBox(ref comp.Items, comp.parent.Position);
                Controllers.CourierController.RecieveLetters(ref comp.IncomingLetters, comp.parent.Position, comp.parent.Map);
            };
            yield return CheckMail;
        }
    }
}
