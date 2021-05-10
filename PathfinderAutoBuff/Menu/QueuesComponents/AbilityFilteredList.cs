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
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.UnitLogic;

namespace PathfinderAutoBuff.Menu.QueuesComponents
{
    internal class AbilityFilteredList
        /*
         * The parent component that combines Filters and Lists
         * Made static since it's heavily reusable within the same interface page. 
         * TODO:
         * Think about the component reset/cleanups
         */

    {
        public static List<QueuesComponents.AbilityFilter> AllFilters { get; private set; }
        public static HashSet<QueuesComponents.AbilityFilter> EnabledFilters { get; private set; }
        private static List<string> m_SelectedAbilities;
        private static List<DataDescriptionInterface> m_CombinedAbilitiesData;
        private static CommandQueueItem.ActionTypes m_ActionType;
        private static bool m_AllowMultiSelectAbilities;
        private static bool m_AllowMultiSelectFilters;
        private static GUIStyle buttonFixed120;
        private static bool styleInit = false;
        private static int filterIndex;

        public static void OnGUI()
        {
            if (AllFilters.Count < 1)
                return;
            if (!styleInit)
            {
                buttonFixed120 = DefaultStyles.ButtonFixed120();
                styleInit = true;
            }
            //Filter buttons
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (filterIndex = 0; filterIndex < AllFilters.Count(); filterIndex++)
            {
                //TODO: Remake this part
                Utility.UI.ToggleButton(EnabledFilters.Contains(AllFilters[filterIndex]),
                    AllFilters[filterIndex].Name,
                    () =>
                    {
                        if (!m_AllowMultiSelectFilters)
                            EnabledFilters.Clear();
                        EnabledFilters.Add(AllFilters[filterIndex]);
                        UpdateAbilitiesData();
                    },
                    () =>
                    {
                        EnabledFilters.Remove(AllFilters[filterIndex]);
                        UpdateAbilitiesData();
                    },
                    buttonFixed120
                    );
                if ((filterIndex % 5) == 4)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            //Actions list
            AbilityList.OnGUI(ref m_SelectedAbilities, m_ActionType, ref m_CombinedAbilitiesData, m_AllowMultiSelectAbilities);
        }

        //Update ability data
        private static void UpdateAbilitiesData()
        {
            m_CombinedAbilitiesData = new List<DataDescriptionInterface>();
            foreach (AbilityFilter afc in EnabledFilters)
            {
                afc.Update();
                m_CombinedAbilitiesData.AddRange(afc.FilteredData);
            }
            m_CombinedAbilitiesData = m_CombinedAbilitiesData.OrderBy(abilityData => abilityData.AbilityName).ToList();
//            AbilityList.UpdateUI();
        }

        //Init the filter depending on the action type
        public static void Init(ref List<string> selectedAbilties,
            QueuesController queueController,
            CommandQueueItem.ActionTypes actionType,
            bool allowMultiselectFilters,
            bool allowMultiSelectActions,
            string casterName = "")
        {
            m_SelectedAbilities = selectedAbilties;
            AllFilters = new List<AbilityFilter>();
            m_CombinedAbilitiesData = new List<DataDescriptionInterface>();
            m_AllowMultiSelectAbilities = allowMultiSelectActions;
            m_AllowMultiSelectFilters = allowMultiselectFilters;
            m_ActionType = actionType;
            //Spell filters
            if (actionType == CommandQueueItem.ActionTypes.Spell)
            {
                string casterCaption = casterName == "" ? "All" : casterName;
                for (int i = 1; i < 10; i++)
                {
                    AllFilters.Add(new AbilityFilter($"{casterCaption} - {i}",
                    casterName,
                    i,
                    (caster, level) =>
                    PartySpellList.SpellDataUIFilterLevels(queueController.partySpellList, caster, level)
                    ));
                }
            }
            //Ability filters
            if (actionType == CommandQueueItem.ActionTypes.Ability)
            {
                AllFilters.Add(new AbilityFilter("All abilities",
                "",
                0,
                (caster, level) =>
                PartyAbilityList.AbilityDataUIFilter(queueController.partyAbilityList, caster, level)
                ));
                if (casterName != "")
                {
                    AllFilters.Add(new AbilityFilter($"{casterName} Abilities",
                    casterName,
                    0,
                    (caster, level) =>
                    PartyAbilityList.AbilityDataUIFilter(queueController.partyAbilityList, caster, level)
                    ));
                }
            }
            //Activatable filters
            if (actionType == CommandQueueItem.ActionTypes.Activatable)
            {
                AllFilters.Add(new AbilityFilter("All activatables",
                "",
                0,
                (caster, level) =>
                PartyActivatableList.ActivatableDataUIFilter(queueController.partyActivatableList, caster, level)
                ));
                if (casterName != "")
                {

                    AllFilters.Add(new AbilityFilter($"{casterName} Activatables",
                    casterName,
                    0,
                    (caster, level) =>
                    PartyActivatableList.ActivatableDataUIFilter(queueController.partyActivatableList, caster, level)
                    ));
                }
            }
            EnabledFilters = new HashSet<AbilityFilter>();
        }

        //Reset GUI
        //TODO
        public static void Reset()
        {

        }

        //Clearing filter data
        public static void Clear()
        {
            AllFilters.Clear();
            EnabledFilters.Clear();
        }


    }
}
