using RimWorld;
using Verse;

namespace Tenants {   
    [DefOf]
    public static class ThingDefOf{
        public static ThingDef Tenants_MailBox;
        public static ThingDef Tenant_LetterDiplomatic;
        public static ThingDef Tenant_LetterAngry;
        public static ThingDef Tenant_LetterInvite;
        static ThingDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDef));
        }
    }
}
