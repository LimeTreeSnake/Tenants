using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenants.Models {
    public class Contract {
        public bool IsTerminated { get; set; } = false;
        public bool IsContracted { get; set; } = false;
        public int ContractLength { get; set; }
        public int ContractDate { get; set; }
        public int ContractEndDate { get; set; }
        public bool AutoRenew { get; set; } = false;
        public int Payment { get; set; }
        public int ContractEndTick => ContractDate + ContractLength;
    }
}
