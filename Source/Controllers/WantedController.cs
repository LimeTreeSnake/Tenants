using Harmony;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.Controllers {
    public static class WantedController {
        public static void Tick(Pawn pawn, WantedComp comp) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (contract == null) {
                pawn.AllComps.Remove(comp);
                return;
            }
            //Tenancy tick per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
                if (ThingCompUtility.TryGetComp<WantedComp>(pawn) != null) {
                    if (!TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                        TenantWanted(pawn);
                    }
                }
            }
        }
        public static void TenantWanted(Pawn pawn) {
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            WantedComp wantedComp = ThingCompUtility.TryGetComp<WantedComp>(pawn);
            if (Rand.Value < 0.66 && wantedComp.WantedBy.HostileTo(Find.FactionManager.OfPlayer) && !TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                int val = FactionController.ChangeRelations(tenantComp.HiddenFaction, true);
                Messages.Message("HarboringWantedTenant".Translate(wantedComp.WantedBy, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Add(pawn);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = Defs.RaidStrategyDefOf.WantedRaid;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.WantedRaid, Find.TickManager.TicksGame + Rand.Range(100000, 300000), parms, 60000);
            }
            else if (Rand.Value < 0.5) {
                int val = FactionController.ChangeRelations(tenantComp.HiddenFaction, true);
                Messages.Message("HarboringWantedTenant".Translate(wantedComp.WantedBy, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}
