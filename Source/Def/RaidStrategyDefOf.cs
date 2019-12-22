using RimWorld;
using Verse;

namespace Tenants {
    [DefOf]
    public static class RaidStrategyDefOf {
        public static RaidStrategyDef Retribution;
        public static RaidStrategyDef MoleRaid;
        public static RaidStrategyDef WantedRaid;

        static RaidStrategyDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
        }
    }
}
