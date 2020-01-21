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
                    if (pawn != null) {
                        UtilityTenant.Contract((Map)parms.target);                       
                    }
                }
            }
            return false;
        }
    }
    public class IncidentWorker_InvitationForTenancy : IncidentWorker {
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
                    else {
                        Messages.Message("EnvoyArriveFailed".Translate(parms.faction), MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
            return false;
        }
        protected override bool TryExecuteWorker(IncidentParms parms) {
            if (parms.target != null) {
                Map map = (Map)parms.target;
                if (map != null) {
                    Pawn pawn = map.mapPawns.FreeColonists.FirstOrDefault(x => x.GetTenantComponent().IsTenant == false && !x.Dead);
                    if (pawn != null) {
                        return UtilityTenant.EnvoyTenancy((Map)parms.target, parms.faction);
                    }
                    else {
                        Messages.Message("EnvoyArriveFailed".Translate(parms.faction), MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
            return false;
        }
    }
}
