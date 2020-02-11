using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class IncidentDefOf {
        public static IncidentDef TenantProposition;
        public static IncidentDef WantedProposition;
        public static IncidentDef EnvoyInvitation;
        public static IncidentDef TenantCourier;

        public static IncidentDef Opportunists;
        public static IncidentDef WantedRaid;
        static IncidentDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }
    }
}
