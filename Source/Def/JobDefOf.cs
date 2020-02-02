using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class JobDefOf {
        public static JobDef JobUseCommsConsoleTenants;
        public static JobDef JobUseCommsConsoleInviteCourier;
        public static JobDef JobSendMail;
        public static JobDef JobCheckMail;
        static JobDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDef));
        }
    }
}
