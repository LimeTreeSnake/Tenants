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
            try {
                Pawn related = pawns[pawns.Count - 1];
                if (MapComponent_Tenants.GetComponent(related.Map).DeadTenantsToAvenge.Count > 0) {
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
            catch (System.Exception) {
                return Utility.NewBasicRaidMessage(parms, pawns);
            }

        }
    }
    public class IncidentWorker_RetributionForCaptured : IncidentWorker_RaidEnemy {

        protected override string GetLetterLabel(IncidentParms parms) {
            return "Retribution".Translate();
        }
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns) {
            try {
                Pawn related = pawns[pawns.Count - 1];
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
            catch (System.Exception) {
                return Utility.NewBasicRaidMessage(parms, pawns);
            }
        }
    }

    public class IncidentWorker_MoleRaid : IncidentWorker_RaidEnemy {

        protected override string GetLetterLabel(IncidentParms parms) {
            return "Mole".Translate();
        }
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns) {
            try {
                Pawn related = pawns[pawns.Count - 1];
                Pawn mole = MapComponent_Tenants.GetComponent((Map)parms.target).Moles[0];

                if(Rand.Value < 0.66f) {
                    mole.SetFaction(mole.GetTenantComponent().HiddenFaction);
                    mole.GetTenantComponent().ResetTenancy();
                }

                string str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
                str += "\n\n";
                str += "TenantMoles".Translate();
                Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
                if (pawn != null) {
                    str += "\n\n";
                    str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
                }
                MapComponent_Tenants.GetComponent((Map)parms.target).CapturedTenantsToAvenge.Remove(mole);
                return str;
            }
            catch (System.Exception) {
                return Utility.NewBasicRaidMessage(parms, pawns);
            }
        }

        protected override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind) {
            base.ResolveRaidStrategy(parms, groupKind);
            Pawn mole = MapComponent_Tenants.GetComponent((Map)parms.target).Moles[0];
            if (mole.GetTenantComponent().HiddenFaction.def.techLevel >= TechLevel.Spacer && Rand.Value < 0.5f) {
                parms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
            }

        }
        protected override bool TryResolveRaidFaction(IncidentParms parms) {
            parms.faction = MapComponent_Tenants.GetComponent((Map)parms.target).Moles[0].GetTenantComponent().HiddenFaction;

            if (FactionCanBeGroupSource(parms.faction, (Map)parms.target)) {
                return true;
            }
            else
                return false;
        }
    }
    public class IncidentWorker_Opportunists : IncidentWorker_RaidEnemy {

        protected override string GetLetterLabel(IncidentParms parms) {
            return "Opportunists".Translate();
        }
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns) {
            MapComponent_Tenants.GetComponent((Map)parms.target).Broadcast = false;
            string basic = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
            basic += "\n\n";
            basic += "TenantOpportunists".Translate();
            Pawn leader = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (leader != null) {
                basic += "\n\n";
                basic += "EnemyRaidLeaderPresent".Translate(leader.Faction.def.pawnsPlural, leader.LabelShort, leader.Named("LEADER"));
            }
            return basic;
        }
    }
}
