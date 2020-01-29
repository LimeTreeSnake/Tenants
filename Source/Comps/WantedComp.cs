using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class WantedComp : WandererComp {
        #region Fields
        private Faction wantedBy;
        #endregion Fields
        #region Properties

        public Faction WantedBy {
            get; set;
        }
        #endregion Properties

        #region Methods
        public override void PostExposeData() {
            Scribe_References.Look(ref wantedBy, "WantedBy");
        }
        #endregion Methods
    }
    public class CompProps_Wanted : CompProperties {
        public CompProps_Wanted() {
            compClass = typeof(WantedComp);
        }
    }
}
