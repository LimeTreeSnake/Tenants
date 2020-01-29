using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Tenants.Comps;
using Verse;

namespace Tenants.IncidentWorkers {
    public class IncidentWorker_WantedProposition : IncidentWorker {
        protected override bool CanFireNowSub(IncidentParms parms) {
            if (!base.CanFireNowSub(parms)) {
                return false;
            }
            if (parms.target != null) {
                Map map = (Map)parms.target;
                List<Map> maps = Find.Maps.Where(x => x.IsPlayerHome).ToList();
                if (map != null && maps.Contains(map)) {
                    Pawn pawn = map.mapPawns.FreeColonists.FirstOrDefault(x => !x.Dead);
                    if (pawn != null)
                        return Utilities.MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot);
                }
            }
            return false;
        }
        protected override bool TryExecuteWorker(IncidentParms parms) {
            if (parms.target != null) {
                Map map = (Map)parms.target;
                if (map != null) {
                    Pawn pawn = map.mapPawns.FreeColonists.FirstOrDefault(x => !x.Dead);
                    if (pawn != null) {
                        Controllers.WantedController.Contract((Map)parms.target);
                    }
                }
            }
            return false;
        }
    }
}
