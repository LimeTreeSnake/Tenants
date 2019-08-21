using System;
using System.Collections.Generic;
using Verse;

namespace Tenants {
    public class Tenant : ThingComp {
        public enum Mood { happy, neutral, sad }
        private bool isTenant = false;
        private int contractLength;
        private int contractDate;
        private int contractEndDate => contractDate + ContractLength;
        private int payment;
        private List<Mood> moods = new List<Mood>();

        public Tenant() {
            moods = new List<Mood>();
        }
        public bool IsTenant {
            get { return isTenant; }
            set { isTenant = value; }
        }
        public int ContractLength {
            get { return contractLength; }
            set { contractLength = value; }
        }
        public int ContractDate {
            get { return contractDate; }
            set { contractDate = value; }
        }
        public int ContractEndDate {
            get { return contractEndDate; }
        }
        public int Payment {
            get { return payment; }
            set { payment = value; }
        }
        public List<Mood> Moods => moods;
        public override void PostExposeData() {
            Scribe_Values.Look(ref isTenant, "IsTenant");
            Scribe_Values.Look(ref contractLength, "ContractLength");
            Scribe_Values.Look(ref contractDate, "ContractDate");
            Scribe_Values.Look(ref payment, "Payment");
            Scribe_Collections.Look(ref moods, "Moods", LookMode.Value);
        }
    }

    public class CompProps_Tenant : CompProperties {
        public CompProps_Tenant() {
            compClass = typeof(Tenant);
        }
    }

}
