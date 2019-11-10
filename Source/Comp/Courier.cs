using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Tenants {
    public class Courier : ThingComp {
        public bool isCourier = false;
        public List<ThingDef> items = new List<ThingDef>();
    }
    public class CompProps_Courier : CompProperties {
        public CompProps_Courier() {
            compClass = typeof(Courier);
        }
    }
}
