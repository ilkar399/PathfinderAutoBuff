using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static PathfinderAutoBuff.Main;
using Kingmaker;
using Kingmaker.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Items.Slots;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UI.GenericSlot;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.Mechanics.Actions.ContextActionSavingThrow;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif


namespace PathfinderAutoBuff.Utility
{
    public static class IOHelpers
    {
        //Delete queue file by queue name. 
        //TODO: additional error handling
        public static bool DeleteQueue(string queueName)
        {
            string filePath = "";
            string filePath2 = "";
            filePath = Path.Combine(ModPath, "scripts") + $"/{queueName}.json";
            filePath2 = Path.Combine(ModPath, "scripts") + $"/{queueName}.metadata";
            if (!Directory.Exists(Path.Combine(ModPath, "scripts")))
                return false;
            try
            {
                File.Delete(filePath);
                File.Delete(filePath2);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Logger.Log(ex.StackTrace);
#endif
                return false;
            }
            return true;
        }

    }


    public static class GUIHelpers
    {
        //Modified LimitPositionRectInRect since we use different anchoring and the one used by game doesn't account for that
        public static Vector2 LimitPositionRectInRect(Vector2 nPos, RectTransform parent, RectTransform child)
        {
            float width = parent.rect.width;
            float height = parent.rect.height;
            float width2 = child.rect.width;
            float height2 = child.rect.height;
            Vector2 pivot = child.pivot;
            Vector2 anchor = child.anchorMin;
            if (nPos.x + width * anchor.x - pivot.x * width2 <= 0f)
            {
                nPos.x = -width * anchor.x + pivot.x * width2;
            }
            else if (nPos.x + width * anchor.x + (1f - pivot.x) * width2 >= width)
            {
                nPos.x = width * (1f-anchor.x) - (1f - pivot.x) * width2;
            }
            if (nPos.y + height * anchor.y - pivot.y * height2 <= 0f)
            {
                nPos.y = -height * anchor.y + pivot.y * height2;
            }
            else if (nPos.y + height * anchor.y + (1f - pivot.y) * height2 >= height)
            {
                nPos.y = height * (1f - anchor.y) - (1f - pivot.y) * height2;
            }
            return nPos;
        }
    }

    public static class LogicHelpers
    {
        //Check if unit has low buff duration
        public static bool UnitHasLowBuffDuration(UnitEntityData target, BlueprintAbility blueprintAbility)
        {
            if (target == null)
            {
                return false;
            }
            //Checking Sticky touch
            AbilityEffectStickyTouch abilityEffectStickyTouch = blueprintAbility.GetComponent<AbilityEffectStickyTouch>();
            /*
            blueprintAbility = blueprintAbility.StickyTouch?.TouchDeliveryAbility == null 
                ? blueprintAbility
                : blueprintAbility.StickyTouch?.TouchDeliveryAbility;
            */
            //Checking ContextActionCastSpell
            ContextActionCastSpell contextActionCastSpell = (Utility.LogicHelpers.FlattenAllActions(blueprintAbility, true).
                Where(action => (action as ContextActionCastSpell) != null).
                FirstOrDefault() as ContextActionCastSpell);
            if (abilityEffectStickyTouch != null)
                blueprintAbility = abilityEffectStickyTouch.TouchDeliveryAbility;
            else if (contextActionCastSpell != null)
                blueprintAbility = contextActionCastSpell.Spell;
            AbilityEffectRunAction component = blueprintAbility.GetComponent<AbilityEffectRunAction>();
            ActionList actionList = (component != null) ? component.Actions : null;
            if (actionList == null)
            {
                return false;
            }
            //Checking if action increases duration (Cackle/Chant)
            if (Utility.LogicHelpers.FlattenAllActions(blueprintAbility, true).
                Where(action => (action as ContextActionReduceBuffDuration) != null).Count() > 0)
            {
                return true;
            }
            //Checking if action applies buff to item (Bless Weapon, etc.)
            ContextActionEnchantWornItem contextActionEnchantWornItem = Utility.LogicHelpers.FlattenAllActions(blueprintAbility, true).
                Where(action => (action as ContextActionEnchantWornItem) != null).FirstOrDefault() as ContextActionEnchantWornItem;
            if (contextActionEnchantWornItem != null)
            {
                ItemSlot itemSlot = EquipSlotBase.ExtractSlot(contextActionEnchantWornItem.Slot, target.Body);
                if (!itemSlot.HasItem)
                {
                    return true;
                }
                ItemEnchantment fact = itemSlot.Item.Enchantments.GetFact(contextActionEnchantWornItem.Enchantment);
                if (fact == null)
                    return true;
                else if (fact.IsTemporary)
                    if (fact.EndTime - Game.Instance.TimeController.GameTime > SettingsWrapper.RefreshTime.Seconds())
                    {
                        return false;
                    }
            }
            //Checking if action applies buff to the pet
#if (KINGMAKER)
            ContextActionsOnPet contextActionsOnPet = Utility.LogicHelpers.FlattenAllActions(blueprintAbility, true).
                Where(action => (action as ContextActionsOnPet) != null).FirstOrDefault() as ContextActionsOnPet;
            if (contextActionsOnPet != null)
            {
                UnitEntityData pet = target.Descriptor.Pet;
                if (pet != null)
                {
                    target = pet;
                }
                else
                    return false;
            }
#elif (WOTR)
            ContextActionsOnPet contextActionsOnPet = Utility.LogicHelpers.FlattenAllActions(blueprintAbility, true).
                Where(action => (action as ContextActionsOnPet) != null).FirstOrDefault() as ContextActionsOnPet;
            if (contextActionsOnPet != null)
            {
                UnitEntityData pet = null;
                foreach (EntityPartRef<UnitEntityData, UnitPartPet> entityPartRef in target.Pets)
                {
                    if (entityPartRef.Entity != null)
                    {
                        if (!contextActionsOnPet.AllPets)
                        {
                            UnitPartPet unitPartPet = entityPartRef.Entity.Get<UnitPartPet>();
                            PetType? petType = (unitPartPet != null) ? new PetType?(unitPartPet.Type) : null;
                            PetType petType2 = contextActionsOnPet.PetType;
                            if (!(petType.GetValueOrDefault() == petType2 & petType != null))
                            {
                                continue;
                            }
                        }
                        pet = entityPartRef;
                    }
                }
                if (pet != null)
                    target = pet;
                else
                    return false;
            }
#endif
            ContextActionApplyBuff contextActionApplyBuff = FindApplyBuffActionAll(actionList);
            ContextActionApplyBuff contextActionApplyBuffFalse = FindApplyBuffActionAll(actionList,true);
            BlueprintBuff blueprintBuff1 = (contextActionApplyBuff != null) ? contextActionApplyBuff.Buff : null;
            BlueprintBuff blueprintBuff2 = (contextActionApplyBuffFalse != null) ? contextActionApplyBuffFalse.Buff : null;
            if (blueprintBuff1 == null && blueprintBuff2 == null)
            {
                return false;
            }
            using (IEnumerator<Buff> enumerator = target.Buffs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if ((enumerator.Current.Blueprint == blueprintBuff1 || enumerator.Current.Blueprint == blueprintBuff2) && enumerator.Current.TimeLeft > RefreshTime.Seconds())
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        /*
         * Recursive Flattening Actions in Ability
         * TODO: Ignore enemies
         */
        public static GameAction[] FlattenAllActions(this BlueprintAbility Ability, bool ignoreEnemy = false)
        {
            return
                Ability.GetComponents<AbilityExecuteActionOnCast>()
                    .SelectMany(a => a.FlattenAllActions(ignoreEnemy))
                .Concat(
                Ability.GetComponents<AbilityEffectRunAction>()
                    .SelectMany(a => a.FlattenAllActions(ignoreEnemy))
                    )
                .ToArray();
        }

        public static GameAction[] FlattenAllActions(this AbilityExecuteActionOnCast Action, bool ignoreEnemy = false)
        {
            return FlattenAllActions(Action.Actions.Actions, ignoreEnemy);
        }

        public static GameAction[] FlattenAllActions(this AbilityEffectRunAction Action, bool ignoreEnemy = false)
        {
            return FlattenAllActions(Action.Actions.Actions, ignoreEnemy);
        }

        public static GameAction[] FlattenAllActions(GameAction[] Actions, bool ignoreEnemy = false)
        {
            List<GameAction> NewActions = new List<GameAction>();
            NewActions.AddRange(Actions.OfType<ContextActionsOnPet>().SelectMany(a => a.Actions.Actions));
            NewActions.AddRange(Actions.OfType<ContextActionConditionalSaved>().SelectMany(a => a.Failed.Actions));
            NewActions.AddRange(Actions.OfType<ContextActionConditionalSaved>().SelectMany(a => a.Succeed.Actions));
            if (ignoreEnemy)
            {
                NewActions.AddRange(Actions.OfType<Conditional>().SelectMany(a => a.IfFalse.Actions));
                NewActions.AddRange(Actions.OfType<Conditional>().
                    Where(a => (a.ConditionsChecker.Conditions.OfType<Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionIsEnemy>().FirstOrDefault()) == null).
                    SelectMany(b => b.IfTrue.Actions));
            }
            else
            {
                NewActions.AddRange(Actions.OfType<Conditional>().SelectMany(a => a.IfFalse.Actions));
                NewActions.AddRange(Actions.OfType<Conditional>().SelectMany(a => a.IfTrue.Actions));
            }
            if (NewActions.Count > 0)
            {
                return Actions.Concat(FlattenAllActions(NewActions.ToArray(),ignoreEnemy)).ToArray();
            }
            return Actions.ToArray();
        }

        //Finds buff action more effectively than the in-game FindApplyBuffAction (that one doesn't check conditional.failed)
        [CanBeNull]
        public static ContextActionApplyBuff FindApplyBuffActionAll(ActionList actions, bool checkFail = false)
        {
            foreach (GameAction gameAction in actions.Actions)
            {
                ContextActionConditionalSaved contextActionConditionalSaved;
                ContextActionsOnPet contextActionsOnPet;
                Conditional conditional;
                ContextActionApplyBuff contextActionApplyBuff;
                if ((contextActionConditionalSaved = (gameAction as ContextActionConditionalSaved)) != null)
                {
                    if (checkFail)
                    {
                        contextActionApplyBuff = FindApplyBuffActionAll(contextActionConditionalSaved.Failed);
                        if (contextActionApplyBuff != null)
                            return contextActionApplyBuff;
                    }
                    else
                    {
                        contextActionApplyBuff = FindApplyBuffActionAll(contextActionConditionalSaved.Succeed);
                        if (contextActionApplyBuff != null)
                            return contextActionApplyBuff;
                    }
                }
                if ((conditional = (gameAction as Conditional)) != null)
                {
                    bool enemyCheck = conditional.ConditionsChecker.Conditions.
                        OfType<Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionIsEnemy>().FirstOrDefault() != null;
                    contextActionApplyBuff = FindApplyBuffActionAll(conditional.IfTrue);
                    if (contextActionApplyBuff != null && !enemyCheck && !checkFail)
                        return contextActionApplyBuff;
                    contextActionApplyBuff = FindApplyBuffActionAll(conditional.IfFalse);
                    if (contextActionApplyBuff != null)
                        return contextActionApplyBuff;
                }
                if ((contextActionsOnPet = (gameAction as ContextActionsOnPet)) != null)
                {
                    contextActionApplyBuff = FindApplyBuffActionAll(contextActionsOnPet.Actions);
                    if (contextActionApplyBuff != null)
                        return contextActionApplyBuff;
                }
                if ((contextActionApplyBuff = (gameAction as ContextActionApplyBuff)) != null)
                    return contextActionApplyBuff;
            }
            return null;
        }

        //IsPet wrapper for both Kingmaker and WoTR
        public static bool IsPetWrapper(UnitEntityData unitEntityData)
        {
#if (WOTR)
            return unitEntityData.IsPet;
#elif (KINGMAKER)
            return unitEntityData.IsPet();                                                  
#endif
        }

        //Master wrapper for both Kingmaker and WoTR
        public static UnitEntityData MasterWrapper(UnitEntityData unitEntityData)
        {
#if (WOTR)
            return unitEntityData.Master;
#elif (KINGMAKER)
            return unitEntityData.Master();                                                 
#endif
        }

        //Positions in action for character names
        public static int ActionOrderPosition(UnitEntityData unit, List<string> characterNames, Dictionary<string,List<int>> petIndex)
        {
            int result = -1;
            int queuePosition = 0;
            if (unit == null || characterNames == null || petIndex == null)
                return -1;
            if (IsPetWrapper(unit) && MasterWrapper(unit) != null)
            {
                string petOwnerName = MasterWrapper(unit).CharacterName;
                queuePosition += characterNames.Count;
                if (petIndex.Keys.IndexOf(petOwnerName) == -1)
                    return -1;
                else
                {
#if (WOTR)
                    queuePosition += petIndex.Keys.IndexOf(petOwnerName) + MasterWrapper(unit).Pets.IndexOf(unit);
#elif (KINGMAKER)
                    queuePosition += petIndex.Keys.IndexOf(petOwnerName) + MasterWrapper(unit).Pets().IndexOf(unit);
#endif
                    return queuePosition;
                }
            }
            else
            {
                queuePosition = characterNames.IndexOf(unit.CharacterName);
                return queuePosition;
            }
            return result;
        }

    }
}
