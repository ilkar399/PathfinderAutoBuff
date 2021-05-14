using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Controllers;
using Kingmaker.Blueprints.Classes;
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif

namespace PathfinderAutoBuff.UnitLogic
{
    //SpellData. PartyPosition and Caster are 0'ed by default as they're not always required
    //TODO: remove the unneeded fields
    public class PartySpellData
    {
        public BlueprintAbility Blueprint { set; get; }
        public Int32 SpellLevel { set; get; }
        public Int32 EffectiveCasterLevel = 0;
        internal AbilityData AbilityData;
        public bool IsExtendible = false;
        public ContextDurationValue DurationValue = null;
        public int PartyPosition = 0;
        public int AvailableCasts = 0;
        public UnitDescriptor Caster = null;
        public bool CanTargetSelf = false;
        public bool CanTargetFriends = false;
        //For the touch spells
        public BlueprintAbility referencedAbility = null;

        //Full constructor
        public PartySpellData(BlueprintAbility blueprintAbility, Int32 spellLevel, Int32 effectiveCasterLevel, AbilityData abilityData, bool isExtendible = false,
                    ContextDurationValue durationValue = null, int partyPosition = -1, UnitDescriptor caster = null,
                    bool canTargetSelf = false, bool CanTargetFriends = false)
        {
            this.Blueprint = blueprintAbility;
            this.SpellLevel = spellLevel;
            this.EffectiveCasterLevel = effectiveCasterLevel;
            this.AbilityData = abilityData;
            this.IsExtendible = isExtendible;
            this.DurationValue = durationValue;
            this.PartyPosition = partyPosition;
            this.Caster = caster;
            this.CanTargetFriends = CanTargetFriends;
            this.CanTargetSelf = canTargetSelf;
        }

        //Reduced constructor (not in usage atm)
        public PartySpellData(BlueprintAbility blueprintAbility, Int32 spellLevel)
        {
            this.Blueprint = blueprintAbility;
            this.SpellLevel = spellLevel;
        }
    }

    public class PartySpellDataUI: DataDescriptionInterface
        //Transforming spell data for UI
    {
        public string AbilityID { get; private set; }
        public string AbilityName { get; private set; }
        public string AbilityDescription { get; private set; }
        public List<string> Casters { get; set; }
        [CanBeNull]
        public string SourceItem { get; private set; }
        public BlueprintAbility blueprintAbility;

        public PartySpellDataUI(string abilityID, BlueprintAbility blueprintAbility, List<string> casters, string sourceItemName = null)
        {
            this.AbilityID = abilityID;
            this.AbilityName = blueprintAbility.Name;
            this.AbilityDescription = blueprintAbility.Description;
            this.blueprintAbility = blueprintAbility;
            this.Casters = casters;
            this.SourceItem = sourceItemName;
        }
    }


    public class PartySpellList
    {
        //All Spell Abilities. Make into private after testing
        public List<PartySpellData> m_AllSpells;
        public List<PartySpellData> m_AvailableSpells;

        //Constructor
        public PartySpellList()
        {
            this.m_AllSpells = GetAllSpells();
            this.m_AvailableSpells = GetAvailableSpells();
        }

        //Refresh data (m_AllSpells). Add Available Spells?
        public List<PartySpellData> GetAllSpells()
        {
            List<PartySpellData> Result = new List<PartySpellData>();
            int num = 0;
#if (WOTR)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets
#elif (KINGMAKER)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets()
#endif
                                             where u.IsDirectlyControllable
                                             select u).ToList<UnitEntityData>())
            {
#if (WOTR)
                var unitSpells = GetUnitSpells(unit, num++);
#elif (KINGMAKER)
                var unitSpells = GetUnitSpells(unit.Descriptor, num++);
#endif
                Result.AddRange(unitSpells);
            }
            return Result;
        }

        //Get available party spells
        public List<PartySpellData> GetAvailableSpells()
        {
            List<PartySpellData> Result = new List<PartySpellData>();
            int num = 0;
#if (WOTR)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets
#elif (KINGMAKER)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets()
#endif
                                             where u.IsDirectlyControllable
                                             select u).ToList())
            {
#if (WOTR)
                var unitSpells = GetUnitSpells(unit, num++, false);
#elif (KINGMAKER)
                var unitSpells = GetUnitSpells(unit.Descriptor, num++,false);
#endif
                Result.AddRange(unitSpells);
            }
            return Result;
        }

        public void RefreshData()
        {
            this.m_AllSpells.Clear();
            this.m_AllSpells = null;
            this.m_AvailableSpells.Clear();
            this.m_AvailableSpells = null;
            this.m_AllSpells = GetAllSpells();
            this.m_AvailableSpells = GetAvailableSpells();
        }

        //Get unit spells into PartySpellData objects
        public static List<PartySpellData> GetUnitSpells(UnitDescriptor unitDescriptor, int partyPosition, bool allSpells = true)
        {
            List<PartySpellData> Result = new List<PartySpellData>();
            //Spellbooks
            foreach (Spellbook spellbook in unitDescriptor.Spellbooks)
            {
                for (int i = 0; i <= spellbook.MaxSpellLevel; i++)
                {
                    foreach (AbilityData spell in spellbook.GetSpecialSpells(i))
                    {
                        SpellDataFromAbility(ref Result, spellbook, spell, unitDescriptor, partyPosition, allSpells);
                    }
                    foreach (AbilityData spell in spellbook.GetCustomSpells(i))
                    {
                        SpellDataFromAbility(ref Result, spellbook, spell, unitDescriptor, partyPosition, allSpells);
                    }

                }
                foreach (AbilityData spell in spellbook.GetAllKnownSpells())
                {
                    SpellDataFromAbility(ref Result, spellbook, spell, unitDescriptor, partyPosition, allSpells);
                }
            }
            return Result;
        }

        private static void SpellDataFromAbility(
            ref List<PartySpellData> Result,
            Spellbook spellbook,
            AbilityData spell,
            UnitDescriptor unitDescriptor,
            int partyPosition,
            bool allSpells = true)
        {
            //TODO Ability variants, move to the separate function?
            BlueprintAbility blueprintAbility = spell.Blueprint;
            AbilityVariants component = blueprintAbility.GetComponent<AbilityVariants>();
#if (WOTR)
            ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference>? referenceArrayProxy =
                (component != null) ? new ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference>?(component.Variants) : null;
            if (referenceArrayProxy != null)
            {
                foreach (BlueprintAbility blueprintAbility1 in referenceArrayProxy.Value)

#elif (KINGMAKER)
                    BlueprintAbility[] variants;
                    if (component != null)
                        variants = component.Variants;
                    else
                        variants = null;
                    if (variants != null)
                    {

                        foreach (BlueprintAbility blueprintAbility1 in variants)
#endif
                {
                    PartySpellData partySpell = PartySpell(spell, blueprintAbility1, unitDescriptor, partyPosition, spell.SpellLevel);
                    if (partySpell != null)
                    {
                        if (allSpells)
                            Result.Add(partySpell);
                        else
                        {
                            if (spellbook.GetAvailableForCastSpellCount(spell) > 0)
                                Result.Add(partySpell);
                        }
                    }
                }
            }
            else
            {
                PartySpellData partySpell = PartySpell(spell, blueprintAbility, unitDescriptor, partyPosition, spell.SpellLevel);
                if (partySpell != null)
                {
                    if (allSpells)
                        Result.Add(partySpell);
                    else
                    {
                        if (spellbook.GetAvailableForCastSpellCount(spell) > 0)
                            Result.Add(partySpell);
                    }
                }
            }
        }


        //Making PartySpellData object out of an ability
        public static PartySpellData PartySpell(
            AbilityData spell,
            BlueprintAbility blueprintAbilityIn,
            UnitDescriptor unitDescriptor,
            int partyPosition,
            Int32 spellLevel)
        {
            PartySpellData spellData;
            BlueprintAbility referencedAbility = null;
            BlueprintAbility blueprintAbility;
            AbilityEffectStickyTouch abilityEffectStickyTouch = blueprintAbilityIn.GetComponent<AbilityEffectStickyTouch>();
            if (abilityEffectStickyTouch != null)
            {
                referencedAbility = abilityEffectStickyTouch.TouchDeliveryAbility;
                blueprintAbility = referencedAbility;
            }
            else
                blueprintAbility = blueprintAbilityIn;
            AbilityEffectRunAction component1 = blueprintAbility.GetComponent<AbilityEffectRunAction>();
            ActionList actionList = (component1 != null) ? component1.Actions : null;
            if (actionList == null)
                return null;
            ContextDurationValue contextDurationValue = null;
            ContextActionApplyBuff contextActionApplyBuff = (Utility.LogicHelpers.FlattenAllActions(blueprintAbility, true).
                Where(action => (action as ContextActionApplyBuff) != null).
                FirstOrDefault() as ContextActionApplyBuff);
            ContextActionEnchantWornItem contextActionEnchantWornItem = (Utility.LogicHelpers.FlattenAllActions(blueprintAbility, true).
                Where(action => (action as ContextActionEnchantWornItem) != null).
                FirstOrDefault() as ContextActionEnchantWornItem);
            if (contextActionApplyBuff != null)
                contextDurationValue = contextActionApplyBuff.DurationValue;
            if (contextActionEnchantWornItem != null)
                contextDurationValue = contextActionEnchantWornItem.DurationValue;
            if (contextDurationValue == null)
                return null;
            /*
            ContextActionApplyBuff contextActionApplyBuff = Utility.LogicHelpers.FindApplyBuffActionAll(actionList);
            if (contextActionApplyBuff == null)
                return null;
            ContextDurationValue contextDurationValue = contextActionApplyBuff.DurationValue;

            */
            UnitDescriptor caster = unitDescriptor;
            AbilityData spell1;
            if (spell.Blueprint.HasVariants || referencedAbility != null)
            {
                spell1 = new AbilityData(spell, blueprintAbility);
            }
            else
            {
                spell1 = spell;
            }
            MechanicsContext mechanicsContext = new AbilityExecutionContext(spell1, spell1.CalculateParams(), unitDescriptor.Unit, null);
            bool isExtendible = contextDurationValue.IsExtendable;
            bool canTargetSelf = blueprintAbility.CanTargetSelf;
            bool canTargetFriends = blueprintAbility.CanTargetFriends;
            int effectiveCasterLevel = ContextValueHelper.CalculateDiceValue(
                contextDurationValue.DiceType,
                contextDurationValue.DiceCountValue,
                contextDurationValue.BonusValue,
                mechanicsContext);
            spellData = new PartySpellData(
                blueprintAbilityIn,
                spellLevel, effectiveCasterLevel,
                spell,
                isExtendible,
                contextDurationValue,
                partyPosition,
                caster,
                canTargetSelf,
                canTargetFriends
                );
            if (referencedAbility != null)
                spellData.referencedAbility = referencedAbility;
            return spellData;
        }

        //i-level spells organazed by the ability blueprint
        public Dictionary<BlueprintAbility, List<PartySpellData>> PartySpellByLevel(int spellLevel, bool allSpells = true, string caster = "")
        {
            List<PartySpellData> partySpellList;
            if (allSpells)
                partySpellList = this.m_AllSpells;
            else
                partySpellList = this.m_AvailableSpells;
            Dictionary<BlueprintAbility, List<PartySpellData>> Result = new Dictionary<BlueprintAbility, List<PartySpellData>>();
            foreach (PartySpellData partySpellData in partySpellList)
            {
                if (partySpellData.SpellLevel == spellLevel && (caster == "" || partySpellData.Caster.CharacterName == caster))
                {
                    if (!Result.ContainsKey(partySpellData.Blueprint))
                    {
                        List<PartySpellData> partySpellDatas = new List<PartySpellData>();
                        partySpellDatas.Add(partySpellData);
                        Result[partySpellData.Blueprint] = partySpellDatas;
                    }
                    else
                    {
                        Result[partySpellData.Blueprint].Add(partySpellData);
                    }
                }
            }
            if (Result.Keys.Count > 0)
                return Result;
            else
                return null;
        }

        //Determine the caster for the selected spell that can do it with the longest duration.
        public PartySpellData LongestDurationSpellCaster(BlueprintAbility blueprintAbility, List<PartySpellData> partySpellDatas = null, bool allSpells = true)
        {
            if (!allSpells)
                partySpellDatas = this.m_AvailableSpells;
            else
                if (partySpellDatas == null)
                    partySpellDatas = this.m_AllSpells;
            if (partySpellDatas.Count == 0)
                return null;
            List<PartySpellData> abilities = partySpellDatas.Where(partySpellData => (partySpellData.Blueprint == blueprintAbility)).ToList();
            if (abilities.Count() == 0)
                return null;
            else
            {
                Rounds maxDuration = GetSpellDuration(abilities[0]);
                PartySpellData Result = abilities[0];
                foreach (PartySpellData ability in abilities)
                {
                    if (GetSpellDuration(ability) > maxDuration)
                    {
                        maxDuration = GetSpellDuration(ability);
                        Result = ability;
                    }
                }
                return Result;
            }
        }

        //Destructor
        public void Clear()
        {
            this.m_AllSpells.Clear();
        }

        //Get available casts for ability
       public int GetAvailableCasts(PartySpellData partySpellData, bool combine = false)
        {
            int Result = 0;
            if (combine)
            {
                foreach (PartySpellData partySpellData1 in this.m_AllSpells.Where(spellData => (partySpellData.Blueprint == spellData.Blueprint)))
                {
                    Result += partySpellData1.AbilityData.Spellbook.GetAvailableForCastSpellCount(partySpellData1.AbilityData);
                }
            }
            else
            {
                AbilityData spell = partySpellData.AbilityData;
                Result = spell.Spellbook.GetAvailableForCastSpellCount(spell);
            }
            return Result;
        }

        //Get available casts for ability on caster
        public int GetAvailableCasts(UnitEntityData caster, BlueprintAbility blueprintAbility)
        {
            int Result = 0;
#if (WOTR)
            foreach (PartySpellData partySpellData in this.m_AvailableSpells.Where(spellData => (spellData.Caster == caster && spellData.Blueprint == blueprintAbility)))
#elif (KINGMAKER)
            foreach (PartySpellData partySpellData in this.m_AvailableSpells.Where(spellData => (spellData.Caster == caster.Descriptor && spellData.Blueprint == blueprintAbility)))
#endif
            {
                Result += partySpellData.AbilityData.Spellbook.GetAvailableForCastSpellCount(partySpellData.AbilityData);
            }
            return Result;
        }

        //Get casters that have an abliity available (memorized/in spellbook)
        public List<UnitEntityData> GetAvailableCasters(BlueprintAbility blueprintAbility, bool allSpells = true)
        {
            List<PartySpellData> spellList;
            List<UnitEntityData> Result = new List<UnitEntityData>();
            if (allSpells)
                spellList = this.m_AllSpells;
            else
                spellList = this.m_AllSpells;
            foreach (PartySpellData partySpellData in spellList.Where(spellData => (spellData.Blueprint == blueprintAbility)))
            {
#if (WOTR)
                if (!Result.Contains(partySpellData.Caster))
                    Result.Add(partySpellData.Caster);
#elif (KINGMAKER)
                if (!Result.Contains(partySpellData.Caster.Unit))
                    Result.Add(partySpellData.Caster.Unit);
#endif
            }
            return Result;
        }

        //Get spelldata for spell on the caster if it's available
        public PartySpellData GetMemorizedSpellData(UnitEntityData caster, BlueprintAbility blueprintAbility)
        {
            if (caster == null || blueprintAbility == null)
                return null;
#if (WOTR)
            PartySpellData partySpellData = this.m_AvailableSpells.FirstOrDefault(spellData => (spellData.Caster == caster && spellData.Blueprint == blueprintAbility));
#elif (KINGMAKER)
            PartySpellData partySpellData = this.m_AvailableSpells.FirstOrDefault(spellData => (spellData.Caster == caster.Descriptor && spellData.Blueprint == blueprintAbility));
#endif
            return partySpellData;
        }

        //Get filtered abilities IDs for UI  
        public List<string[]> AbilityFilteredIds(int availabilityCode, int levelFilter, string caster = "")
        {
            List<string[]> result = new List<string[]>();
            Dictionary<BlueprintAbility, List<PartySpellData>> spellData;
            if (availabilityCode == 0 && caster != "")
            {
                spellData = this.PartySpellByLevel(levelFilter + 1, true, caster);
            }
            else
            {
                spellData = this.PartySpellByLevel(levelFilter + 1, false);
            }
            if (spellData != null && spellData.Keys.Count > 0)
            {
                result.Add(spellData.Keys.Select(kvp => (kvp.AssetGuid)).ToArray());
                result.Add(spellData.Keys.Select(kvp => (kvp.Name)).ToArray());
            }
            else
            {
                result.Add(null);
                result.Add(null);
            }
            return result;
        }

        /*
         * Get DataDescription spelldata for the UI 
         */
        public static List<DataDescriptionInterface> SpellDataUIFilterLevels(PartySpellList partySpellList, string caster, int spellLevel)
        {
            List<PartySpellDataUI> result = new List<PartySpellDataUI>();
            Dictionary<BlueprintAbility, List<PartySpellData>> spellData;
            spellData = partySpellList.PartySpellByLevel(spellLevel, false, caster);
            if (spellData?.Keys.Count > 0)
            {
                foreach (KeyValuePair<BlueprintAbility, List<PartySpellData>> keyValuePair in spellData )
                {
                    PartySpellDataUI spellDataUI = new PartySpellDataUI(
                        keyValuePair.Key.AssetGuid,
                        keyValuePair.Key,
                        keyValuePair.Value.Select(spellData1 => spellData1.Caster.CharacterName).ToList()
                        );
                    result.Add(spellDataUI);
                }
            }
            return result.ToList<DataDescriptionInterface>();
        }

        //Return calculated spell duration in Rounds
        public static Rounds GetSpellDuration(PartySpellData partySpellData)
        {
            string enduringSpellsID = "2f206e6d292bdfb4d981e99dcf08153f";
            string enduringSpellsGreaterID = "13f9269b3b48ae94c896f0371ce5e23c";
            if (partySpellData == null)
                throw new Exception("Party Spell Data is absent");
            if (partySpellData.Caster == null || partySpellData.DurationValue == null)
                throw new Exception("Party Spell Data is lacking");
            AbilityData spell1;
            if (partySpellData.AbilityData.Blueprint.HasVariants)
            {
                spell1 = new AbilityData(partySpellData.AbilityData, partySpellData.Blueprint);
            }
            else
                if (partySpellData.referencedAbility != null)
                {
                spell1 = new AbilityData(partySpellData.AbilityData, partySpellData.referencedAbility);
            }
                else
                {
                    spell1 = partySpellData.AbilityData;
                }
            MechanicsContext mechanicsContext = new AbilityExecutionContext(
                spell1,
                spell1.CalculateParams(),
                spell1.Caster.Unit,
                null);
            Rounds Result = partySpellData.DurationValue.Calculate(mechanicsContext);
            BlueprintFeature enduringSpells = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>(enduringSpellsID);
            BlueprintFeature enduringSpellsGreater = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>(enduringSpellsGreaterID);
            if (enduringSpells != null && enduringSpellsGreater != null)
                if ((Result.Seconds >= 60.Minutes() && partySpellData.Caster.HasFact(enduringSpells)) 
                    || (Result.Seconds >= 5.Minutes() && partySpellData.Caster.HasFact(enduringSpellsGreater)))
                {
                    Result = new Rounds(14400);
                }
            return Result;
        }

    }
}
