using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using static PathfinderAutoBuff.Main;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using PathfinderAutoBuff.QueueOperations;
using PathfinderAutoBuff.UnitLogic;

namespace PathfinderAutoBuff.QueueOperations
    /*
     * Queue controller for UI/GUI. In the future might be used as a base for the menu viewmodel 
     */
{
    class QueuesController
    {    
        //UI queue
        string m_CurrentQueueName = "";
        int m_CurrentQueueIndex;
        public QueueController queueController;
        //GUI queue
        string m_GUIQueueName = "";
        int m_GUIQueueIndex;
        private QueueController m_GUIQueueController;
        internal string[] m_Queues;
        public PartySpellList partySpellList;
        public PartyAbilityList partyAbilityList;
        public PartyActivatableList partyActivatableList;

        //Constructor
        public QueuesController()
        {
            ReloadQueues();
        }

        public string CurrentQueueName
        {
            get => this.m_CurrentQueueName;
            set => this.m_CurrentQueueName = value;
        }

        public int CurrentQueueIndex
        {
            get => this.m_CurrentQueueIndex;
            set {
                if (this.m_Queues.Length < value || value < 0)
                {
                    this.m_CurrentQueueIndex = -1;
                    this.m_CurrentQueueName = "";
                }
                else
                {
                    this.m_CurrentQueueIndex = value;
                    this.m_CurrentQueueName = m_Queues[value];
                }
            }
        }

        public string GUIQueueName
        {
            get => this.m_GUIQueueName;
            set => this.m_GUIQueueName = value;
        }

        public int GUIQueueIndex
        {
            get => this.m_GUIQueueIndex;
            set
            {
                if (this.m_Queues.Length < value || value < 0)
                {
                    this.m_GUIQueueIndex = -1;
                    this.m_GUIQueueName = "";
                }
                else
                {
                    this.m_GUIQueueIndex = value;
                    this.m_GUIQueueName = m_Queues[value];
                }
            }
        }

        public QueueController GUIQueueController
        {
            get => this.m_GUIQueueController;
            set => this.m_GUIQueueController = value;
        }

        //Reload queues from directory
        public void ReloadQueues()
        {
            string loadPath = Path.Combine(ModPath, "scripts");
            this.m_Queues = null;
            if (!Directory.Exists(loadPath))
            {
                Directory.CreateDirectory(loadPath);
                this.m_Queues = new string[] { };
            }
            List<string> queuesList = new List<string>();
            try
            {
                CommandQueue commandQueue = new CommandQueue();
                string [] queueFiles = Directory.GetFiles(loadPath, "*.json");
                foreach (string queueFile in queueFiles)
                {
                    string queueName = Path.GetFileName(queueFile);
                    if (commandQueue.LoadFromFile(queueName))
                    {
                        queuesList.Add(queueName.Substring(0, queueName.Length-5));
                    }
                }
            }
            catch (Exception e)
            {
#if (DEBUG)
                Logger.Debug(e.StackTrace);
                throw e;
#endif
            }
            this.m_Queues = queuesList.ToArray();
            Logger.Debug("Queues: " + String.Join("; ", m_Queues));
            try
            {
                if ((this.partySpellList == null) || (this.partyAbilityList == null) || (this.partyActivatableList == null))
                {
                    this.partySpellList = new PartySpellList();
                    this.partyAbilityList = new PartyAbilityList();
                    this.partyActivatableList = new PartyActivatableList();
                }
                else
                {
                    this.partySpellList.RefreshData();
                    this.partyAbilityList.RefreshData();
                    this.partyActivatableList.RefreshData();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
                throw ex;
            }
            //Cleaning up selected queues
            this.m_CurrentQueueName = "";
            this.m_CurrentQueueIndex = -1;
            if (this.queueController != null)
            {
                this.queueController.Clear();
                this.queueController = null;
            }
            this.m_GUIQueueIndex = -1;
            this.m_GUIQueueName = "";
            if (this.m_GUIQueueController != null)
            {
                this.m_GUIQueueController.Clear();
                this.m_GUIQueueController = null;
            }
        }
    }

    class QueueController
    {
        bool m_EditMode;
        internal string[] m_ActionNames;
        string m_CurrentActionName;
        int m_CurrentActionIndex;
        CommandQueue m_CurrentQueue;
        internal QueueMetadata m_CurrentMetadata;
        public bool actionsInit;
        public ActionController actionController;

        //Constructor
        public QueueController(CommandQueue commandQueue)
        {
            this.m_CurrentQueue = commandQueue;
            this.Refresh();
        }

        public void LoadMetadata(string queueName)
        {
            this.m_CurrentMetadata = new QueueMetadata(queueName);
        }

        //TODO: Refresh content
        public void Refresh()
        {
            this.m_ActionNames = GetActionNames();
            this.m_CurrentActionIndex = -1;
            this.m_CurrentActionName = "";
            if (this.actionController != null)
            {
                this.actionController.Clear();
                this.actionController = null;
            }
            this.actionsInit = false;
        }

        public void Clear()
        {
            this.m_ActionNames = new string[] { };
            this.m_CurrentActionIndex = -1;
            this.m_CurrentActionName = "";
            this.m_CurrentQueue = null;
            this.m_EditMode = false;
            if (this.actionController != null)
            {
                this.actionController.Clear();
                this.actionController = null;
            }
        }


        public int CurrentActionIndex
        {
            get => this.m_CurrentActionIndex;
            set => this.m_CurrentActionIndex = value;
        }

        public string CurrentActionName
        {
            get => this.m_CurrentActionName;
            set => this.m_CurrentActionName = value;
        }

        //Get queue action names for UI to display
        string[] GetActionNames()
        {
            List<string> ResultList = new List<string>();
            for (int i = 0; i < this.m_CurrentQueue.CommandList.Count; i++)
            {
                CommandQueueItem commandQueueItem = this.m_CurrentQueue.CommandList[i];
                string caster = "";
                if (commandQueueItem.CasterName != "" && commandQueueItem.CasterName != null)
                    caster = commandQueueItem.CasterName;
                else
                    caster = "Highest Duration";
                string actionName = $"#{i}-{caster}: {commandQueueItem.GetAbilityName()}. Targets: {commandQueueItem.GetRawTargetCount()}";
                ResultList.Add(actionName);
            }
            return ResultList.ToArray();
        }

        public CommandQueue CurrentQueue()
        {
            return m_CurrentQueue;
        }

        public QueueMetadata CurrentMetadata()
        {
            return m_CurrentMetadata;
        }
    }

    class ActionController
    {
        CommandQueueItem m_CurrentAction;
        bool m_EditMode;
        internal string casterName = null;
        //remove
        internal string abilityID = null;
        internal List<string> abilityIDs;
        internal Dictionary<string, List<int>> petIndex = null;
        public bool targetSelf = false;
        internal List<int> positions = null;
        internal List<string> characterNames = null;
        //Modifiers
        internal List<string> abilityMods;
        internal List<string> activatableMods;
        internal CommandQueueItem.ActionTypes actionType;

        //Constructor for existing action
        public ActionController(CommandQueueItem commandQueueItem)
        {
            this.m_CurrentAction = commandQueueItem;
            abilityIDs = new List<string>();
            abilityMods = new List<string>();
            activatableMods = new List<string>();
        }

        //Constructor for new action
        public ActionController()
        {
            this.m_CurrentAction = new CommandQueueItem();
            abilityIDs = new List<string>();
            abilityMods = new List<string>();
            activatableMods = new List<string>();
        }


        //RefreshData
        public void Refresh()
        {

        }

        public CommandQueueItem CurrentAction()
        {
            return this.m_CurrentAction;
        }

        public void SetCurrentAction(CommandQueueItem commandQueueItem)
        {
            this.m_CurrentAction = commandQueueItem;
        }

        //Cleanup
        public void Clear()
        {
            this.casterName = null;
            this.abilityIDs = new List<string>();
            this.petIndex = null;
            this.positions = null;
            this.characterNames = null;
            this.abilityMods = new List<string>();
            this.activatableMods = new List<string>();
            this.m_CurrentAction = null;
            this.targetSelf = false;
        }
    }
}
