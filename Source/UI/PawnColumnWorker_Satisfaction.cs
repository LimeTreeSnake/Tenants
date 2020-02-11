using RimWorld;
using UnityEngine;
using Verse;

namespace Tenants.UI {
	public class PawnColumnWorker_Satisfaction : PawnColumnWorker {
		public override void DoCell(Rect rect, Pawn pawn, PawnTable table) {
			if (pawn.needs.TryGetNeed(Defs.NeedDefOf.Satisfaction) != null) {
				float num = rect.x;
				float num2 = rect.width / 24f;
				Rect rect2 = new Rect(num, rect.y, num2, rect.height); 
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect2, (pawn.needs.TryGetNeed(Defs.NeedDefOf.Satisfaction).CurLevel * 100).ToString());
				Text.Anchor = TextAnchor.UpperLeft;

			}
		}
		public override int GetMinWidth(PawnTable table) {
			return Mathf.Max(base.GetMinWidth(table), 50);
		}
		public override int GetOptimalWidth(PawnTable table) {
			return Mathf.Clamp(100, GetMinWidth(table), GetMaxWidth(table));
		}
	}
}
