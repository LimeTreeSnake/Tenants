using Verse;

namespace Tenants.Models {
    public enum ContractType { Regular = 1, Wanted = 2, Envoy = 3 }
    public class Contract {
        public ContractType ContractType { get; set; } = ContractType.Regular;
        public bool IsTerminated { get; set; } = false;
        public int ContractLength { get; set; }
        public int ContractEndDate { get; set; }
        public int ContractStartDate { get; set; } = Find.TickManager.TicksGame;
        public bool AutoRenew { get; set; } = false;
        public int Payment { get; set; }    
        //Enable Work Bools?
        //
    
    }
}