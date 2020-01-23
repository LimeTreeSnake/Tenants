using Harmony;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.Controllers {
    public static class EnvoyController {

        public static void Tick(Pawn pawn, EnvoyComp comp) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (contract == null) {
                pawn.AllComps.Remove(comp);
                return;
            }

        }


        public static Pawn FindRandomEnvoy(Faction faction) {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where ThingCompUtility.TryGetComp<TenantComp>(p) != null && !p.Dead && !p.Spawned && !p.Discarded && p.Faction != null && p.Faction == faction
                                select p).ToList();
            if (pawns.Count < 3)
                for (int i = 0; i < 3; i++)
                    pawns.Add(GenerateEnvoy(faction));
            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns[0];
        }
        public static Pawn GenerateEnvoy(Faction faction) {
            bool generation = true;
            Pawn newTenant = null;
            while (generation) {
                string race = Settings.SettingsHelper.LatestVersion.AvailableRaces.RandomElement();
                PawnKindDef random = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.defName == race && x.defaultFactionType == faction.def).RandomElement();
                if (random == null)
                    return null;
                newTenant = PawnGenerator.GeneratePawn(random, faction);
                if (newTenant != null && !newTenant.Dead && !newTenant.IsDessicated() && !newTenant.AnimalOrWildMan()) {
                    {
                        TenantComp tenantComp = new TenantComp();
                        tenantComp.HiddenFaction = faction;
                        newTenant.AllComps.Add(tenantComp);
                        if (Settings.SettingsHelper.LatestVersion.SimpleClothing) {
                            FloatRange range = newTenant.kindDef.apparelMoney;
                            newTenant.kindDef.apparelMoney = new FloatRange(0f, Settings.SettingsHelper.LatestVersion.SimpleClothingMax);
                            PawnApparelGenerator.GenerateStartingApparelFor(newTenant, new PawnGenerationRequest(random));
                            newTenant.kindDef.apparelMoney = range;
                        }
                        newTenant.SetFaction(faction);
                        if (Settings.SettingsHelper.LatestVersion.Weapons) {
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
        public static bool EnvoyTenancy(Map map, Faction faction) {
            if (!MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return false;
            }
            Pawn pawn = FindRandomEnvoy(faction);
            if (pawn == null) {
                Messages.Message("EnvoyArriveFailed".Translate(faction), MessageTypeDefOf.NeutralEvent);
                return false;
            }
            pawn.relations.everSeenByPlayer = true;
            TenantComp tenantComp = pawn.TryGetComp<TenantComp>();
            tenantComp.IsEnvoy = true;
            SpawnTenant(pawn, map, spawnSpot);
            MapUtilities.GenerateBasicContract(tenantComp, 0, 2);
            Messages.Message("EnvoyArriveSuccess".Translate(faction, pawn.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            CameraJumper.TryJump(pawn);
            return true;
        }
    }
}
