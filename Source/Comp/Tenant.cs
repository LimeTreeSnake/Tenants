﻿using RimWorld;
using Verse;

namespace Tenants {
    public class Tenant : ThingComp {
        #region Fields
        private bool isTenant = false;
        private bool isTerminated = false;
        private bool capturedTenant = false;
        private bool mayJoin = false;
        private bool autoRenew = false;
        private bool contracted = false;
        private bool wanted = false;
        private Faction wantedBy;
        private bool mole = false;
        private bool moleActivated = false;
        private bool moleMessage = false;
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
        public bool IsTenant {
            get { return isTenant; }
            set { isTenant = value; if (isTenant == false) { CleanTenancy(); } }
        }
        public bool IsTerminated {
            get { return isTerminated; }
            set { isTerminated = value; }
        }
        public bool CapturedTenant {
            get { return capturedTenant; }
            set { capturedTenant = value; }
        }
        public bool MayJoin {
            get { return mayJoin; }
            set { mayJoin = value; }
        }
        public bool AutoRenew {
            get { return autoRenew; }
            set { autoRenew = value; }
        }
        public bool Contracted {
            get { return contracted; }
            set { contracted = value; }
        }
        public bool Wanted {
            get { return wanted; }
            set { wanted = value; }
        }
        public Faction WantedBy {
            get { return wantedBy; }
            set { wantedBy = value; }
        }
        public bool Mole {
            get { return mole; }
            set { mole = value; }
        }
        public bool MoleActivated {
            get { return moleActivated; }
            set { moleActivated = value; }
        }
        public bool MoleMessage {
            get { return moleMessage; }
            set { moleMessage = value; }
        }
        public Faction HiddenFaction {
            get { return hiddenFaction; }
            set { hiddenFaction = value; }
        }
        public bool MayFirefight {
            get { return mayFirefight; }
            set { mayFirefight = value; }
        }
        public bool MayBasic {
            get { return mayBasic; }
            set { mayBasic = value; }
        }
        public bool MayHaul {
            get { return mayHaul; }
            set { mayHaul = value; }
        }
        public bool MayClean {
            get { return mayClean; }
            set { mayClean = value; }
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
        public int SurgeryQueue {
            get { return surgeryQueue; }
            set { surgeryQueue = value; }
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
            wanted = false;
            wantedBy = null;
            mole = false;
            moleActivated = false;
            moleMessage = false;
            contractLength = 0;
            contractDate = 0;
            contractEndDate = 0;
            payment = 0;
            surgeryQueue = 0;
            ResetMood();
        }
        public override void PostExposeData() {
            Scribe_Values.Look(ref isTenant, "IsTenant");
            Scribe_Values.Look(ref isTerminated, "IsTerminated");
            Scribe_Values.Look(ref capturedTenant, "CapturedTenant");
            Scribe_Values.Look(ref mayJoin, "MayJoin");
            Scribe_Values.Look(ref autoRenew, "AutoRenew");
            Scribe_Values.Look(ref contracted, "Contracted");
            Scribe_Values.Look(ref wanted, "Wanted");
            Scribe_References.Look(ref wantedBy, "WantedBy");
            Scribe_Values.Look(ref mole, "Mole");
            Scribe_Values.Look(ref moleActivated, "MoleActivated");
            Scribe_Values.Look(ref moleMessage, "MoleMessage");
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
