using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class ThingDefOf{
        public static ThingDef Tenants_MailBox;
        public static ThingDef Tenant_LetterDiplomatic;
        public static ThingDef Tenant_LetterAngry;
        static ThingDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDef));
        }
    }
}
