using RimWorld;
using Tenants.Comps;
using UnityEngine;
using Verse;

namespace Tenants.UI {
    public class PawnColumnWorker_AutoRenew : PawnColumnWorker_Checkbox {

        public PawnColumnWorker_AutoRenew() {
            foreach(PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if(def.defName == "AutoRenew") {
                    def.label = "AutoRenew".Translate();
                }
            }
        }
        protected override string GetTip(Pawn pawn) {
            return "AutoRenewTip".Translate();
        }
        protected override bool GetValue(Pawn pawn) {
            return ThingCompUtility.TryGetComp<ContractComp>(pawn).AutoRenew;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            ThingCompUtility.TryGetComp<ContractComp>(pawn).AutoRenew = value;            
        }
    }
}
