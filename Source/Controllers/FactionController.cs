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
    public static class FactionController {
        public static int ChangeRelations(Faction faction, bool reverse = false) {
            int val = Rand.Range(Settings.SettingsHelper.LatestVersion.MinRelation, Settings.SettingsHelper.LatestVersion.MaxRelation + 1);
            _ = reverse == false ? faction.RelationWith(Find.FactionManager.OfPlayer).goodwill += val : faction.RelationWith(Find.FactionManager.OfPlayer).goodwill -= val;
            _ = reverse == false ? Find.FactionManager.OfPlayer.RelationWith(faction).goodwill += val : Find.FactionManager.OfPlayer.RelationWith(faction).goodwill -= val;
            return val;
        }
    }
}
