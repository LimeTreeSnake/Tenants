using Tenants.Comps;
using Verse;
using Verse.AI;

namespace Tenants.ThinkNodes {
	public class ThinkNode_ConditionalTenant : ThinkNode_Conditional {
		protected override bool Satisfied(Pawn pawn) {
			return ThingCompUtility.TryGetComp<WandererComp>(pawn) != null && ThingCompUtility.TryGetComp<ContractComp>(pawn) != null;
		}
	}
}