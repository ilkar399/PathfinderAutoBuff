using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;


namespace KingmakerAutoBuff.Extensions
/*
 * WoTR=>Kingmaker conversion notices:
 * 0. 
 * Rename namespace from WoTRAutoBuff to KingmakerAutoBuff
 * 1.
 * UnitDescriptor<->UnitEntityData
 * UnitDescriptor.Unit
 * UnitEntityData.Descriptor
 * 2.
 * A bunch of wrapped properties converted to the extension methods
 *
 * 3.
 * Interfaces used in UIController:
 * ISceneHandler -> IAreaHandler
 *
 * 4. 
 * ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference> returned by 
 * blueprintAbility.GetComponent<AbilityVariants>().Variants  replaced by just Variants[]
 *
 * 5. 
 * Replace Button with ButtonPF in ABQueuesToolbar 
 *TODO:
 *PartyAndPets
 *BlueprintAbilityReference
 *ReferenceArrayProxy
 */

{
    public static class WoTRExtensions
	{
        [CanBeNull]
        public static ContextActionApplyBuff FindApplyBuffAction(ActionList actions)
        {
            foreach (GameAction gameAction in actions.Actions)
            {
                ContextActionConditionalSaved contextActionConditionalSaved;
                Conditional conditional;
                ContextActionApplyBuff contextActionApplyBuff;
                if ((contextActionConditionalSaved = (gameAction as ContextActionConditionalSaved)) != null)
                {
                    contextActionApplyBuff = FindApplyBuffAction(contextActionConditionalSaved.Succeed);
                    if (contextActionApplyBuff != null)
                        return contextActionApplyBuff;
                    contextActionApplyBuff = FindApplyBuffAction(contextActionConditionalSaved.Failed);
                    if (contextActionApplyBuff != null)
                        return contextActionApplyBuff;
                }
                if ((conditional = (gameAction as Conditional)) != null)
                {
                    contextActionApplyBuff = FindApplyBuffAction(conditional.IfTrue);
                    if (contextActionApplyBuff != null)
                        return contextActionApplyBuff;
                    contextActionApplyBuff = FindApplyBuffAction(conditional.IfFalse);
                    if (contextActionApplyBuff != null)
                        return contextActionApplyBuff;
                }
                if ((contextActionApplyBuff = (gameAction as ContextActionApplyBuff)) != null)
                    return contextActionApplyBuff;
            }
            return null;
        }

        [NotNull]
		public static IList<UnitEntityData> Pets (this UnitEntityData unitEntityData)
		{
			List<UnitEntityData> result = new List<UnitEntityData>();
			if (unitEntityData.Descriptor.Pet != null)
				result.Add(unitEntityData.Descriptor.Pet);
			return result;
		}

		[CanBeNull]
		public static UnitEntityData Master(this UnitEntityData unitEntityData)
        {
			return unitEntityData.Descriptor.Master;
        }

        //TODO
        public static List<UnitEntityData> PartyAndPets(this Player player)
        {
			List<UnitEntityData> result = new List<UnitEntityData>();
			foreach (UnitEntityData unit in player.Party)
            {
				result.Add(unit);
				if (unit.Descriptor.Pet != null)
					result.Add(unit.Descriptor.Pet);
            }
			return result;
        }

		public static bool IsPet(this UnitEntityData unitEntityData)
        {
			return (unitEntityData.Descriptor.Master != null);
		}

        public static T MinBy<T>(this IEnumerable<T> enumerable, Func<T, float> selector)
        {
            float num = float.MaxValue;
            T result = default(T);
            foreach (T t in enumerable)
            {
                float num2 = selector(t);
                if (num2 < num)
                {
                    num = num2;
                    result = t;
                }
            }
            return result;
        }
    }
}
