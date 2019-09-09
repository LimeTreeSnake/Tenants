using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Tenants {
    public class IncidentWorker_RetributionForDead : IncidentWorker_RaidEnemy {

        protected override string GetLetterLabel(IncidentParms parms) {
            return "Retribution".Translate();
        }
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns) {
            Pawn related = pawns[pawns.Count - 1];
            if (MapComponent_Tenants.GetComponent(related.Map).CapturedTenantsToAvenge.Count > 0) {
                Pawn dead = MapComponent_Tenants.GetComponent(related.Map).DeadTenantsToAvenge[0];
                if (dead.ageTracker.AgeBiologicalYears > 25) {
                    related.relations.AddDirectRelation(PawnRelationDefOf.Parent, dead);
                }
                else {
                    dead.relations.AddDirectRelation(PawnRelationDefOf.Parent, related);
                }
                string str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
                str += "\n\n";
                str += "TenantDeathRetribution".Translate(related.GetRelations(dead).FirstOrDefault().GetGenderSpecificLabel(dead), related.Named("PAWN"));
                Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
                if (pawn != null) {
                    str += "\n\n";
                    str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
                }
                MapComponent_Tenants.GetComponent(pawns[0].Map).DeadTenantsToAvenge.Remove(dead);
                return str;
            }
            else {
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
    public class IncidentWorker_RetributionForCaptured : IncidentWorker_RaidEnemy {

        protected override string GetLetterLabel(IncidentParms parms) {
            return "Retribution".Translate();
        }
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns) {
            Pawn related = pawns[pawns.Count - 1];
            if (MapComponent_Tenants.GetComponent(related.Map).CapturedTenantsToAvenge.Count > 0) {
                Pawn captured = MapComponent_Tenants.GetComponent(related.Map).CapturedTenantsToAvenge[0];

                if (captured.ageTracker.AgeBiologicalYears > 25) {
                    related.relations.AddDirectRelation(PawnRelationDefOf.Parent, captured);
                }
                else {
                    captured.relations.AddDirectRelation(PawnRelationDefOf.Parent, related);
                }
                string str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
                str += "\n\n";
                str += "TenantCapturedRetribution".Translate(related.GetRelations(captured).FirstOrDefault().GetGenderSpecificLabel(captured), related.Named("PAWN"));
                Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
                if (pawn != null) {
                    str += "\n\n";
                    str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
                }
                MapComponent_Tenants.GetComponent(pawns[0].Map).CapturedTenantsToAvenge.Remove(captured);
                return str;
            }
            else {
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


    [DefOf]
    public static class IncidentDefOf {
        public static IncidentDef RetributionForCaptured;
        public static IncidentDef RetributionForDead;
        static IncidentDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }
    }
    [DefOf]
    public static class RaidStrategyDefOf {
        public static RaidStrategyDef Retribution;

        static RaidStrategyDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
        }
    }
}
