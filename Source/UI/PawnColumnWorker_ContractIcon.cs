using RimWorld;
using UnityEngine;
using Verse;

namespace Tenants {
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_ContractIcon : PawnColumnWorker_Icon {

        private static readonly Texture2D ContractIcon = Textures.ContractIcon;

        protected override Texture2D GetIconFor(Pawn pawn) {
            return ContractIcon;
        }

        protected override string GetIconTip(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            if (tenantComp != null && !tenantComp.IsTenant) {
                return string.Empty;
            }
            string value = "FullDate".Translate(Find.ActiveLanguageWorker.OrdinalNumber(GenDate.DayOfSeason(tenantComp.ContractEndDate, 0f)), QuadrumUtility.Label(GenDate.Quadrum(tenantComp.ContractEndDate, 0f)), GenDate.Year(tenantComp.ContractEndDate, 0f));
            string a = "ContractEndDate".Translate(value);
            string b = "ContractPayment".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000);
            string c = "ContractLength".Translate(tenantComp.ContractLength / 60000);
            string d = "ContractDaily".Translate(tenantComp.Payment);
            return a + " | " + b + " | " + c + " | " + d;

        }
    }
}
