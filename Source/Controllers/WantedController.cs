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

namespace Tenants.Controllers {
    public static class WantedController {
        public static void Tick(Pawn pawn, WantedComp comp, ContractComp contract) {
            WandererController.Tick(pawn, comp, contract);
            //Tenancy tick per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
                if (ThingCompUtility.TryGetComp<WantedComp>(pawn) != null) {
                    if (!TenantsMapComp.GetComponent(pawn.Map).WantedTenants.Contains(pawn)) {
                        TenantWanted(pawn);
                    }
                }
            }
        }
        public static void TenantWanted(Pawn pawn) {
            WantedComp wantedComp = ThingCompUtility.TryGetComp<WantedComp>(pawn);
            if (Rand.Value < 0.5) {
                int val = Utilities.FactionUtilities.ChangeRelations(wantedComp.WantedBy, true);
                Messages.Message("HarboringWantedTenant".Translate(wantedComp.WantedBy, val, pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }
        }
        internal static void Contract(Map map) {
            if (!Utilities.MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return;
            }
            Pawn tenant = TenantController.GetContractedPawn();
            WantedComp comp = Generate(tenant);
            if(comp == null) {
                TenantController.RemoveAllComp(tenant);
                return;
            }
            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append("RequestFromWantedInitial".Translate(tenant.Named("PAWN")));
            stringBuilder.Append(ContractController.GenerateContractMessage(tenant));
            ContractController.GenerateContractDialogue("RequestFromWantedTitle".Translate(map.Parent.Label), stringBuilder.ToString(), tenant, map, spawnSpot);
        }
        public static WantedComp Generate(Pawn pawn) {
            WantedComp comp = new WantedComp();
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
            if (comp.WantedBy == null) {
                return null;
            }
            pawn.AllComps.Add(comp);
            return comp;
        }
    }
}
