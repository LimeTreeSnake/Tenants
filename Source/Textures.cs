using UnityEngine;
using Verse;

namespace Tenants {
    [StaticConstructorOnStartup]
    public static class Textures {
        public static readonly Texture2D ContractIcon;

        static Textures() {
            ContractIcon = ContentFinder<Texture2D>.Get("UI/Icons/Contract");
        }
    }


}
