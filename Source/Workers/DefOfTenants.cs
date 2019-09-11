using RimWorld;
using Verse;

namespace Tenants {
    [DefOf]
    public static class IncidentDefOf {
        public static IncidentDef RetributionForCaptured;
        public static IncidentDef RetributionForDead;
        public static IncidentDef Opportunists;
        public static IncidentDef RequestForTenancy;
        static IncidentDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }
    }
    [DefOf]
    public static class RaidStrategyDefOf {
        public static RaidStrategyDef Retribution;

        static RaidStrategyDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
        }
    }
    [DefOf]
    public static class JobDefOf {
        public static JobDef JobUseCommsConsoleTenants;
        static JobDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
        }
    }

}
