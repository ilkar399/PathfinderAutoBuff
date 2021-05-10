using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using PathfinderAutoBuff.Controllers;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Main;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using static PathfinderAutoBuff.Utility.IOHelpers;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;
using static PathfinderAutoBuff.Utility.Extensions.CommonExtensions;
using PathfinderAutoBuff.Menu.QueuesComponents;
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.UnitLogic;
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif

namespace PathfinderAutoBuff.Menu
{
    class QueuesTest : IMenuSelectablePage
    /*
     * Queues Manu Page
     * Relies on QueueController
     */
    {
        public string Name => "QueuesTest";
        public int Priority => 800;
        private Vector2 _scrollPosition;
        private Vector2 _scrollPosition2;
        private Vector2 _scrollPositionSpells;
        private Vector2 _scrollPositionAbilities;
        private QueuesController queuesController;
        private string uiQueueName;
        private int actionEditStage = -1;
        private int currentQueueIndex = -1;
        private int currentActionIndex = -1;
        private Dictionary<int, bool> targetSelection;
        private List<string> partyNamesOrdered;
        private float uiScale = 1f;
        private float leftBoxWidth = 300f;
        private float rightBoxWidth = 600f;
        private string[] styledActionNames;
        private string[] castersUIArray;
        private int currentCasterIndex = -1;
        private int currentTargetTypeIndex = -1;
        private string[] targetTypeArray = { "Caster", "Character name", "Formation Position" };
        private int abilityFilterIndex = 0;
        private string[] abilityFilterArray = { "Selected caster", "Available to all Party" };
        private int abilityLevelIndex = 0;
        private string[] abilityLevelArray = { "#1", "#2", "#3", "#4", "#5", "#6", "#7", "#8", "#9" };
        private int selectedSpellIndex = -1;
        private string[] selectedSpellIDs, selectedSpellNames;
        private int selectedAbilityIndex = -1;
        private string[] selectedAbilityIDs, selectedAbilityNames;
        private AbilityDataUI selectedAbility;
        private ActivatableDataUI selectedActivatable;
        private bool styleInit = false;
        private List<ActionItemView> actionItemsViews = new List<ActionItemView>();
        private GUIStyle buttonFixed120, labelfixed120, buttonDefault, buttonWrapped, buttonSelector, labelDefault, textField200, buttonSelectorLeft;


        public void OnGUI(UnityModManager.ModEntry modentry)
        {
            if (!Main.Enabled) return;
            if (!Main.IsInGame)
            {
                GUILayout.Label(Local["Menu_All_Label_NotInGame"].Color(RGBA.red));
                return;
            }
            if (!styleInit)
            {
                buttonFixed120 = DefaultStyles.ButtonFixed120();
                labelDefault = DefaultStyles.LabelDefault();
                labelfixed120 = DefaultStyles.LabelFixed120();
                buttonDefault = DefaultStyles.ButtonDefault();
                buttonWrapped = DefaultStyles.ButtonWrapped();
                buttonSelector = DefaultStyles.ButtonSelector();
                buttonSelectorLeft = DefaultStyles.ButtonSelectorLeft();
                textField200 = DefaultStyles.TextField200();
                styleInit = true;
            }
            bool queueDeletionFlag = true;
            string activeScene = SceneManager.GetActiveScene().name;
            if (queuesController == null)
                queuesController = new QueuesController();
            //Overall layout
            GUILayout.BeginVertical();
            //Queues
            //QueueList
            using (new GUILayout.VerticalScope("box", GUILayout.ExpandHeight(true)))
            {
                //Reload list
                if (GUILayout.Button(Local["Menu_Queues_ReloadData"], buttonFixed120))
                {
                    ReloadData();
                    return;
                }
                if (queuesController.queueController == null)
                {
                    //New queue
                    if (GUILayout.Button(Local["Menu_Queues_NewQueue"], buttonFixed120))
                    {
                        List<CommandQueueItem> commandList = new List<CommandQueueItem>();
                        CommandQueue commandQueue = new CommandQueue();
                        commandQueue.CommandList = commandList;
                        queuesController.queueController = new QueueController(commandQueue);
                        uiQueueName = "New queue";
                        queuesController.CurrentQueueName = "New queue";
                        queuesController.CurrentQueueIndex = -1;
                    }
                    GUILayout.Label(Local["Menu_Queues_QueueList"], labelfixed120, GUILayout.ExpandWidth(true));
                    //Queue list UI
                    using (var ScrollView = new GUILayout.ScrollViewScope(_scrollPosition))
                    {
                        _scrollPosition = ScrollView.scrollPosition;
                        Utility.UI.SelectionGrid(ref currentQueueIndex, queuesController.m_Queues, 5, () =>
                        {
                            queuesController.CurrentQueueIndex = currentQueueIndex;
                            if (queuesController.queueController == null)
                            {
                                CommandQueue commandQueue = new CommandQueue();
                                commandQueue.LoadFromFile($"{queuesController.CurrentQueueName}.json");
                                queuesController.queueController = new QueueController(commandQueue);
                                uiQueueName = queuesController.CurrentQueueName;
                            }
                            else
                            {
                                queuesController.queueController.Clear();
                                queuesController.queueController = null;
                                CommandQueue commandQueue = new CommandQueue();
                                commandQueue.LoadFromFile($"{queuesController.CurrentQueueName}.json");
                                queuesController.queueController = new QueueController(commandQueue);
                                uiQueueName = queuesController.CurrentQueueName;
                            }

                        }, buttonSelector, GUILayout.ExpandWidth(false));
                    }
                }
            }
            if (queuesController.queueController != null)
            {
                //Selected Queue
                GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Local["Menu_Queues_QueueName"],
                        labelDefault, UI.ExpandWidth(false));
                    Utility.UI.TextField(ref uiQueueName, textField200);
                }
                using (new GUILayout.HorizontalScope())
                {
                    //Execute queue
                    GUILayout.Space(5f);
                    if (GUILayout.Button(Local["Menu_Queues_ExecuteQueue"], buttonFixed120, GUILayout.ExpandWidth(false)))
                    {
                        ScriptController.Reset();
                        ScriptController.CreateFromQueue(queuesController.queueController.CurrentQueue().CommandList,
                                                        queuesController.CurrentQueueName);
                        ScriptController.Run();
                    }
                    //Favorite queue saving
                    if (GUILayout.Button(Local["Menu_Queues_SaveQueue"], buttonFixed120, GUILayout.ExpandWidth(false)))
                    {
                        bool saveResult = queuesController.queueController.CurrentQueue().SaveToFile(uiQueueName);
                        if (saveResult)
                        {
                            ReloadData();
                            return;
                        }
                        else
                            GUILayout.Label(string.Format(Local["Menu_Queues_ErrorSaving"], uiQueueName).Color(RGBA.red));
                    }
                    //Cancel changes
                    if (GUILayout.Button(Local["Menu_Queues_CancelChanges"], buttonFixed120, GUILayout.ExpandWidth(false)))
                    {
                        ReloadData();
                        return;
                    }
                    //Delete
                    if (GUILayout.Button(Local["Menu_Queues_DeleteQueue"], buttonFixed120, GUILayout.ExpandWidth(false)))
                    {
                        queueDeletionFlag = DeleteQueue(uiQueueName);
                        if (!queueDeletionFlag)
                            GUILayout.Label(string.Format(Local["Menu_Queues_ErrorDeleting"], uiQueueName).Color(RGBA.red));
                        else
                        {
                            ReloadData();
                            return;
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            //Actions
            //Queue Edit/Info Mode
            if (queuesController.queueController != null)
            {
                //New Spell/Ability
                UI.Horizontal(() => {
                    if (GUILayout.Button(Local["Menu_Queues_NewSpell"], buttonFixed120, GUILayout.ExpandWidth(false)))
                    {
                        NewAction(queuesController,CommandQueueItem.ActionTypes.Spell);
                    }
                    if (GUILayout.Button(Local["Menu_Queues_NewAbility"], buttonFixed120, GUILayout.ExpandWidth(false)))
                    {
                        NewAction(queuesController,CommandQueueItem.ActionTypes.Ability);
                    }
                },"box");
                //Action list init/view/edit
                if (!queuesController.queueController.actionsInit)
                {
                    actionItemsViews.Clear();
                    foreach (CommandQueueItem commandQueueItem in queuesController.queueController.CurrentQueue().CommandList)
                    {
                        ActionItemView actionItemView = new ActionItemView(queuesController, commandQueueItem);
                        actionItemsViews.Add(actionItemView);
                    }
                    queuesController.queueController.actionsInit = true;
                }
                if (queuesController.queueController.actionsInit)
                {
                    for (int i = 0; i < actionItemsViews.Count; i++)
                        actionItemsViews[i].OnGUI();
                }
            }
            GUILayout.EndVertical();
        }

        //
        private void NewAction(QueuesController queuesController, CommandQueueItem.ActionTypes actionType)
        {
            queuesController.queueController.actionController = null;
            queuesController.queueController.actionController = new ActionController();
            if (targetSelection == null)
                targetSelection = new Dictionary<int, bool>();
            Target.GetTargetSelectionDict(ref targetSelection);
            queuesController.queueController.actionController.actionType = actionType;
            CommandQueueItem commandQueueItemNew = queuesController.queueController.actionController.CurrentAction();
            queuesController.queueController.CurrentQueue().CommandList.Insert(0, commandQueueItemNew);
            queuesController.queueController.actionsInit = false;

        }

        private void ResetActionEdit()
        {
            actionEditStage = -1;
            currentTargetTypeIndex = -1;
            currentCasterIndex = -1;
            selectedSpellIndex = -1;
            selectedAbility = null;
            selectedActivatable = null;
            castersUIArray = null;
            partyNamesOrdered = Target.GetPartyNamesOrder();
        }

        private void ReloadData()
        {
            queuesController.ReloadQueues();
            currentQueueIndex = -1;
            actionEditStage = -1;
            currentQueueIndex = -1;
            currentActionIndex = -1;
            targetSelection = null;
            partyNamesOrdered = null;
            styledActionNames = null;
            castersUIArray = null;
            currentTargetTypeIndex = -1;
            abilityFilterIndex = 0;
            abilityLevelIndex = 0;
            selectedSpellIndex = -1;
            selectedSpellIDs = null;
            selectedSpellNames = null;
        }
    }
}
