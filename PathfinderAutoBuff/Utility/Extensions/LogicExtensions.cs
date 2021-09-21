using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.EntitySystem.Entities;

namespace PathfinderAutoBuff.Utility.Extensions
{
    public static class LogicExtensions
    {
		internal static bool PACanSpendSpell(this Spellbook spellbook, [NotNull] BlueprintAbility blueprint)
       {
			return PACanSpendSpell(spellbook, blueprint, null, false);
       }

		//TODO: Extension for metamagic
		internal static bool PACanSpendSpell(this Spellbook spellbook, [NotNull] BlueprintAbility blueprint, [CanBeNull] AbilityData spell, bool excludeSpecial = false)
		{
            int num = (spell != null) ? spellbook.GetSpellLevel(spell) : spellbook.MaxSpellLevel;
			if (num < 0)
			{
				return false;
			}
			if (spellbook.MaxSpellLevel != 0)
			{
				int maxSpellLevel = spellbook.MaxSpellLevel;
				if (num > maxSpellLevel)
				{
					return false;
				}
			}
#if (WOTR)
			if (!spellbook.IsStandaloneMythic && spellbook.Owner.Stats.GetStat(spellbook.Blueprint.CastingAttribute) < 10 + num)
			{
				return false;
			}
#endif
#if (WOTR)
            int blueprintLevel = spellbook.GetMinSpellLevel(blueprint);
#else
            int blueprintLevel = spellbook.GetSpellLevel(blueprint);
#endif
            if (blueprintLevel < 0)
                return false;
            if (spellbook.Blueprint.Spontaneous)
            {
                int spontSlots = spellbook.GetSpontaneousSlots(blueprintLevel);
                if (spontSlots > 0)
                {
                    return true;
                }
            }
            else
            {
                for (int k = blueprintLevel; k <= num; k++)
                {
                    List<SpellSlot> list = spellbook.GetMemorizedSpellSlots(k).ToList();
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        SpellSlot spellSlot = list[i];
                        if (spellSlot.Available && (spellSlot.Type != SpellSlotType.Favorite || !excludeSpecial))
                        {
                            if (spell != null)
                            {
                                if (spell.Equals(spellSlot.Spell))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                AbilityData spell2 = spellSlot.Spell;
                                if (blueprint == ((spell2 != null) ? spell2.Blueprint : null))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
			return false;
		}

		internal static AbilityData PASpellAbility(this Spellbook spellbook, [NotNull] BlueprintAbility blueprint, bool excludeSpecial = false, UnitEntityData executor = null)
		{
			int maxSpellLevel = spellbook.MaxSpellLevel;
            if (spellbook.Blueprint.Spontaneous)
            {
                List<AbilityData> availableSpells = new List<AbilityData>();

#if (WOTR)
                for (int spellLevel = maxSpellLevel; spellLevel >= spellbook.GetMinSpellLevel(blueprint); spellLevel--)
#else
                for (int spellLevel = maxSpellLevel; spellLevel >= spellbook.GetSpellLevel(blueprint); spellLevel--)
#endif
                {
                    List<AbilityData> customSpells = spellbook.GetCustomSpells(spellLevel).Where(spell => spell.Blueprint == blueprint).ToList();
                    if (customSpells?.Count() > 0)
                        foreach (AbilityData spell2 in customSpells)
                            if (spellbook.GetAvailableForCastSpellCount(spell2) > 0)
                                availableSpells.Add(spell2);
                    availableSpells.AddRange(spellbook.GetAllKnownSpells().Where(spell => spell.Blueprint == blueprint).ToList());
                }
                AbilityData result = null;
                List<AbilityData> results = availableSpells.Where(spell => spell.HasMetamagic(Metamagic.Extend)).ToList();
                if (results != null)
                    result = results.OrderBy(spell => spell.SpellLevel).FirstOrDefault();
                return result != null ? result : availableSpells.OrderBy(spell => spell.SpellLevel).FirstOrDefault();
            }
            else
            {
                List<AbilityData> availableSpells = new List<AbilityData>();
                for (int spellLevel = maxSpellLevel; spellLevel >= 0; spellLevel--)
                {
                    List<SpellSlot> list = spellbook.GetMemorizedSpellSlots(spellLevel).ToList();
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        SpellSlot spellSlot = list[i];
                        if (spellSlot.Available && (spellSlot.Type != SpellSlotType.Favorite || !excludeSpecial))
                        {
                            AbilityData spell2 = spellSlot.Spell;
                            if (blueprint == ((spell2 != null) ? spell2.Blueprint : null))
                            {
                                availableSpells.Add(spell2);
                            }
                        }
                    }
                }
                availableSpells.AddRange(spellbook.GetAllKnownSpells().Where(spell => spell.Blueprint == blueprint).ToList());
                AbilityData result = availableSpells.Where(spell => spell.HasMetamagic(Metamagic.Extend)).FirstOrDefault();
                return result != null ? result : availableSpells.OrderBy(spell => spell.SpellLevel).FirstOrDefault();
            }
		}
	}

}
