using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace Tenants {
    public class IncidentWorker_TenantProposition : IncidentWorker {
        private const float RelationWithColonistWeight = 20f;
        protected override bool CanFireNowSub(IncidentParms parms) {
            if (!base.CanFireNowSub(parms)) {
                return false;
            }
            Map map = (Map)parms.target;
            return Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot);
        }


        protected override bool TryExecuteWorker(IncidentParms parms) {
            //Map and spot finder.
            return Utility.GenerateNewContract((Map)parms.target);
        }
    }
}
