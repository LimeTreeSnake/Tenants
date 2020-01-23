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
    public static class MoleController {
        public static void Tick(Pawn pawn, MoleComp comp) {
            ContractComp contract = ThingCompUtility.TryGetComp<ContractComp>(pawn);
            if (contract == null) {
                pawn.AllComps.Remove(comp);
                return;
            }
            if (Find.TickManager.TicksGame % 60000 == 0) {
                MoleComp moleComp = ThingCompUtility.TryGetComp<MoleComp>(pawn);
                if (moleComp != null && !moleComp.Activated) {
                    if (Rand.Bool) {
                        Building building = pawn.Map.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x.def.defName.ToLower().Contains("commsconsole"));
                        if (building != null) {
                            Job job = new Job(Defs.JobDefOf.JobUseCommsConsoleMole, building);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }
                    }
                }
            }
        }
    }
}
