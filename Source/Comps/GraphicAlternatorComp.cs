using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class GraphicAlternatorComp : ThingComp {
        public CompProps_GraphicAlternator Props => (CompProps_GraphicAlternator)props;
    }
    public class CompProps_GraphicAlternator : CompProperties {
        public readonly GraphicData Texture;
        public readonly GraphicData TextureAlternate;
        public CompProps_GraphicAlternator() {
            compClass = typeof(GraphicAlternatorComp);
        }
    }
    public class EraAlternatorComp : ThingComp {
        public CompProps_EraAlternator Props => (CompProps_EraAlternator)props;
    }
    public class CompProps_EraAlternator : CompProperties {
        public readonly GraphicData Texture;
        public readonly GraphicData TextureAlternate;
        public readonly TechLevel TechLevel;
        public CompProps_EraAlternator() {
            compClass = typeof(EraAlternatorComp);
        }
    }

}
