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
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.UnitLogic;

namespace PathfinderAutoBuff.Scripting
    /*
     * Queue controller for UI. In the future might be used as a base for the menu viewmodel 
     */
{
    class QueuesController
    {    
        string m_CurrentQueueName = "";
        int m_CurrentQueueIndex;
        internal string [] m_Queues;
        public QueueController queueController;
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

        /*
         * Not used. Try to change to something less resource-intensive
         * Initially planned as a check for party changes
         * Maybe chenge into handlers?
         */
        public bool TestCriticalChanges()
        { 
            PartySpellList partySpellList1 = new PartySpellList();
            List<string> units1 = new List<string>();
            List<string> abilities1 = new List<string>();
            List<string> units2 = new List<string>();
            List<string> abilities2 = new List<string>();

            foreach (PartySpellData partySpellData in partySpellList1.m_AllSpells)
            {
                if (!units1.Contains(partySpellData.Caster.CharacterName))
                    units1.Add(partySpellData.Caster.CharacterName);
                if (!abilities1.Contains(partySpellData.Blueprint.Name))
                    abilities1.Add(partySpellData.Blueprint.Name);
            }
            foreach (PartySpellData partySpellData in this.partySpellList.m_AllSpells)
            {
                if (!units2.Contains(partySpellData.Caster.CharacterName))
                    units2.Add(partySpellData.Caster.CharacterName);
                if (!abilities2.Contains(partySpellData.Blueprint.Name))
                    abilities2.Add(partySpellData.Blueprint.Name);
            }
            return (Enumerable.SequenceEqual(units1, units2) && Enumerable.SequenceEqual(abilities1,abilities2));
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
            this.m_CurrentQueueName = "";
            this.m_CurrentQueueIndex = -1;
            if (this.queueController != null)
            {
                this.queueController.Clear();
                this.queueController = null;
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
        public bool actionsInit;
        public ActionController actionController;
        public Dictionary<int, string> favoriteQueues;

        //Constructor
        public QueueController(CommandQueue commandQueue)
        {
            this.m_CurrentQueue = commandQueue;
            this.Refresh();
        }

        //TODO: Refresh content
        public void Refresh()
        {
            this.m_ActionNames = GetActionNames();
            this.m_CurrentActionIndex = -1;
            this.m_CurrentActionName = "";
            EditMode = false;
            if (this.actionController != null)
            {
                this.actionController.Clear();
                this.actionController = null;
            }
            favoriteQueues = new Dictionary<int, string>(FavoriteQueues);
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

        //Queue Edit Mode
        public bool EditMode
        {
            get => this.m_EditMode;
            set
            {
                this.m_EditMode = value;
                //TODO: Reset stuff
            }
        }

        public CommandQueue CurrentQueue()
        {
            return m_CurrentQueue;
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
            this.EditMode = false;
        }

        //Constructor for new action
        public ActionController()
        {
            abilityIDs = new List<string>();
            abilityMods = new List<string>();
            activatableMods = new List<string>();
            this.EditMode = true;
        }


        //RefreshData
        public void Refresh()
        {

        }

        //Action Edit Mode
        public bool EditMode
        {
            get => this.m_EditMode;
            set
            {
                this.m_EditMode = value;
                //TODO: Reset stuff
            }
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
            EditMode = false;
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
