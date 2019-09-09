using Verse;
using UnityEngine;
using System.Collections.Generic;

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
        private static readonly bool weapons = false;
        private static float r = 127f, g = 63f, b = 191f;
        private static Color color = new Color(r / 255f, g / 255f, b / 255f);
        private static readonly float levelOfHappinessToWork = 70f;
        private static readonly bool cleaning = true, cleaningHappy = true;
        private static readonly bool hauling = true, haulingHappy = true;
        private static readonly bool basicWorker = true, basicWorkerHappy = true;
        private static readonly bool patient = true, patientHappy = false;
        private static readonly bool patientBedRest = true, patientBedRestHappy = false;
        private static readonly bool firefighter = true, firefighterHappy = true;
        private static readonly bool workIsDirty = true;
        //private static readonly bool mechanoids = false;
        //private static readonly bool insectoids = false;
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
        public float R { get { return r; } set { r = value; color = new Color(r / 255, g / 255, b / 255); } }
        public float G { get { return g; } set { g = value; color = new Color(r / 255, g / 255, b / 255); } }
        public float B { get { return b; } set { b = value; color = new Color(r / 255, g / 255, b / 255); } }
        public Color Color => color;
        public float LevelOfHappinessToWork = levelOfHappinessToWork;
        public bool Cleaning = cleaning;
        public bool CleaningHappy = cleaningHappy;
        public bool Hauling = hauling;
        public bool HaulingHappy = haulingHappy;
        public bool BasicWorker = basicWorker;
        public bool BasicWorkerHappy = basicWorkerHappy;
        public bool Patient = patient;
        public bool PatientHappy = patientHappy;
        public bool PatientBedRest = patientBedRest;
        public bool PatientBedRestHappy = patientBedRestHappy;
        public bool Firefighter = firefighter;
        public bool FirefighterHappy = firefighterHappy;
        //public bool Mechanoids = mechanoids;
        //public bool Insectoids = insectoids;

        public bool WorkIsDirty = workIsDirty;
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
            Scribe_Values.Look(ref r, "R", r);
            Scribe_Values.Look(ref g, "G", g);
            Scribe_Values.Look(ref b, "B", b);
            Scribe_Values.Look(ref LevelOfHappinessToWork, "LevelOfHappinessToWork", levelOfHappinessToWork);
            Scribe_Values.Look(ref Cleaning, "Cleaning", cleaning);
            Scribe_Values.Look(ref CleaningHappy, "CleaningHappy", cleaningHappy);
            Scribe_Values.Look(ref Hauling, "Hauling", hauling);
            Scribe_Values.Look(ref HaulingHappy, "HaulingHappy", haulingHappy);
            Scribe_Values.Look(ref BasicWorker, "BasicWorker", basicWorker);
            Scribe_Values.Look(ref BasicWorkerHappy, "BasicWorkerHappy", basicWorkerHappy);
            Scribe_Values.Look(ref Patient, "Patient", patient);
            Scribe_Values.Look(ref PatientHappy, "PatientHappy", patientHappy);
            Scribe_Values.Look(ref PatientBedRest, "PatientBedRest", patientBedRest);
            Scribe_Values.Look(ref PatientBedRestHappy, "PatientBedRestHappy", patientBedRestHappy);
            Scribe_Values.Look(ref Firefighter, "Firefighter", firefighter);
            Scribe_Values.Look(ref FirefighterHappy, "FirefighterHappy", firefighterHappy);
            Scribe_Values.Look(ref Weapons, "Weapons", weapons);
            //Scribe_Values.Look(ref Mechanoids, "Mechanoids", mechanoids);
            //Scribe_Values.Look(ref Insectoids, "Insectoids", insectoids);
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
            Cleaning = cleaning;
            CleaningHappy = cleaningHappy;
            Hauling = hauling;
            HaulingHappy = haulingHappy;
            BasicWorker = basicWorker;
            BasicWorkerHappy = basicWorkerHappy;
            Patient = patient;
            PatientHappy = patientHappy;
            PatientBedRest = patientBedRest;
            PatientBedRestHappy = patientBedRestHappy;
            Firefighter = firefighter;
            FirefighterHappy = firefighterHappy;
            Weapons = weapons;
            //Mechanoids = mechanoids;
            //Insectoids = insectoids;
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
            tenantsSettings.MinDailyCost = (int)Mathf.Round(list.Slider(tenantsSettings.MinDailyCost, 50, 100));
            list.Label(string.Format("({0}) Max contract daily cost.", tenantsSettings.MaxDailyCost));
            tenantsSettings.MaxDailyCost = (int)Mathf.Round(list.Slider(tenantsSettings.MaxDailyCost, tenantsSettings.MinDailyCost, 10 * tenantsSettings.MinDailyCost));
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
            //list.CheckboxLabeled("Should mechanoids be tenants?", ref tenantsSettings.Mechanoids, "This only makes sure that intelligent mechanoids that are considered humanlike, can use tools and eat food are tenants.");
            //list.Gap();
            //list.CheckboxLabeled("Should insectoids be tenants?", ref tenantsSettings.Insectoids, "This only makes sure that intelligent insectoids that are considered humanlike, can use tools and eat food are tenants.");
            //list.Gap();
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
            list.CheckboxLabeled("Cleaning", ref tenantsSettings.Cleaning, "Should a tenant clean?");
            if (tenantsSettings.Cleaning)
                list.CheckboxLabeled("Clean only when happy", ref tenantsSettings.CleaningHappy);
            list.Gap();
            list.CheckboxLabeled("Hauling", ref tenantsSettings.Hauling, "Should a tenant haul?");
            if (tenantsSettings.Hauling)
                list.CheckboxLabeled("Hauling only when happy", ref tenantsSettings.HaulingHappy);
            list.Gap();
            list.CheckboxLabeled("Basic working", ref tenantsSettings.BasicWorker, "Should a tenant do basic work like flicking?");
            if (tenantsSettings.BasicWorker)
                list.CheckboxLabeled("Basic working only when happy", ref tenantsSettings.BasicWorkerHappy);
            list.Gap();
            list.CheckboxLabeled("Patient", ref tenantsSettings.Patient, "Should a tenant wait to be treated in bed?");
            if (tenantsSettings.Patient)
                list.CheckboxLabeled("Patient only when happy", ref tenantsSettings.PatientHappy);
            list.Gap();
            list.CheckboxLabeled("PatientBedRest", ref tenantsSettings.PatientBedRest, "Should a tenant rest when hurt or sick?");
            if (tenantsSettings.PatientBedRest)
                list.CheckboxLabeled("PatientBedRest only when happy", ref tenantsSettings.PatientBedRestHappy);
            list.Gap();
            list.CheckboxLabeled("Firefighter", ref tenantsSettings.Firefighter, "Should a tenant help beating down fires?");
            if (tenantsSettings.Firefighter)
                list.CheckboxLabeled("Firefighter only when happy", ref tenantsSettings.FirefighterHappy);
            tenantsSettings.WorkIsDirty = true;
                        
            list.End();



            Widgets.EndScrollView();
            tenantsSettings.Write();
        }
    }
}
