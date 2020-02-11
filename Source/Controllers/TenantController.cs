using RimWorld;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Verse;
using Tenants.Utilities;

namespace Tenants.Controllers {
    public static class TenantController {
        public static void Tick(Pawn tenant, TenantComp comp) {
            //Tenant alone with no colonist
            if (tenant.Map.mapPawns.FreeColonists.FirstOrDefault(x => ThingCompUtility.TryGetComp<TenantComp>(x)?.Tenancy == TenancyType.None) == null) {
                comp.Contract.IsTerminated = true;
                ContractConclusion(tenant, comp, 0.75f);
                return;
            }
            if (comp.Contract.IsTerminated) {
                Find.LetterStack.ReceiveLetter("ContractBreach".Translate(), "ContractDoneTerminated".Translate(tenant.Named("PAWN")), LetterDefOf.NeutralEvent);
                TenantUtilities.Leave(tenant, comp);
                return;
            }
            //Tenant contract is out
            if (Find.TickManager.TicksGame >= comp.Contract.ContractEndTick) {
                ContractConclusion(tenant, comp);
                return;
            }
            //Tenancy tick 1/10 per day
            if (Find.TickManager.TicksGame % 60000 == 0) {
                //Join
                if (tenant.needs.mood.CurInstantLevel > 0.8f && Rand.Bool) {
                    TenantWantToJoin(tenant, comp);
                    return;
                }
            }
        }
        public static void Contract(Map map) {
            if (!MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return;
            }
            Pawn tenant = TenantUtilities.CreateContractedPawn(out TenantComp comp);
            StringBuilder stringBuilder = new StringBuilder("");
            if (TenantsMapComp.GetComponent(map).Broadcast) {
                TenantsMapComp.GetComponent(map).Broadcast = false;
                stringBuilder.Append("TenancyOpportunity".Translate(tenant.Named("PAWN")));
            }
            else {
                stringBuilder.Append("TenancyInitial".Translate(tenant.Named("PAWN")));
            }
            stringBuilder.Append(GenerateContractMessage(tenant, comp));
            GenerateContractDialogue("TenancyTitle".Translate(map.Parent.Label), stringBuilder.ToString(), tenant, comp, map, spawnSpot);
        }
        public static void ContractConclusion(Pawn tenant, TenantComp comp, float stealChance = 0.5f) {
            if (comp.Contract.IsTerminated) {
                if (Rand.Value > stealChance) {
                    ContractUtility.ContractPayment(comp.Contract, tenant.Map);
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTerminated".Translate(comp.Contract.Payment * comp.Contract.ContractLength / 60000, tenant.Named("PAWN")), LetterDefOf.NeutralEvent);
                    TenantUtilities.Leave(tenant, comp);
                }
                else {
                    Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDoneTheft".Translate(tenant.Named("PAWN")), LetterDefOf.NegativeEvent);
                    TenantUtilities.Theft(tenant, comp);
                }
                return;
            }
            else {
                ContractUtility.ContractPayment(comp.Contract, tenant.Map);
                Find.LetterStack.ReceiveLetter("ContractEnd".Translate(), "ContractDone".Translate(comp.Contract.Payment * comp.Contract.ContractLength / 60000, tenant.Named("PAWN")), LetterDefOf.PositiveEvent);
                if (comp.Contract.AutoRenew) {
                    string letterLabel = "ContractNew".Translate();
                    string letterText = "ContractRenewedMessage".Translate(tenant.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                }
                else {
                    StringBuilder stringBuilder = new StringBuilder("");
                    stringBuilder.Append("TenancyContinued".Translate(tenant.Named("PAWN")));
                    stringBuilder.Append(GenerateContractMessage(tenant, comp));
                    GenerateContractDialogue("TenancyTitle".Translate(tenant.Map.Parent.Label), stringBuilder.ToString(), tenant, comp);
                }
            }
        }
        public static void TenantWantToJoin(Pawn pawn, TenantComp comp) {
            TenantComp tenantComp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (tenantComp.MayJoin) {
                string text = "RequestWantToJoin".Translate(pawn.Named("PAWN"));

                DiaNode diaNode = new DiaNode(text);
                DiaOption diaOption = new DiaOption("ContractAgree".Translate()) {
                    action = delegate {
                        ContractUtility.ContractPayment(comp.Contract, pawn.Map);
                        Messages.Message("ContractDone".Translate(pawn.Name.ToStringFull, comp.Contract.Payment * comp.Contract.ContractLength / 60000, pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                        Find.ColonistBar.MarkColonistsDirty();
                    },
                    resolveTree = true,
                };
                diaNode.options.Add(diaOption);
                //Denied offer
                string text2 = "RequestWantToJoinRejected".Translate(pawn.Named("PAWN"));
                DiaNode diaNode2 = new DiaNode(text2);
                DiaOption diaOption2 = new DiaOption("OK".Translate()) {
                    resolveTree = true
                };
                diaNode2.options.Add(diaOption2);
                DiaOption diaOption3 = new DiaOption("ContractReject".Translate()) {
                    action = delegate {
                    },
                    link = diaNode2
                };
                diaNode.options.Add(diaOption3);
                string title = "RequestFromTenant".Translate(pawn.Map.Parent.Label);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: true, title));
                Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
            }
        }
        public static void Invite(Building_CommsConsole comms, Pawn pawn) {
            Messages.Message("TenantInviteMessage".Translate(), MessageTypeDefOf.NeutralEvent);
            TenantsMapComp.GetComponent(pawn.Map).Broadcast = true;
            if (Rand.Value < 0.20f) {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.Opportunists, Find.TickManager.TicksGame + Rand.Range(25000, 150000), parms, 240000);
            }
            else {
                IncidentParms parms = new IncidentParms() { target = pawn.Map, forced = true };
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.TenantProposition, Find.TickManager.TicksGame + Rand.Range(15000, 120000), parms, 240000);
            }
        }
        public static void GenerateContractDialogue(string title, string text, Pawn tenant, TenantComp comp, Map map = null, IntVec3? spawnSpot = null) {
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOptionAgree = new DiaOption("ContractAgree".Translate()) {
                action = delegate {
                    if (tenant.Spawned)
                        ContractUtility.ContractProlong(comp.Contract);
                    else
                        TenantUtilities.SpawnTenant(tenant, map, spawnSpot.Value);
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOptionAgree);
            DiaOption diaOptionReject = new DiaOption("ContractReject".Translate()) {
                action = delegate {
                    if (tenant.Spawned)
                        TenantUtilities.Leave(tenant, comp);
                    else
                        TenantUtilities.CleanComp(comp);
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOptionReject);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: false, radioMode: true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
        }
        private static string GenerateContractDetails(Pawn pawn, TenantComp comp) {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.Append("TenancyContract".Translate(comp.Contract.ContractLength / 60000, comp.Contract.Payment, pawn.Named("PAWN")));

            return stringBuilder.ToString();
        }
        public static string GenerateContractMessage(Pawn pawn, TenantComp comp) {
            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append(GenerateContractDetails(pawn, comp));
            stringBuilder.Append(GeneratePawnDescription(pawn));
            string text = "";
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
            stringBuilder.Append(text);
            text = stringBuilder.ToString();
            return text.AdjustedFor(pawn);
        }
        private static string GeneratePawnDescription(Pawn pawn) {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("ContractTenantDescription".Translate(pawn.ageTracker.AgeBiologicalYears, pawn.def.label, pawn.Named("PAWN")));
            stringBuilder.AppendLine();
            stringBuilder.Append("Traits".Translate() + ": ");
            if (pawn.story.traits.allTraits.Count == 0) {
                stringBuilder.AppendLine();
                stringBuilder.Append("(" + "NoneLower".Translate() + ")");
            }
            else {
                stringBuilder.Append("(");
                for (int i = 0; i < pawn.story.traits.allTraits.Count; i++) {
                    if (i != 0) {
                        stringBuilder.Append(" ,");
                    }
                    stringBuilder.Append(pawn.story.traits.allTraits[i].LabelCap);
                }
                stringBuilder.Append(")");
            }
            return stringBuilder.ToString();
        }
    }
}
