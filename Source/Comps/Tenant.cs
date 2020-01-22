using RimWorld;
using Verse;

namespace Tenants.Comps {
    public enum Mood { Neutral = 1, Happy = 2, Sad = 3 };
    public class Tenant : ThingComp {
        #region Fields
        private bool isTerminated = false;
        private bool isEnvoy = false;
        private bool capturedTenant = false;
        private bool mayJoin = false;
        private bool autoRenew = false;
        private bool contracted = false;
        private bool wanted = false;
        private Faction hiddenFaction;
        private bool mayFirefight = false;
        private bool mayBasic = false;
        private bool mayHaul = false;
        private bool mayClean = false;

        private int contractLength;
        private int contractDate;
        private int contractEndDate;
        private int recentBadMoodCount, happyMoodCount, sadMoodCount, neutralMoodCount;
        private int payment;
        private int surgeryQueue;
        #endregion Fields
        #region Properties
        public bool IsTerminated {
            get => isTerminated;
            set => isTerminated = value;
        }
        public bool IsEnvoy {
            get => isEnvoy;
            set => isEnvoy = value;
        }
        public bool CapturedTenant {
            get => capturedTenant;
            set => capturedTenant = value;
        }
        public bool MayJoin {
            get => mayJoin;
            set => mayJoin = value;
        }
        public bool AutoRenew {
            get => autoRenew;
            set => autoRenew = value;
        }
        public bool Contracted {
            get => contracted;
            set => contracted = value;
        }
        public bool Wanted {
            get => wanted;
            set => wanted = value;
        }
        public Faction HiddenFaction {
            get => hiddenFaction;
            set => hiddenFaction = value;
        }
        public bool MayFirefight {
            get => mayFirefight;
            set => mayFirefight = value;
        }
        public bool MayBasic {
            get => mayBasic;
            set => mayBasic = value;
        }
        public bool MayHaul {
            get => mayHaul;
            set => mayHaul = value;
        }
        public bool MayClean {
            get => mayClean;
            set => mayClean = value;
        }
        public int ContractLength {
            get => contractLength;
            set => contractLength = value;
        }
        public int ContractDate {
            get => contractDate;
            set => contractDate = value;
        }
        public int ContractEndDate {
            get => contractEndDate;
            set => contractEndDate = value;
        }
        public int ContractEndTick => contractDate + contractLength;
        public int RecentBadMoodsCount {
            get => recentBadMoodCount;
            set => recentBadMoodCount = value;
        }
        public int HappyMoodCount {
            get => happyMoodCount;
            set {
                RecentBadMoodsCount = 0;
                happyMoodCount = value;
            }
        }
        public int SadMoodCount {
            get => sadMoodCount;
            set {
                RecentBadMoodsCount++;
                sadMoodCount = value;
            }
        }
        public int NeutralMoodCount {
            get => neutralMoodCount;
            set {
                RecentBadMoodsCount = 0;
                neutralMoodCount = value;
            }
        }
        public int Payment {
            get => payment;
            set => payment = value;
        }
        public int SurgeryQueue {
            get => surgeryQueue;
            set => surgeryQueue = value;
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Used to reset tenant mood.
        /// </summary>
        public void ResetMood() {
            recentBadMoodCount = 0;
            happyMoodCount = 0;
            sadMoodCount = 0;
            neutralMoodCount = 0;
        }
        /// <summary>
        /// Used when a Tenant should leave.
        /// </summary>
        public void CleanTenancy() {
            contracted = false;
            isEnvoy = false;
            wanted = false;
            contractLength = 0;
            contractDate = 0;
            contractEndDate = 0;
            payment = 0;
            surgeryQueue = 0;
            ResetMood();
        }
        public override void PostExposeData() {
            Scribe_Values.Look(ref isTerminated, "IsTerminated");
            Scribe_Values.Look(ref isEnvoy, "IsEnvoy");
            Scribe_Values.Look(ref capturedTenant, "CapturedTenant");
            Scribe_Values.Look(ref mayJoin, "MayJoin");
            Scribe_Values.Look(ref autoRenew, "AutoRenew");
            Scribe_Values.Look(ref contracted, "Contracted");
            Scribe_Values.Look(ref wanted, "Wanted");
            Scribe_References.Look(ref hiddenFaction, "HiddenFaction");
            Scribe_Values.Look(ref mayFirefight, "MayFirefight");
            Scribe_Values.Look(ref mayBasic, "MayBasic");
            Scribe_Values.Look(ref mayHaul, "MayHaul");
            Scribe_Values.Look(ref mayClean, "MayClean");
            Scribe_Values.Look(ref contractLength, "ContractLength");
            Scribe_Values.Look(ref contractDate, "ContractDate");
            Scribe_Values.Look(ref contractEndDate, "ContractEndDate");
            Scribe_Values.Look(ref recentBadMoodCount, "RecentBadMoodCount");
            Scribe_Values.Look(ref happyMoodCount, "HappyMoodCount");
            Scribe_Values.Look(ref sadMoodCount, "SadMoodCount");
            Scribe_Values.Look(ref neutralMoodCount, "NeutralMoodCount");
            Scribe_Values.Look(ref payment, "Payment");
            Scribe_Values.Look(ref surgeryQueue, "SurgeryQueue");
        }
        #endregion Methods
    }
    public class CompProps_Tenant : CompProperties {
        public CompProps_Tenant() {
            compClass = typeof(Tenant);
        }
    }
}
