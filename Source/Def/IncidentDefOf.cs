using RimWorld;
using Verse;

namespace Tenants {
    [DefOf]
    public static class IncidentDefOf {
        public static IncidentDef RequestForTenancy;
        public static IncidentDef TenantCourier;

        public static IncidentDef RetributionForDead;
        public static IncidentDef RetributionForCaptured;
        public static IncidentDef Opportunists;
        public static IncidentDef MoleRaid;
        public static IncidentDef WantedRaid;
        static IncidentDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }
    }
}
