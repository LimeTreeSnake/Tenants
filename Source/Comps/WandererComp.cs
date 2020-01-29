using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class WandererComp : TenantComp {
        #region Fields
        private bool mayJoin;
        #endregion Fields
        #region Properties
        public bool MayJoin {
            get => mayJoin;
            set => mayJoin = value;
        }
        #endregion Properties

        #region Methods
        public override void PostExposeData() {
            Scribe_Values.Look(ref mayJoin, "MayJoin");
        }
        #endregion Methods
    }
    public class CompProps_Wanderer : CompProperties {
        public CompProps_Wanderer() {
            compClass = typeof(WandererComp);
        }
    }
}
