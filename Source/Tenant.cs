using Verse;

namespace Tenants {
    public class Tenant : ThingComp {
        #region Fields
        private bool isTenant = false;
        private bool isTerminated = false;
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
        public override void PostExposeData() {
            Scribe_Values.Look(ref isTenant, "IsTenant");
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
