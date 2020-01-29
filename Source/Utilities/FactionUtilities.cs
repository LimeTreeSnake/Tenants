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

namespace Tenants.Utilities {
    public static class FactionUtilities {
        public static int ChangeRelations(Faction faction, bool reverse = false) {
            int val = Rand.Range(Settings.Settings.MinRelation, Settings.Settings.MaxRelation + 1);
            if (reverse)
                val *= -1;
            faction.RelationWith(Find.FactionManager.OfPlayer).goodwill += val;
            Find.FactionManager.OfPlayer.RelationWith(faction).goodwill += val;
            return val;
        }
    }
}
