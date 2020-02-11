using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Tenants.Comps;
using Tenants.Controllers;
using Verse;

namespace Tenants.IncidentWorkers {    
    public class IncidentWorker_Opportunists : IncidentWorker_RaidEnemy {
        protected override string GetLetterLabel(IncidentParms parms) {
            return "Opportunists".Translate();
        }
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns) {
            try {
                TenantsMapComp.GetComponent((Map)parms.target).Broadcast = false;
                string basic = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
                basic += "\n\n";
                basic += "TenantOpportunists".Translate();
                Pawn leader = pawns.Find((Pawn x) => x.Faction.leader == x);
                if (leader != null) {
                    basic += "\n\n";
                    basic += "EnemyRaidLeaderPresent".Translate(leader.Faction.def.pawnsPlural, leader.LabelShort, leader.Named("LEADER"));
                }
                return basic;
            }
            catch (System.Exception) {
                return base.GetLetterText(parms, pawns);
            }
        }
    }
    public class IncidentWorker_Wanted : IncidentWorker_RaidEnemy {

        protected override bool CanFireNowSub(IncidentParms parms) {
            bool canFire = base.CanFireNowSub(parms);

            if (TenantsMapComp.GetComponent((Map)parms.target).WantedTenants.Count < 1)
                canFire = false;

            return canFire;
        }
        protected override string GetLetterLabel(IncidentParms parms) {
            return "Wanted".Translate();
        }
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns) {
            try {
                if (TenantsMapComp.GetComponent((Map)parms.target).WantedTenants.Count > 0)
                    TenantsMapComp.GetComponent((Map)parms.target).WantedTenants.RemoveAt(0);
                string basic = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
                basic += "\n\n";
                basic += "WantedTenant".Translate();
                Pawn leader = pawns.Find((Pawn x) => x.Faction.leader == x);
                if (leader != null) {
                    basic += "\n\n";
                    basic += "EnemyRaidLeaderPresent".Translate(leader.Faction.def.pawnsPlural, leader.LabelShort, leader.Named("LEADER"));
                }
                return basic;
            }
            catch (System.Exception) {
                return base.GetLetterText(parms, pawns);
            }
        }
        protected override bool TryResolveRaidFaction(IncidentParms parms) {
            try {
                parms.faction = ThingCompUtility.TryGetComp<TenantComp>(TenantsMapComp.GetComponent((Map)parms.target).WantedTenants[0]).WantedBy;
                if (FactionCanBeGroupSource(parms.faction, (Map)parms.target)) {
                    return true;
                }
                else
                    return false;
            }
            catch (System.Exception) {
                return base.TryResolveRaidFaction(parms);
            }
        }
    }
}
