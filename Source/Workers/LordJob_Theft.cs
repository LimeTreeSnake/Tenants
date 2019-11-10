using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Linq;

namespace Tenants {
    public class LordJob_TenantTheft : LordJob {
        public override bool GuiltyOnDowned => true;
        public override StateGraph CreateGraph() {
            StateGraph stateGraph = new StateGraph();
            LordToil lordToil_Steal = stateGraph.AttachSubgraph(new LordJob_TenantSteal().CreateGraph()).StartingToil;
            return stateGraph;
        }
    }

    public class LordJob_TenantSteal : LordJob {
        public override bool GuiltyOnDowned => true;
        public override StateGraph CreateGraph() {
            StateGraph stateGraph = new StateGraph();
            LordToil_TenantStealCover lordToil_TenantStealCover = new LordToil_TenantStealCover {
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_TenantStealCover);
            LordToil_TenantStealCover lordToil_TenantStealCover2 = new LordToil_TenantStealCover {
                cover = false,
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_TenantStealCover2);
            Transition transition = new Transition(lordToil_TenantStealCover, lordToil_TenantStealCover2);
            transition.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(1200));
            stateGraph.AddTransition(transition);
            return stateGraph;
        }
    }

    public class LordToil_TenantStealCover : LordToil_DoOpportunisticTaskOrCover {
        protected override DutyDef DutyDef => DutyDefOf.Steal;
        public override bool ForceHighStoryDanger => false;
        public override bool AllowSelfTend => false;
        protected override bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets) {
            if (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDef && pawn.carryTracker.CarriedThing != null) {
                target = pawn.carryTracker.CarriedThing;
                return true;
            }
            return StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 33f, out target, pawn, alreadyTakenTargets);
        }
    }

}
