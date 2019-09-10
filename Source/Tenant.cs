using Verse;

namespace Tenants {
    public class Tenant : ThingComp {
        #region Fields
        private bool isTenant = false;
        private bool isTerminated = false;
        private bool wasTenant = false;
        private bool mayJoin = false;
        private bool autoRenew = false;
        private int contractLength;
        private int contractDate;
        private int contractEndDate;
        private int workCooldown;
        private int recentBadMoodCount, happyMoodCount, sadMoodCount, neutralMoodCount;

        private int payment;
        #endregion Fields
        #region Properties
        public bool IsTenant {
            get { return isTenant; }
            set { isTenant = value; }
        }
        public bool IsTerminated {
            get { return isTerminated; }
            set { isTerminated = value; }
        }
        public bool WasTenant {
            get { return wasTenant; }
            set { wasTenant = value; }
        }
        public bool MayJoin {
            get { return mayJoin; }
            set { mayJoin = value; }
        }
        public bool AutoRenew {
            get { return autoRenew; }
            set { autoRenew = value; }
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
            set { contractEndDate = value; }
        }
        public int ContractEndTick {
            get { return contractDate + contractLength; }
        }
        public int WorkCooldown {
            get { return workCooldown; }
            set { workCooldown = value; }
        }
        public int RecentBadMoodsCount {
            get { return recentBadMoodCount; }
            set { recentBadMoodCount = value; }
        }
        public int HappyMoodCount {
            get { return happyMoodCount; }
            set { happyMoodCount = value; }
        }
        public int SadMoodCount {
            get { return sadMoodCount; }
            set { sadMoodCount = value; }
        }
        public int NeutralMoodCount {
            get { return neutralMoodCount; }
            set { neutralMoodCount = value; }
        }
        public int Payment {
            get { return payment; }
            set { payment = value; }
        }
        #endregion Properties

        #region Methods
        public void ResetMood() {
            recentBadMoodCount = 0;
            happyMoodCount = 0;
            sadMoodCount = 0;
            neutralMoodCount = 0;
        }
        public void Reset() {
            isTenant = false;
            isTerminated = false;
            wasTenant = false;
            mayJoin = false;
            autoRenew = false;
            contractLength = 0;
            contractDate = 0;
            contractEndDate = 0;
            workCooldown = 0;
            ResetMood();
        }
        public override void PostExposeData() {
            Scribe_Values.Look(ref isTenant, "IsTenant");
            Scribe_Values.Look(ref isTerminated, "IsTerminated");
            Scribe_Values.Look(ref wasTenant, "WasTenant");
            Scribe_Values.Look(ref mayJoin, "MayJoin");
            Scribe_Values.Look(ref autoRenew, "AutoRenew");
            Scribe_Values.Look(ref contractLength, "ContractLength");
            Scribe_Values.Look(ref contractDate, "ContractDate");
            Scribe_Values.Look(ref contractEndDate, "ContractEndDate");
            Scribe_Values.Look(ref workCooldown, "WorkCooldown");
            Scribe_Values.Look(ref recentBadMoodCount, "RecentBadMoodCount");
            Scribe_Values.Look(ref happyMoodCount, "HappyMoodCount");
            Scribe_Values.Look(ref sadMoodCount, "SadMoodCount");
            Scribe_Values.Look(ref neutralMoodCount, "NeutralMoodCount");
            Scribe_Values.Look(ref payment, "Payment");
        }
        #endregion Methods
    }
    public class CompProps_Tenant : CompProperties {
        public CompProps_Tenant() {
            compClass = typeof(Tenant);
        }
    }
}
