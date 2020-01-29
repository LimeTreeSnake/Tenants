using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Tenants.Comps {
    public class TenantsMapComp : MapComponent {
        #region Fields
        private bool broadcast = false;
        private bool broadcastCourier = false;
        private int killedCourier = 0;      
        private List<Pawn> wantedTenants = new List<Pawn>();
        private List<Thing> outgoingLetters = new List<Thing>();
        private List<Thing> incomingLetters = new List<Thing>();
        private List<Thing> incomingMail = new List<Thing>();
        private List<Thing> courierCost = new List<Thing>();
        private float karma;
        #endregion Fields
        #region Properties     
        public List<Pawn> WantedTenants {
            get {
                if (wantedTenants == null) { wantedTenants = new List<Pawn>(); }
                return wantedTenants;
            }
        }
        public bool Broadcast { get { return broadcast; } set { broadcast = value; } }
        public bool BroadcastCourier { get { return broadcastCourier; } set { broadcastCourier = value; } }
        public int KilledCourier { get { return killedCourier; } set { killedCourier = value; } }
        public float Karma { get { return karma; } set { karma = value; } }
        public List<Thing> OutgoingLetters {
            get {
                if (outgoingLetters == null) { outgoingLetters = new List<Thing>(); }
                return outgoingLetters;
            }
        }
        public List<Thing> IncomingLetters {
            get {
                if (incomingLetters == null) { incomingLetters = new List<Thing>(); }
                return incomingLetters;
            }
        }
        public List<Thing> IncomingMail {
            get {
                if (incomingMail == null) { incomingMail = new List<Thing>(); }
                return incomingMail;
            }
        }
        public List<Thing> CourierCost {
            get {
                if (courierCost == null) { courierCost = new List<Thing>(); }
                return courierCost;
            }
        }
        #endregion Properties
        #region Constructors
        public TenantsMapComp(Map map)
            : base(map) {
        }
        public TenantsMapComp(bool generateComponent, Map map)
            : base(map) {
            if (generateComponent) {
                map.components.Add(this);
            }
        }
        #endregion Constructors
        #region Methods
        public static TenantsMapComp GetComponent(Map map) {
            return map.GetComponent<TenantsMapComp>() ?? new TenantsMapComp(generateComponent: true, map);
        }
        public override void ExposeData() {
            Scribe_Collections.Look(ref wantedTenants, "WantedTenants", LookMode.Reference);
            Scribe_Collections.Look(ref incomingMail, "IncomingMail", LookMode.Deep);
            Scribe_Collections.Look(ref outgoingLetters, "OutgoingMail", LookMode.Deep);
            Scribe_Collections.Look(ref courierCost, "CourierCost", LookMode.Deep);
            Scribe_Values.Look(ref broadcast, "Broadcast");
            Scribe_Values.Look(ref broadcastCourier, "BroadcastCourier");
            Scribe_Values.Look(ref killedCourier, "KilledCourier");
            Scribe_Values.Look(ref karma, "Karma");
        }
        #endregion Methods
    }
}
