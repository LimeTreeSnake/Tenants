using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class TenantComp : ThingComp {
        #region Fields
        #endregion Fields
        #region Properties
        #endregion Properties
        #region Methods
        public override void PostExposeData() {
        }
        #endregion Methods
    }

    public class CompProps_Tenant : CompProperties {
        public CompProps_Tenant() {
            compClass = typeof(TenantComp);
        }
    }
}
