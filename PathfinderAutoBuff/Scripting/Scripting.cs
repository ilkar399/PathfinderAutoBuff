using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using PathfinderAutoBuff.UnitLogic;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using static PathfinderAutoBuff.Utility.Extensions.LogicExtensions;

namespace PathfinderAutoBuff.Scripting
{
    /*
     * Unit command handlers and action queue=>command converters.
     * Based on Holic's KingmakerAI
     * https://github.com/Holic75/KingmakerAi
     */
    //Deactivate Ability command extension
    public class UnitDeactivateAbility: UnitCommand
    {
        ActivatableAbility activatableAbility;

        public UnitDeactivateAbility(ActivatableAbility ability)
            :base(CommandType.Free, null)
        {
            activatableAbility = ability;
        }

        protected override ResultType OnAction()
        {
            if (!activatableAbility.IsTurnedOn)
            {
                return ResultType.Fail;
            }
            else
            {
                activatableAbility.IsOn = false;
                return ResultType.Success;
            }
        }
    }

    //Interface for running commands
    public interface CommandProvider
    {
        UnitCommand getCommand();
        UnitEntityData getExecutor();
        string getDescription();
    }

    public class ActivateAbilityProvider : CommandProvider
    {
        UnitEntityData executor;
        BlueprintActivatableAbility activatableAbility;


        public ActivateAbilityProvider(UnitEntityData executor, BlueprintActivatableAbility ability)
        {
            this.executor = executor;
            activatableAbility = ability;
        }

        public UnitCommand getCommand()
        {
            var aa = executor.ActivatableAbilities.Enumerable.Where(a => a.Blueprint == activatableAbility && !a.IsOn).FirstOrDefault();
            if (aa == null)
            {
                return null;
            }
            else
            {
                aa.IsOn = true;
                return new UnitActivateAbility(aa);
            }
        }

        public UnitEntityData getExecutor()
        {
            return executor;
        }

        public string getDescription()
        {
            return "Activate " + activatableAbility.Name;
        }
    }

    public class DeactivateAbilityProvider : CommandProvider
    {
        UnitEntityData executor;
        BlueprintActivatableAbility activatableAbility;

        public DeactivateAbilityProvider(UnitEntityData executor, BlueprintActivatableAbility ability)
        {
            this.executor = executor;
            activatableAbility = ability;
        }

        public UnitCommand getCommand()
        {
            var aa = executor.ActivatableAbilities.Enumerable.Where(a => a.Blueprint == activatableAbility && a.IsOn).FirstOrDefault();
            if (aa == null)
            {
                return null;
            }
            else
            {
                return new UnitDeactivateAbility(aa);
            }
        }

        public UnitEntityData getExecutor()
        {
            return executor;
        }

        public string getDescription()
        {
            return "Deactivate " + activatableAbility.Name;
        }

    }

    public class MoveToTargetProvider : CommandProvider
    {
        UnitEntityData executor;
        TargetWrapper target;

        public MoveToTargetProvider(UnitEntityData executor, TargetWrapper moveTarget)
        {
            this.executor = executor;
            target = moveTarget;
        }

        public UnitCommand getCommand()
        {
            return new UnitMoveTo(target.Point, 2.Feet().Meters);
        }

        public UnitEntityData getExecutor()
        {
            return executor;
        }

        public string getDescription()
        {
            return "MoveToTarget";
        }
    }

    //TODO cleanup
    public class UseAbilityProvider: CommandProvider
    {
        BlueprintAbility ability;
        BlueprintAbility variant;
        UnitEntityData executor;
        TargetWrapper target;
        bool useSpellbook;

        public UseAbilityProvider(UnitEntityData executor, TargetWrapper spell_target, BlueprintAbility ability_to_use, BlueprintAbility variant_to_use = null, bool useSpellbook = true)
        {
            this.ability = ability_to_use;
            this.variant = variant_to_use;
            this.executor = executor;
            this.target = spell_target;
            this.useSpellbook = useSpellbook;
        }

        public UnitCommand getCommand()
        {
            var ab = getAbilityData();
            if (ab == null)
            {
                return null;
            }
            return new UnitUseAbility(ab, target);
        }

        private AbilityData getAbilityData()
        {
            var descriptor = executor.Descriptor;
            var ad = this.getAbilityForUse(descriptor);
            if ((ad != null) && !useSpellbook)
                return ad;
            Spellbook spellbook = this.getSpellbookForUse(descriptor);
            if (spellbook == null && useSpellbook)
                return null;
            if (useSpellbook)
            {
                AbilityData fact = spellbook.PASpellAbility(ability);
                if (variant == null)
                    return fact;
                else
                {
#if (KINGMAKER)
                    return new AbilityData(variant,executor.Descriptor, null, spellbook.Blueprint)
#elif (WOTR)
                    return new AbilityData(variant,executor.Descriptor, null, spellbook.Blueprint)
#endif
                    {
                        ConvertedFrom = fact
                    };
                }
                //return this.CreateAbilityData(descriptor, null, spellbook?.Blueprint);
            }
            else
                return this.CreateAbilityData(descriptor, ad.Fact, null);
        }

        private Spellbook getSpellbookForUse(UnitDescriptor unit)
        {
            //            return unit.Spellbooks.FirstOrDefault<Spellbook>((Func<Spellbook, bool>)(spellbook => spellbook.CanSpend(ability)))?.Blueprint;
            IEnumerable<Kingmaker.UnitLogic.Spellbook> spellbooks = unit.Spellbooks.Where(spellbook => spellbook.PACanSpendSpell(ability)).ToList();
            Logger.Debug(unit.CharacterName);
            Logger.Debug(ability.Name);
            Logger.Debug(spellbooks.Count());
            return spellbooks.Count() > 0 ? spellbooks.MaxBy(spellbook => spellbook.CasterLevel) : null;
        }

        private AbilityData getAbilityForUse(UnitDescriptor unit)
        {
            Kingmaker.UnitLogic.Abilities.Ability ab = unit.Abilities.GetAbility(ability);
            if (ab == null)
                return (AbilityData)null;
            return this.CreateAbilityData(unit, ab, null);
        }

        private AbilityData CreateAbilityData(
          UnitDescriptor executor,
          Kingmaker.UnitLogic.Abilities.Ability fact,
          BlueprintSpellbook spellbook)
        {
            AbilityData ad = new AbilityData(ability, executor, fact, spellbook);
            if (variant == null)
                return ad;
            return new AbilityData(variant, executor, fact, spellbook)
            {
                ConvertedFrom = ad
            };
        }

        public UnitEntityData getExecutor()
        {
            return executor;
        }

        public string getDescription()
        {
            return "Use " + ability.Name;
        }
    }

    //Parse the command queue and create a Script Executor to execute
    public class QueueParser
    {
        public Dictionary<UnitEntityData, List<CommandProvider>> commandProviders = new Dictionary<UnitEntityData, List<CommandProvider>>();
        bool ignoreMods;
        bool refreshShort;
        PartySpellList m_PartySpellList = new PartySpellList();

        public QueueParser(List<CommandQueueItem> commandQueueItems, bool ignoreMods, bool refreshShort)
        {
            this.ignoreMods = ignoreMods;
            this.refreshShort = refreshShort;
            int queueIndex = 0;
            foreach (CommandQueueItem commandQueueItem in commandQueueItems)
            {
                string errorMessage = "";
                queueIndex++;
                //Passing ignoreMods and dontRefresh here as it can differ from global Settings in the queue execution process
                List<CommandProvider> commandList = commandQueueItem.Parse(out errorMessage, ignoreMods, refreshShort, m_PartySpellList);
#if (DEBUG)
                if (errorMessage != "")
                    Logger.Log($"#{queueIndex - 1} {errorMessage}");
#endif
                if (commandList == null)
                    continue;
                foreach (CommandProvider command in commandList)
                {
                    if (commandProviders.ContainsKey(command.getExecutor()))
                    {
                        commandProviders[command.getExecutor()].Add(command);
                    }
                    else
                    {
                        commandProviders[command.getExecutor()] = new List<CommandProvider>();
                        commandProviders[command.getExecutor()].Add(command);
                    }
                }
            }
        }
    }

    //Execute the script
    public class ScriptExecutor
    {
        List<CommandProvider> commandProviders = new  List<CommandProvider>();
        bool errorStatus;
        UnitEntityData executor;
        public string errorMessage;
        public bool ErrorStatus() { return errorStatus; }
        public string ErrorMessage() { return errorMessage; }
        private UnitCommand currentCommand_ = null;
        private BlueprintAbility touchDeliveryAbility;
        private object current_command_lock = new object();

        public UnitCommand currentCommand
        {
            get
            {
                lock (current_command_lock)
                {
                    return currentCommand_;
                }
            }
            set
            {
                lock (current_command_lock)
                {
                    currentCommand_ = value;
                }
            }
        }

        public ScriptExecutor(UnitEntityData executor, List<CommandProvider> commandProviders)
        {
            this.executor = executor;
            this.commandProviders = commandProviders;
            if (commandProviders == null)
            {
                errorStatus = true;
                return;
            }
            errorStatus = false;
        }

        public void Run()
        {
            TryNextCommand();
        }

        private void TryNextCommand()
        {
            currentCommand = null;
            if (commandProviders.Empty())
            {
                return;
            }
            currentCommand = commandProviders.First().getCommand();
            commandProviders.RemoveAt(0);
            if (currentCommand == null)
            {
                TryNextCommand();
            }
            executor.Commands.InterruptAll();
            if (currentCommand != null)
                executor.Commands.AddToQueue(currentCommand);
        }

        public bool HandleUnitCommandDidEnd(UnitCommand command)
        {
            UnitUseAbility commandAbility = command as UnitUseAbility;
            //Checking if the command was a stickytouch delivered ability and continuing with execution
            if (commandAbility != null && touchDeliveryAbility != null)
            {
#if (WOTR)
                if (commandAbility.Ability.Blueprint == touchDeliveryAbility)
#elif (KINGMAKER)
                if (commandAbility.Spell.Blueprint == touchDeliveryAbility)
#endif
                {
                    touchDeliveryAbility = null;
                    TryNextCommand();
                    return true;
                }
            }
            if (currentCommand != command)
            {
                return false;
            }
#if (DEBUG)
            Logger.Debug($"Command " + command.Result.ToString());
#endif
            if (command.Result != UnitCommand.ResultType.Success && !(command is UnitMoveTo || command is UnitInteractWithUnit))
            {
//                touchDeliveryAbility = null;
                return false;
            }
            //Checking if the command has a stickytouch component and saving the ability id
            if (commandAbility != null)
            {
#if (WOTR)
                AbilityEffectStickyTouch abilityEffectStickyTouch = commandAbility.Ability.Blueprint.GetComponent<AbilityEffectStickyTouch>();
#elif (KINGMAKER)
                AbilityEffectStickyTouch abilityEffectStickyTouch = commandAbility.Spell.Blueprint.GetComponent<AbilityEffectStickyTouch>();
#endif
                if (abilityEffectStickyTouch != null)
                {
                    if (currentCommand.TargetUnit == currentCommand.Executor)
                    {
                        TryNextCommand();
                        return true;
                    }
                    touchDeliveryAbility = abilityEffectStickyTouch.TouchDeliveryAbility;
                    return false;
                }
            }
            TryNextCommand();
            return true;
        }

    }

    public class ScriptController
    {
        public static string executingQueueName = "";

        static List<ScriptExecutor> scriptExecutors = new List<ScriptExecutor>();

        public static void Run()
        {
            RunScripts();
        }

        public static void Reset()
        {
            executingQueueName = "";
            scriptExecutors.Clear();
            UnitCommand__OnEnded__Patch.clearScripts();
        }

        static void RunScripts()
        {
            if (scriptExecutors.Count == 0)
            {
                return;
            }
            UnitCommand__OnEnded__Patch.updateScripts(scriptExecutors.ToArray());
            foreach (var s in scriptExecutors)
            {
                s.Run();
            }
        }

        static public void CreateFromQueue(List<CommandQueueItem> commandQueue, string queueName)
        {
            var party = Kingmaker.Game.Instance?.Player?.Party;
            QueueParser queueParser;
            if (party == null || party.Empty() || commandQueue == null)
            {
                return;
            }
            try
            {
                queueParser = new QueueParser(commandQueue, IgnoreModifiiers, RefreshShort);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to parse command queue");
                Logger.Debug(ex.StackTrace);
                return;
            }
            if (queueName != "")
                executingQueueName = queueName;
            if (queueParser.commandProviders.Count < 1)
                return;
            foreach (KeyValuePair<UnitEntityData,List<CommandProvider>> kvp in queueParser.commandProviders)
            {
                var scriptExec = new ScriptExecutor(kvp.Key, kvp.Value);
                if (scriptExec.ErrorStatus())
                {
                    Logger.Log("Character: " + kvp.Key.CharacterName);
                    Logger.Log("Failed to create an executor for the command queue " + scriptExec.ErrorMessage());
                    continue;
                }
                scriptExecutors.Add(scriptExec);
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(UnitCommand), "OnEnded", HarmonyLib.MethodType.Normal)]
    class UnitCommand__OnEnded__Patch
    {
        static List<ScriptExecutor> script_executors = new List<ScriptExecutor>();
        private static Mutex script_execution_mutex = new Mutex();

        static public void updateScripts(params ScriptExecutor[] new_script_executors)
        {
            script_execution_mutex.WaitOne();
            script_executors = new_script_executors.ToList();
            script_execution_mutex.ReleaseMutex();
        }

        static public void clearScripts()
        {
            script_execution_mutex.WaitOne();
            script_executors.Clear();
            script_execution_mutex.ReleaseMutex();
        }

        static void Postfix(UnitCommand __instance, bool raiseEvent)
        {
            //Action recording test1
            RecordController recordQueue = Main.recordQueue;
            bool actionRecordFlag = recordQueue.Enabled;
            bool actionProcessedFlag = recordQueue.lastAction == __instance;
            bool stickyTouchFlag = recordQueue.StickyTouch;
            //Skip everything in non-debug mode
#if (!DEBUG)
            if (!actionRecordFlag && script_executors.Count == 0)
                return;
#endif
            UnitUseAbility unitUseAbility = (__instance as UnitUseAbility);
            /*
            if (unitUseAbility != null)
                Logger.Debug($"{__instance.Executor} - {__instance.Target} - {unitUseAbility.Ability.Blueprint.name}/{unitUseAbility.Ability.Blueprint.AssetGuid}");
            */
            //Action recording
            if (Kingmaker.Game.Instance.Player.Party.Contains(__instance.Executor) &&
                unitUseAbility != null && actionRecordFlag && !actionProcessedFlag)
            {
                if (stickyTouchFlag)
                {
                    recordQueue.StickyTouch = false;
                }
                 else
                {
                    try
                    {
                        recordQueue.AddAction(__instance);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.StackTrace);
                    }
#if (WOTR)
                    Logger.Debug($"{__instance.Executor} - {__instance.Target} - {unitUseAbility.Ability.Blueprint.name} {unitUseAbility.Ability.Blueprint.AssetGuid}");
#elif (KINGMAKER)
                Logger.Debug($"{__instance.Executor} - {__instance.Target} - {unitUseAbility.Spell.Name}");
#endif
                }
                //Sticky touch handling. Might need a rework to be more robust
#if (WOTR)
                if (unitUseAbility.Ability.Blueprint.StickyTouch != null)
#elif (KINGMAKER)
                if (unitUseAbility.Spell.Blueprint.StickyTouch != null)
#endif
                {
                    recordQueue.StickyTouch = true;
                }
                recordQueue.lastAction = __instance;
            }
            script_execution_mutex.WaitOne();
            foreach (var s in script_executors)
            {
                if (s.HandleUnitCommandDidEnd(__instance))
                {
                    break;
                }
            }
            if (script_executors.Count < 1)
            {
                ScriptController.executingQueueName = "";
                //TODO UIController.ResetToggles;
            }
            script_execution_mutex.ReleaseMutex();
        }
    }
}
