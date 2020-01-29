using System.Collections.Generic;
using Verse;
using Verse.AI;
using Tenants.Comps;

namespace Tenants.JobDrivers {
    public class JobDriver_CheckLetters : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
            Toil CheckLetters = new Toil();
            CheckLetters.initAction = delegate {
                Thing building_MessageBox = CheckLetters.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                MessageBoxComp comp = ThingCompUtility.TryGetComp<MessageBoxComp>(building_MessageBox);
                Controllers.CourierController.EmptyMessageBox(ref comp.Items, comp.parent.Position);
                Controllers.CourierController.RecieveLetters(ref comp.IncomingLetters, comp.parent.Position, comp.parent.Map);
            };
            yield return CheckLetters;
        }
    }
}
