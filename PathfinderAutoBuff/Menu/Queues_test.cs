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
                //Edit/View queue
                QueueController selectedQueueController = queuesController.queueController;
                List<string> styledActionNamesList = new List<string>();
                foreach (CommandQueueItem commandQueueItem in selectedQueueController.CurrentQueue().CommandList)
                {
                    int i = selectedQueueController.CurrentQueue().CommandList.IndexOf(commandQueueItem);
                    string styledString = StatusStyled(commandQueueItem.GetStatus(queuesController.partySpellList), selectedQueueController.m_ActionNames[i]);
                    styledActionNamesList.Add(styledString);
                }
                styledActionNames = styledActionNamesList.ToArray();
                styledActionNamesList = null;
                GUILayout.BeginHorizontal("box", GUILayout.MinHeight(300f));
                //Action List
                using (new GUILayout.VerticalScope("box", GUILayout.Width(Math.Max(300f, Main.ummWidth / 4)), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(Local["Menu_Queues_ActionList"], labelfixed120, GUILayout.ExpandWidth(true));
                        //New spellcasting action
                        if (GUILayout.Button(Local["Menu_Queues_NewSpell"], buttonFixed120, GUILayout.ExpandWidth(false)))
                        {
                            selectedQueueController.actionController = null;
                            selectedQueueController.actionController = new ActionController();
                            if (targetSelection == null)
                                targetSelection = new Dictionary<int, bool>();
                            Target.GetTargetSelectionDict(ref targetSelection);
                            selectedQueueController.actionController.actionType = CommandQueueItem.ActionTypes.Spell;
                            ResetActionEdit();
                            actionEditStage = 0;
                            currentActionIndex = -1;
                            return;
                        }
                        //New ability action
                        if (GUILayout.Button(Local["Menu_Queues_NewAbility"], buttonFixed120, GUILayout.ExpandWidth(false)))
                        {
                            selectedQueueController.actionController = null;
                            selectedQueueController.actionController = new ActionController();
                            if (targetSelection == null)
                                targetSelection = new Dictionary<int, bool>();
                            Target.GetTargetSelectionDict(ref targetSelection);
                            selectedQueueController.actionController.actionType = CommandQueueItem.ActionTypes.Ability;
                            ResetActionEdit();
                            actionEditStage = 0;
                            currentActionIndex = -1;
                            return;
                        }
                    }
                    GUILayout.Space(10f);
                    //Action List
                    using (var ScrollView = new GUILayout.ScrollViewScope(_scrollPosition2))
                    {
                        _scrollPosition2 = ScrollView.scrollPosition;
                        Utility.UI.SelectionGrid(ref currentActionIndex, styledActionNames, 1, () =>
                        {
                            ResetActionEdit();
                            if (selectedQueueController.actionController == null)
                            {
                                selectedQueueController.actionController = new ActionController(selectedQueueController.CurrentQueue().CommandList[currentActionIndex]);
                            }
                            else
                            {
                                selectedQueueController.actionController.Clear();
                                selectedQueueController.actionController = null;
                                selectedQueueController.actionController = new ActionController(selectedQueueController.CurrentQueue().CommandList[currentActionIndex]);
                            }
                        }, buttonSelectorLeft, GUILayout.ExpandWidth(false));
                    }
                }
                //Action data panel
                ActionController selectedActionController = selectedQueueController.actionController;
                using (new GUILayout.VerticalScope("box", GUILayout.ExpandHeight(true)))
                {
                    if (selectedQueueController.actionController != null)
                    {
                        //Edit/View action
                        PartySpellList partySpellList = queuesController.partySpellList;
                        if (selectedQueueController.actionController.EditMode)
                        {
                            //Edit action
                            GUILayout.BeginVertical();
                            if (selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell)
                            {
                                string[] actionEditStrings = {Local["Menu_Queues_Caster"],
                                    Local["Menu_Queues_Spell"],
                                    Local["Menu_Queues_Target"],
                                    Local["Menu_Queues_Precast"],
                                    Local["Menu_Queues_PrecastActivatable"] };
                                using (new GUILayout.HorizontalScope())
                                {
                                    //TODO - part of the UI remake

                                    Utility.UI.SelectionGrid(ref actionEditStage, actionEditStrings, 5, () =>
                                    {
                                        if (actionEditStage == 1)
                                        {
                                            AbilityFilteredList.Init(ref selectedActionController.abilityIDs,
                                                queuesController,
                                                selectedActionController.actionType,
                                                (selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell) ? true: false,
                                                false);
                                        }
                                        if (actionEditStage == 3)
                                        {
                                            AbilityFilteredList.Init(ref selectedActionController.abilityMods,
                                                queuesController,
                                                CommandQueueItem.ActionTypes.Ability,
                                                false,
                                                true,
                                                selectedActionController.casterName);
                                        }
                                        if (actionEditStage == 4)
                                        {
                                            AbilityFilteredList.Init(ref selectedActionController.activatableMods,
                                                queuesController,
                                                CommandQueueItem.ActionTypes.Activatable,
                                                false,
                                                true,
                                                selectedActionController.casterName);
                                        }
                                    }
                                        , buttonSelector);
                                }
                            }
                            else
                            {
                                string[] actionEditStrings = {Local["Menu_Queues_Caster"],
                                    Local["Menu_Queues_Ability"],
                                    Local["Menu_Queues_Target"] };
                                using (new GUILayout.HorizontalScope())
                                {
                                    Utility.UI.SelectionGrid(ref actionEditStage, actionEditStrings,3, () =>
                                    {
                                        if (actionEditStage == 1)
                                        {
                                            AbilityFilteredList.Init(ref selectedActionController.abilityIDs,
                                                queuesController,
                                                selectedActionController.actionType,
                                                false,
                                                false,
                                                selectedActionController.casterName);
                                        }
                                    }
                                        , buttonSelector);
                                }
                            }
                            using (new GUILayout.HorizontalScope())
                            {
                                //Finish editing action
                                string finishText;
                                bool editionComplete;
                                if (selectedActionController.casterName == null ||
                                    (selectedActionController.abilityIDs.Count < 1) ||
                                    (selectedActionController.targetSelf == false & selectedActionController.characterNames == null & selectedActionController.positions == null & selectedActionController.petIndex == null)
                                    )
                                {
                                    finishText = Local["Menu_Queues_Finish"].Color(RGBA.red);
                                    editionComplete = false;
                                }
                                else
                                {
                                    finishText = Local["Menu_Queues_Finish"];
                                    editionComplete = true;
                                }
                                if (GUILayout.Button(finishText, buttonFixed120, GUILayout.ExpandWidth(false)) && editionComplete)
                                {
                                    CommandQueueItem commandQueueItem = new CommandQueueItem(
                                        selectedActionController.casterName,
                                        selectedActionController.abilityIDs[0],
                                        selectedActionController.targetSelf,
                                        selectedActionController.characterNames,
                                        selectedActionController.positions,
                                        selectedActionController.petIndex,
                                        selectedActionController.abilityMods,
                                        selectedActionController.activatableMods,
                                        selectedActionController.actionType
                                        );
                                    if (currentActionIndex > -1)
                                    {
                                        selectedQueueController.CurrentQueue().CommandList.RemoveAt(currentActionIndex);
                                        selectedQueueController.CurrentQueue().CommandList.Insert(currentActionIndex, commandQueueItem);
                                    }
                                    else
                                        selectedQueueController.CurrentQueue().CommandList.Add(commandQueueItem);
                                    selectedQueueController.actionController.EditMode = false;
                                    ResetActionEdit();
                                    selectedQueueController.Refresh();
                                    return;
                                }
                                //Cancel action changes
                                if (GUILayout.Button(Local["Menu_Queues_Cancel"], buttonFixed120, GUILayout.ExpandWidth(false)))
                                {
                                    ResetActionEdit();
                                    selectedQueueController.actionController.EditMode = false;
                                    selectedQueueController.Refresh();
                                    return;
                                }
                            }
                            //Caster
                            if (actionEditStage == 0)
                            {
                                if (castersUIArray == null)
                                {
                                    List<string> partyList = selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell
                                        ? new List<string> { "Highest duration" } : new List<string>();
                                    foreach (UnitEntityData unit in (from u in Kingmaker.Game.Instance?.Player?.Party where u.IsDirectlyControllable select u))
                                    {
                                        partyList.Add(unit.CharacterName);
                                    }
                                    castersUIArray = partyList.ToArray();
                                }
                                //Caster selector
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label(Local["Menu_Queues_CurrentCaster"], labelfixed120, GUILayout.ExpandWidth(false));
                                    if (selectedActionController.casterName == null)
                                        GUILayout.Label(Local["Menu_Queues_None"], labelfixed120);
                                    else
                                    {
                                        if (selectedActionController.casterName == "")
                                        {
                                            GUILayout.Label(Local["Menu_Queues_HighestCasterLevel"], labelfixed120);
                                            currentCasterIndex = 0;
                                        }
                                        else
                                        {
                                            GUILayout.Label(selectedActionController.casterName, labelfixed120);
                                            currentCasterIndex = Array.IndexOf(castersUIArray, selectedActionController.casterName);
                                        }
                                    }
                                }
                                Utility.UI.SelectionGrid(ref currentCasterIndex, castersUIArray, 3, () =>
                                {
                                    if (currentCasterIndex == 0 && selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell)
                                    {
                                        selectedActionController.casterName = "";
                                    }
                                    else
                                    {
                                        selectedActionController.casterName = castersUIArray[currentCasterIndex];
                                    }
                                }, buttonSelector, GUILayout.ExpandWidth(false));
                            }
                            //Spells and abilities
                            if (actionEditStage == 1)
                            {
                                if (selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell)
                                {
                                    //TODO
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button("All casters", buttonFixed120, GUILayout.ExpandWidth(false)))
                                        {
                                            AbilityFilteredList.Init(ref selectedActionController.abilityIDs,
                                                queuesController,
                                                selectedActionController.actionType,
                                                false,
                                                false,
                                                "");
                                        }
                                        if (GUILayout.Button("Selected caster", buttonFixed120, GUILayout.ExpandWidth(false)))
                                        {
                                            AbilityFilteredList.Init(ref selectedActionController.abilityIDs,
                                                queuesController,
                                                selectedActionController.actionType,
                                                false,
                                                false,
                                                selectedActionController.casterName);
                                        }
                                    }
                                }
                                AbilityFilteredList.OnGUI();
                            }
                            //Targets
                            if (actionEditStage == 2)
                            {
                                GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_TargetSelectionType"]), labelDefault);
                                Utility.UI.SelectionGrid(ref currentTargetTypeIndex, targetTypeArray, 3, buttonSelector, GUILayout.ExpandWidth(false));
                                //Target toggle selector
                                if (currentTargetTypeIndex > 0 && targetSelection.Count == partyNamesOrdered.Count)
                                {
                                    for (int partyOrder = 0; partyOrder < partyNamesOrdered.Count; partyOrder++)
                                    {
                                        bool nameToggle = targetSelection[partyOrder];
                                        Utility.UI.ToggleButton(
                                        ref nameToggle, partyNamesOrdered[partyOrder],
                                        buttonFixed120);
                                        targetSelection[partyOrder] = nameToggle;

                                    }
                                }
                                if (GUILayout.Button(Local["Menu_Queues_ApplyTargetSelection"], buttonDefault, GUILayout.Width(leftBoxWidth)))
                                {
                                    switch (currentTargetTypeIndex)
                                    {
                                        case 0:
                                            selectedActionController.targetSelf = true;
                                            break;
                                        case 1:
                                            List<int> positions = targetSelection.Where(kvp => (kvp.Value)).Select(kvp => kvp.Key).ToList();
                                            List<UnitEntityData> characters = Target.GetPartyOrder();
                                            List<string> characterNames = new List<string>();
                                            Dictionary<string, List<int>> petIndex = new Dictionary<string, List<int>>();
                                            for (int i = 0; i < characters.Count; i++)
                                            {
                                                if (positions.Contains(i))
                                                {
#if (WOTR)
                                                    if (characters[i].IsPet)
#elif (KINGMAKER)
                                                    if (characters[i].IsPet())
#endif
                                                    {
                                                        foreach (UnitEntityData unit in (from u in Kingmaker.Game.Instance?.Player?.Party where u.IsDirectlyControllable select u))
                                                        {
#if (WOTR)
                                                            foreach (UnitEntityData pet in unit.Pets)
#elif (KINGMAKER)
                                                            foreach(UnitEntityData pet in unit.Pets())
#endif
                                                            {
                                                                if (pet == characters[i])
                                                                {
                                                                    if (!petIndex.ContainsKey(unit.CharacterName))
#if (WOTR)
                                                                        petIndex[unit.CharacterName] = new List<int> { unit.Pets.IndexOf(pet) };
                                                                    else
                                                                        petIndex[unit.CharacterName].Add(unit.Pets.IndexOf(pet));
#elif (KINGMAKER)
                                                                        petIndex[unit.CharacterName] = new List<int> { unit.Pets().IndexOf(pet) };
                                                                    else
                                                                        petIndex[unit.CharacterName].Add(unit.Pets().IndexOf(pet));
#endif
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                        characterNames.Add(characters[i].CharacterName);
                                                }
                                            }
                                            selectedActionController.petIndex = petIndex;
                                            selectedActionController.characterNames = characterNames;
                                            selectedActionController.targetSelf = false;
                                            selectedActionController.positions = null;
                                            break;
                                        case 2:
                                            selectedActionController.positions = targetSelection.Where(kvp => (kvp.Value)).Select(kvp => kvp.Key).ToList();
                                            selectedActionController.characterNames = null;
                                            selectedActionController.petIndex = null;
                                            selectedActionController.targetSelf = false;
                                            break;
                                    }
                                }
                                if (GUILayout.Button(Local["Menu_Queues_ClearTargetSelection"], buttonDefault, GUILayout.Width(leftBoxWidth)))
                                {
                                    targetSelection = new Dictionary<int, bool>();
                                    Target.GetTargetSelectionDict(ref targetSelection);
                                    currentTargetTypeIndex = -1;
                                    selectedActionController.targetSelf = false;
                                    selectedActionController.petIndex = null;
                                    selectedActionController.characterNames = null;
                                    selectedActionController.petIndex = null;
                                    selectedActionController.targetSelf = false;
                                }
                            }
                            //Pre-cast ability
                            if (actionEditStage == 3 && selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell)
                            {
                                GUILayout.Label(Local["Menu_Queues_ModNote"]);
                                AbilityFilteredList.OnGUI();
                            }
                            //Activatable ability
                            if (actionEditStage == 4 && selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell)
                            {
                                GUILayout.Label(Local["Menu_Queues_ModNote"]);
                                AbilityFilteredList.OnGUI();
                            }
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            //View action data
                            GUILayout.BeginVertical();
                            using (new GUILayout.HorizontalScope())
                            {
                                //Move action up in the queue
                                if (GUILayout.Button(Local["Menu_Queues_Up"], buttonFixed120, GUILayout.ExpandWidth(false)))
                                {
                                    selectedQueueController.CurrentQueue().CommandList.MoveUpList(selectedQueueController.actionController.CurrentAction());
                                    int newActionIndex = selectedQueueController.CurrentQueue().CommandList.IndexOf(selectedQueueController.actionController.CurrentAction());
                                    selectedQueueController.Refresh();
                                    selectedQueueController.CurrentActionIndex = newActionIndex;
                                    this.currentActionIndex = newActionIndex;
                                    selectedQueueController.actionController = new ActionController(selectedQueueController.CurrentQueue().CommandList[newActionIndex]);
                                    return;
                                }
                                //Move action own in the queue
                                if (GUILayout.Button(Local["Menu_Queues_Down"], buttonFixed120, GUILayout.ExpandWidth(false)))
                                {
                                    selectedQueueController.CurrentQueue().CommandList.MoveDownList(selectedQueueController.actionController.CurrentAction());
                                    int newActionIndex = selectedQueueController.CurrentQueue().CommandList.IndexOf(selectedQueueController.actionController.CurrentAction());
                                    selectedQueueController.Refresh();
                                    selectedQueueController.CurrentActionIndex = newActionIndex;
                                    this.currentActionIndex = newActionIndex;
                                    selectedQueueController.actionController = new ActionController(selectedQueueController.CurrentQueue().CommandList[newActionIndex]);
                                    return;
                                }
                                //Changing to edit action mode
                                //TODO - into the function?
                                if (GUILayout.Button(Local["Menu_Queues_Edit"], buttonFixed120, GUILayout.ExpandWidth(false)))
                                {
                                    //TODO - remove
                                    selectedActionController.abilityID = selectedActionController.CurrentAction().AbilityId;
                                    selectedActionController.abilityIDs.Clear();
                                    selectedActionController.abilityIDs.Add(selectedActionController.CurrentAction().AbilityId);
                                    selectedActionController.casterName = selectedActionController.CurrentAction().CasterName;
                                    selectedActionController.characterNames = selectedActionController.CurrentAction().CharacterNames;
                                    selectedActionController.petIndex = selectedActionController.CurrentAction().PetIndex;
                                    if (selectedActionController.CurrentAction().AbilityMods == null)
                                        selectedActionController.abilityMods = new List<string>();
                                    else
                                        selectedActionController.abilityMods = selectedActionController.CurrentAction().AbilityMods;
                                    if (selectedActionController.CurrentAction().ActivatableMods == null)
                                        selectedActionController.activatableMods = new List<string>();
                                    else
                                        selectedActionController.activatableMods = selectedActionController.CurrentAction().ActivatableMods;
                                    selectedActionController.positions = selectedActionController.CurrentAction().Positions;
                                    selectedActionController.actionType = selectedActionController.CurrentAction().ActionType;
                                    selectedActionController.targetSelf = selectedActionController.CurrentAction().TargetType == CommandQueueItem.TargetTypes.Self;
                                    ResetActionEdit();
                                    if (targetSelection == null)
                                        targetSelection = new Dictionary<int, bool>();
                                    Target.GetTargetSelectionDict(ref targetSelection, selectedActionController.CurrentAction());
                                    switch (selectedActionController.CurrentAction().TargetType)
                                    {
                                        case CommandQueueItem.TargetTypes.Self:
                                            currentTargetTypeIndex = 0;
                                            break;
                                        case CommandQueueItem.TargetTypes.CharacterNames:
                                            currentTargetTypeIndex = 1;
                                            break;
                                        case CommandQueueItem.TargetTypes.Positions:
                                            currentTargetTypeIndex = 2;
                                            break;
                                    }
                                    selectedQueueController.actionController.EditMode = true;
                                }
                                //Delete action
                                if (GUILayout.Button(Local["Menu_Queues_Delete"], buttonFixed120, GUILayout.ExpandWidth(false)))
                                {
                                    selectedQueueController.CurrentQueue().CommandList.Remove(selectedQueueController.actionController.CurrentAction());
                                    selectedQueueController.Refresh();
                                    return;
                                }
                            }
                            //View action description
                            using (new GUILayout.HorizontalScope())
                            {
                                CommandQueueItem commandQueueItem = selectedQueueController.actionController.CurrentAction();
                                //Caster/Targets/Mods
                                using (new GUILayout.VerticalScope("box", GUILayout.Width((float)(Math.Max(300f, rightBoxWidth * 0.3))), GUILayout.ExpandWidth(false)))
                                {
                                    UnitEntityData caster = commandQueueItem.GetCaster(partySpellList);
                                    //Caster
                                    GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Caster"]), labelDefault, GUILayout.ExpandWidth(false));
                                    if (commandQueueItem.CasterName == "")
                                    {
                                        if (caster != null)
                                            GUILayout.Label($"Highest Duration ({caster.CharacterName})".Color(RGBA.green), labelfixed120, GUILayout.ExpandWidth(false));
                                        else
                                            GUILayout.Label("Highest Duration (N /A)".Color(RGBA.red), labelfixed120, GUILayout.ExpandWidth(false));
                                    }
                                    else
                                    {
                                        if (caster != null)
                                            GUILayout.Label($"{caster.CharacterName}(Available)".Color(RGBA.green), labelDefault, GUILayout.ExpandWidth(false));
                                        else
                                            GUILayout.Label($"Character name (N/A)".Color(RGBA.red), labelDefault, GUILayout.ExpandWidth(false));
                                    }
                                    //Targets
                                    GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Target"]), labelfixed120, GUILayout.ExpandWidth(false));
                                    switch (commandQueueItem.TargetType)
                                    {
                                        case CommandQueueItem.TargetTypes.Self:
                                            GUILayout.Label("Self".Color(RGBA.green), labelDefault, GUILayout.ExpandWidth(false));
                                            break;
                                        case CommandQueueItem.TargetTypes.Positions:
                                            GUILayout.Label("(Formation positions:)".Italic(), labelfixed120, GUILayout.ExpandWidth(false));
                                            if (commandQueueItem.Positions != null)
                                                foreach (int position in commandQueueItem.Positions)
                                                {
                                                    UnitEntityData target = Target.GetTarget(position);
                                                    if (target != null)
                                                    {
                                                        GUILayout.Label($"#{position} ({target.CharacterName.Color(RGBA.green)})",
                                                            labelDefault, GUILayout.ExpandWidth(false));
                                                    }
                                                    else
                                                    {
                                                        GUILayout.Label($"#{position} (" + "N/A".Color(RGBA.red) + ")"
                                                            , labelDefault, GUILayout.ExpandWidth(false));
                                                    }
                                                }
                                            break;
                                        case CommandQueueItem.TargetTypes.CharacterNames:
                                            GUILayout.Label("(Character names):".Italic(), labelfixed120, GUILayout.ExpandWidth(false));
                                            if (commandQueueItem.CharacterNames != null)
                                            {
                                                foreach (string characterName in commandQueueItem.CharacterNames)
                                                {
                                                    List<UnitEntityData> targets = Target.GetTarget(characterName);
                                                    if (targets.Count > 0)
                                                    {
                                                        GUILayout.Label($"{characterName.Color(RGBA.green)}",
                                                            labelfixed120, GUILayout.ExpandWidth(false));
                                                    }
                                                    else
                                                    {
                                                        GUILayout.Label($"{characterName.Color(RGBA.red)}"
                                                            , labelfixed120, GUILayout.ExpandWidth(false));
                                                    }
                                                }
                                            }
                                            if (commandQueueItem.PetIndex != null)
                                            {
                                                foreach (string characterName in commandQueueItem.PetIndex.Keys)
                                                {
                                                    foreach (int petIndex in commandQueueItem.PetIndex[characterName])
                                                    {
                                                        UnitEntityData target = Target.GetTargetPet(characterName, petIndex);
                                                        if (target != null)
                                                        {
                                                            GUILayout.Label($"{characterName} pet ({target.CharacterName})".Color(RGBA.green),
                                                                labelfixed120, GUILayout.ExpandWidth(false));
                                                        }
                                                        else
                                                        {
                                                            GUILayout.Label($"#{characterName} pet (" + "N/A".Color(RGBA.red) + ")"
                                                                , labelfixed120, GUILayout.ExpandWidth(false));
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                    //Status
                                    GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Status"]), labelfixed120, GUILayout.ExpandWidth(false));
                                    IEnumerable<string> actionStatus = commandQueueItem.GetStatus(queuesController.partySpellList);
                                    GUILayout.Label(StatusStyled(actionStatus, string.Join("; ", actionStatus)));
                                }
                                //Ability
                                using (new GUILayout.VerticalScope("box", GUILayout.ExpandWidth(true)))
                                {
                                    string abilityName = commandQueueItem.GetAbilityName();
                                    GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Spell"]), labelfixed120, GUILayout.ExpandWidth(false));
                                    if (abilityName == "")
                                        GUILayout.Label(Local["Menu_Queues_NotAvailable"].Color(RGBA.red), labelfixed120, GUILayout.ExpandWidth(false));
                                    else if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Spell)
                                    {
                                        //Spell description
                                        UnitEntityData caster = commandQueueItem.GetCaster(partySpellList);
                                        BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(commandQueueItem.AbilityId);
                                        string spellDescription = blueprintAbility.Description;
                                        PartySpellData memorizedSpellData = queuesController.partySpellList.GetMemorizedSpellData(caster, blueprintAbility);
                                        string spellDuration = "";
                                        if (memorizedSpellData != null)
                                        {
                                            spellDuration = PartySpellList.GetSpellDuration(memorizedSpellData).Seconds.ToString();
                                        }
                                        else
                                        {
                                            spellDuration = Local["Menu_Queues_NotMemorized"].Color(RGBA.red);
                                        }
                                        List<UnitEntityData> availableCasters = queuesController.partySpellList.GetAvailableCasters(blueprintAbility);
                                        List<string> availableCastersStrings = new List<string>();
                                        foreach (UnitEntityData caster1 in availableCasters)
                                        {
                                            availableCastersStrings.Add(caster1.CharacterName);
                                        }
                                        string availableCastersString = String.Join(",\n", availableCastersStrings);
                                        GUILayout.Label(abilityName, labelfixed120, GUILayout.ExpandWidth(false));
                                        GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Description"]), labelfixed120);
                                        GUILayout.Label(spellDescription.RemoveHtmlTags(), labelDefault);
                                        GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Duration"]), labelfixed120);
                                        GUILayout.Label(spellDuration, labelDefault, GUILayout.ExpandWidth(false));
                                        GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Availableto"]), labelfixed120);
                                        GUILayout.Label(availableCastersString, labelDefault);
                                    }
                                    else if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Ability)
                                    {
                                        //TODO duration, casters
                                        //Ability description
                                        GUILayout.Label(abilityName, labelfixed120, GUILayout.ExpandWidth(false));
                                        UnitEntityData caster = commandQueueItem.GetCaster(partySpellList);
                                        BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(commandQueueItem.AbilityId);
                                        string abilityDescription = blueprintAbility.Description;
                                        GUILayout.Label(abilityName, labelfixed120, GUILayout.ExpandWidth(false));
                                        GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Description"]), labelfixed120, GUILayout.ExpandWidth(false));
                                        GUILayout.Label(abilityDescription.RemoveHtmlTags(), labelDefault, GUILayout.ExpandWidth(false));
                                    }
                                    //Mods
                                    if (commandQueueItem.AbilityMods != null)
                                    {
                                        GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_PreUseAbilities"]));
                                        foreach (string abilityID in commandQueueItem.AbilityMods)
                                        {
                                            AbilityDataUI abilityData = queuesController.partyAbilityList.GetAbility(abilityID);
                                            if (abilityData.Casters != null)
                                                GUILayout.Label(abilityData.AbilityName.Color(RGBA.green));
                                            else
                                                GUILayout.Label(abilityData.AbilityName.Color(RGBA.red));
                                        }
                                    }
                                    if (commandQueueItem.ActivatableMods != null)
                                    {
                                        GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_ToggleAbilities"]));
                                        foreach (string abilityID in commandQueueItem.ActivatableMods)
                                        {
                                            ActivatableDataUI abilityData = queuesController.partyActivatableList.GetActivatable(abilityID);
                                            if (abilityData.Casters != null)
                                                GUILayout.Label(abilityData.AbilityName.Color(RGBA.green));
                                            else
                                                GUILayout.Label(abilityData.AbilityName.Color(RGBA.red));
                                        }
                                    }
                                }
                            }
                            GUILayout.EndVertical();
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        //Get returned styled status string from the text status
        private string StatusStyled(IEnumerable<string> textStatus, string capition)
        {
            if (textStatus.Count() > 0)
            {
                if (textStatus.Contains(Local["Menu_Queues_StatusNoErrors"]))
                    return capition.Color(RGBA.green);
                if (textStatus.Contains(Local["Menu_Queues_StatusNoCasterName"])
                    || textStatus.Contains(Local["Menu_Queues_StatusNoCasterSpell"])
                    || textStatus.Contains(Local["Menu_Queues_StatusFatal"])
                    || textStatus.Contains(Local["Menu_Queues_StatusNoTargets"]))
                    return ("(!!)" + capition).Color(RGBA.red);
                if (textStatus.Contains(Local["Menu_Queues_StatusNotMemorized"])
                    || textStatus.Contains(Local["Menu_Queues_StatusNotAllTargets"])
                    || textStatus.Contains(Local["Menu_Queues_StatusNoAbilityResources"]))
                    return ("(!)" + capition).Color(RGBA.yellow);
                return capition.Color(RGBA.red);
            }
            else
                return capition.Color(RGBA.green);
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
