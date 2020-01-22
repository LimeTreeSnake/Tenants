using RimWorld;
using Verse;

namespace Tenants.Defs {
    [DefOf]
    public static class JobDefOf {
        public static JobDef JobUseCommsConsoleTenants;
        public static JobDef JobUseCommsConsoleMole;
        public static JobDef JobUseCommsConsoleInviteCourier;
        public static JobDef JobSendLetter;
        public static JobDef JobCheckLetters;
        static JobDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDef));
        }
    }
}
