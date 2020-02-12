using RimWorld;
using System.Collections.Generic;
using Verse;
using Tenants.Models;

namespace Tenants.Components {
    public class TenancyComp : MapComponent {
        #region Fields
        private Dictionary<Pawn, Contract> contracts = new Dictionary<Pawn, Contract>();
        private List<Pawn> pawnList = new List<Pawn>();
        private List<Contract> contractList = new List<Contract>();
        private List<Thing> outgoingMail = new List<Thing>();
        private List<Thing> incomingMail = new List<Thing>();
        private List<Thing> courierDebt = new List<Thing>();
        #endregion Fields
        #region Properties 
        public Dictionary<Pawn, Contract> Contracts {
            get {
                if (contracts == null) { contracts = new Dictionary<Pawn, Contract>(); }
                return contracts;
            }
        }
        public List<Thing> OutgoingMail {
            get {
                if (outgoingMail == null) { outgoingMail = new List<Thing>(); }
                return outgoingMail;
            }
        }
        public List<Thing> IncomingMail {
            get {
                if (incomingMail == null) { incomingMail = new List<Thing>(); }
                return incomingMail;
            }
        }
        public List<Thing> CourierDebt {
            get {
                if (courierDebt == null) { courierDebt = new List<Thing>(); }
                return courierDebt;
            }
        }
        #endregion Properties
        #region Constructors
        public TenancyComp(bool generateComponent, Map map) : base(map) {
            if (generateComponent) {
                map.components.Add(this);
            }
        }
        #endregion Constructors

        #region Methods
        public static TenancyComp GetComponent(Map map) {
            return map.GetComponent<TenancyComp>() ?? new TenancyComp(generateComponent: true, map);
        }
        public override void ExposeData() {
            Scribe_Collections.Look(ref contracts, "contracts", LookMode.Reference, LookMode.Value, ref pawnList, ref contractList);
            Scribe_Collections.Look(ref incomingMail, "incomingMail", LookMode.Deep);
            Scribe_Collections.Look(ref outgoingMail, "outgoingMail", LookMode.Deep);
            Scribe_Collections.Look(ref courierDebt, "courierDebt", LookMode.Deep);
        }

        public override void MapComponentTick() {
            base.MapComponentTick();
            if (contracts.Count > 0) {
                if (Find.TickManager.TicksGame % Settings.Settings.TickFrequency == 0) {
                    foreach (KeyValuePair<Pawn, Contract> entry in contracts) {
                        // do something with entry.Value or entry.Key
                    }
                }
            }
        }
        #endregion Methods
    }
}
