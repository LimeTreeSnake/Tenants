using RimWorld;
using Verse;

namespace Tenants.Defs {   
    [DefOf]
    public static class ThingDefOf{
        public static ThingDef Tenant_MessageBox;
        public static ThingDef Tenant_LetterDiplomatic;
        public static ThingDef Tenant_LetterAngry;
        public static ThingDef Tenant_LetterInvite;
        public static ThingDef Tenant_ScrollCase;
        static ThingDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDef));
        }
    }
}
