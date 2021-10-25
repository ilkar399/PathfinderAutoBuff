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
    //Ability data element
    public class AbilityDataUI: DataDescriptionInterface
    {
        public string AbilityID { get; private set; }
        public string AbilityName { get; private set; }
        public string AbilityDescription { get; private set; }
        public List<string> Casters { get;  set; }
        [CanBeNull] public string SourceItem { get; private set; }
        public BlueprintAbility blueprintAbility;

        public AbilityDataUI(string abilityID, BlueprintAbility blueprintAbility, string caster, string sourceItemName = null)
        {
            this.AbilityID = abilityID;
            this.AbilityName = blueprintAbility.Name;
            this.AbilityDescription = blueprintAbility.Description;
            this.blueprintAbility = blueprintAbility;
            this.Casters = new List<string> { caster };
            this.SourceItem = sourceItemName;
        }
    }

    //Ability lists with getters
    public class PartyAbilityList
    {
        public Dictionary<string, AbilityDataUI> m_Abilities;

        //Constructor
        public PartyAbilityList()
        {
            this.m_Abilities = GetPartyAbilities();
        }

        //Get party Ability data
        private Dictionary<string, AbilityDataUI> GetPartyAbilities()
        {
            Dictionary<string, AbilityDataUI> result = new Dictionary<string, AbilityDataUI>();
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.Party
                                             where u.IsDirectlyControllable
                                             select u).ToList<UnitEntityData>())
            {
                foreach (Ability ability in unit.Abilities)
                {
#if (WOTR)
                    string abilityId = ability.Blueprint.AssetGuid.ToString();
#elif (KINGMAKER)
                    string abilityId = ability.Blueprint.AssetGuid;
#endif

                    if (!ability.Hidden && (ability.Blueprint.CanTargetSelf || ability.Blueprint.CanTargetFriends))
                    {
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
                            result[abilityId] = new AbilityDataUI(
                                abilityId,
                                ability.Blueprint,
                                unit.CharacterName,
                                itemSource
                                );
                        }
                    }
                }
            }
            return result;
        }


        //Refresher
        public void RefreshData()
        {
            Logger.Debug("partyAbilityList.RefreshData");
            this.m_Abilities.Clear();
            this.m_Abilities = null;
            this.m_Abilities = GetPartyAbilities();
        }

        //Abilities for abilities selector in Queues UI
        public List<string[]> GetAbilitiesSelectorData(string casterName = null)
        {
            List<string[]> result = new List<string[]>();
            List<string> nameList = new List<string>();
            List<string> idList = new List<string>();
            if (casterName == null || casterName == "")
            {
                foreach (KeyValuePair<string, AbilityDataUI> kvp in this.m_Abilities)
                {
                    idList.Add(kvp.Key);
                    nameList.Add(kvp.Value.AbilityName);
                }
            }
            else
            {
                foreach (KeyValuePair<string, AbilityDataUI> kvp in this.m_Abilities.Where(kvp => kvp.Value.Casters.Contains(casterName)))
                {
                    idList.Add(kvp.Key);
                    nameList.Add(kvp.Value.AbilityName);
                }
            }
            result.Add(idList.ToArray());
            result.Add(nameList.ToArray());
            return result;
        }

        //Abilities for mod lists in Queues UI
        public List<string[]> GetAllCastersAbilities()
        {
            List<string[]> result = new List<string[]>();
            List<string> idList = new List<string>();
            List<string> nameList = new List<string>();
            foreach (KeyValuePair<string,AbilityDataUI> kvp in this.m_Abilities)
            {
                idList.Add(kvp.Key);
                nameList.Add(kvp.Value.AbilityName);
            }
            result.Add(idList.ToArray());
            result.Add(nameList.ToArray());
            return result;
        }


        //Data Ability getter
        public AbilityDataUI GetAbility(string abilityID)
        {
            if (this.m_Abilities.ContainsKey(abilityID))
                return this.m_Abilities[abilityID];
            else
            {
                BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(abilityID);
                if (blueprintAbility != null)
                    return new AbilityDataUI(abilityID,blueprintAbility,null,null);
                else
                    return null;
            }
        }

        //TODO Destructor

        //Number of Ability usages for caster. If not limited - returns 999
        public static int GetAvailableUsages(UnitEntityData caster, BlueprintAbility blueprintAbility)
        {
            Ability casterAbility = caster.Abilities.Enumerable.Where(ability => ability.Blueprint == blueprintAbility).FirstOrDefault();
            if (casterAbility == null)
                return 0;
            if (!casterAbility.Blueprint.GetComponent<AbilityKineticist>())
                return casterAbility.Data.GetAvailableForCastCount();
            return casterAbility.Data.ResourceCost;
        }

        //Get casters that have an abliity available
        public List<string> GetAvailableCasters(BlueprintAbility blueprintAbility)
        {
            if (this.m_Abilities.ContainsKey(blueprintAbility.AssetGuidThreadSafe))
                return new List<string>(m_Abilities[blueprintAbility.AssetGuidThreadSafe].Casters);
            else
                return null;
        }

        /*
        * Ability filter function for Queues UI
        */
        public static List<DataDescriptionInterface> AbilityDataUIFilter(PartyAbilityList partyAbilityList, string caster = null, int dummyfilter = 0)
        {
            List<AbilityDataUI> result;
            if (caster == null || caster == "")
                result = partyAbilityList.m_Abilities.Values.ToList();
            else
            {
                result = partyAbilityList.m_Abilities.Values.Where(activatable => activatable.Casters.Contains(caster)).ToList();
            }
            return result.ToList<DataDescriptionInterface>();
        }
    }
}
