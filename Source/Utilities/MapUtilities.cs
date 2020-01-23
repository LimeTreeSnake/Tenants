using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Tenants.Comps;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.Controllers {
    public static class MapUtilities {     
        public static bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot) {
            bool validator(IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map);
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }
    }
}
