using RimWorld;
using Verse;

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
            return Utility.ContractGenerateNew((Map)parms.target);
        }
    }
}
