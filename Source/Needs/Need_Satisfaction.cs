using RimWorld;
using Tenants.Comps;
using Verse;

namespace Tenants.Needs {
    public class Need_Satisfaction : Need {
		public override bool ShowOnNeedList => ThingCompUtility.TryGetComp<TenantComp>(pawn).Tenancy != TenancyType.None;
		public override void NeedInterval() {
        }
		public Need_Satisfaction(Pawn pawn)
		: base(pawn) {

		}
		public override string GetTipString() {
			return base.LabelCap + ": " + base.CurLevelPercentage.ToStringPercent() + " (" + CurLevel.ToString("0.##") + " / " + MaxLevel.ToString("0.##") + ")\n" + def.description;
		}
	}
}
