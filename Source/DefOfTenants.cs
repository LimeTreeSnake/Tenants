using RimWorld;
using Verse;

namespace Tenants {
    [DefOf]
    public static class IncidentDefOf {
        public static IncidentDef RetributionForCaptured;
        public static IncidentDef RetributionForDead;
        public static IncidentDef Opportunists;
        public static IncidentDef RequestForTenancy;
        public static IncidentDef MoleRaid;
        public static IncidentDef WantedRaid;
        static IncidentDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }
    }
    [DefOf]
    public static class RaidStrategyDefOf {
        public static RaidStrategyDef Retribution;
        public static RaidStrategyDef MoleRaid;
        public static RaidStrategyDef WantedRaid;

        static RaidStrategyDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
        }
    }
    [DefOf]
    public static class JobDefOf {
        public static JobDef JobUseCommsConsoleTenants;
        public static JobDef JobUseCommsConsoleMole;
        static JobDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDef));
        }
    }

}
