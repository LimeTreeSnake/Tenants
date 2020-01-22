using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Tenants.Comps
{
    public enum LetterType { Diplomatic = 1, Angry = 2, Invite = 3};
   public class Letter : ThingComp
    {
        public CompProps_Letter Props => (CompProps_Letter)props;
        public int Skill;
        public int TypeValue;
        public Faction Faction;
        public override void PostExposeData() {
            Scribe_Values.Look(ref Skill, "Skill");
            Scribe_References.Look(ref Faction, "Faction");
            Scribe_Values.Look(ref TypeValue, "TypeValue");
        }
    }


    public class CompProps_Letter : CompProperties
    {
        public LetterType letter;
        public CompProps_Letter() {
            compClass = typeof(Letter);
        }
    }
}
