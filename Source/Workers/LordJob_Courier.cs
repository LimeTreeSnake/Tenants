using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Linq;
using System.Text;

namespace Tenants {
    public class LordJob_CourierDeliver : LordJob {
        Thing Mailbox;

        public LordJob_CourierDeliver() {

        }
        public LordJob_CourierDeliver(Thing mailbox) {
            Mailbox = mailbox;
        }
        public override StateGraph CreateGraph() {
            StateGraph StateGraph = new StateGraph();
            if (Mailbox == null) {
                Mailbox = Map.listerThings.ThingsOfDef(ThingDefOf.Tenants_MailBox).RandomElement();
            }

            LordToil toilTravel = new LordToil_Travel(Mailbox.Position) {
                useAvoidGrid = true
            };
            StateGraph.AddToil(toilTravel);
            LordToil toilDeliver = new LordToil_CourierDeliver(Mailbox);
            StateGraph.AddToil(toilDeliver);
            LordToil toilLeave = new LordToil_ExitMap() {
                useAvoidGrid = true
            };
            StateGraph.AddToil(toilLeave);

            Transition transitionWait = new Transition(toilTravel, toilDeliver);
            transitionWait.AddTrigger(new Trigger_Memo("TravelArrived"));
            StateGraph.AddTransition(transitionWait);

            Transition transitionLeave = new Transition(toilDeliver, toilLeave);
            transitionLeave.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(3000));
            StateGraph.AddTransition(transitionLeave);
            return StateGraph;
        }

        public override void ExposeData() {
            Scribe_References.Look(ref Mailbox, "Mailbox");
        }

    }
    public class LordToil_CourierDeliver : LordToil {
        readonly Thing MailBox;
        public override bool ForceHighStoryDanger => false;
        public override bool AllowSelfTend => true;
        public LordToil_CourierDeliver(Thing mailBox) {
            MailBox = mailBox;
        }
        public override void UpdateAllDuties() {
            for (int i = 0; i < lord.ownedPawns.Count; i++) {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.TravelOrWait);
            }
            if (MailBox != null && MapComponent_Tenants.GetComponent(MailBox.Map).IncomingMail.Count > 0) {
                int cost = 0, taken = 0;
                if (MapComponent_Tenants.GetComponent(MailBox.Map).CourierCost.Count > 0) {
                    foreach (Thing thing in MapComponent_Tenants.GetComponent(MailBox.Map).CourierCost) {
                        cost += thing.stackCount;
                    }
                }
                MailBox mailBoxComp = MailBox.GetMailBoxComponent();
                foreach (Thing thing in MapComponent_Tenants.GetComponent(MailBox.Map).IncomingMail) {
                    if (cost > 0) {
                        if (thing.stackCount > cost) {
                            thing.stackCount -= cost;
                            taken += cost;
                            cost = 0;
                        }
                        else {
                            cost -= thing.stackCount;
                            taken += thing.stackCount;
                            thing.stackCount = 0;
                        }
                    }
                    if (thing.stackCount > 0)
                        mailBoxComp.Items.Add(thing);
                }
                MapComponent_Tenants.GetComponent(MailBox.Map).IncomingMail.Clear();
                StringBuilder stringBuilder = new StringBuilder("");
                stringBuilder.Append("MailDelivered".Translate());
                if(taken > 0) {
                    stringBuilder.Append("CourierCost".Translate(taken));
                }
                Messages.Message(stringBuilder.ToString(), MailBox, MessageTypeDefOf.NeutralEvent);
            }
        }
    }
}
