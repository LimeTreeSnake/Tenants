using RimWorld;
using Verse;
using Tenants.Models;

namespace Tenants.Comps {
    public enum TenancyType { None = 0, Tenant = 1, Wanted, Envoy };
    public class TenantComp : ThingComp {
        #region Fields
        private Contract contract;
        private TenancyType tenancy = 0;
        private bool mayJoin = false;
        private Faction wantedBy;
        #endregion Fields
        #region Properties
        public TenancyType Tenancy { get => tenancy; set => tenancy = value; }
        public bool MayJoin { get => mayJoin; set => mayJoin = value; }
        public Faction WantedBy { get => wantedBy; set => wantedBy = value; }
        public Contract Contract { get => contract; set => contract = value; }
        #endregion Properties
        #region Methods
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref mayJoin, "MayJoin");
            Scribe_Values.Look(ref tenancy, "Tenancy");
            Scribe_References.Look(ref wantedBy, "WantedBy");
            Scribe_Deep.Look(ref contract, "Contract");
        }
        #endregion Methods
    }
    public class CompProps_Tenant : CompProperties {
        public CompProps_Tenant() {
            compClass = typeof(TenantComp);
        }
    }
}
