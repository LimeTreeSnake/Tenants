using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class Wanted : ThingComp {
        #region Fields
        private Faction wantedBy;
        #endregion Fields
        #region Properties
        public Faction WantedBy {
            get => wantedBy; 
            set => wantedBy = value;
        }

        #endregion Properties

        #region Methods
        public override void PostExposeData() {
            Scribe_References.Look(ref wantedBy, "WantedBy");
        }
        #endregion Methods
    }
}
