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
        public static void Tick(Pawn pawn, WandererComp comp) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (contract == null || pawn.IsColonist) {
                TenantController.RemoveAllComp(pawn);
                return;
            }
            //Tenant alone with no colonist
            if (pawn.Map.mapPawns.FreeColonists.FirstOrDefault(x => ThingCompUtility.TryGetComp<WandererComp>(x) == null) == null) {
                contract.IsTerminated = true;
                ContractConclusion(pawn, 0.75f);
                return;
            }
            //Tenant contract is out
            if (Find.TickManager.TicksGame >= contract.ContractEndTick) {
                ContractConclusion(pawn);
                return;
            }
            //Tenancy tick 1/10 per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
                //Join
                if (pawn.needs.mood.CurInstantLevel > 0.8f && Rand.Bool) {
                    TenantController.TenantWantToJoin(pawn);
                    return;
                }
            }
        }
        public static bool Contract(Map map) {
            if (!Utilities.MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return false;
            }
            Pawn pawn = TenantController.FindRandomPawn();
            if (pawn == null)
                return false;
            WandererComp comp = Generate(pawn);
            if (comp == null)
                return false;
            pawn.relations.everSeenByPlayer = true;
            ContractComp contract = ContractController.GenerateContract(pawn);
            StringBuilder stringBuilder = new StringBuilder("");
            if (TenantsMapComp.GetComponent(map).Broadcast) {
                TenantsMapComp.GetComponent(map).Broadcast = false;
                stringBuilder.Append("RequestForTenancyOpportunity".Translate(pawn.Named("PAWN")));
            }
            else {
                stringBuilder.Append("RequestForTenancyInitial".Translate(pawn.Named("PAWN")));
            }
            stringBuilder.Append(ContractController.GenerateContractMessage(pawn));
            bool agree = ContractController.GenerateContractDialogue("RequestForTenancyTitle".Translate(map.Parent.Label), stringBuilder.ToString());
            if (agree) {
                TenantController.SpawnTenant(pawn, map, spawnSpot);
            }
            else {
                pawn.AllComps.Remove(contract);
                pawn.AllComps.Remove(comp);
                return false;
            }
            return true;
        }
        public static void ContractConclusion(Pawn pawn, float stealChance = 0.5f) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            WandererComp comp = ThingCompUtility.TryGetComp<WandererComp>(pawn);
            if (contract == null || comp == null)
                return;
            if (contract.IsTerminated) {
                if (Rand.Value > stealChance) {
                    ContractController.ContractPayment(pawn);
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTerminated".Translate(contract.Payment * contract.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                    TenantController.Leave(pawn);
                }
                else {
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTheft".Translate(pawn.Named("PAWN")), LetterDefOf.NegativeEvent);
                    pawn.AllComps.Remove(comp);
                    TenantController.Theft(pawn);
                }
                return;
            }
            else {
                ContractController.ContractPayment(pawn);
                Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDone".Translate(contract.Payment * contract.ContractLength / 60000, pawn.Named("PAWN")), LetterDefOf.PositiveEvent);
                if (contract.AutoRenew) {
                    string letterLabel = "ContractNew".Translate();
                    string letterText = "ContractRenewedMessage".Translate(pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                }
                else {
                    StringBuilder stringBuilder = new StringBuilder("");
                    stringBuilder.Append("RequestForTenancyContinued".Translate(pawn.Named("PAWN")));
                    stringBuilder.Append(ContractController.GenerateContractMessage(pawn));
                    bool stay = ContractController.GenerateContractDialogue("RequestForTenancyTitle".Translate(pawn.Map.Parent.Label), stringBuilder.ToString());
                    if (stay) {
                        ContractController.ContractProlong(pawn);
                    }
                    else {
                        TenantController.Leave(pawn);
                    }
                }
            }

        }
        public static WandererComp Generate(Pawn pawn) {
            WandererComp comp = new WandererComp();

            pawn.AllComps.Add(comp);
            return comp;
        }
    }
}
