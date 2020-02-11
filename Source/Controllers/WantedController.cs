using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Tenants.Utilities;

namespace Tenants.Controllers {
    public static class WantedController {
        public static void Tick(Pawn pawn, TenantComp comp) {
            TenantController.Tick(pawn, comp);
            if (comp.Tenancy == TenancyType.None) {
                TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Remove(pawn);
                return;
            }
            //Tenancy tick per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
                if (ThingCompUtility.TryGetComp<TenantComp>(pawn) != null) {
                    if (!TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                        TenantWanted(pawn);
                    }
                }
            }
        }
        public static void TenantWanted(Pawn pawn) {
            TenantComp wantedComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (Rand.Value < 0.5) {
                int val = Utilities.FactionUtilities.ChangeRelations(wantedComp.WantedBy, true);
                Messages.Message("HarboringWantedTenant".Translate(wantedComp.WantedBy, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }
        }
        internal static void Contract(Map map) {
            if (!MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return;
            }
            Pawn tenant = TenantUtilities.CreateContractedPawn(out TenantComp comp);
            Generate(tenant, comp);
            if(comp.WantedBy == null) {
                TenantUtilities.CleanComp(comp);
            }
            else {
                StringBuilder stringBuilder = new StringBuilder("");
                stringBuilder.Append("RequestFromWantedInitial".Translate(tenant.Named("PAWN")));
                stringBuilder.Append(TenantController.GenerateContractMessage(tenant, comp));
                TenantController.GenerateContractDialogue("RequestFromWantedTitle".Translate(map.Parent.Label), stringBuilder.ToString(), tenant, comp, map, spawnSpot);
            }
           
        }
        public static void Generate(Pawn pawn, TenantComp comp) {
            List<FactionRelation> entries = Traverse.Create(pawn.Faction).Field("relations").GetValue<List<FactionRelation>>().Where(p => p.kind == FactionRelationKind.Hostile).ToList();
            if (entries.Count > 0) {
                int count = 0;
                while (comp.WantedBy == null && count < 10) {
                    count++;
                    entries.Shuffle();
                    if (entries[0].other.def.pawnGroupMakers != null && !entries[0].other.IsPlayer)
                        comp.WantedBy = entries[0].other;
                }
            }
        }
    }
}
