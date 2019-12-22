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
        private static readonly float levelOfHappinessToWork = 70f;
        private static readonly float levelOfHappinessToJoin = 80f;
        private static readonly bool weapons = true;
        private static readonly bool simpleClothing = true;
        private static readonly int simpleClothingMin = 100;
        private static readonly int simpleClothingMax = 300;
        private static readonly int courierCost = 30;
        private static readonly int envoyDayMultiplier = 2;
        private static readonly int minRelation = 4;
        private static readonly int maxRelation = 10;
        private static float r = 127f, g = 63f, b = 191f;
        private static Color color = new Color(r / 255f, g / 255f, b / 255f);
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
        public float LevelOfHappinessToWork = levelOfHappinessToWork;
        public float LevelOfHappinessToJoin = levelOfHappinessToJoin;
        public bool Weapons = weapons;
        public bool SimpleClothing = simpleClothing;
        public int SimpleClothingMin = simpleClothingMin;
        public int SimpleClothingMax = simpleClothingMax;
        public int CourierCost = courierCost;
        public int EnvoyDayMultiplier = envoyDayMultiplier;
        public int MinRelation = minRelation;
        public int MaxRelation = maxRelation;
        public string Filter { get; set; }
        public float R { get { return r; } set { r = value; color = new Color(r / 255, g / 255, b / 255); } }
        public float G { get { return g; } set { g = value; color = new Color(r / 255, g / 255, b / 255); } }
        public float B { get { return b; } set { b = value; color = new Color(r / 255, g / 255, b / 255); } }
        public Color Color => color;

        #endregion Properties

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref AvailableRaces, "AvailableRaces", LookMode.Value);
            Scribe_Values.Look(ref MinDailyCost, "MinDailyCost", minDailyCost);
            Scribe_Values.Look(ref MaxDailyCost, "MaxDailyCost", maxDailyCost);
            Scribe_Values.Look(ref MinContractTime, "MinContractTime", minContractTime);
            Scribe_Values.Look(ref MaxContractTime, "MaxContractTime", maxContractTime);
            Scribe_Values.Look(ref StayChanceHappy, "StayChanceHappy", stayChanceHappy);
            Scribe_Values.Look(ref StayChanceNeutral, "StayChanceNeutral", stayChanceNeutral);
            Scribe_Values.Look(ref LevelOfHappinessToWork, "LevelOfHappinessToWork", levelOfHappinessToWork);
            Scribe_Values.Look(ref LevelOfHappinessToJoin, "LevelOfHappinessToJoin", levelOfHappinessToJoin);
            Scribe_Values.Look(ref Weapons, "Weapons", weapons);
            Scribe_Values.Look(ref SimpleClothing, "SimpleClothing", simpleClothing);
            Scribe_Values.Look(ref SimpleClothingMin, "SimpleClothingMin", simpleClothingMin);
            Scribe_Values.Look(ref SimpleClothingMax, "SimpleClothingMax", simpleClothingMax);
            Scribe_Values.Look(ref CourierCost, "CourierCost", courierCost);
            Scribe_Values.Look(ref EnvoyDayMultiplier, "EnvoyDayMultiplier", envoyDayMultiplier);
            Scribe_Values.Look(ref MinRelation, "MinEnvoyRelation", minRelation);
            Scribe_Values.Look(ref MaxRelation, "MaxEnvoyRelation", maxRelation);
            Scribe_Values.Look(ref r, "R", r);
            Scribe_Values.Look(ref g, "G", g);
            Scribe_Values.Look(ref b, "B", b);
        }
        internal void Reset() {
            AvailableRaces = availableRaces.Count > 0 ? availableRaces.ListFullCopy() : new List<string>();
            MinDailyCost = minDailyCost;
            MaxDailyCost = maxDailyCost;
            MinContractTime = minContractTime;
            MaxContractTime = maxContractTime;
            StayChanceHappy = stayChanceHappy;
            StayChanceNeutral = stayChanceNeutral;
            LevelOfHappinessToWork = levelOfHappinessToWork;
            LevelOfHappinessToJoin = levelOfHappinessToJoin;
            Weapons = weapons;
            SimpleClothing = simpleClothing;
            SimpleClothingMin = simpleClothingMin;
            SimpleClothingMax = simpleClothingMax;
            CourierCost = courierCost;
            EnvoyDayMultiplier = envoyDayMultiplier;
            MinRelation = minRelation;
            MaxRelation = maxRelation;
            R = 127f;
            G = 63f;
            B = 191f;
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
                if (list.ButtonText("DefaultSettings".Translate())) {
                    settings.Reset();
                };
                list.Label("MinDailyCost".Translate(settings.MinDailyCost));
                settings.MinDailyCost = (int)Mathf.Round(list.Slider(settings.MinDailyCost, 0, 100));
                list.Label("MaxDailyCost".Translate(settings.MaxDailyCost));
                settings.MaxDailyCost = (int)Mathf.Round(list.Slider(settings.MaxDailyCost, settings.MinDailyCost, 1000));
                list.Label("MinContractTime".Translate(settings.MinContractTime));
                settings.MinContractTime = (int)Mathf.Round(list.Slider(settings.MinContractTime, 1, 100));
                list.Label("MaxContractTime".Translate(settings.MaxContractTime));
                settings.MaxContractTime = (int)Mathf.Round(list.Slider(settings.MaxContractTime, 1, 100));
                list.Label("StayChanceHappy".Translate(settings.StayChanceHappy));
                settings.StayChanceHappy = (int)Mathf.Round(list.Slider(settings.StayChanceHappy, settings.StayChanceNeutral, 100f));
                list.Label("StayChanceNeutral".Translate(settings.StayChanceNeutral));
                settings.StayChanceNeutral = (int)Mathf.Round(list.Slider(settings.StayChanceNeutral, 0f, 100f));
                list.Label("LevelOfHappinessToWork".Translate(settings.LevelOfHappinessToWork));
                settings.LevelOfHappinessToWork = (byte)Mathf.Round(list.Slider(settings.LevelOfHappinessToWork, 0f, 100f));
                list.Label("LevelOfHappinessToJoin".Translate(settings.LevelOfHappinessToJoin));
                settings.LevelOfHappinessToJoin = (byte)Mathf.Round(list.Slider(settings.LevelOfHappinessToJoin, 50f, 100f));
                list.Gap();
                list.CheckboxLabeled("TenantWeaponry".Translate(), ref settings.Weapons, "TenantWeaponryDesc".Translate());
                list.Gap();
                list.CheckboxLabeled("TenantApparel".Translate(), ref settings.SimpleClothing, "TenantApparelDesc".Translate());
                if (settings.SimpleClothing) {
                    list.Gap();
                    list.Label("SimpleClothingMin".Translate(settings.SimpleClothingMin));
                    settings.SimpleClothingMin = (int)Mathf.Round(list.Slider(settings.SimpleClothingMin, 0f, 500f));
                    list.Label("SimpleClothingMax".Translate(settings.SimpleClothingMax));
                    settings.SimpleClothingMax = (int)Mathf.Round(list.Slider(settings.SimpleClothingMax, 0f, 1000f));
                }
                list.Label("CourierCost".Translate(settings.CourierCost));
                settings.CourierCost = (byte)Mathf.Round(list.Slider(settings.CourierCost, 10f, 100f));
                list.Label("EnvoyDayMultiplier".Translate(settings.EnvoyDayMultiplier));
                settings.EnvoyDayMultiplier = (byte)Mathf.Round(list.Slider(settings.EnvoyDayMultiplier, 1f, 10));
                list.Label("MinRelation".Translate(settings.MinRelation));
                settings.MinRelation = (byte)Mathf.Round(list.Slider(settings.MinRelation, 1f, 10));
                list.Label("MaxRelation".Translate(settings.MaxRelation));
                settings.MaxRelation = (byte)Mathf.Round(list.Slider(settings.MaxRelation, settings.MinRelation, 20f));
                list.Gap();
                list.GapLine();
                float R = settings.R, G = settings.G, B = settings.B;
                string buffer1 = R.ToString(), buffer2 = G.ToString(), buffer3 = B.ToString();
                list.Label("TenantRGB".Translate() +" <color=#" + ColorUtility.ToHtmlStringRGB(settings.Color) + ">" + "Color".Translate() + "</color>");
                list.TextFieldNumericLabeled("R", ref R, ref buffer1, 0, 255);
                list.TextFieldNumericLabeled("G", ref G, ref buffer2, 0, 255);
                list.TextFieldNumericLabeled("B", ref B, ref buffer3, 0, 255);
                settings.R = R;
                settings.G = G;
                settings.B = B;
                list.Gap();
                list.GapLine();
                if (settings.Races != null && settings.Races.Count() > 0) {
                    list.Label("AvailableRaces".Translate());
                    settings.Filter = list.TextEntryLabeled("Filter".Translate(), settings.Filter, 1);
                    Listing_Standard list2 = list.BeginSection(settings.RaceViewHeight);
                    list2.ColumnWidth = (rect2.width - 50) / 4;
                    foreach (ThingDef def in settings.Races) {
                        if (def.defName.ToUpper().Contains(settings.Filter.ToUpper())) {
                            bool contains = settings.AvailableRaces.Contains(def.defName);
                            list2.CheckboxLabeled(def.defName, ref contains);
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
