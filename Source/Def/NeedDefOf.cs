using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class NeedDefOf {
        public static NeedDef Satisfaction;
        static NeedDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(NeedDef));
        }
    }
}
