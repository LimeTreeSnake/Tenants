using RimWorld;
using Tenants.Comps;
using UnityEngine;
using Verse;

namespace Tenants.UI {
    public class PawnColumnWorker_TenantLabel : PawnColumnWorker_Text {
        
        protected override string GetTextFor(Pawn pawn) {
            WandererComp tenantComp = ThingCompUtility.TryGetComp<WandererComp>(pawn);
            if (tenantComp != null) {
                return string.Empty;
            }
            return pawn.Name.ToStringFull;
        }
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table) {
            base.DoCell(rect, pawn, table);
            Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
            if (Widgets.ButtonInvisible(rect2)) {
                CameraJumper.TryJumpAndSelect(pawn);
                if (Current.ProgramState == ProgramState.Playing && Event.current.button == 0) {
                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
                }
            }
            else {
                TipSignal tooltip = pawn.GetTooltip();
                tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
                TooltipHandler.TipRegion(rect2, tooltip);
            }
        }
    }
}
