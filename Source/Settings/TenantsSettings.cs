using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using System;

namespace Tenants {
    internal class TenantsSettings : ModSettings {
        #region Fields
        private static readonly List<string> availableRaces = new List<string>() { "Human" };
        private static readonly int minDailyCost = 50;
        private static readonly int maxDailyCost = 100;
        private static readonly int minContractTime = 3;
        private static readonly int maxContractTime = 7;
        private static readonly float stayChanceHappy = 95F;
        private static readonly float stayChanceNeutral = 50F;
        private static readonly float stayChanceSad = 5f;
        private static readonly int harborPenalty = 5;
        private static readonly int outragePenalty = 8;
        private static readonly bool weapons = true;
        private static readonly bool simpleClothing = true;
        private static readonly int simpleClothingMin = 100;
        private static readonly int simpleClothingMax = 300;
        private static readonly int courierCost = 30;
        private static float r = 127f, g = 63f, b = 191f;
        private static Color color = new Color(r / 255f, g / 255f, b / 255f);
        private static readonly float levelOfHappinessToWork = 70f;
        #endregion Fields
        #region Properties
        public List<string> AvailableRaces = availableRaces != null|| availableRaces.Count > 0 ? availableRaces.ListFullCopy() : new List<string>() { "Human" };
        public IEnumerable<ThingDef> Races = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race != null && x.RaceProps.Humanlike && x.RaceProps.IsFlesh && x.RaceProps.ResolvedDietCategory != DietCategory.NeverEats).Select(s => s.race).Distinct();
        public float RaceViewHeight = 300f;
        public int MinDailyCost = minDailyCost;
        public int MaxDailyCost = maxDailyCost;
        public int MinContractTime = minContractTime;
        public int MaxContractTime = maxContractTime;
        public float StayChanceHappy = stayChanceHappy;
        public float StayChanceNeutral = stayChanceNeutral;
        public float StayChanceSad = stayChanceSad;
        public int HarborPenalty = harborPenalty;
        public int OutragePenalty = outragePenalty;
        public bool Weapons = weapons;
        public bool SimpleClothing = simpleClothing;
        public float SimpleClothingMin = simpleClothingMin;
        public float SimpleClothingMax = simpleClothingMax;
        public float CourierCost = courierCost;
        public string Filter { get; set; }
        public float R { get { return r; } set { r = value; color = new Color(r / 255, g / 255, b / 255); } }
        public float G { get { return g; } set { g = value; color = new Color(r / 255, g / 255, b / 255); } }
        public float B { get { return b; } set { b = value; color = new Color(r / 255, g / 255, b / 255); } }
        public Color Color => color;
        public float LevelOfHappinessToWork = levelOfHappinessToWork;

        #endregion Properties

        public override void ExposeData() {

            base.ExposeData();
            Scribe_Collections.Look(ref AvailableRaces, "AvailableRaces", LookMode.Deep);
            Scribe_Values.Look(ref MinDailyCost, "MinDailyCost", minDailyCost);
            Scribe_Values.Look(ref MaxDailyCost, "MaxDailyCost", maxDailyCost);
            Scribe_Values.Look(ref MinContractTime, "MinContractTime", minContractTime);
            Scribe_Values.Look(ref MaxContractTime, "MaxContractTime", maxContractTime);
            Scribe_Values.Look(ref StayChanceHappy, "StayChanceHappy", stayChanceHappy);
            Scribe_Values.Look(ref StayChanceNeutral, "StayChanceNeutral", stayChanceNeutral);
            Scribe_Values.Look(ref StayChanceSad, "StayChanceSad", stayChanceSad);
            Scribe_Values.Look(ref HarborPenalty, "HarborPenalty", harborPenalty);
            Scribe_Values.Look(ref OutragePenalty, "OutragePenalty", outragePenalty);
            Scribe_Values.Look(ref Weapons, "Weapons", weapons);
            Scribe_Values.Look(ref SimpleClothing, "SimpleClothing", simpleClothing);
            Scribe_Values.Look(ref SimpleClothingMin, "SimpleClothingMin", simpleClothingMin);
            Scribe_Values.Look(ref SimpleClothingMax, "SimpleClothingMax", simpleClothingMax);
            Scribe_Values.Look(ref CourierCost, "CourierCost", courierCost);
            Scribe_Values.Look(ref r, "R", r);
            Scribe_Values.Look(ref g, "G", g);
            Scribe_Values.Look(ref b, "B", b);
            Scribe_Values.Look(ref LevelOfHappinessToWork, "LevelOfHappinessToWork", levelOfHappinessToWork);
        }
        internal void Reset() {
            AvailableRaces = availableRaces.Count > 0 ? availableRaces.ListFullCopy() : new List<string>();
            MinDailyCost = minDailyCost;
            MaxDailyCost = maxDailyCost;
            MinContractTime = minContractTime;
            MaxContractTime = maxContractTime;
            StayChanceHappy = stayChanceHappy;
            StayChanceNeutral = stayChanceNeutral;
            StayChanceSad = stayChanceSad;
            R = 127f; G = 63f; B = 191f;
            LevelOfHappinessToWork = levelOfHappinessToWork;
            Weapons = weapons;
            SimpleClothing = simpleClothing;
            SimpleClothingMin = simpleClothingMin;
            SimpleClothingMax = simpleClothingMax;
        }
    }
    internal static class SettingsHelper {
        public static TenantsSettings LatestVersion;
        public static void Reset() {
            LatestVersion.Reset();
        }
    }
    public class ModMain : Mod {
        private TenantsSettings settings;
        public ModMain(ModContentPack content) : base(content) {
            settings = GetSettings<TenantsSettings>();
            SettingsHelper.LatestVersion = settings;
        }
        public override string SettingsCategory() {
            return "Tenants";
        }
        public static Vector2 scrollPosition = Vector2.zero;


        public override void DoSettingsWindowContents(Rect inRect) {
            try {
                inRect.yMin += 20;
                inRect.yMax -= 20;
                Listing_Standard list = new Listing_Standard();
                Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
                Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, inRect.height * 2 + settings.RaceViewHeight);
                Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
                list.Begin(rect2);
                if (list.ButtonText("Default Settings")) {
                    settings.Reset();
                };
                list.Label(string.Format("Minimum daily tenant contract payment ({0}).", settings.MinDailyCost));
                settings.MinDailyCost = (int)Mathf.Round(list.Slider(settings.MinDailyCost, 0, 100));
                list.Label(string.Format("Maximum daily tenant contract payment ({0}).", settings.MaxDailyCost));
                settings.MaxDailyCost = (int)Mathf.Round(list.Slider(settings.MaxDailyCost, settings.MinDailyCost, 1000));
                list.Label(string.Format("Minimum contracted days ({0}).", settings.MinContractTime));
                settings.MinContractTime = (int)Mathf.Round(list.Slider(settings.MinContractTime, 1, 100));
                list.Label(string.Format("Maximum contracted days ({0}).", settings.MaxContractTime));
                settings.MaxContractTime = (int)Mathf.Round(list.Slider(settings.MaxContractTime, 1, 100));
                list.Label(string.Format("({0}) Chance of contract extension when the tenant is satisfied.", settings.StayChanceHappy));
                settings.StayChanceHappy = (int)Mathf.Round(list.Slider(settings.StayChanceHappy, settings.StayChanceNeutral, 100f));
                list.Label(string.Format("({0}) Chance of contract extension when the tenant is okay.", settings.StayChanceNeutral));
                settings.StayChanceNeutral = (int)Mathf.Round(list.Slider(settings.StayChanceNeutral, settings.StayChanceSad, 100f));
                list.Label(string.Format("({0}) Chance of contract extension when the tenant is dissatisfied.", settings.StayChanceSad));
                settings.StayChanceSad = (int)Mathf.Round(list.Slider(settings.StayChanceSad, 0f, 100f));
                list.Label(string.Format("Faction penalty to relations for harboring fugitives. ({0})", settings.HarborPenalty));
                settings.HarborPenalty = (int)Mathf.Round(list.Slider(settings.HarborPenalty, 1f, 100f));
                list.Label(string.Format("Faction penalty to relations for tenancy accidents. ({0})", settings.OutragePenalty));
                settings.OutragePenalty = (int)Mathf.Round(list.Slider(settings.OutragePenalty, 5f, 100f));
                list.Label(string.Format("Needed level of tenancy happiness to do labor: ({0}).", settings.LevelOfHappinessToWork));
                settings.LevelOfHappinessToWork = (byte)Mathf.Round(list.Slider(settings.LevelOfHappinessToWork, 0f, 100f));
                list.Label(string.Format("Basic courier cost: ({0}).", settings.CourierCost));
                settings.CourierCost = (byte)Mathf.Round(list.Slider(settings.CourierCost, 10f, 100f));
                list.Gap();
                list.CheckboxLabeled("Should tenants spawn without weapons?", ref settings.Weapons, "Keep in mind that this removes any weapon when a tenant spawns. Have you given a weapon to a tenant once before, it'll be removed should they leave the map and spawn again somewhere.");
                list.Gap();
                list.CheckboxLabeled("Should tenants spawn simpler clothing?", ref settings.SimpleClothing, "Upon tenant creation, tenants will spawn with simpler clothing within the selected value range.");
                if (settings.SimpleClothing) {
                    list.Gap();
                    list.Label(string.Format("Min total apparel value ({0}).", settings.SimpleClothingMin));
                    settings.SimpleClothingMin = Mathf.Round(list.Slider(settings.SimpleClothingMin, 0f, 500f));
                    list.Label(string.Format("Max total apparel value ({0}).", settings.SimpleClothingMax));
                    settings.SimpleClothingMax = Mathf.Round(list.Slider(settings.SimpleClothingMax, 0f, 1000f));
                }
                list.Gap();
                list.GapLine();
                float R = settings.R, G = settings.G, B = settings.B;
                string buffer1 = R.ToString(), buffer2 = G.ToString(), buffer3 = B.ToString();
                list.Label("RGB value for tenants name: <color=#" + ColorUtility.ToHtmlStringRGB(settings.Color) + ">" + "Color" + "</color>");
                list.TextFieldNumericLabeled("R", ref R, ref buffer1, 0, 255);
                list.TextFieldNumericLabeled("G", ref G, ref buffer2, 0, 255);
                list.TextFieldNumericLabeled("B", ref B, ref buffer3, 0, 255);
                settings.R = R;
                settings.G = G;
                settings.B = B;
                list.Gap();
                list.GapLine();
                if (settings.Races != null && settings.Races.Count() > 0) {
                    list.Label(string.Format("Available races"));
                    settings.Filter = list.TextEntryLabeled("Filter:", settings.Filter, 1);
                    Listing_Standard list2 = list.BeginSection(settings.RaceViewHeight);
                    list2.ColumnWidth = (rect2.width - 50) / 4;
                    foreach (ThingDef def in settings.Races) {
                        if (def.defName.ToUpper().Contains(settings.Filter.ToUpper())) {
                            bool contains = settings.AvailableRaces.Contains(def.defName);
                            list2.CheckboxLabeled(def.defName, ref contains, "");
                            if (contains == false && settings.AvailableRaces.Contains(def.defName)) {
                                settings.AvailableRaces.Remove(def.defName);
                            }
                            else if (contains == true && !settings.AvailableRaces.Contains(def.defName)) {
                                settings.AvailableRaces.Add(def.defName);
                            }
                        }
                    }
                    list.EndSection(list2);
                }

                list.End();
                Widgets.EndScrollView();
                settings.Write();
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
            }
        }
    }
}
