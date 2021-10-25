using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using static PathfinderAutoBuff.Main;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;
using static PathfinderAutoBuff.Utility.Extensions.CommonExtensions;
using PathfinderAutoBuff.QueueOperations;
using PathfinderAutoBuff.UnitLogic;

namespace PathfinderAutoBuff.Menu.QueuesComponents
{
    internal class AbilityList
    /*
    * Ability list component for ability editing
    * Used to show&select a list of abilities/spells in Queues menu
    * Supports multiselect
    * Selected items highlighted with a different background/coolored box?
    * 
    * View structure:
    * Select button
    * Ability name
    * Debug ability GUID
    * Available to casters
    * Ability description
    * ?Total uses in party?
    */
    {
        private static GUIStyle buttonEmpty;
        private static bool styleInit = false;

        public static void OnGUI (
            ref List<string> selectedAbilties,
            CommandQueueItem.ActionTypes actionType,                    //Action type for ability/spell/activatable selection
            ref List<DataDescriptionInterface> dataDescriptionList,                     //Filtered action IDs
            bool multiSelect = false                                    //MulutiSelection flag
            )
        {
            GUIStyle selectedBackground = BackgroundStyledBox.Get(RGBAToColor((uint)RGBA.midnightblue));
            GUIStyle defaultBackground = new GUIStyle("box");
            if (!styleInit)
            {
                buttonEmpty = DefaultStyles.ButtonEmpty();
                styleInit = true;
            }
            int defaultFontSize = UnityEngine.GUI.skin.label.fontSize;
            UnityEngine.GUI.skin.label.wordWrap = true;
            //Action description for title section
            string actionDescription;
            switch(actionType)
            {
                case CommandQueueItem.ActionTypes.Ability:
                    actionDescription = "Ability";
                    break;
                case CommandQueueItem.ActionTypes.Spell:
                    actionDescription = "Spell";
                    break;
                case CommandQueueItem.ActionTypes.Activatable:
                    actionDescription = "Activatable";
                    break;
                default:
                    actionDescription = "Spell";
                    break;
            }
            //Title section
            UI.BeginHorizontal();

            UI.Label(DefaultStyles.TextHeader2("Select"), GUILayout.Width(50f * UIScale));
            UI.Label(DefaultStyles.TextHeader2($"{actionDescription} name"), GUILayout.Width(200f * UIScale));
#if (DEBUG)
//            UI.Label(DefaultStyles.TextHeader2($"{actionDescription} GUID"), GUILayout.Width(240f * UIScale));
#endif
            UI.Label(DefaultStyles.TextHeader2("Available to"), GUILayout.Width(120f * UIScale));
            UI.Label(DefaultStyles.TextHeader2($"{actionDescription} description"));
            UI.EndHorizontal();
            //Add abilities that are not available to the party
            foreach (string selectedAbility in selectedAbilties)
            {
                if (!dataDescriptionList.Select(data => data.AbilityID).Contains(selectedAbility))
                {
                    if (actionType == CommandQueueItem.ActionTypes.Activatable)
                    {
                        BlueprintActivatableAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>(selectedAbility);
                        if (blueprintAbility != null)
                        {
#if (WOTR)
                            string abilityId = blueprintAbility.AssetGuid.ToString();
#elif (KINGMAKER)
                            string abilityId = blueprintAbility.AssetGuid;
#endif
                            GenericDataDescription genericDataDescription = new GenericDataDescription(
                                abilityId,
                                blueprintAbility.Name,
                                blueprintAbility.Description,
                                new List<string>()
                                );
                            dataDescriptionList.Insert(0, genericDataDescription);
                        }
                    }
                    else
                    {
                        BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(selectedAbility);
                        if (blueprintAbility != null)
                        {
#if (WOTR)
                            string abilityId = blueprintAbility.AssetGuid.ToString();
#elif (KINGMAKER)
                            string abilityId = blueprintAbility.AssetGuid;
#endif
                            GenericDataDescription genericDataDescription = new GenericDataDescription(
                                abilityId,
                                blueprintAbility.Name,
                                blueprintAbility.Description,
                                new List<string>()
                                );
                            dataDescriptionList.Insert(0, genericDataDescription);
                        }

                    }
                }
            }
            //Content section
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            foreach (DataDescriptionInterface dataDescription in dataDescriptionList)
            {
                //Set background and selector button symbol
                GUIStyle currentBackground;
                string selectorButtonText;
                if (selectedAbilties.Contains(dataDescription.AbilityID))
                {
                    currentBackground = selectedBackground;
                    selectorButtonText = "-".Red().Bold().Size((int)(30 * UIScale));
                }
                else
                {
                    currentBackground = defaultBackground;
                    selectorButtonText = "+".Green().Bold().Size((int)(30 * UIScale));
                }
                UI.Splitter(Color.grey);
                UI.BeginHorizontal(currentBackground, GUILayout.ExpandHeight(true));
                //Selector button
                if (GUILayout.Button(selectorButtonText, buttonEmpty, GUILayout.ExpandHeight(true),GUILayout.Width(50f*UIScale)))
                {
                    if (selectedAbilties.Contains(dataDescription.AbilityID))
                    {
                        selectedAbilties.Remove(dataDescription.AbilityID);
                    }
                    else
                    {
                        if (multiSelect)
                            selectedAbilties.Add(dataDescription.AbilityID);
                        else
                        {
                            selectedAbilties.Clear();
                            selectedAbilties.Add(dataDescription.AbilityID);
                        }
                    }
                }
                //Ability Name
                string abilityNameText = dataDescription.SourceItem == null ? ($"{dataDescription.AbilityName} from {dataDescription.SourceItem}") : dataDescription.AbilityName; ;
                UI.Label(dataDescription.AbilityName.RemoveHtmlTags(), GUILayout.Width(200f * UIScale));
#if (DEBUG)
                //Ability ID
 //               UI.Label(dataDescription.AbilityID, GUILayout.Width(240f * UIScale));
#endif
                //Available to
                string availableCastersString = (dataDescription.Casters.Count > 0) ? String.Join(",\n", dataDescription.Casters) : "None in filtered".Yellow();
                UI.Label(availableCastersString, GUILayout.Width(120f * UIScale));
                //Ability Description
                float remainingWidth = Main.ummWidth - (50f + 200f + 120f) * UIScale;
                UI.Label(dataDescription.AbilityDescription.RemoveHtmlTags(),GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
                //TestButton
                if (GUILayout.Button("", GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandHeight(true)))
                {
                    Logger.Debug("test");
                }
                UI.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}
