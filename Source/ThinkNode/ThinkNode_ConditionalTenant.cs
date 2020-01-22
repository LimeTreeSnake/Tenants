using Verse;
using Verse.AI;

namespace Tenants.ThinkNodes {
	public class ThinkNode_ConditionalTenant : ThinkNode_Conditional {
		protected override bool Satisfied(Pawn pawn) {
			return pawn.IsColonist && Utilities.Utility.GetTenantComponent(pawn).IsTenant;
		}
	}
}