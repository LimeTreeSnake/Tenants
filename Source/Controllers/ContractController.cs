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
    public static class ContractController {

        public static void ContractPayment(Pawn pawn) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            int payment = (contract.ContractLength / 60000) * contract.Payment;
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = payment;
            TenantsMapComp.GetComponent(pawn.Map).IncomingMail.Add(silver);
        }
        public static void Payment(Map map, int payment) {
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = payment;
            TenantsMapComp.GetComponent(map).IncomingMail.Add(silver);
        }
        public static ContractComp GenerateContract(Pawn pawn) {
            int payment = Rand.Range(Settings.Settings.MinDailyCost, Settings.Settings.MaxDailyCost);
            ContractComp contract = new ContractComp();
            contract.Payment = payment;
            contract.ContractLength = (Rand.Range(Settings.Settings.MinContractTime, Settings.Settings.MaxContractTime)) * 60000;
            contract.ContractDate = Find.TickManager.TicksGame;
            contract.ContractEndDate = Find.TickManager.TicksAbs + contract.ContractLength + 60000;
            pawn.AllComps.Add(contract);
            return contract;
        }
        public static void ContractProlong(Pawn pawn) {
            ContractComp comp = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            comp.ContractDate = Find.TickManager.TicksGame;
            comp.ContractEndDate = Find.TickManager.TicksAbs + comp.ContractLength + 60000;
            //TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            //EnvoyComp envoy = ThingCompUtility.TryGetComp<EnvoyComp>(pawn);
            //if (envoy != null) {
            //    if (Rand.Value > 0.01f) {
            //        string letterLabel = "Envoy".Translate(tenantComp.HiddenFaction);
            //        string letterText = "EnvoyStays".Translate(tenantComp.HiddenFaction, pawn.Named("PAWN"));
            //        Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent);
            //    }
            //    else {
            //        return false;
            //    }
            //}
        }
        public static string GenerateContractMessage(Pawn pawn, ContractComp comp = null) {
            comp = comp ?? ThingCompUtility.TryGetComp<ContractComp>(pawn);
            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append(GenerateContractDetails(pawn, comp));
            stringBuilder.Append(GeneratePawnDescription(pawn));
            string text = "";
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
            stringBuilder.Append(text);
            text = stringBuilder.ToString();
            return text.AdjustedFor(pawn);
        }
        public static void GenerateContractDialogue(string title, string text, Pawn tenant, Map map = null, IntVec3? spawnSpot = null) {
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOptionAgree = new DiaOption("ContractAgree".Translate()) {
                action = delegate {
                    if (tenant.Spawned)
                        ContractProlong(tenant);
                    else
                        TenantController.SpawnTenant(tenant, map, spawnSpot.Value);
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOptionAgree);
            DiaOption diaOptionReject = new DiaOption("ContractReject".Translate()) {
                action = delegate {
                    if (tenant.Spawned)
                        TenantController.Leave(tenant);
                    else
                        TenantController.RemoveAllComp(tenant);
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOptionReject);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: false, radioMode: true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
        }
        private static string GenerateContractDetails(Pawn pawn, ContractComp comp = null) {
            comp = comp ?? ThingCompUtility.TryGetComp<ContractComp>(pawn);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.Append("RequestForTenancyContract".Translate(comp.ContractLength / 60000, comp.Payment, pawn.Named("PAWN")));

            return stringBuilder.ToString();
        }
        private static string GeneratePawnDescription(Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("ContractTenantDescription".Translate(pawn.ageTracker.AgeBiologicalYears, pawn.def.label, pawn.Named("PAWN")));
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
    }
}
