using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class ThoughtDefOf {
        public static ThoughtDef TenantGotStuff;
        public static ThoughtDef TenantLostStuff;
        static ThoughtDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDef));
        }
    }
}
