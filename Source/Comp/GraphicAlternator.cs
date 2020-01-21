using RimWorld;
using Verse;

namespace Tenants.Comp {
    public class GraphicAlternator : ThingComp {
        public CompProps_GraphicAlternator Props => (CompProps_GraphicAlternator)props;
    }
    public class CompProps_GraphicAlternator : CompProperties {
        public readonly GraphicData Texture;
        public readonly GraphicData TextureAlternate;
        public CompProps_GraphicAlternator() {
            compClass = typeof(GraphicAlternator);
        }
    }
    public class EraAlternator : ThingComp {
        public CompProps_EraAlternator Props => (CompProps_EraAlternator)props;
    }
    public class CompProps_EraAlternator : CompProperties {
        public readonly GraphicData Texture;
        public readonly GraphicData TextureAlternate;
        public readonly TechLevel TechLevel;
        public CompProps_EraAlternator() {
            compClass = typeof(EraAlternator);
        }
    }

}
