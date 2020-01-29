
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Tenants.Comps;
using Verse;

namespace Tenants.UI {
    public class MainTabWindow_Tenant : MainTabWindow_PawnTable {
        private static PawnTableDef pawnTableDef;
        protected override PawnTableDef PawnTableDef => pawnTableDef ?? (pawnTableDef = DefDatabase<PawnTableDef>.GetNamed("Tenants"));
        protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.AllPawns
                                                      where ThingCompUtility.TryGetComp<ContractComp>(p) != null
                                                      select p;
        public override void PostOpen() {
            base.PostOpen();
            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }
    }
    public class PawnTable_Tenants : PawnTable {
        public PawnTable_Tenants(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
        : base(def, pawnsGetter, uiWidth, uiHeight) {
        }
    }


}
