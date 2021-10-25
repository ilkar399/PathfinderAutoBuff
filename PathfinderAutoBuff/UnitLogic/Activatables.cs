using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.Controllers;
using Kingmaker.Blueprints.Classes;

namespace PathfinderAutoBuff.UnitLogic
{
    //Activatable ability data element
    public class ActivatableDataUI : DataDescriptionInterface
    {
        public string AbilityID { get; private set; }
        public string AbilityName { get; private set; }
        public string AbilityDescription { get; private set; }
        public List<string> Casters { get; set; }
        [CanBeNull] public string SourceItem { get; private set; }
        public BlueprintActivatableAbility blueprintAbility;

        public ActivatableDataUI(string abilityID, BlueprintActivatableAbility blueprintAbility, string caster, string sourceItemName = null)
        {
            this.AbilityID = abilityID;
            this.AbilityName = blueprintAbility.Name;
            this.AbilityDescription = blueprintAbility.Description;
            this.blueprintAbility = blueprintAbility;
            this.Casters = new List<string> { caster };
            this.SourceItem = sourceItemName;
        }
    }

    public class PartyActivatableList
    {
        public Dictionary<string, ActivatableDataUI> m_Activatables;

        //Constructor
        public PartyActivatableList()
        {
            this.m_Activatables = GetPartyActivatables();
        }

        //Get party Activatable data
        private Dictionary<string, ActivatableDataUI> GetPartyActivatables()
        {
            Dictionary<string, ActivatableDataUI> result = new Dictionary<string, ActivatableDataUI>();
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.Party
                                             where u.IsDirectlyControllable
                                             select u).ToList<UnitEntityData>())
            {
                foreach (ActivatableAbility ability in unit.ActivatableAbilities)
                {
#if (WOTR)
                    string abilityId = ability.Blueprint.AssetGuid.ToString();
#elif (KINGMAKER)
                    string abilityId = ability.Blueprint.AssetGuid;
#endif
                    if (result.ContainsKey(abilityId))
                    {
                        result[abilityId].Casters.Add(unit.CharacterName);
                    }
                    else
                    {
                        string itemSource = null;
                        if (ability.SourceItem != null)
                        {
                            itemSource = ability.SourceItem.Name;
                        }
                        result[abilityId] = new ActivatableDataUI(
                            abilityId,
                            ability.Blueprint,
                            unit.CharacterName,
                            itemSource
                            );
                    }
                }
            }
            return result;
        }

        //Refresher
        public void RefreshData()
        {
            Logger.Debug("partyActivatableList.RefreshData");
            this.m_Activatables.Clear();
            this.m_Activatables = null;
            this.m_Activatables = GetPartyActivatables();
        }

        //Activatables for mod lists in Queues UI
        public List<string[]> GetAllCastersActivatables()
        {
            List<string[]> result = new List<string[]>();
            List<string> idList = new List<string>();
            List<string> nameList = new List<string>();
            foreach (KeyValuePair<string, ActivatableDataUI> kvp in this.m_Activatables)
            {
                idList.Add(kvp.Key);
                nameList.Add(kvp.Value.AbilityName);
            }
            result.Add(idList.ToArray());
            result.Add(nameList.ToArray());
            return result;
        }


        //Data Activatable getter
        public ActivatableDataUI GetActivatable(string abilityID)
        {
            if (this.m_Activatables.ContainsKey(abilityID))
                return this.m_Activatables[abilityID];
            else
            {
                BlueprintActivatableAbility blueprintActivatable = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>(abilityID);
                if (blueprintActivatable != null)
                    return new ActivatableDataUI(abilityID, blueprintActivatable, null, null);
                else
                    return null;
            }
        }

        /*
         * Activatable filter function for Queues UI
         */
        public static List<DataDescriptionInterface> ActivatableDataUIFilter(PartyActivatableList partyActivatableList,string caster = null, int dummyfilter = 0)
        {
            List<ActivatableDataUI> result;
            if (caster == null || caster == "")
                result = partyActivatableList.m_Activatables.Values.ToList();
            else
            {
                result = partyActivatableList.m_Activatables.Values.Where(activatable => activatable.Casters.Contains(caster)).ToList();
            }
            return result.ToList<DataDescriptionInterface>();
        }
    }

}
