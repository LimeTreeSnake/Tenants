using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class TenantComp : ThingComp {
        #region Fields
        private bool capturedTenant;
        private bool mayJoin;
        private Faction hiddenFaction;
        #endregion Fields
        #region Properties
        public bool CapturedTenant {
            get => capturedTenant;
            set => capturedTenant = value;
        }
        public bool MayJoin {
            get => mayJoin;
            set => mayJoin = value;
        }
        public Faction HiddenFaction {
            get => hiddenFaction;
            set => hiddenFaction = value;
        }

        #endregion Properties

        #region Methods
        public override void PostExposeData() {
            Scribe_Values.Look(ref capturedTenant, "CapturedTenant");
            Scribe_Values.Look(ref mayJoin, "MayJoin");
            Scribe_References.Look(ref hiddenFaction, "HiddenFaction");
        }
        #endregion Methods
    }
    public class CompProps_Tenant : CompProperties {
        public CompProps_Tenant() {
            compClass = typeof(TenantComp);
        }
    }
}
