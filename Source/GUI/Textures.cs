using UnityEngine;
using Verse;

namespace Tenants {
    [StaticConstructorOnStartup]
    public static class Textures {
        public static readonly Texture2D ContractIcon;
        public static readonly Texture2D EnvoyIcon;

        static Textures() {
            ContractIcon = ContentFinder<Texture2D>.Get("LTS/Icons/Contract");
            EnvoyIcon = ContentFinder<Texture2D>.Get("LTS/Icons/Envoy");
        }
    }


}
