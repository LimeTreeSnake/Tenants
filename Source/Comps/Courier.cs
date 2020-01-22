using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Tenants.Comps {
    public class Courier : ThingComp {
        public List<ThingDef> items = new List<ThingDef>();
    }
    public class CompProps_Courier : CompProperties {
        public CompProps_Courier() {
            compClass = typeof(Courier);
        }
    }
}
