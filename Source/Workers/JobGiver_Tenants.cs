using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Harmony;

namespace Tenants.Workers {
    public class JobGiver_Tenants : JobGiver_Work {

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams) {
            if (pawn.needs.joy.CurInstantLevel > SettingsHelper.LatestVersion.LevelOfHappinessToWork)
                return base.TryIssueJobPackage(pawn, jobParams);
            return ThinkResult.NoJob;
        }
    }
}
