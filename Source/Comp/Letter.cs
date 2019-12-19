using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Tenants
{
    public enum LetterType { Diplomatic = 1, Mean = 2};
   public class Letter : ThingComp
    {
        public CompProps_Letter Props => (CompProps_Letter)props;
        public Faction faction;
        public override void PostExposeData() {
            Scribe_References.Look(ref faction, "Faction");
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
