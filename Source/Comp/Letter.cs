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
        public Faction faction;
        public LetterType letter;
        public override void PostExposeData() {
            Scribe_References.Look(ref faction, "Faction");
            Scribe_Values.Look(ref letter, "Letter");
        }
    }
}
