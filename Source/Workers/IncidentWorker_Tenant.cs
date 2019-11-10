using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Tenants {
    public class IncidentWorker_TenantProposition : IncidentWorker {
        protected override bool CanFireNowSub(IncidentParms parms) {
            if (!base.CanFireNowSub(parms)) {
                return false;
            }
            if (parms.target != null) {
                Map map = (Map)parms.target;
                List<Map> maps = Find.Maps.Where(x => x.IsPlayerHome).ToList();
                if (map != null && maps.Contains(map)) {
                    Pawn pawn = map.mapPawns.FreeColonists.FirstOrDefault(x => x.GetTenantComponent().IsTenant == false && !x.Dead);
                    if (pawn != null)
                        return Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot);
                }
            }
            return false;
        }
        protected override bool TryExecuteWorker(IncidentParms parms) {
            if (parms.target != null) {
                Map map = (Map)parms.target;
                if (map != null) {
                    Pawn pawn = map.mapPawns.FreeColonists.FirstOrDefault(x => x.GetTenantComponent().IsTenant == false && !x.Dead);
                    Building building = map.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x.def == ThingDefOf.Tenants_MailBox);
                    if (pawn != null && building != null)
                        return Events.ContractTenancy((Map)parms.target);
                }
            }
            return false;
        }
    }
    public class IncidentWorker_TenantCourier : IncidentWorker {
        protected override bool CanFireNowSub(IncidentParms parms) {
            if (!base.CanFireNowSub(parms)) {
                return false;
            }
            if (parms.target != null) {
                Map map = (Map)parms.target;
                List<Map> maps = Find.Maps.Where(x => x.IsPlayerHome).ToList();
                if (map != null && maps.Contains(map)) {
                    return Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot);
                }
            }
            return false;
        }
        protected override bool TryExecuteWorker(IncidentParms parms) {
            if (parms.target != null) {
                Map map = (Map)parms.target;
                if (map != null) {
                    Building building = map.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x.def == ThingDefOf.Tenants_MailBox);
                    if (building != null ) {
                        return Events.Courier((Map)parms.target, building);
                    }
                }
            }
            return false;
        }
    }

}
