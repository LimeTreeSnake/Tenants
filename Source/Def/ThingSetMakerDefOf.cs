using RimWorld;
using Verse;

namespace Tenants {
    public static class ThingSetMakerDefOf {
        public static ThingSetMakerDef Gift_Diplomatic;
        static ThingSetMakerDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingSetMakerDef));
        }
    }
}
