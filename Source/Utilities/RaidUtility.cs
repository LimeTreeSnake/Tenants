using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.Utilities {
    public static class RaidUtility {
        public static string NewBasicRaidMessage(IncidentParms parms, List<Pawn> pawns) {
            Log.Message("Couldn't spawn correct letter for retribution.");
            string basic = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
            basic += "\n\n";
            basic += parms.raidStrategy.arrivalTextEnemy;
            Pawn leader = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (leader != null) {
                basic += "\n\n";
                basic += "EnemyRaidLeaderPresent".Translate(leader.Faction.def.pawnsPlural, leader.LabelShort, leader.Named("LEADER"));
            }
            return basic;
        }
    }
}
