using RimWorld;
using UnityEngine;
using Verse;

namespace Tenants {
    public class PawnColumnWorker_AutoRenew : PawnColumnWorker_Checkbox {

        public PawnColumnWorker_AutoRenew() {
            foreach(PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if(def.defName == "AutoRenew") {
                    def.label = "AutoRenew".Translate();
                }
            }
        }
        protected override bool GetValue(Pawn pawn) {
            return pawn.GetTenantComponent().AutoRenew;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            pawn.GetTenantComponent().AutoRenew = value;
            
        }
    }
}
