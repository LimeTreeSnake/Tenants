using RimWorld;
using Verse;

namespace Tenants.Defs {   
    [DefOf]
    public static class ThingDefOf{
        public static ThingDef Tenant_MailBox;
        public static ThingDef Tenant_ScrollDiplomatic;
        public static ThingDef Tenant_ScrollMean;
        public static ThingDef Tenant_ScrollInvite;
        public static ThingDef Tenant_ScrollCase;
        static ThingDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDef));
        }
    }
}
