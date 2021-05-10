﻿using System;
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
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
            filePath = Path.Combine(ModPath, "scripts") + $"/{queueName}.json";
            if (!Directory.Exists(Path.Combine(ModPath, "scripts")))
                return false;
            try
            {
                File.Delete(filePath);
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


    public static class LogicHelpers
    {
        //Check if unit has low buff duration
        public static bool UnitHasLowBuffDuration(UnitEntityData target, BlueprintAbility blueprintAbility)
        {
            if (target == null)
            {
                Logger.Debug("target");
                return false;
            }
            AbilityEffectStickyTouch stickyTouch = blueprintAbility.StickyTouch;
            if (stickyTouch != null)
            {
                blueprintAbility = stickyTouch.TouchDeliveryAbility;
            }
            AbilityEffectRunAction component = blueprintAbility.GetComponent<AbilityEffectRunAction>();
            ActionList actionList = (component != null) ? component.Actions : null;
            if (actionList == null)
            {
                Logger.Debug("actionList");
                return false;
            }
            ContextActionApplyBuff contextActionApplyBuff = FindApplyBuffActionAll(actionList);
            ContextActionApplyBuff contextActionApplyBuffFalse = FindApplyBuffActionAll(actionList,true);
            BlueprintBuff blueprintBuff1 = (contextActionApplyBuff != null) ? contextActionApplyBuff.Buff : null;
            BlueprintBuff blueprintBuff2 = (contextActionApplyBuffFalse != null) ? contextActionApplyBuffFalse.Buff : null;
            if (blueprintBuff1 == null && blueprintBuff2 == null)
            {
                Logger.Debug("blueprintBuff");
                return false;
            }
            TimeSpan settingsDuration = TimeSpan.FromSeconds(RefreshTime);
            using (IEnumerator<Buff> enumerator = target.Buffs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if ((enumerator.Current.Blueprint == blueprintBuff1 || enumerator.Current.Blueprint == blueprintBuff2) && enumerator.Current.TimeLeft > settingsDuration)
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
                    .SelectMany(a => a.FlattenAllActions())
                .Concat(
                Ability.GetComponents<AbilityEffectRunAction>()
                    .SelectMany(a => a.FlattenAllActions()))
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
    }
}