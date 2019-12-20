using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Tenants {
    public class MapComponent_Tenants : MapComponent {
        #region Fields
        private bool broadcast = false;
        private bool broadcastCourier = false;
        private int killedCourier = 0;
        private List<Pawn> deadTenantsToAvenge = new List<Pawn>();
        private List<Pawn> capturedTenantsToAvenge = new List<Pawn>();
        private List<Pawn> moles = new List<Pawn>();
        private List<Pawn> wantedTenants = new List<Pawn>();
        private List<Letter> outgoingLetters = new List<Letter>();
        private List<Letter> incomingLetters = new List<Letter>();
        private List<Thing> incomingMail = new List<Thing>();
        private List<Thing> courierCost = new List<Thing>();
        private float karma;
        #endregion Fields
        #region Properties
        public List<Pawn> DeadTenantsToAvenge {
            get {
                if (deadTenantsToAvenge == null) { deadTenantsToAvenge = new List<Pawn>(); }
                return deadTenantsToAvenge;
            }
        }
        public List<Pawn> CapturedTenantsToAvenge {
            get {
                if (capturedTenantsToAvenge == null) { capturedTenantsToAvenge = new List<Pawn>(); }
                return capturedTenantsToAvenge;
            }
        }
        public List<Pawn> Moles {
            get {
                if (moles == null) { moles = new List<Pawn>(); }
                return moles;
            }
        }
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
        public List<Letter> OutgoingLetters {
            get {
                if (outgoingLetters == null) { outgoingLetters = new List<Letter>(); }
                return outgoingLetters;
            }
        }
        public List<Letter> IncomingLetters {
            get {
                if (incomingLetters == null) { incomingLetters = new List<Letter>(); }
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
        public MapComponent_Tenants(Map map)
            : base(map) {
        }
        public MapComponent_Tenants(bool generateComponent, Map map)
            : base(map) {
            if (generateComponent) {
                map.components.Add(this);
            }
        }
        #endregion Constructors
        #region Methods
        public static MapComponent_Tenants GetComponent(Map map) {
            return map.GetComponent<MapComponent_Tenants>() ?? new MapComponent_Tenants(generateComponent: true, map);
        }
        public override void ExposeData() {
            Scribe_Collections.Look(ref deadTenantsToAvenge, "DeadTenants", LookMode.Reference);
            Scribe_Collections.Look(ref capturedTenantsToAvenge, "CapturedTenants", LookMode.Reference);
            Scribe_Collections.Look(ref moles, "Moles", LookMode.Reference);
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
