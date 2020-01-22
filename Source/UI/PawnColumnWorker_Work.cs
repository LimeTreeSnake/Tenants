using RimWorld;
using UnityEngine;
using Verse;

namespace Tenants.UI {
    public class PawnColumnWorker_FireFight : PawnColumnWorker_Checkbox {
        public PawnColumnWorker_FireFight() {
            foreach (PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if (def.defName == "TenantWorkFireFighting") {
                    def.label = "FireFighting".Translate();
                }
            }
        }
        protected override string GetTip(Pawn pawn) {
            return "FireFightingTip".Translate();
        }
        protected override bool GetValue(Pawn pawn) {
            return pawn.GetTenantComponent().MayFirefight;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp.IsEnvoy) {
                tenantComp.MayFirefight = false;
            }
            else if(value && !pawn.story.WorkTagIsDisabled(WorkTags.Firefighting)) {
                pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Firefighter"), 3);
                tenantComp.MayFirefight = true;
            }
            else {
                if(value == true) {
                    Messages.Message("FireFightingError".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }
                pawn.workSettings.Disable(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Firefighter"));
                tenantComp.MayFirefight = false;
            }
        }
    }
    public class PawnColumnWorker_Basic : PawnColumnWorker_Checkbox {
        public PawnColumnWorker_Basic() {
            foreach (PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if (def.defName == "TenantWorkBasic") {
                    def.label = "Basic".Translate();
                }
            }
        }
        protected override string GetTip(Pawn pawn) {
            return "BasicTip".Translate();
        }
        protected override bool GetValue(Pawn pawn) {
            return pawn.GetTenantComponent().MayBasic;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp.IsEnvoy) {
                tenantComp.MayBasic = false;
            }
            else if (value && !pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb)) {
                pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "BasicWorker"), 3);
                tenantComp.MayBasic = true;
            }
            else {
                if (value == true) {
                    Messages.Message("BasicError".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }
                pawn.workSettings.Disable(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "BasicWorker"));
                tenantComp.MayBasic = false;
            }
        }
    }
    public class PawnColumnWorker_Hauling : PawnColumnWorker_Checkbox {
        public PawnColumnWorker_Hauling() {
            foreach (PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if (def.defName == "TenantWorkHauling") {
                    def.label = "Hauling".Translate();
                }
            }
        }
        protected override string GetTip(Pawn pawn) {
            return "HaulingTip".Translate();
        }
        protected override bool GetValue(Pawn pawn) {
            return pawn.GetTenantComponent().MayHaul;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp.IsEnvoy) {
                tenantComp.MayHaul = false;
            }
            else if(value && !(pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) || pawn.story.WorkTagIsDisabled(WorkTags.Hauling))) {
                pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Hauling"), 3);
                tenantComp.MayHaul = true;
            }
            else {
                if (value == true) {
                    Messages.Message("HaulingError".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }
                pawn.workSettings.Disable(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Hauling"));
                tenantComp.MayHaul = false;
            }
        }
    }
    public class PawnColumnWorker_Cleaning : PawnColumnWorker_Checkbox {
        public PawnColumnWorker_Cleaning() {
            foreach (PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if (def.defName == "TenantWorkCleaning") {
                    def.label = "Cleaning".Translate();
                }
            }
        }
        protected override string GetTip(Pawn pawn) {
            return "CleaningTip".Translate();
        }
        protected override bool GetValue(Pawn pawn) {
            return pawn.GetTenantComponent().MayClean;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp.IsEnvoy) {
                tenantComp.MayClean = false;
            }
            else if(value && !(pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) || pawn.story.WorkTagIsDisabled(WorkTags.Cleaning))) {
                pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Cleaning"), 3);
                tenantComp.MayClean = true;
            }
            else {
                if (value == true) {
                    Messages.Message("CleaningError".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }
                pawn.workSettings.Disable(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Cleaning"));
                tenantComp.MayClean = false;
            }
        }
    }
}
