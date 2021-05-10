using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.UnitLogic;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Main;
using static PathfinderAutoBuff.Utility.IOHelpers;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;
using static PathfinderAutoBuff.Utility.Extensions.CommonExtensions;

namespace PathfinderAutoBuff.Menu.QueuesComponents
{
    //Action item viewer
    class ActionItemView
    {
        private readonly QueuesController queuesController;
        private readonly QueueController selectedQueue;
        private CommandQueueItem commandQueueItem;


        //Constructor
        public ActionItemView(QueuesController queuesController, CommandQueueItem commandQueueItem)
        {
            this.queuesController = queuesController;
            this.selectedQueue = queuesController.queueController;
            this.commandQueueItem = commandQueueItem;
        }

        //GUI
        public void OnGUI()
        {
            UI.Splitter(Color.grey);
            UI.BeginHorizontal("box");
            //Control Block
            UI.Vertical(() =>
            {
                UI.ActionButton(Local["Menu_Queues_Up"], () => MoveUp(), DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                UI.ActionButton(Local["Menu_Queues_Down"], () => MoveDown(), DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                UI.ActionButton(Local["Menu_Queues_Edit"], () => Edit(), DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                UI.ActionButton(Local["Menu_Queues_Delete"], () =>
                {
                    Delete();
                    return;
                }, DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
            });
            UI.Vertical(() =>
            {
                //Caster
                CasterView();
                //Availability
                AvailabilityView();

            }, "box", GUILayout.Width((float)(Math.Max(300f, ummWidth * 0.2))), GUILayout.ExpandWidth(false));
            //Target
            //Status
            //SpellName
            //Duration
            //Description
            UI.EndHorizontal();
        }

        private void CasterView()
        {
            UnitEntityData caster = commandQueueItem.GetCaster(queuesController.partySpellList);
            //Caster
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Caster"]), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
            if (commandQueueItem.CasterName == "")
            {
                if (caster != null)
                    GUILayout.Label($"Highest Duration ({caster.CharacterName})".Color(RGBA.green), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                else
                    GUILayout.Label("Highest Duration (N /A)".Color(RGBA.red), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            }
            else
            {
                if (caster != null)
                    GUILayout.Label($"{caster.CharacterName}(Available)".Color(RGBA.green), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                else
                    GUILayout.Label($"Character name (N/A)".Color(RGBA.red), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
            }
        }

        private void AvailabilityView()
        {
            GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Availableto"]), DefaultStyles.LabelFixed120());
            UnitEntityData caster = commandQueueItem.GetCaster(queuesController.partySpellList);
            BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(commandQueueItem.AbilityId);
            if (blueprintAbility == null)
            {
                GUILayout.Label(Local["Menu_Queues_NotAvailable"].Color(RGBA.red), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            }
            else
            {
                string availableCastersString = "";
                if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Spell)
                {
                    availableCastersString = String.Join(",\n", queuesController.partySpellList.GetAvailableCasters(blueprintAbility)?.Select(casterUnit => casterUnit.CharacterName));
                }
                if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Ability)
                {
                    availableCastersString = String.Join(",\n", queuesController.partyAbilityList.GetAvailableCasters(blueprintAbility));
                }
                GUILayout.Label(availableCastersString, DefaultStyles.LabelDefault());
            }
        }

        //Move action up the queue
        private void MoveUp()
        {
            this.selectedQueue.CurrentQueue().CommandList.MoveUpList(commandQueueItem);
            this.selectedQueue.Refresh();
        }

        //Move action down the queue

        private void MoveDown()
        {
            this.selectedQueue.CurrentQueue().CommandList.MoveDownList(commandQueueItem);
            this.selectedQueue.Refresh();
        }

        //Edit
        private void Edit()
        {

        }

        //Destructor
        private void Delete()
        {
            this.selectedQueue.CurrentQueue().CommandList.Remove(this.commandQueueItem);
            this.commandQueueItem = null;
            this.selectedQueue.Refresh();
        }
    }
}
