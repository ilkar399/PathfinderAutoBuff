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
                UI.Space(25f);
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
                //Ability name and status
                AbilityNameStatusView();
            }, GUILayout.Width((float)(Math.Max(250f, ummWidth * 0.15))), GUILayout.ExpandWidth(false));
            UI.Vertical(() =>
            {
                //Caster
                CasterView();
                //Availability
                AvailabilityView();

            }, GUILayout.Width((float)(Math.Max(250f, ummWidth * 0.2))), GUILayout.ExpandWidth(false));
            UI.Vertical(() =>
            {
                //Target
                TargetView();

            }, GUILayout.Width((float)(Math.Max(200, ummWidth * 0.15))), GUILayout.ExpandWidth(false));
            UI.Vertical(() =>
            {
                //Ability description and duration
                AbilityDescriptionView();
            }, GUILayout.ExpandWidth(true));
            //Description
            //Duration
            UI.EndHorizontal();
        }

        private void AbilityDescriptionView()
        {
            if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Spell)
            {
                //Spell description
                UnitEntityData caster = commandQueueItem.GetCaster(queuesController.partySpellList);
                BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(commandQueueItem.AbilityId);
                string spellDescription = blueprintAbility.Description;
                PartySpellData memorizedSpellData = queuesController.partySpellList.GetMemorizedSpellData(caster, blueprintAbility);
                string spellDuration = "";
                if (memorizedSpellData != null)
                {
                    spellDuration = PartySpellList.GetSpellDuration(memorizedSpellData).Seconds.ToString();
                }
                else
                {
                    spellDuration = Local["Menu_Queues_NotMemorized"].Color(RGBA.red);
                }
                GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Description"]), DefaultStyles.LabelFixed120());
                GUILayout.Label(spellDescription.RemoveHtmlTags(), DefaultStyles.LabelDefault());
                GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Duration"]), DefaultStyles.LabelFixed120());
                GUILayout.Label(spellDuration, DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
            }
            else if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Ability)
            {
                //TODO duration, casters
                //Ability description
                BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(commandQueueItem.AbilityId);
                string abilityDescription = blueprintAbility.Description;
                GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Description"]), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                GUILayout.Label(abilityDescription.RemoveHtmlTags(), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
            }
            //Mods
            if (commandQueueItem.AbilityMods != null)
            {
                GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_PreUseAbilities"]));
                foreach (string abilityID in commandQueueItem.AbilityMods)
                {
                    AbilityDataUI abilityData = queuesController.partyAbilityList.GetAbility(abilityID);
                    if (abilityData.Casters != null)
                        GUILayout.Label(abilityData.AbilityName.Color(RGBA.green));
                    else
                        GUILayout.Label(abilityData.AbilityName.Color(RGBA.red));
                }
            }
            if (commandQueueItem.ActivatableMods != null)
            {
                GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_ToggleAbilities"]));
                foreach (string abilityID in commandQueueItem.ActivatableMods)
                {
                    ActivatableDataUI abilityData = queuesController.partyActivatableList.GetActivatable(abilityID);
                    if (abilityData.Casters != null)
                        GUILayout.Label(abilityData.AbilityName.Color(RGBA.green));
                    else
                        GUILayout.Label(abilityData.AbilityName.Color(RGBA.red));
                }
            }
        }

        //Ability name and status
        private void AbilityNameStatusView()
        {

            string abilityName = commandQueueItem.GetAbilityName();
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Spell"]), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            if (abilityName == "")
                GUILayout.Label(Local["Menu_Queues_NotAvailable"].Color(RGBA.red), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            else
                GUILayout.Label(abilityName.RemoveHtmlTags().Color(RGBA.green).Bold(), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Status"]), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            IEnumerable<string> actionStatus = commandQueueItem.GetStatus(queuesController.partySpellList);
            GUILayout.Label(StatusStyled(actionStatus, string.Join(";\n", actionStatus)));
        }

        //Target determination and view
        private void TargetView()
        {
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Target"]), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            switch (commandQueueItem.TargetType)
            {
                case CommandQueueItem.TargetTypes.Self:
                    GUILayout.Label("Self".Color(RGBA.green), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                    break;
                case CommandQueueItem.TargetTypes.Positions:
                    GUILayout.Label("(Formation positions:)".Italic(), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                    if (commandQueueItem.Positions != null)
                        foreach (int position in commandQueueItem.Positions)
                        {
                            UnitEntityData target = Target.GetTarget(position);
                            if (target != null)
                            {
                                GUILayout.Label($"#{position} ({target.CharacterName.Color(RGBA.green)})",
                                    DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                            }
                            else
                            {
                                GUILayout.Label($"#{position} (" + "N/A".Color(RGBA.red) + ")"
                                    , DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                            }
                        }
                    break;
                case CommandQueueItem.TargetTypes.CharacterNames:
                    GUILayout.Label("(Character names):".Italic(), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                    if (commandQueueItem.CharacterNames != null)
                    {
                        foreach (string characterName in commandQueueItem.CharacterNames)
                        {
                            List<UnitEntityData> targets = Target.GetTarget(characterName);
                            if (targets.Count > 0)
                            {
                                GUILayout.Label($"{characterName.Color(RGBA.green)}",
                                    DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                            }
                            else
                            {
                                GUILayout.Label($"{characterName.Color(RGBA.red)}"
                                    , DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                            }
                        }
                    }
                    if (commandQueueItem.PetIndex != null)
                    {
                        foreach (string characterName in commandQueueItem.PetIndex.Keys)
                        {
                            foreach (int petIndex in commandQueueItem.PetIndex[characterName])
                            {
                                UnitEntityData target = Target.GetTargetPet(characterName, petIndex);
                                if (target != null)
                                {
                                    GUILayout.Label($"{characterName} pet ({target.CharacterName})".Color(RGBA.green),
                                        DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                                }
                                else
                                {
                                    GUILayout.Label($"#{characterName} pet (" + "N/A".Color(RGBA.red) + ")"
                                        , DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                                }
                            }
                        }
                    }
                    break;
            }
        }

        //Caster determination and view
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


        //Spell availability to casters view
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
        //TODO
        private void Edit()
        {

        }

        //Destructor
        private void Delete()
        {
            this.selectedQueue.CurrentQueue().CommandList.Remove(this.commandQueueItem);
 //           this.commandQueueItem = null;
            this.selectedQueue.Refresh();
        }

        //Get returned styled status string from the text status
        private string StatusStyled(IEnumerable<string> textStatus, string capition)
        {
            if (textStatus.Count() > 0)
            {
                if (textStatus.Contains(Local["Menu_Queues_StatusNoErrors"]))
                    return capition.Color(RGBA.green);
                if (textStatus.Contains(Local["Menu_Queues_StatusNoCasterName"])
                    || textStatus.Contains(Local["Menu_Queues_StatusNoCasterSpell"])
                    || textStatus.Contains(Local["Menu_Queues_StatusFatal"])
                    || textStatus.Contains(Local["Menu_Queues_StatusNoTargets"]))
                    return ("(!!)" + capition).Color(RGBA.red);
                if (textStatus.Contains(Local["Menu_Queues_StatusNotMemorized"])
                    || textStatus.Contains(Local["Menu_Queues_StatusNotAllTargets"])
                    || textStatus.Contains(Local["Menu_Queues_StatusNoAbilityResources"]))
                    return ("(!)" + capition).Color(RGBA.yellow);
                return capition.Color(RGBA.red);
            }
            else
                return capition.Color(RGBA.green);
        }
    }
}
