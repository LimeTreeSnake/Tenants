using UnityEngine;
using Verse;

namespace Tenants.Utilities {
    public static class TextureUtility {
        public static Texture2D ContractIcon => ContentFinder<Texture2D>.Get("LTS/Icons/Contract");
        public static Texture2D EnvoyIcon => ContentFinder<Texture2D>.Get("LTS/Icons/Envoy");
        public static Texture2D WantedIcon => ContentFinder<Texture2D>.Get("LTS/Icons/Wanted");


    }
}
