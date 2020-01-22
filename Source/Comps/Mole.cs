using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class Mole : ThingComp {

        #region Fields
        private bool activated = false;
        #endregion Fields
        #region Properties
        public bool Activated {
            get => activated;
            set => activated = value;
        }
        #endregion Properties

        #region Methods
        public override void PostExposeData() {
            Scribe_Values.Look(ref activated, "Activated");
        }
        #endregion Methods
    }
}
