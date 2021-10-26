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
            int castCount = 0;
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
            int blueprintLevel = spellbook.GetMinSpellLevel(blueprint);
#else
            int blueprintLevel = spellbook.GetSpellLevel(blueprint);
#endif
#if (WOTR)
            if (!spellbook.IsStandaloneMythic && spellbook.Owner.Stats.GetStat(spellbook.Blueprint.CastingAttribute) < 10 + blueprintLevel)
            {
                return false;
            }
#endif
            if (blueprintLevel < 0)
                return false;
            if (spellbook.Blueprint.Spontaneous)
            {
                castCount = spellbook.GetSpontaneousSlots(blueprintLevel);
                if (castCount > 0)
                {
                    return true;
                }
            }
            else
            {
#if (WOTR)
                //Taking CompletelyNormal metamagic in account
                for (int k = Math.Min(0,blueprintLevel - 1); k <= num; k++)
#elif (KINGMAKER)
                for (int k = blueprintLevel; k <= num; k++)
#endif
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
//                                    castCount++;
                                    return true;
                                }
                            }
                            else
                            {
                                AbilityData spell2 = spellSlot.Spell;
                                if (blueprint == ((spell2 != null) ? spell2.Blueprint : null))
                                {
//                                    castCount++;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
 //           Logger.Debug($"Spellbook {spellbook.Blueprint.Name} spell {blueprint.Name} casts {castCount}");
 //           if (castCount > 0)
 //               return true;
			return false;
		}

        //Determining which exact spellslot to use
        internal static AbilityData PASpellAbility(this Spellbook spellbook, [NotNull] BlueprintAbility blueprint, bool excludeSpecial = false, UnitEntityData executor = null)
        {
            //TODO replace with metadata class
            bool metadataIgnoreMetamagic;
            bool metadataLowestSlotFirst;
            List<Metamagic> metamagicPriority;
            if (Main.QueuesController?.queueController?.CurrentMetadata() != null)
            {
                metadataIgnoreMetamagic = Main.QueuesController.queueController.CurrentMetadata().MetadataIgnoreMetamagic;
                metadataLowestSlotFirst = Main.QueuesController.queueController.CurrentMetadata().MetadataLowestSlotFirst;
                metamagicPriority = Main.QueuesController.queueController.CurrentMetadata().MetamagicPriority;
            }
            else
            {
                metadataIgnoreMetamagic = SettingsWrapper.MetadataIgnoreMetamagic;
                metadataLowestSlotFirst = SettingsWrapper.MetadataLowestSlotFirst;
                metamagicPriority = SettingsWrapper.MetamagicPriority;
            }
			int maxSpellLevel = spellbook.MaxSpellLevel;
            List<AbilityData> availableSpells = new List<AbilityData>();
            //Getting available spelllist/spellslot
            //Spontaneous
            if (spellbook.Blueprint.Spontaneous)
            {
#if (WOTR)
                for (int spellLevel = maxSpellLevel; spellLevel >= Math.Max(0,spellbook.GetMinSpellLevel(blueprint) -1); spellLevel--)
#else
                for (int spellLevel = maxSpellLevel; spellLevel >= spellbook.GetSpellLevel(blueprint); spellLevel--)
#endif
                {
                    List<AbilityData> customSpells = spellbook.GetCustomSpells(spellLevel).Where(spell => spell.Blueprint == blueprint).ToList();
                    if (customSpells?.Count() > 0)
                        foreach (AbilityData spell2 in customSpells)
                            if (spellbook.GetAvailableForCastSpellCount(spell2) > 0)
                                availableSpells.Add(spell2);
                    availableSpells.AddRange(spellbook.GetAllKnownSpells().Where(spell => {
                        return (spell.Blueprint == blueprint && spell.GetAvailableForCastCount() > 0);
                        }).ToList());
                }
            }
            else
            //Non-spontaneous
            {
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
            }
            //Prioretizing spells in accordance to the metadata settings
            AbilityData result = null;
            List<AbilityData> results = availableSpells.ToList();
            //Metamagic
            if (!metadataIgnoreMetamagic)
            {
                foreach (Metamagic metamagic in metamagicPriority)
                {
                    List<AbilityData> results2 = new List<AbilityData>();
                    if (metamagic == 0)
                        results2 = results.Where(spell =>
                        {
                            if (spell.MetamagicData == null)
                                return true;
                            if (!spell.MetamagicData.NotEmpty)
                                return true;
                            return false;
                        }).ToList();
                    else
                    {
                        results2 = results.Where(spell => spell.HasMetamagic(metamagic)).ToList();
                    }
                    if (results2.Count == 0)
                    {
                        break;
                    }
                    else
                        results = results2;
                }
            }
            if (results.Count < 1)
                results = availableSpells.ToList();
            //Spell level priority
            if (results.Count < 1)
                results = availableSpells.ToList();
            if (metadataLowestSlotFirst)
                result = results.OrderBy(spell => spell.SpellLevel).FirstOrDefault();
            else
                result = results.OrderByDescending(spell => spell.SpellLevel).FirstOrDefault();
            return result;
        }
	}

}
