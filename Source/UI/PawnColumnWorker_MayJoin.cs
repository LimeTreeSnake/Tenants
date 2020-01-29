using RimWorld;
using Tenants.Comps;
using UnityEngine;
using Verse;

namespace Tenants.UI {
    public class PawnColumnWorker_MayJoin : PawnColumnWorker_Checkbox {

        public PawnColumnWorker_MayJoin() {
            foreach(PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if(def.defName == "MayJoin") {
                    def.label = "MayJoin".Translate();
                }
            }
        }
        protected override string GetTip(Pawn pawn) {
            return "MayJoinTip".Translate();
        }
        protected override bool GetValue(Pawn pawn) {
            return ThingCompUtility.TryGetComp<WandererComp>(pawn).MayJoin;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            ThingCompUtility.TryGetComp<WandererComp>(pawn).MayJoin = value;
        }
    }
}
