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

namespace Tenants {
    public static class Utility {
        public static Tenant GetTenantComponent(this Pawn pawn) {
            if (ThingCompUtility.TryGetComp<Tenant>(pawn) != null) {
                return ThingCompUtility.TryGetComp<Tenant>(pawn);
            }
            else {
                pawn.def.comps.Add(new CompProps_Tenant());
                return ThingCompUtility.TryGetComp<Tenant>(pawn);
            }
        }
        public static Courier GetCourierComponent(this Pawn pawn) {
            if (ThingCompUtility.TryGetComp<Courier>(pawn) != null) {
                return ThingCompUtility.TryGetComp<Courier>(pawn);
            }
            else {
                pawn.def.comps.Add(new CompProps_Courier());
                return ThingCompUtility.TryGetComp<Courier>(pawn);
            }
        }
        public static MessageBox GetMailBoxComponent(this Thing thing) {
            if (ThingCompUtility.TryGetComp<MessageBox>(thing) != null) {
                return ThingCompUtility.TryGetComp<MessageBox>(thing);
            }
            else {
                thing.def.comps.Add(new CompProps_MessageBox());
                return ThingCompUtility.TryGetComp<MessageBox>(thing);
            }
        }

        public static bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot) {
            bool validator(IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map);
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }
        public static void UpdateAllRestrictions(Pawn pawn) {
            UpdateWork(pawn);
            UpdateOutfitManagement(pawn);
            UpdateFoodManagement(pawn);
            UpdateDrugManagement(pawn);
        }
        public static void UpdateWork(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs) {

                if (def.defName == "Patient") {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (def.defName == "PatientBedRest") {
                    pawn.workSettings.SetPriority(def, 3);
                }
                else if (!pawn.story.WorkTagIsDisabled(WorkTags.Firefighting) && def.defName == "Firefighter") {
                    pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Firefighter"), 3);
                    tenantComp.MayFirefight = true;
                }
                else if (!pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) && def.defName == "BasicWorker") {
                    pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "BasicWorker"), 3);
                    tenantComp.MayBasic = true;
                }
                else if (!(pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) || pawn.story.WorkTagIsDisabled(WorkTags.Hauling)) && def.defName == "Hauling") {
                    pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Hauling"), 3);
                    tenantComp.MayHaul = true;
                }
                else if (!(pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) || pawn.story.WorkTagIsDisabled(WorkTags.Cleaning)) && def.defName == "Cleaning") {
                    pawn.workSettings.SetPriority(DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Cleaning"), 3);
                    tenantComp.MayClean = true;
                }
                else
                    pawn.workSettings.Disable(def);
            }
        }
        public static bool UpdateEmergencyWork(WorkGiver giver) {
            if (giver is WorkGiver_PatientGoToBedEmergencyTreatment
                || giver is WorkGiver_PatientGoToBedTreatment
                || giver is WorkGiver_PatientGoToBedRecuperate
                || giver.def.workTags == WorkTags.Firefighting) {
                return true;
            }
            return false;
        }
        public static void UpdateOutfitManagement(Pawn pawn) {
            Outfit restriction = Current.Game.outfitDatabase.AllOutfits.FirstOrDefault(x => x.label == "Tenants".Translate());
            if (restriction == null) {
                int uniqueId = (!Current.Game.outfitDatabase.AllOutfits.Any()) ? 1 : (Current.Game.outfitDatabase.AllOutfits.Max((Outfit o) => o.uniqueId) + 1);
                restriction = new Outfit(uniqueId, "Tenants".Translate());
                restriction.filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
                Current.Game.outfitDatabase.AllOutfits.Add(restriction);
            }
            pawn.outfits.CurrentOutfit = restriction;
        }
        public static void UpdateFoodManagement(Pawn pawn) {
            FoodRestriction restriction = Current.Game.foodRestrictionDatabase.AllFoodRestrictions.FirstOrDefault(x => x.label == "Tenants".Translate());
            if (restriction == null) {
                int uniqueId = (!Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Any()) ? 1 : (Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Max((FoodRestriction o) => o.id) + 1);
                restriction = new FoodRestriction(uniqueId, "Tenants".Translate());
                restriction.filter.SetAllow(ThingCategoryDefOf.FoodMeals, allow: true);
                restriction.filter.SetAllow(ThingCategoryDefOf.Foods, allow: true);
                Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Add(restriction);
            }
            pawn.foodRestriction.CurrentFoodRestriction = restriction;
        }
        public static void UpdateDrugManagement(Pawn pawn) {
            DrugPolicy restriction = Current.Game.drugPolicyDatabase.AllPolicies.FirstOrDefault(x => x.label == "Tenants".Translate());
            if (restriction == null) {
                int uniqueId = (!Current.Game.drugPolicyDatabase.AllPolicies.Any()) ? 1 : (Current.Game.drugPolicyDatabase.AllPolicies.Max((DrugPolicy o) => o.uniqueId) + 1);
                restriction = new DrugPolicy(uniqueId, "Tenants".Translate());
                Current.Game.drugPolicyDatabase.AllPolicies.Add(restriction);
            }
            pawn.drugs.CurrentPolicy = restriction;
        }
        public static void UpdateNightOwl(Pawn pawn) {
            pawn.timetable.times = new List<TimeAssignmentDef>(24);
            for (int i = 0; i < 24; i++) {
                TimeAssignmentDef item = (i >= 10 && i <= 17) ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                pawn.timetable.times.Add(item);
            }
        }
        public static void RemoveExpensiveItems(Pawn pawn) {
            if (pawn.apparel.WornApparel != null && pawn.apparel.WornApparel.Count > 0)
                foreach (Apparel item in pawn.apparel.WornApparel)
                    if (item.MarketValue > 400)
                        pawn.apparel.Remove(item);
            if (pawn.inventory.innerContainer != null && pawn.inventory.innerContainer.Count > 0)
                foreach (Thing item in pawn.inventory.innerContainer)
                    if (item.MarketValue > 400)
                        pawn.inventory.innerContainer.Remove(item);
            if (pawn.equipment.Primary != null)
                if (pawn.equipment.Primary.MarketValue > 400)
                    pawn.equipment.Primary.Destroy();
        }
        public static int ChangeEnvoyRelations(Faction faction, bool reverse = false) {
            int val = Rand.Range(SettingsHelper.LatestVersion.MinEnvoyRelation, SettingsHelper.LatestVersion.MaxEnvoyRelation + 1);
            _ = reverse == false ? faction.RelationWith(Find.FactionManager.OfPlayer).goodwill += val : faction.RelationWith(Find.FactionManager.OfPlayer).goodwill -= val;
            _ = reverse == false ? Find.FactionManager.OfPlayer.RelationWith(faction).goodwill += val : Find.FactionManager.OfPlayer.RelationWith(faction).goodwill -= val;
            return val;
        }
        public static int ChangeTenantRelations(Faction faction, bool reverse = false) {
            int val = Rand.Range(SettingsHelper.LatestVersion.MinTenantRelation, SettingsHelper.LatestVersion.MaxTenantRelation + 1);
            _ = reverse == false ? faction.RelationWith(Find.FactionManager.OfPlayer).goodwill += val : faction.RelationWith(Find.FactionManager.OfPlayer).goodwill -= val;
            _ = reverse == false ? Find.FactionManager.OfPlayer.RelationWith(faction).goodwill += val : Find.FactionManager.OfPlayer.RelationWith(faction).goodwill -= val;
            return val;
        }
        public static void GenerateBasicContract(Tenant tenantComp, int payment, int timeMultiplier = 1) {
            tenantComp.Payment = payment;
            tenantComp.ContractLength = (Rand.Range(SettingsHelper.LatestVersion.MinContractTime, SettingsHelper.LatestVersion.MaxContractTime) * timeMultiplier) * 60000;
            tenantComp.ContractDate = Find.TickManager.TicksGame;
            tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
            tenantComp.ResetMood();
        }
    }
}
