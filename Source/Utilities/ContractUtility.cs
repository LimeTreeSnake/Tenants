using RimWorld;
using System.Text;
using Tenants.Comps;
using Tenants.Models;
using Verse;

namespace Tenants.Utilities {
    public static class ContractUtility {
        public static void ContractPayment(Contract contract, Map map) {
            int payment = (contract.ContractLength / 60000) * contract.Payment;
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = payment;
            TenantsMapComp.GetComponent(map).IncomingMail.Add(silver);
        }
        public static void Payment(Map map, int payment) {
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = payment;
            TenantsMapComp.GetComponent(map).IncomingMail.Add(silver);
        }
        public static Contract GenerateContract() {
            int payment = Rand.Range(Settings.Settings.MinDailyCost, Settings.Settings.MaxDailyCost);
            Contract contract = new Contract();
            contract.Payment = payment;
            contract.ContractLength = (Rand.Range(Settings.Settings.MinContractTime, Settings.Settings.MaxContractTime)) * 60000;
            contract.ContractDate = Find.TickManager.TicksGame;
            contract.ContractEndDate = Find.TickManager.TicksAbs + contract.ContractLength + 60000;
            contract.IsContracted = true;
            return contract;
        }
        public static void ContractProlong(Contract contract) {
            contract.ContractDate = Find.TickManager.TicksGame;
            contract.ContractEndDate = Find.TickManager.TicksAbs + contract.ContractLength + 60000;
        }  
    }
}
