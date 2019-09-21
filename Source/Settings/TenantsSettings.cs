using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Tenants {
    internal class TenantsSettings : ModSettings {
        #region Fields
        private static readonly int minDailyCost = 50;
        private static readonly int maxDailyCost = 100;
        private static readonly int minContractTime = 5;
        private static readonly int maxContractTime = 5;
        private static readonly float stayChanceHappy = 95F;
        private static readonly float stayChanceNeutral = 50F;
        private static readonly float stayChanceSad = 5f;
        private static readonly bool weapons = true;
        private static readonly bool simpleClothing = true;
        private static readonly int simpleClothingMin = 100;
        private static readonly int simpleClothingMax = 300;
        private static float r = 127f, g = 63f, b = 191f;
        private static Color color = new Color(r / 255f, g / 255f, b / 255f);
        private static readonly float levelOfHappinessToWork = 70f;
        #endregion Fields
        #region Properties
        public int MinDailyCost = minDailyCost;
        public int MaxDailyCost = maxDailyCost;
        public int MinContractTime = minContractTime;
        public int MaxContractTime = maxContractTime;
        public float StayChanceHappy = stayChanceHappy;
        public float StayChanceNeutral = stayChanceNeutral;
        public float StayChanceSad = stayChanceSad;
        public bool Weapons = weapons;
        public bool SimpleClothing = simpleClothing;
        public float SimpleClothingMin = simpleClothingMin;
        public float SimpleClothingMax = simpleClothingMax;
        public float R { get { return r; } set { r = value; color = new Color(r / 255, g / 255, b / 255); } }
        public float G { get { return g; } set { g = value; color = new Color(r / 255, g / 255, b / 255); } }
        public float B { get { return b; } set { b = value; color = new Color(r / 255, g / 255, b / 255); } }
        public Color Color => color;
        public float LevelOfHappinessToWork = levelOfHappinessToWork;
        
        #endregion Properties

        public override void ExposeData() {

            base.ExposeData();
            Scribe_Values.Look(ref MinDailyCost, "MinDailyCost", minDailyCost);
            Scribe_Values.Look(ref MaxDailyCost, "MaxDailyCost", maxDailyCost);
            Scribe_Values.Look(ref MinContractTime, "MinContractTime", minContractTime);
            Scribe_Values.Look(ref MaxContractTime, "MaxContractTime", maxContractTime);
            Scribe_Values.Look(ref StayChanceHappy, "StayChanceHappy", stayChanceHappy);
            Scribe_Values.Look(ref StayChanceNeutral, "StayChanceNeutral", stayChanceNeutral);
            Scribe_Values.Look(ref StayChanceSad, "StayChanceSad", stayChanceSad);
            Scribe_Values.Look(ref Weapons, "Weapons", weapons);
            Scribe_Values.Look(ref SimpleClothing, "SimpleClothing", simpleClothing);
            Scribe_Values.Look(ref SimpleClothingMin, "SimpleClothingMin", simpleClothingMin);
            Scribe_Values.Look(ref SimpleClothingMax, "SimpleClothingMax", simpleClothingMax);
            Scribe_Values.Look(ref r, "R", r);
            Scribe_Values.Look(ref g, "G", g);
            Scribe_Values.Look(ref b, "B", b);
            Scribe_Values.Look(ref LevelOfHappinessToWork, "LevelOfHappinessToWork", levelOfHappinessToWork);
        }
        public void Reset() {
            MinDailyCost = minDailyCost;
            MaxDailyCost = maxDailyCost;
            MinContractTime = minContractTime;
            MaxContractTime = maxContractTime;
            StayChanceHappy = stayChanceHappy;
            StayChanceNeutral = stayChanceNeutral;
            StayChanceSad = stayChanceSad;
            r = 127f; g = 63f; b = 191f;
            LevelOfHappinessToWork = levelOfHappinessToWork;
            Weapons = weapons;
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
            if (list.ButtonText("Default Settings")) {
                tenantsSettings.Reset();
            };
            list.Label(string.Format("({0}) Min contract daily cost.", tenantsSettings.MinDailyCost));
            tenantsSettings.MinDailyCost = (int)Mathf.Round(list.Slider(tenantsSettings.MinDailyCost, 0, 100));
            list.Label(string.Format("({0}) Max contract daily cost.", tenantsSettings.MaxDailyCost));
            tenantsSettings.MaxDailyCost = (int)Mathf.Round(list.Slider(tenantsSettings.MaxDailyCost, tenantsSettings.MinDailyCost, 1000));
            list.Label(string.Format("({0}) Min contract time.", tenantsSettings.MinContractTime));
            tenantsSettings.MinContractTime = (int)Mathf.Round(list.Slider(tenantsSettings.MinContractTime, 1, 100));
            list.Label(string.Format("({0}) Max contract time.", tenantsSettings.MaxContractTime));
            tenantsSettings.MaxContractTime = (int)Mathf.Round(list.Slider(tenantsSettings.MaxContractTime, 1, 100));
            list.Label(string.Format("({0}) Extend Contract Chance when Happy.", tenantsSettings.StayChanceHappy));
            tenantsSettings.StayChanceHappy = (int)Mathf.Round(list.Slider(tenantsSettings.StayChanceHappy, tenantsSettings.StayChanceNeutral, 100f));
            list.Label(string.Format("({0}) Extend Contract Chance when Neutral.", tenantsSettings.StayChanceNeutral));
            tenantsSettings.StayChanceNeutral = (int)Mathf.Round(list.Slider(tenantsSettings.StayChanceNeutral, tenantsSettings.StayChanceSad, 100f));
            list.Label(string.Format("({0}) Extend Contract Chance when Sad.", tenantsSettings.StayChanceSad));
            tenantsSettings.StayChanceSad = (int)Mathf.Round(list.Slider(tenantsSettings.StayChanceSad, 0f, 100f));
            list.Gap();
            list.CheckboxLabeled("Should tenants spawn without weapons?", ref tenantsSettings.Weapons, "Keep in mind that this removes any weapon when a tenant spawns. Have you given a weapon to a tenant once before, it'll be removed should they leave the map and spawn again somewhere.");
            list.Gap();
            list.CheckboxLabeled("Should tenants spawn simple clothing?", ref tenantsSettings.SimpleClothing, "Upon tenant creation, tenants will spawn with simple clothing within the selected money range.");
            if (tenantsSettings.SimpleClothing) {
                list.Label(string.Format("({0}) Min money.", tenantsSettings.SimpleClothingMin));
                tenantsSettings.SimpleClothingMin = Mathf.Round(list.Slider(tenantsSettings.SimpleClothingMin, 0f, 500f));
                list.Label(string.Format("({0}) Max Money.", tenantsSettings.SimpleClothingMax));
                tenantsSettings.SimpleClothingMax = Mathf.Round(list.Slider(tenantsSettings.SimpleClothingMax, 0f, 1000f));

            }
            list.Gap();
            list.Label("RGB value for tenant name:");
            list.Label(string.Format("({0}) R.", tenantsSettings.R));
            tenantsSettings.R = (byte)Mathf.Round(list.Slider(tenantsSettings.R, 0f, 255f));
            list.Label(string.Format("({0}) G.", tenantsSettings.G));
            tenantsSettings.G = (byte)Mathf.Round(list.Slider(tenantsSettings.G, 0f, 255f));
            list.Label(string.Format("({0}) B.", tenantsSettings.B));
            tenantsSettings.B = (byte)Mathf.Round(list.Slider(tenantsSettings.B, 0f, 255f));
            list.Gap();
            list.GapLine();
            list.Gap();
            list.Label(string.Format("({0}) Needed level of happiness to work.", tenantsSettings.LevelOfHappinessToWork));
            tenantsSettings.LevelOfHappinessToWork = (byte)Mathf.Round(list.Slider(tenantsSettings.LevelOfHappinessToWork, 0f, 100f));

            list.End();
            Widgets.EndScrollView();
            tenantsSettings.Write();
        }
    }
}
