using RimWorld;
using Verse;

namespace Tenants.Comps {
    public class ContractComp : ThingComp {
        #region Fields
        private bool isTerminated = false;
        private bool contracted = false;
        private int contractLength;
        private int contractDate;
        private int contractEndDate;
        private bool autoRenew = false;
        private int payment;
        #endregion Fields
        #region Properties
        public bool IsTerminated {
            get => isTerminated;
            set => isTerminated = value;
        }
        public bool AutoRenew {
            get => autoRenew;
            set => autoRenew = value;
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
        public int Payment {
            get => payment;
            set => payment = value;
        }
        #endregion Properties
        #region Methods
        public override void PostExposeData() {
            Scribe_Values.Look(ref isTerminated, "IsTerminated");
            Scribe_Values.Look(ref autoRenew, "AutoRenew");
            Scribe_Values.Look(ref contracted, "Contracted");
            Scribe_Values.Look(ref contractLength, "ContractLength");
            Scribe_Values.Look(ref contractDate, "ContractDate");
            Scribe_Values.Look(ref contractEndDate, "ContractEndDate");
            Scribe_Values.Look(ref payment, "Payment");
        }
        #endregion Methods
    }
    public class CompProps_Contract : CompProperties {
        public CompProps_Contract() {
            compClass = typeof(ContractComp);
        }
    }
}
