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

        public static void ContractPayment(ContractComp contract) {
            int payment = (contract.ContractLength / 60000) * contract.Payment;
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = payment;
            TenantsMapComp.GetComponent(contract.parent.Map).IncomingMail.Add(silver);
        }
        public static void Payment(Map map, int payment) {
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = payment;
            TenantsMapComp.GetComponent(map).IncomingMail.Add(silver);           
        }
        public static void GenerateBasicContract(ContractComp contract, int payment, int timeMultiplier = 1) {
            contract.Payment = payment;
            contract.ContractLength = (Rand.Range(Settings.SettingsHelper.LatestVersion.MinContractTime, Settings.SettingsHelper.LatestVersion.MaxContractTime) * timeMultiplier) * 60000;
            contract.ContractDate = Find.TickManager.TicksGame;
            contract.ContractEndDate = Find.TickManager.TicksAbs + contract.ContractLength + 60000;
        }
    }
}
