using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Tenants.Comps
{
    public enum ScrollType { Diplomatic = 1, Angry = 2, Invite = 3};
   public class ScrollComp : ThingComp
    {
        public CompProps_Scroll Props => (CompProps_Scroll)props;
        public int Skill;
        public int TypeValue;
        public Faction Faction;
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref Skill, "Skill");
            Scribe_References.Look(ref Faction, "Faction");
            Scribe_Values.Look(ref TypeValue, "TypeValue");
        }
    }


    public class CompProps_Scroll : CompProperties
    {
        public ScrollType scroll;
        public CompProps_Scroll() {
            compClass = typeof(ScrollComp);
        }
    }
}
