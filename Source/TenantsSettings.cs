using Verse;
using UnityEngine;

namespace Tenants {
    internal class TenantsSettings : ModSettings {

        #region Fields
        private static readonly int minDailyCost = 50;
        private static readonly int maxDailyCost = 100;
        private static readonly int minContractTime = 5;
        private static readonly int maxContractTime = 5;
        #endregion Fields
        #region Properties

        public int MinDailyCost = minDailyCost;
        public int MaxDailyCost = maxDailyCost;
        public int MinContractTime = minContractTime;
        public int MaxContractTime = maxContractTime;
        #endregion Properties

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref MinDailyCost, "MinDailyCost", minDailyCost);
            Scribe_Values.Look(ref MaxDailyCost, "MaxDailyCost", maxDailyCost);
            Scribe_Values.Look(ref MinContractTime, "MinContractTime", minContractTime);
            Scribe_Values.Look(ref MaxContractTime, "MaxContractTime", maxContractTime);
        }
        public void Reset() {
            MinDailyCost = minDailyCost;
            MaxDailyCost = maxDailyCost;
            MinContractTime = minContractTime;
            MaxContractTime = maxContractTime;
        }
    }
    internal static class SettingsHelper {
        public static TenantsSettings LatestVersion;
        public static void Reset() {
            LatestVersion.Reset();
        }
    }
    public class ModMain : Mod {
        private TenantsSettings tenantsSettings;
        public ModMain(ModContentPack content) : base(content) {
            tenantsSettings = GetSettings<TenantsSettings>();
            SettingsHelper.LatestVersion = tenantsSettings;
        }
        public override string SettingsCategory() {
            return "Tenants";
        }
        public static Vector2 scrollPosition = Vector2.zero;


        public override void DoSettingsWindowContents(Rect inRect) {
            inRect.yMin += 20;
            inRect.yMax -= 20;
            Listing_Standard list = new Listing_Standard();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 16f, inRect.height * 2 + 450f);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            list.Begin(rect2);

            list.Label(string.Format("({0}) Min contract daily cost.", tenantsSettings.MinDailyCost));
            tenantsSettings.MinDailyCost = (int)Mathf.Round(list.Slider((float)tenantsSettings.MinDailyCost, 50, 100));
            list.Label(string.Format("({0}) Max contract daily cost.", tenantsSettings.MaxDailyCost));
            tenantsSettings.MaxDailyCost = (int)Mathf.Round(list.Slider((float)tenantsSettings.MaxDailyCost, tenantsSettings.MinDailyCost, 10 * tenantsSettings.MinDailyCost));
            list.Label(string.Format("({0}) Min contract time.", tenantsSettings.MinContractTime));
            tenantsSettings.MinContractTime = (int)Mathf.Round(list.Slider((float)tenantsSettings.MinContractTime, 1, 100));
            list.Label(string.Format("({0}) Max contract time.", tenantsSettings.MaxContractTime));
            tenantsSettings.MaxContractTime = (int)Mathf.Round(list.Slider((float)tenantsSettings.MaxContractTime, 1, 100));

            list.End();
            Widgets.EndScrollView();
            tenantsSettings.Write();
        }
    }
}
