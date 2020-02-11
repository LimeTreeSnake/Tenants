using RimWorld;
using Tenants.Comps;
using UnityEngine;
using Verse;

namespace Tenants.UI {
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_ContractIcon : PawnColumnWorker_Icon {

        protected override Texture2D GetIconFor(Pawn pawn) {
            TenancyType tenancyType = ThingCompUtility.TryGetComp<TenantComp>(pawn).Tenancy;
            if (tenancyType == TenancyType.Envoy)
                return Utilities.TextureUtility.EnvoyIcon;
            else if (tenancyType == TenancyType.Wanted)
                return Utilities.TextureUtility.WantedIcon;
            else
                return Utilities.TextureUtility.ContractIcon;

        }

        protected override string GetIconTip(Pawn pawn) {
            Models.Contract comp = ThingCompUtility.TryGetComp<TenantComp>(pawn).Contract;
            if (comp == null) {
                return string.Empty;
            }
            string value = "FullDate".Translate(Find.ActiveLanguageWorker.OrdinalNumber(GenDate.DayOfSeason(comp.ContractEndDate, 0f)), QuadrumUtility.Label(GenDate.Quadrum(comp.ContractEndDate, 0f)), GenDate.Year(comp.ContractEndDate, 0f));
            string a = "ContractEndDate".Translate(value);
            string b = comp.Payment != 0 ? "ContractPayment".Translate(comp.Payment * comp.ContractLength / 60000) : "0";
            string c = "ContractLength".Translate(comp.ContractLength / 60000);
            string d = "ContractDaily".Translate(comp.Payment);
            return a + " \n " + b + " \n " + c + " \n " + d;

        }
    }
}
