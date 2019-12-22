using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants {
    public static class UtilityCourier {

        public static bool Courier(Map map, Building box) {
            try {
                if (!Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                    return false;
                }
                if (MapComponent_Tenants.GetComponent(map).BroadcastCourier == true) {
                    MapComponent_Tenants.GetComponent(map).BroadcastCourier = false;
                }
                if (MapComponent_Tenants.GetComponent(map).KilledCourier > 0) {
                    MapComponent_Tenants.GetComponent(map).KilledCourier--;
                    string courierDeniedLabel = "CourierDeniedTitle".Translate(map.Parent.Label);
                    string courierDeniedText = "CourierDeniedMessage".Translate();
                    Find.LetterStack.ReceiveLetter(courierDeniedLabel, courierDeniedText, LetterDefOf.NegativeEvent);
                    return true;
                }
                Pawn pawn = FindRandomCourier();
                if (pawn == null)
                    return false;
                GenSpawn.Spawn(pawn, spawnSpot, map);
                pawn.SetFaction(Faction.OfAncients);
                pawn.relations.everSeenByPlayer = true;
                CourierDress(pawn, map);
                CourierInventory(pawn, map);
                string letterLabel = "CourierArrivedTitle".Translate(map.Parent.Label);
                string letterText = "CourierArrivedMessage".Translate(pawn.Named("PAWN"));
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, pawn);
                LordMaker.MakeNewLord(pawn.Faction, new LordJob_CourierDeliver(map.listerThings.ThingsOfDef(ThingDefOf.Tenants_MailBox).RandomElement()), pawn.Map, new List<Pawn> { pawn });
                return true;
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
                return false;
            }
        }
        public static void CourierInvite(Building_CommsConsole comms, Pawn pawn) {
            if (MapComponent_Tenants.GetComponent(pawn.Map).KilledCourier > 0) {
                string courierDeniedLabel = "CourierDeniedTitle".Translate(pawn.Map.Parent.Label);
                string courierDeniedText = "CourierDeniedRadioMessage".Translate();
                Find.LetterStack.ReceiveLetter(courierDeniedLabel, courierDeniedText, LetterDefOf.NegativeEvent);
            }
            else {
                Messages.Message("CourierInvited".Translate(SettingsHelper.LatestVersion.CourierCost), MessageTypeDefOf.NeutralEvent);
                MapComponent_Tenants.GetComponent(pawn.Map).BroadcastCourier = true;
                IncidentParms parms = new IncidentParms() { target = pawn.Map, forced = true };
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.TenantCourier, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                Thing silver = ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver);
                silver.stackCount = (int)SettingsHelper.LatestVersion.CourierCost;
                MapComponent_Tenants.GetComponent(pawn.Map).CourierCost.Add(silver);

            }
        }
        public static Pawn FindRandomCourier() {
            List<Pawn> pawns = (from p in Find.WorldPawns.AllPawnsAlive
                                where p.GetCourierComponent() != null && p.GetCourierComponent().isCourier && !p.Dead && !p.Spawned && !p.Discarded
                                select p).ToList();
            if (pawns.Count < 20)
                for (int i = 0; i < 3; i++)
                    pawns.Add(GenerateNewCourier());

            if (pawns.Count == 0)
                return null;
            pawns.Shuffle();
            return pawns[0];
        }
        public static Pawn GenerateNewCourier() {
            bool generation = true;
            Pawn newCourier = null;
            while (generation) {
                newCourier = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
                if (newCourier != null && !newCourier.Dead && !newCourier.IsDessicated() && !newCourier.AnimalOrWildMan() && !newCourier.story.WorkTagIsDisabled(WorkTags.Violent)) {
                    {
                        newCourier.GetCourierComponent().isCourier = true;
                        newCourier.DestroyOrPassToWorld();
                        generation = false;
                    }
                }
            }
            return newCourier;
        }
        public static void CourierDress(Pawn pawn, Map map) {
            pawn.apparel.DestroyAll();
            ThingDef hatDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_cowboyhat");
            Thing hat = ThingMaker.MakeThing(hatDef, GenStuff.RandomStuffByCommonalityFor(hatDef));
            pawn.apparel.Wear((Apparel)hat);
            ThingDef pantsDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_flakpants");
            Thing pants = ThingMaker.MakeThing(pantsDef, GenStuff.RandomStuffByCommonalityFor(pantsDef));
            pawn.apparel.Wear((Apparel)pants);
            ThingDef shirtDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_basicshirt");
            Thing shirt = ThingMaker.MakeThing(shirtDef, GenStuff.RandomStuffByCommonalityFor(shirtDef));
            pawn.apparel.Wear((Apparel)shirt);
            if (map.mapTemperature.OutdoorTemp < 0) {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_parka");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }
            else if (map.mapTemperature.OutdoorTemp < 15) {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_jacket");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }
            else {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_duster");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }
        }
        public static void CourierInventory(Pawn pawn, Map map) {
            ThingDef bowDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "bow_recurve");
            Thing bow = ThingMaker.MakeThing(bowDef, GenStuff.RandomStuffByCommonalityFor(bowDef));
            pawn.equipment.AddEquipment((ThingWithComps)bow);
        }
        public static void EmptyMessageBox(ref List<Thing> content, IntVec3 pos) {
            if (content.Count > 0) {
                foreach (Thing thing in content) {
                    DebugThingPlaceHelper.DebugSpawn(thing.def, pos, thing.stackCount);
                }
                content.Clear();
            }
        }
        public static void RecieveLetters(ref List<Letter> content, IntVec3 pos, Map map) {
            if (content.Count > 0) {
                foreach (Letter thing in content) {
                    switch (thing.Props.letter) {
                        case LetterType.Diplomatic: {


                                break;
                            }
                        case LetterType.Mean: {


                                break;
                            }
                        case LetterType.Invite: {


                                break;
                            }
                        default:
                            break;
                    }

                }
                content.Clear();
            }
            //DO STUFF
        }
    }
}
