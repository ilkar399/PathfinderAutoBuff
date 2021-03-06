using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.PubSubSystem;
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
using PathfinderAutoBuff.QueueOperations;
using PathfinderAutoBuff.UnitLogic;
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif

namespace PathfinderAutoBuff.Menu
{
    class Queues : IMenuSelectablePage,
#if (WOTR)
    IAreaHandler
#elif (KINGMAKER)
        ISceneHandler
#endif
    /*
     * Queues Manu Page
     * Relies on QueueController
     */
    {
        public string Name => "Queues";
        public int Priority => 100;
        private Vector2 _scrollPosition;
        private string uiQueueName;
        private int currentQueueIndex = -1;
        private Dictionary<int, bool> targetSelection;
        private List<string> partyNamesOrdered;
        private float uiScale = 1f;
        private List<ActionItemView> actionItemsViews = new List<ActionItemView>();
        bool init;
        QueueMetadataSettings queueMetadataSettings;


        public void OnGUI(UnityModManager.ModEntry modentry)
        {
            if (!Main.Enabled)
            {
                Main.QueuesController = null;
                EventBus.Unsubscribe(this);
                return;
            }
            if (!Main.IsInGame)
            {
                GUILayout.Label(Local["Menu_All_Label_NotInGame"].Color(RGBA.red));
                return;
            }
            if (!init)
            {
                EventBus.Subscribe(this);
                init = true;
            }
            bool queueDeletionFlag = true;
            string activeScene = SceneManager.GetActiveScene().name;
            if (Main.QueuesController == null)
            {
                EventBus.Subscribe(this);
                Main.QueuesController = new QueuesController();
            }
            //Overall layout
            GUILayout.BeginVertical();
            //Queues
            //QueueList
            using (new GUILayout.VerticalScope("box", GUILayout.ExpandHeight(true)))
            {
                //Reload list
                if (GUILayout.Button(Local["Menu_Queues_ReloadData"], DefaultStyles.ButtonFixed120()))
                {
                    ReloadData();
                    return;
                }
                /*
                if (Main.QueuesController.CurrentQueueName != uiQueueName && 
                        Main.QueuesController.CurrentQueueName != null)
                    uiQueueName = Main.QueuesController.CurrentQueueName;
                */
                //New queue
                if (GUILayout.Button(Local["Menu_Queues_NewQueue"], DefaultStyles.ButtonFixed120()))
                {
                    List<CommandQueueItem> commandList = new List<CommandQueueItem>();
                    CommandQueue commandQueue = new CommandQueue();
                    commandQueue.CommandList = commandList;
                    Main.QueuesController.queueController = new QueueController(commandQueue);
                    Main.QueuesController.CurrentQueueName = "New queue";
                    uiQueueName = Main.QueuesController.CurrentQueueName;
                    Main.QueuesController.CurrentQueueIndex = -1;
                    Main.QueuesController.queueController.LoadMetadata("");
                    Main.QueuesController.queueController.CurrentMetadata().QueueName = Main.QueuesController.CurrentQueueName;
                }
                GUILayout.Label(Local["Menu_Queues_QueueList"], DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(true));
                //Queue list UI
                using (var ScrollView = new GUILayout.ScrollViewScope(_scrollPosition))
                {
                    _scrollPosition = ScrollView.scrollPosition;
                    Utility.UI.SelectionGrid(ref currentQueueIndex, Main.QueuesController.m_Queues, 5, () =>
                    {
                        CommandQueue commandQueue;
                        Main.QueuesController.CurrentQueueIndex = currentQueueIndex;
                        if (Main.QueuesController.queueController == null)
                        {
                            commandQueue = new CommandQueue();
                            commandQueue.LoadFromFile($"{Main.QueuesController.CurrentQueueName}.json");
                        }
                        else
                        {
                            Main.QueuesController.queueController.Clear();
                            Main.QueuesController.queueController = null;
                            commandQueue = new CommandQueue();
                            commandQueue.LoadFromFile($"{Main.QueuesController.CurrentQueueName}.json");
                        }
                        Main.QueuesController.queueController = new QueueController(commandQueue);
                        uiQueueName = Main.QueuesController.CurrentQueueName;
                        Main.QueuesController.queueController.LoadMetadata(Main.QueuesController.CurrentQueueName);
                        queueMetadataSettings = new QueueMetadataSettings(Main.QueuesController);

                    }, DefaultStyles.ButtonSelector(), GUILayout.ExpandWidth(false));
                }
            }
            if (Main.QueuesController.queueController != null && uiQueueName != null)
            {
                //Selected Queue
                GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Local["Menu_Queues_QueueName"],
                        DefaultStyles.LabelDefault(), UI.ExpandWidth(false));
                    Utility.UI.TextField(ref uiQueueName, DefaultStyles.TextField200());
                }
                using (new GUILayout.HorizontalScope())
                {
                    //Execute queue
                    GUILayout.Space(5f);
                    if (GUILayout.Button(Local["Menu_Queues_ExecuteQueue"], DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
                    {
                        ScriptController.Reset();
                        ScriptController.CreateFromQueue(Main.QueuesController.queueController.CurrentQueue().CommandList,
                                                        Main.QueuesController.CurrentQueueName);
                        ScriptController.Run();
                    }
                    //Queue saving
                    if (GUILayout.Button(Local["Menu_Queues_SaveQueue"], DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
                    {
                        Logger.Debug(uiQueueName);
                        bool saveResult = Main.QueuesController.queueController.CurrentQueue().SaveToFile(uiQueueName);
                        Main.QueuesController.queueController.CurrentMetadata().QueueName = uiQueueName;
                        saveResult = saveResult && Main.QueuesController.queueController.CurrentMetadata().Save();
                        if (saveResult)
                        {
                            ReloadData();
                            return;
                        }
                        else
                            GUILayout.Label(string.Format(Local["Menu_Queues_ErrorSaving"], uiQueueName).Color(RGBA.red));
                    }
                    //Cancel changes
                    if (GUILayout.Button(Local["Menu_Queues_CancelChanges"], DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
                    {
                        ReloadData();
                        return;
                    }
                    //Favorite
                    Utility.UI.ToggleButton(FavoriteQueues2.Contains(uiQueueName),Local["Menu_Queues_Favorite"],
                    () =>
                    {
                        if (!FavoriteQueues2.Contains(uiQueueName))
                            FavoriteQueues2.Add(uiQueueName);
                    },
                    () =>
                    {
                        if (FavoriteQueues2.Contains(uiQueueName))
                            FavoriteQueues2.Remove(uiQueueName);
                    },
                    DefaultStyles.ButtonFixed120()
                    );
                    //Delete
                    if (GUILayout.Button(Local["Menu_Queues_DeleteQueue"], DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
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
                //Queue Metadata Settings
                if (this.queueMetadataSettings != null)
                {
                    this.queueMetadataSettings.OnGUI();
                }
                GUILayout.EndVertical();
            }
            //Actions
            //Queue Edit/Info Mode
            if (Main.QueuesController.queueController != null && uiQueueName != null)
            {
                //New Spell/Ability
                UI.Horizontal(() => {
                    if (GUILayout.Button(Local["Menu_Queues_NewSpell"], DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
                    {
                        NewAction(Main.QueuesController, CommandQueueItem.ActionTypes.Spell);
                    }
                    if (GUILayout.Button(Local["Menu_Queues_NewAbility"], DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
                    {
                        NewAction(Main.QueuesController, CommandQueueItem.ActionTypes.Ability);
                    }
                }, "box");
                //Action list init/view/edit
                if (!Main.QueuesController.queueController.actionsInit)
                {
                    actionItemsViews.Clear();
                    foreach (CommandQueueItem commandQueueItem in Main.QueuesController.queueController.CurrentQueue().CommandList)
                    {
                        ActionItemView actionItemView = new ActionItemView(Main.QueuesController, commandQueueItem);
                        actionItemsViews.Add(actionItemView);
                    }
                    Main.QueuesController.queueController.actionsInit = true;
                }
                if (Main.QueuesController.queueController.actionsInit)
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
            ActionController selectedActionController = queuesController.queueController.actionController;
            if (selectedActionController != null)
                if (selectedActionController.casterName == null ||
                (selectedActionController.abilityIDs?.Count < 1) ||
                (selectedActionController.targetSelf == false & selectedActionController.characterNames == null & selectedActionController.positions == null & selectedActionController.petIndex == null)
                )
                    return;
            queuesController.queueController.actionController = null;
            queuesController.queueController.actionController = new ActionController();
            queuesController.queueController.actionController.actionType = actionType;
            CommandQueueItem commandQueueItemNew = queuesController.queueController.actionController.CurrentAction();
            queuesController.queueController.CurrentQueue().CommandList.Insert(0, commandQueueItemNew);
            queuesController.queueController.actionsInit = false;

        }

        private void ReloadData()
        {
            Main.QueuesController.ReloadQueues();
            if (Main.uiController != null)
                if (Main.uiController.AutoBuffGUI.isActiveAndEnabled)
                    Main.uiController.AutoBuffGUI.RefreshView();
//            currentQueueIndex = Array.IndexOf(Main.QueuesController.m_Queues, Main.QueuesController.CurrentQueueName);
            currentQueueIndex = -1;
            targetSelection = null;
            partyNamesOrdered = null;
            uiQueueName = null;
        }

        private void OnGUIUpdate()
        {
            if (Main.QueuesController.queueController != null)
            {
                this.uiQueueName = Main.QueuesController.queueController.CurrentActionName;
                this.queueMetadataSettings = new QueueMetadataSettings(Main.QueuesController);
            }
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            Logger.Debug("OnAreaDidLoad");
            this.ReloadData();
            Main.QueuesController.queueController.actionsInit = false;
        }
    }
}
