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
    public static class WandererController {
        public static void Tick(Pawn tenant, WandererComp comp, ContractComp contract) {
            if (contract.IsTerminated) {
                Find.LetterStack.ReceiveLetter("ContractBreach".Translate(), "ContractDoneTerminated".Translate(tenant.Named("PAWN")), LetterDefOf.NeutralEvent);
                TenantController.Leave(tenant);
            }
            //Tenant alone with no colonist
            if (tenant.Map.mapPawns.FreeColonists.FirstOrDefault(x => ThingCompUtility.TryGetComp<WandererComp>(x) == null) == null) {
                contract.IsTerminated = true;
                ContractConclusion(tenant, comp, contract, 0.75f);
                return;
            }
            //Tenant contract is out
            if (Find.TickManager.TicksGame >= contract.ContractEndTick) {
                ContractConclusion(tenant, comp, contract);
                return;
            }
            //Tenancy tick 1/10 per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
                //Join
                if (tenant.needs.mood.CurInstantLevel > 0.8f && Rand.Bool) {
                    TenantController.TenantWantToJoin(tenant);
                    return;
                }
            }
        }
        public static void Contract(Map map) {
            if (!Utilities.MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return;
            }
            Pawn tenant = TenantController.GetContractedPawn();
            tenant.AllComps.Add(new WandererComp());
            StringBuilder stringBuilder = new StringBuilder("");
            if (TenantsMapComp.GetComponent(map).Broadcast) {
                TenantsMapComp.GetComponent(map).Broadcast = false;
                stringBuilder.Append("RequestForTenancyOpportunity".Translate(tenant.Named("PAWN")));
            }
            else {
                stringBuilder.Append("RequestForTenancyInitial".Translate(tenant.Named("PAWN")));
            }
            stringBuilder.Append(ContractController.GenerateContractMessage(tenant));
            ContractController.GenerateContractDialogue("RequestForTenancyTitle".Translate(map.Parent.Label), stringBuilder.ToString(), tenant, map, spawnSpot);
        }
        public static void ContractConclusion(Pawn tenant, WandererComp comp, ContractComp contract, float stealChance = 0.5f) {
            if (contract == null || comp == null)
                return;
            if (contract.IsTerminated) {
                if (Rand.Value > stealChance) {
                    ContractController.ContractPayment(tenant);
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTerminated".Translate(contract.Payment * contract.ContractLength / 60000, tenant.Named("PAWN")), LetterDefOf.NeutralEvent);
                    TenantController.Leave(tenant);
                }
                else {
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTheft".Translate(tenant.Named("PAWN")), LetterDefOf.NegativeEvent);
                    TenantController.Theft(tenant);
                }
                return;
            }
            else {
                ContractController.ContractPayment(tenant);
                Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDone".Translate(contract.Payment * contract.ContractLength / 60000, tenant.Named("PAWN")), LetterDefOf.PositiveEvent);
                if (contract.AutoRenew) {
                    string letterLabel = "ContractNew".Translate();
                    string letterText = "ContractRenewedMessage".Translate(tenant.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                }
                else {
                    StringBuilder stringBuilder = new StringBuilder("");
                    stringBuilder.Append("RequestForTenancyContinued".Translate(tenant.Named("PAWN")));
                    stringBuilder.Append(ContractController.GenerateContractMessage(tenant));
                    ContractController.GenerateContractDialogue("RequestForTenancyTitle".Translate(tenant.Map.Parent.Label), stringBuilder.ToString(), tenant);
                }
            }

        }
    }
}
