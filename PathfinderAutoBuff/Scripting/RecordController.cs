using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.EntitySystem.Entities;
using PathfinderAutoBuff.Utility;
using PathfinderAutoBuff.Scripting;
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif

namespace PathfinderAutoBuff.Scripting
{
    /*
     * Controller for the action recording
     *  
     * 
     */
    public class RecordController :
    /*
     * Used interfaces:
     *  IModEventHandler
     *  IAreaHandler/ISceneHandler (WotR/PKM)
     * 
    */
#if (WOTR)
    IAreaHandler
#elif (KINGMAKER)
        ISceneHandler
#endif
    {

        public class RecordedAction
        /*
         * Raw recorded action
         * Action description
         * UnitEntity Caster
         * UnitEntity Target
         * BlueprintAbility Ability
         * enum ActionType
         */
        {
            public UnitEntityData Caster;
            [CanBeNull]
            public UnitEntityData Target;
            public BlueprintAbility Ability;
            public CommandQueueItem.ActionTypes RecordedActionType;

            public RecordedAction(UnitEntityData caster, BlueprintAbility blueprintAbility, CommandQueueItem.ActionTypes actionType, UnitEntityData target = null)
            {
                this.Caster = caster;
                this.Target = target;
                this.Ability = blueprintAbility;
                this.RecordedActionType = actionType;
            }
        }

        internal class PreparedAction
         /*
          * Action prepared for saving into action queue. After grouping actions, converting to positions, etc.
          */
        {
            public string CasterName;
            [CanBeNull] public List<int> Positions;
            [CanBeNull] public List<UnitEntityData> TargetUnits;
            public string AbilityId;
            public CommandQueueItem.ActionTypes ActionType;

            public PreparedAction(string casterName, List<int> positions, List<UnitEntityData> targetUnits, string abilityId, CommandQueueItem.ActionTypes actionType)
            {
                this.CasterName = casterName;
                this.Positions = positions;
                this.TargetUnits = targetUnits;
                this.AbilityId = abilityId;
                this.ActionType = actionType;
            }
        }

        [CanBeNull]
        public Kingmaker.UnitLogic.Commands.Base.UnitCommand lastAction;
        [CanBeNull]
        public bool StickyTouch;
        public int Priority => 700;
        //Add wrappers
        private bool m_Enabled = false;
        private int maxActions = 100;
        public Queue<RecordedAction> RecordedQueue;

        //Constructor
        public RecordController()
        {
            this.m_Enabled = false;
        }

        //Enable status wrapper
        public bool Enabled
        {
            get => this.m_Enabled;
        }

        //TODO: remake into the Enabled wrapper?
        public void Disable()
        {
            this.m_Enabled = false;
            Logger.Debug("Record disabled");
        }

        public void Enable()
        {
            this.m_Enabled = true;
            this.RecordedQueue = new Queue<RecordedAction>(maxActions);
            Logger.Debug("Record enabled");
        }

        //TODO: Cleaning up
        public void Clear()
        {
            Disable();
            this.RecordedQueue = new Queue<RecordedAction>(maxActions); ;
        }

        //Handlers
        public void ModEnable()
        {
            EventBus.Subscribe(this);
        }

        public void ModDisable()
        {
            EventBus.Unsubscribe(this);
            Main.recordQueue = null;
            this.Disable();
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            this.Disable();
        }

        //Adding new action
        public bool AddAction(UnitCommand unitCommand)
        {
            //Caster
            if (!Kingmaker.Game.Instance.Player.Party.Contains(unitCommand.Executor))
                return false;
            if (unitCommand as UnitUseAbility == null)
                return false;
            UnitEntityData caster = unitCommand.Executor;
            //Ability
#if (WOTR)
            AbilityData abilityData = (unitCommand as UnitUseAbility).Ability;
#elif  (KINGMAKER)
            AbilityData abilityData = (unitCommand as UnitUseAbility).Spell;
#endif
            UnitUseAbility unitUseAbility = unitCommand as UnitUseAbility;
            if (abilityData.Blueprint.StickyTouch != null)
            {
                if (LogicHelpers.FlattenAllActions(abilityData.Blueprint.StickyTouch.TouchDeliveryAbility, true).OfType<ContextActionApplyBuff>().Count() < 1)
                    return false;
            }
            else
            {
                if (LogicHelpers.FlattenAllActions(abilityData.Blueprint, true).OfType<ContextActionApplyBuff>().Count() < 1)
                    return false;
            }
            BlueprintAbility blueprintAbility = abilityData.Blueprint;
            //Target
            UnitEntityData target;
#if (WOTR)
            if (unitCommand.TargetUnit != null && Kingmaker.Game.Instance.Player.PartyAndPets.Contains(unitCommand.TargetUnit))
#elif  (KINGMAKER)
            if (unitCommand.TargetUnit != null && Kingmaker.Game.Instance.Player.PartyAndPets().Contains(unitCommand.TargetUnit))
#endif
            {
                target = unitCommand.TargetUnit;
            }
            else
                target = null;
            //Action Type
            CommandQueueItem.ActionTypes actionType = abilityData.Spellbook != null ? CommandQueueItem.ActionTypes.Spell : CommandQueueItem.ActionTypes.Ability;
            if (this.RecordedQueue == null)
                this.RecordedQueue = new Queue<RecordedAction>(maxActions);
            RecordedQueue.Enqueue(new RecordedAction(caster, blueprintAbility, actionType, target));
            return true;
        }

        //Saving record controller
        public (bool, string) SaveRecordQueue(string queueName, bool groupActions = true, bool usePositions = false)
        {
            List<PreparedAction> preparedActions = new List<PreparedAction>();
            List<UnitEntityData> partyOrder = Target.GetPartyOrder();
            //Processing recording queue, grouping actions if necessary and preparing data for the new CommandQueue
            foreach (RecordedAction recordedAction in this.RecordedQueue)
            {
                int position = (recordedAction.Target == null) ? -1 : partyOrder.IndexOf(recordedAction.Target);
                if (groupActions)
                {
                    PreparedAction existingAction = preparedActions.FirstOrDefault(preparedAction =>
                    {
                        if (preparedAction.CasterName == recordedAction.Caster.CharacterName
                         && preparedAction.AbilityId == recordedAction.Ability.AssetGuid)
                        {
                            return true;
                        }
                        return false;
                    });
                    if (existingAction == null)
                    {
                        preparedActions.Add(new PreparedAction(
                            recordedAction.Caster.CharacterName,
                            new List<int> { position },
                            new List<UnitEntityData> { recordedAction.Target },
                            recordedAction.Ability.AssetGuid,
                            recordedAction.RecordedActionType));
                    }
                    else
                    {
                        if (!(existingAction.Positions.Contains(position)))
                        {
                            existingAction.TargetUnits.Add(recordedAction.Target);
                            existingAction.Positions.Add(position);
                        }
                    }

                }
                else
                {
                    preparedActions.Add(new PreparedAction(
                        recordedAction.Caster.CharacterName,
                        new List<int> { position },
                        new List<UnitEntityData> { recordedAction.Target },
                        recordedAction.Ability.AssetGuid,
                        recordedAction.RecordedActionType));
                }
            }
            //Creating commands and putting them in the queue
            CommandQueue commandQueue = new CommandQueue();
            commandQueue.CommandList = new List<CommandQueueItem>();
            foreach (PreparedAction preparedAction in preparedActions)
            {
                //If the ability is self-casted
                if (preparedAction.Positions.Count == 1)
                {
                    if (preparedAction.Positions[0] == -1)
                    {
                        CommandQueueItem commandQueueItem = new CommandQueueItem(
                            preparedAction.CasterName,
                            preparedAction.AbilityId,
                            true,
                            null,
                            null,
                            null,
                            null,
                            null,
                            preparedAction.ActionType
                            );
                        commandQueue.CommandList.Add(commandQueueItem);
                        continue;
                    }
                }

                if (usePositions)
                {
                    //If the normal target list
                    CommandQueueItem commandQueueItem1 = new CommandQueueItem(
                        preparedAction.CasterName,
                        preparedAction.AbilityId,
                        false,
                        null,
                        preparedAction.Positions,
                        null,
                        null,
                        null,
                        preparedAction.ActionType
                        );
                    commandQueue.CommandList.Add(commandQueueItem1);
                }
                else
                {
                    //Creating List <string> CharacterNames and Dictionary<string, List<int>> PetIndex for CommandQueue
                    List<string> characterNames = new List<string>();
                    Dictionary<string, List<int>> petIndex = new Dictionary<string, List<int>>();
                    foreach (UnitEntityData targetUnit in preparedAction.TargetUnits)
                    {
                        if (LogicHelpers.IsPetWrapper(targetUnit))
                        {
                            UnitEntityData master = LogicHelpers.MasterWrapper(targetUnit);
                            if (petIndex.ContainsKey(master.CharacterName))
                            {
#if (WOTR)
                                petIndex[master.CharacterName].Add(master.Pets.IndexOf(targetUnit));
#elif (KINGMAKER)
                                petIndex[master.CharacterName].Add(master.Pets().IndexOf(targetUnit));
#endif
                            }
                            else
                            {
#if (WOTR)
                                petIndex[master.CharacterName] = new List<int> { master.Pets.IndexOf(targetUnit) };
#elif (KINGMAKER)
                                petIndex[master.CharacterName] = new List<int> { master.Pets().IndexOf(targetUnit) };
#endif
                            }
                        }
                        else
                        {
                            characterNames.Add(targetUnit.CharacterName);
                        }
                    }
                    //Creating and adding a new command
                    //If the normal target list
                    CommandQueueItem commandQueueItem = new CommandQueueItem(
                        preparedAction.CasterName,
                        preparedAction.AbilityId,
                        false,
                        characterNames,
                        null,
                        petIndex,
                        null,
                        null,
                        preparedAction.ActionType
                        );
                    commandQueue.CommandList.Add(commandQueueItem);
                }
            }
            if (commandQueue.CommandList.Count > 0)
            {
                commandQueue.SaveToFile(queueName);
                return (true, $"Queue saved with {queueName}. Number of items in the queue: {commandQueue.CommandList.Count}");
            }
            else
                return (false, "Queue not saved");
        }

    }
}
