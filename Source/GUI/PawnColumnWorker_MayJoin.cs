using RimWorld;
using UnityEngine;
using Verse;

namespace Tenants {
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
            return pawn.GetTenantComponent().MayJoin;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            pawn.GetTenantComponent().MayJoin = value;
        }
    }
}
