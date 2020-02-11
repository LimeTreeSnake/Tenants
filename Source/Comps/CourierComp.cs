using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Tenants.Comps {
    public class CourierComp : ThingComp {
        public List<ThingDef> items = new List<ThingDef>();
        #region Fields
        #endregion Fields
        #region Properties
        #endregion Properties
        #region Methods
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref items, "Items", LookMode.Deep);
        }
        #endregion Methods
    }
    public class CompProps_Courier : CompProperties {
        public CompProps_Courier() {
            compClass = typeof(CourierComp);
        }
    }
}
