using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Tenants {
    public class JobDriver_UseCommsConsoleTenants : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            Pawn pawn = base.pawn;
            LocalTargetInfo targetA = base.job.targetA;
            Job job = base.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to)
            {
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                return !building_CommsConsole.CanUseCommsNow;
            });
            Toil inviteTenant = new Toil();
            inviteTenant.initAction = delegate
            {
                Pawn actor = inviteTenant.actor;
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (building_CommsConsole.CanUseCommsNow) {
                    Utility.InviteTenant(building_CommsConsole, actor);
                }
            };
            yield return inviteTenant;
        }
    }
}
