using RimWorld;
using Verse;

namespace Tenants.UI {
    public class PawnColumnWorker_CancelContract : PawnColumnWorker_Checkbox {
        public PawnColumnWorker_CancelContract() {
            foreach (PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if (def.defName == "CancelContract") {
                    def.label = "Terminate".Translate();
                }
            }
        }
        protected override string GetTip(Pawn pawn) {
            return "TerminateTip".Translate();
        }
        protected override bool GetValue(Pawn pawn) {
            return pawn.GetTenantComponent().IsTerminated;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            pawn.GetTenantComponent().IsTerminated = value;
        }
    }
}
