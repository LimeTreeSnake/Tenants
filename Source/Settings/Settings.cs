using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using System;
using Tenants.Translation;

namespace Tenants.Settings {
    public enum Style { Auto = 1, Medieval = 2, Industrial = 3 };
    internal class Settings : ModSettings {
        public static int TickFrequency = 1000;
        public static Style TextureStyle = (Style)1;
        public static List<string> AvailableRaces = new List<string>() { "Human" };
        public static IEnumerable<ThingDef> Races = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race != null && x.RaceProps.Humanlike && x.RaceProps.IsFlesh && x.RaceProps.ResolvedDietCategory != DietCategory.NeverEats).Select(s => s.race).Distinct();
        public static float RaceViewHeight = 300f;
        public static int MinDailyCost = 50;
        public static int MaxDailyCost = 100;
        public static int MinContractTime = 3;
        public static int MaxContractTime = 7;
        public static int CourierCost = 30;
        public static int MinRelation = 4;
        public static int MaxRelation = 10;
        public static Vector2 scrollPosition = Vector2.zero;
        public string Filter { get; set; }
        public static Color Color = new Color(127f, 63f, 191f);

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref TickFrequency, "TickFrequency", 1000);
            Scribe_Values.Look(ref TextureStyle, "TextureStyle", Style.Medieval);
            Scribe_Collections.Look(ref AvailableRaces, "AvailableRaces", LookMode.Value);
            Scribe_Values.Look(ref MinDailyCost, "MinDailyCost", 50);
            Scribe_Values.Look(ref MaxDailyCost, "MaxDailyCost", 100);
            Scribe_Values.Look(ref MinContractTime, "MinContractTime", 3);
            Scribe_Values.Look(ref MaxContractTime, "MaxContractTime", 7);
            Scribe_Values.Look(ref CourierCost, "CourierCost", 30);
            Scribe_Values.Look(ref MinRelation, "MinEnvoyRelation", 4);
            Scribe_Values.Look(ref MaxRelation, "MaxEnvoyRelation", 10);
        }
        public void DoWindowContents(Rect inRect) {
            try {
                inRect.yMin += 20;
                inRect.yMax -= 20;
                Listing_Standard list = new Listing_Standard();
                Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
                Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, inRect.height * 2 + RaceViewHeight);
                Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
                list.Begin(rect2);
                list.Label(Language.TickFrequency.Translate(TickFrequency));
                TickFrequency = (int)Mathf.Round(list.Slider(TickFrequency, 250, 2500));
                list.Label("Texture Style : " + TextureStyle);
                list.Label("2 = Medieval, 3 = Industrial");
                TextureStyle = (Style)Mathf.Round(list.Slider((float)TextureStyle, 2f, 3f));
                list.Label(Language.MinDailyCost.Translate(MinDailyCost));
                MinDailyCost = (int)Mathf.Round(list.Slider(MinDailyCost, 0, 100));
                list.Label(Language.MaxDailyCost.Translate(MaxDailyCost));
                MaxDailyCost = (int)Mathf.Round(list.Slider(MaxDailyCost, MinDailyCost, 1000));
                list.Label(Language.MinContractTime.Translate(MinContractTime));
                MinContractTime = (int)Mathf.Round(list.Slider(MinContractTime, 1, 100));
                list.Label(Language.MaxContractTime.Translate(MaxContractTime));
                MaxContractTime = (int)Mathf.Round(list.Slider(MaxContractTime, 1, 100));
                list.Label(Language.CourierCost.Translate(CourierCost));
                CourierCost = (byte)Mathf.Round(list.Slider(CourierCost, 10f, 100f));
                list.Label(Language.MinRelation.Translate(MinRelation));
                MinRelation = (byte)Mathf.Round(list.Slider(MinRelation, 1f, 10));
                list.Label(Language.MaxRelation.Translate(MaxRelation));
                MaxRelation = (byte)Mathf.Round(list.Slider(MaxRelation, MinRelation, 20f));
                list.Gap();
                list.GapLine();
                if (Races != null && Races.Count() > 0) {
                    list.Label(Language.AvailableRaces.Translate());
                    Filter = list.TextEntryLabeled(Language.Filter.Translate(), Filter, 1);
                    Listing_Standard list2 = list.BeginSection(RaceViewHeight);
                    list2.ColumnWidth = (rect2.width - 50) / 4;
                    foreach (ThingDef def in Races) {
                        if (def.defName.ToUpper().Contains(Filter.ToUpper())) {
                            bool contains = AvailableRaces.Contains(def.defName);
                            list2.CheckboxLabeled(def.defName, ref contains);
                            if (contains == false && AvailableRaces.Contains(def.defName)) {
                                AvailableRaces.Remove(def.defName);
                            }
                            else if (contains == true && !AvailableRaces.Contains(def.defName)) {
                                AvailableRaces.Add(def.defName);
                            }
                        }
                    }
                    list.EndSection(list2);
                }
                list.End();
                Widgets.EndScrollView();
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
            }
        }
    }
    internal static class SettingsHelper {
        public static Settings LatestVersion;
    }
    public class ModMain : Mod {
        private Settings settings;
        public ModMain(ModContentPack content) : base(content) {
            settings = GetSettings<Settings>();
            SettingsHelper.LatestVersion = settings;
        }
        public override string SettingsCategory() {
            return "Tenants";
        }


        public override void DoSettingsWindowContents(Rect inRect) {
            settings.DoWindowContents(inRect);
        }
    }
}