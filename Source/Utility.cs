using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public static MailBox GetMailBoxComponent(this Thing thing) {
            if (ThingCompUtility.TryGetComp<MailBox>(thing) != null) {
                return ThingCompUtility.TryGetComp<MailBox>(thing);
            }
            else {
                thing.def.comps.Add(new CompProps_MailBox());
                return ThingCompUtility.TryGetComp<MailBox>(thing);
            }            
        }
        public static bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot) {
            bool validator(IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map);
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }
        public static int CalculateMood(Tenant tenant) {
            float count = tenant.HappyMoodCount + tenant.NeutralMoodCount + tenant.SadMoodCount;
            if (tenant.NeutralMoodCount / count >= 0.5f)
                return 0;
            else if (tenant.HappyMoodCount > tenant.SadMoodCount)
                return 1;
            else
                return -1;
        }
        public static List<Pawn> RemoveTenantsFromList(List<Pawn> pawns) {
            List<Pawn> tenants = new List<Pawn>();
            foreach (Pawn pawn in pawns) {
                Tenant tenantComp = pawn.GetTenantComponent();
                if (tenantComp != null && tenantComp.IsTenant)
                    tenants.Add(pawn);
            }
            foreach (Pawn pawn in tenants) {
                pawns.Remove(pawn);
            }
            return pawns;
        }
        public static IEnumerable<GlobalTargetInfo> RemoveTenantsFromList(ref IEnumerable<GlobalTargetInfo> pawns) {
            List<GlobalTargetInfo> tenants = new List<GlobalTargetInfo>();
            foreach (GlobalTargetInfo pawn in pawns) {
                if (pawn.Thing.TryGetComp<Tenant>().IsTenant) {
                    tenants.Add(pawn);
                }
            }
            List<GlobalTargetInfo> list = pawns.ToList();
            foreach (GlobalTargetInfo pawn in tenants) {
                list.Remove(pawn);
            }
            pawns = list.AsEnumerable();
            return pawns;
        }
        public static Pawn FindRandomTenant() {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.GetTenantComponent() != null && p.GetTenantComponent().IsTenant && !p.Dead && !p.Spawned && !p.Discarded
                                select p).ToList();
            if (pawns.Count < 20)
                for (int i = 0; i < 3; i++)
                    pawns.Add(GenerateNewTenant());

            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns[0];
        }
        public static Pawn FindRandomCourier() {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.GetCourierComponent() != null && p.GetCourierComponent().isCourier && !p.Dead && !p.Spawned && !p.Discarded
                                select p).ToList();
            if (pawns.Count < 20)
                for (int i = 0; i < 3; i++)
                    pawns.Add(GenerateNewCourier());

            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns[0];
        }
        public static void GenerateWanted(Pawn pawn) {
            Tenant tenantComp = pawn.TryGetComp<Tenant>();
            if (tenantComp.HiddenFaction.def != FactionDefOf.Ancients) {
                List<FactionRelation> entries = Traverse.Create(tenantComp.HiddenFaction).Field("relations").GetValue<List<FactionRelation>>().Where(p => p.kind == FactionRelationKind.Hostile).ToList();
                if (entries.Count > 0) {
                    int count = 0;
                    while (tenantComp.WantedBy == null && count < 10) {
                        count++;
                        entries.Shuffle();
                        if (entries[0].other.def.pawnGroupMakers != null && !entries[0].other.IsPlayer)
                            tenantComp.WantedBy = entries[0].other;
                    }
                    if (tenantComp.WantedBy != null)
                        tenantComp.Wanted = true;
                }
            }
        }
        public static Pawn GenerateNewTenant() {
            bool generation = true;
            Pawn newTenant = null;
            while (generation) {
                string race = SettingsHelper.LatestVersion.AvailableRaces.RandomElement();
                PawnKindDef random = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.defName == race).RandomElement();
                if (random == null)
                    return null;
                Faction faction = FactionUtility.DefaultFactionFrom(random.defaultFactionType);
                newTenant = PawnGenerator.GeneratePawn(random, faction);
                if (newTenant != null && !newTenant.Dead && !newTenant.IsDessicated() && !newTenant.AnimalOrWildMan()) {
                    {
                        if (SettingsHelper.LatestVersion.SimpleClothing) {
                            FloatRange range = newTenant.kindDef.apparelMoney;
                            newTenant.kindDef.apparelMoney = new FloatRange(SettingsHelper.LatestVersion.SimpleClothingMin, SettingsHelper.LatestVersion.SimpleClothingMax);
                            PawnApparelGenerator.GenerateStartingApparelFor(newTenant, new PawnGenerationRequest(random));
                            newTenant.kindDef.apparelMoney = range;
                        }
                        RemoveExpensiveItems(newTenant);
                        newTenant.GetTenantComponent().IsTenant = true;
                        newTenant.GetTenantComponent().HiddenFaction = faction;
                        newTenant.SetFaction(Faction.OfAncients);
                        if (SettingsHelper.LatestVersion.Weapons) {
                            List<Thing> ammo = newTenant.inventory.innerContainer.Where(x => x.def.defName.Contains("Ammunition")).ToList();
                            foreach (Thing thing in ammo)
                                newTenant.inventory.innerContainer.Remove(thing);
                        }
                        newTenant.DestroyOrPassToWorld();
                        generation = false;
                    }
                }
            }
            return newTenant;
        }
        public static Pawn GenerateNewCourier() {
            bool generation = true;
            Pawn newCourier = null;
            while (generation) {
                newCourier = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
                if (newCourier != null && !newCourier.Dead && !newCourier.IsDessicated() && !newCourier.AnimalOrWildMan()) {
                    {
                        newCourier.GetCourierComponent().isCourier = true;
                        newCourier.DestroyOrPassToWorld();
                        generation = false;
                    }
                }
            }
            return newCourier;
        }
        public static void GenerateBasicTenancyContract(Tenant tenantComp) {
            tenantComp.Payment = Rand.Range(SettingsHelper.LatestVersion.MinDailyCost, SettingsHelper.LatestVersion.MaxDailyCost);
            tenantComp.ContractLength = Rand.Range(SettingsHelper.LatestVersion.MinContractTime, SettingsHelper.LatestVersion.MaxContractTime) * 60000;
            tenantComp.ContractDate = Find.TickManager.TicksGame;
            tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
            tenantComp.ResetMood();
        }
        public static bool GenerateContractDialogue(string title, string text, Pawn pawn, Map map, IntVec3 spawnSpot) {
            DiaNode diaNode = new DiaNode(text);
            //Accepted offer, generating tenant.
            DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                action = delegate {
                    pawn.SetFaction(Faction.OfPlayer);
                    pawn.GetTenantComponent().Contracted = true;
                    GenSpawn.Spawn(pawn, spawnSpot, map);
                    pawn.needs.SetInitialLevels();
                    pawn.playerSettings.AreaRestriction = map.areaManager.Home;
                    UpdateAllRestrictions(pawn);
                    TraitDef nightOwl = DefDatabase<TraitDef>.GetNamedSilentFail("NightOwl");
                    if (nightOwl != null && pawn.story.traits.HasTrait(nightOwl)) {
                        UpdateNightOwl(pawn);
                    }
                    if (SettingsHelper.LatestVersion.Weapons) {
                        pawn.equipment.DestroyAllEquipment();
                    }
                    CameraJumper.TryJump(pawn);
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOption);
            //Denied tenant offer
            string text2 = "RequestForTenancyRejected".Translate(pawn.LabelShort, pawn, pawn.Named("PAWN"));
            DiaNode diaNode2 = new DiaNode(text2);
            DiaOption diaOption2 = new DiaOption("OK".Translate()) {
                resolveTree = true
            };
            diaNode2.options.Add(diaOption2);
            DiaOption diaOption3 = new DiaOption("ContractReject".Translate()) {
                action = delegate {
                    pawn.GetTenantComponent().CleanTenancy();
                },
                link = diaNode2
            };
            diaNode.options.Add(diaOption3);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            return true;
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
        public static void MakePayment(Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            int payment = (tenantComp.ContractLength / 60000) * tenantComp.Payment;
            while (payment > 500) {
                Thing silver = ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver);
                silver.stackCount = 500;
                MapComponent_Tenants.GetComponent(pawn.Map).IncomingMail.Add(silver);
                payment = payment - 500;
            }
            Thing silverRest = ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver);
            silverRest.stackCount = payment;
            MapComponent_Tenants.GetComponent(pawn.Map).IncomingMail.Add(silverRest);
        }
        public static void RemoveExpensiveItems(Pawn pawn) {
            if (pawn.apparel.WornApparel != null && pawn.apparel.WornApparel.Count > 0)
                foreach (Apparel item in pawn.apparel.WornApparel)
                    if (item.MarketValue > 400) pawn.apparel.Remove(item);
            if (pawn.inventory.innerContainer != null && pawn.inventory.innerContainer.Count > 0)
                foreach (Thing item in pawn.inventory.innerContainer)
                    if (item.MarketValue > 400) pawn.inventory.innerContainer.Remove(item);
            if (pawn.equipment.Primary != null)
                if (pawn.equipment.Primary.MarketValue > 400) pawn.equipment.Primary.Destroy();
        }
        public static string AppendPawnDescription(string text, Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder(text);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("TenantDescription".Translate(pawn.ageTracker.AgeBiologicalYears, pawn.def.label, pawn.Named("PAWN")));
            stringBuilder.AppendLine();
            stringBuilder.Append("Traits".Translate() + ": ");
            if (pawn.story.traits.allTraits.Count == 0) {
                stringBuilder.AppendLine();
                stringBuilder.Append("(" + "NoneLower".Translate() + ")");
            }
            else {
                stringBuilder.Append("(");
                for (int i = 0; i < pawn.story.traits.allTraits.Count; i++) {
                    if (i != 0) {
                        stringBuilder.Append(" ,");
                    }
                    stringBuilder.Append(pawn.story.traits.allTraits[i].LabelCap);
                }
                stringBuilder.Append(")");
            }
            return stringBuilder.ToString();
        }
        public static string AppendContractDetails(string text, Pawn pawn) {
            Tenant tenantComp = pawn.GetTenantComponent();
            StringBuilder stringBuilder = new StringBuilder(text);
            stringBuilder.AppendLine();
            stringBuilder.Append("RequestForTenancyContract".Translate(tenantComp.ContractLength / 60000, tenantComp.Payment, pawn.Named("PAWN")));
            text = stringBuilder.ToString();
            text = text.AdjustedFor(pawn);
            text = AppendPawnDescription(text, pawn);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
            return text;
        }
        public static string ProlongContractMessage(Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append("RequestForTenancyContinued".Translate(pawn.Named("PAWN")));
            return AppendContractDetails(stringBuilder.ToString(), pawn);
        }
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
        public static void CourierDress(Pawn pawn, Map map) {
            pawn.apparel.DestroyAll();
            ThingDef hatDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == "Apparel_CowboyHat");
            Thing hat = ThingMaker.MakeThing(hatDef, GenStuff.RandomStuffByCommonalityFor(hatDef));
            pawn.apparel.Wear((Apparel)hat);
            ThingDef pantsDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == "Apparel_FlakPants");
            Thing pants = ThingMaker.MakeThing(pantsDef, GenStuff.RandomStuffByCommonalityFor(pantsDef));
            pawn.apparel.Wear((Apparel)pants);
            ThingDef shirtDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == "Apparel_BasicShirt");
            Thing shirt = ThingMaker.MakeThing(shirtDef, GenStuff.RandomStuffByCommonalityFor(shirtDef));
            pawn.apparel.Wear((Apparel)shirt);
            if (map.mapTemperature.OutdoorTemp < 0) {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == "Apparel_Parka");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }
            else if (map.mapTemperature.OutdoorTemp < 15) {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == "Apparel_Jacket");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }
            else {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == "Apparel_Duster");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }
        }
        public static void CourierInventory(Pawn pawn, Map map) {
            ThingDef swordDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == "MeleeWeapon_LongSword");
            Thing sword = ThingMaker.MakeThing(swordDef, GenStuff.RandomStuffByCommonalityFor(swordDef));
            pawn.equipment.AddEquipment((ThingWithComps) sword);
        }
    }
}
