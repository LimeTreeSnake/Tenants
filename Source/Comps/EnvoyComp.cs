using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class EnvoyComp : TenantComp {
    }
    public class CompProps_Envoy : CompProperties {
        public CompProps_Envoy() {
            compClass = typeof(EnvoyComp);
        }
    }
}
