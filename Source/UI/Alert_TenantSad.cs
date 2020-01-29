using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Tenants.Controllers;
using Verse;

namespace Tenants.UI {
    public class Alert_TenantSad : Alert {

        private IEnumerable<Pawn> SadTenants {
            get {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++) {
                    if (maps[i].IsPlayerHome) {
                        foreach (Pawn item in maps[i].mapPawns.FreeColonistsSpawned) {
                            if (ThingCompUtility.TryGetComp<WandererComp>(item) != null && item.needs.mood.CurInstantLevel < item.mindState.mentalBreaker.BreakThresholdMinor) {
                                yield return item;
                            }
                        }
                    }
                }
                yield break;
            }
        }

        public override string GetLabel() {
            return "TenantSad".Translate(SadTenants.Count().ToStringCached());
        }
        public override string GetExplanation() {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn sadTenant in SadTenants) {
                stringBuilder.AppendLine("    " + sadTenant.LabelShort.CapitalizeFirst());
            }
            return "TenantSadDesc".Translate(stringBuilder.ToString());

        }
        public override AlertReport GetReport() {
            if (GenDate.DaysPassed < 1) {
                return false;
            }
            return AlertReport.CulpritsAre(SadTenants);
        }
    }
}
