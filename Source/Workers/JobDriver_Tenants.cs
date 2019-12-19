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
            Toil invite = new Toil();
            invite.initAction = delegate
            {
                Pawn actor = invite.actor;
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (building_CommsConsole.CanUseCommsNow) {
                    Events.TenantInvite(building_CommsConsole, actor);
                }
            };
            yield return invite;
        }
    }
    public class JobDriver_UseCommsConsoleMole : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            Pawn pawn = base.pawn;
            LocalTargetInfo targetA = base.job.targetA;
            Job job = base.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to) {
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                return !building_CommsConsole.CanUseCommsNow;
            });
            Toil mole = new Toil();
            mole.initAction = delegate {
                Pawn actor = mole.actor;
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (building_CommsConsole.CanUseCommsNow) {
                    Events.TenantMole(actor);
                }
            };
            yield return mole;
        }
    }
    public class JobDriver_UseCommsConsoleInviteCourier : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            Pawn pawn = base.pawn;
            LocalTargetInfo targetA = base.job.targetA;
            Job job = base.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to) {
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                return !building_CommsConsole.CanUseCommsNow;
            });
            Toil invite = new Toil();
            invite.initAction = delegate {
                Pawn actor = invite.actor;
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (building_CommsConsole.CanUseCommsNow) {
                    Events.CourierInvite(building_CommsConsole, actor);
                }
            };
            yield return invite;
        }
    }
    public class JobDriver_CheckMailBox : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
            Toil checkMailBox = new Toil();
            checkMailBox.initAction = delegate {
                Thing building_MailBox = checkMailBox.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                building_MailBox.GetMailBoxComponent().EmptyMailBox();
            };
            yield return checkMailBox;
        }
    }

    public class JobDriver_SendMail : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
            Toil checkMailBox = new Toil();
            checkMailBox.initAction = delegate {
                Thing building_MailBox = checkMailBox.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                building_MailBox.GetMailBoxComponent().Letters.Add(TargetThingB);
            };
            yield return checkMailBox;
        }
    }
}
