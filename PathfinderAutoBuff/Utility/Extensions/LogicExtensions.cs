﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic;

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
			/*
						if (spellbook.Blueprint.Spontaneous)
						{
							int num3 = spellbook.GetSpontaneousSlots(num);
							if (num3 > 0)
							{
								return true;
							}
						}
						else
			*/
			int num2 = spellbook.GetSpellLevel(blueprint);
            for (int k = num2; k <= num; k++)
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
			return false;
		}

		internal static AbilityData PASpellAbility(this Spellbook spellbook, [NotNull] BlueprintAbility blueprint, bool excludeSpecial = false)
		{
			int maxSpellLevel = spellbook.MaxSpellLevel;
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
                            return spell2;
                        }
                    }
                }
            }
			return null;
		}
	}

}
