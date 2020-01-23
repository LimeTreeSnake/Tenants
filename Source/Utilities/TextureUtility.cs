using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using UnityEngine;
using Verse;

namespace Tenants.Utilities {
    public static class TextureUtility {
        public static Texture2D ContractIcon => ContentFinder<Texture2D>.Get("LTS/Icons/Contract");
        public static Texture2D EnvoyIcon => ContentFinder<Texture2D>.Get("LTS/Icons/Envoy");

        public static Graphic GraphicFinder(GraphicAlternatorComp graphicAlternator, EraAlternatorComp eraAlternator, bool useAlternate, Thing thing) {
            if (useAlternate) {
                switch (Settings.SettingsHelper.LatestVersion.TextureStyle) {
                    case Settings.Style.Auto: {
                            if (eraAlternator != null && Find.FactionManager.OfPlayer.def.techLevel <= eraAlternator.Props.TechLevel) {
                                return eraAlternator.Props.Texture.GraphicColoredFor(thing);
                            }
                            return graphicAlternator.Props.Texture.GraphicColoredFor(thing);
                        }
                    case Settings.Style.LTS: {
                            if (eraAlternator != null) {
                                return eraAlternator.Props.Texture.GraphicColoredFor(thing);
                            }
                            return graphicAlternator.Props.Texture.GraphicColoredFor(thing);
                        }
                    case Settings.Style.Oskar:
                        return graphicAlternator.Props.Texture.GraphicColoredFor(thing);
                    default:
                        return graphicAlternator.Props.Texture.GraphicColoredFor(thing);

                }
            }
            else {
                switch (Settings.SettingsHelper.LatestVersion.TextureStyle) {
                    case Settings.Style.Auto: {
                            if (eraAlternator != null && Find.FactionManager.OfPlayer.def.techLevel <= eraAlternator.Props.TechLevel) {
                                return eraAlternator.Props.TextureAlternate.GraphicColoredFor(thing);
                            }
                            return graphicAlternator.Props.TextureAlternate.GraphicColoredFor(thing);
                        }
                    case Settings.Style.LTS: {
                            if (eraAlternator != null) {
                                return eraAlternator.Props.TextureAlternate.GraphicColoredFor(thing);
                            }
                            return graphicAlternator.Props.TextureAlternate.GraphicColoredFor(thing);
                        }
                    case Settings.Style.Oskar:
                        return graphicAlternator.Props.TextureAlternate.GraphicColoredFor(thing);
                    default:
                        return graphicAlternator.Props.TextureAlternate.GraphicColoredFor(thing);
                }

            }
        }
    }
