using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.UnitLogic;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Main;
using static PathfinderAutoBuff.Utility.IOHelpers;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;
using static PathfinderAutoBuff.Utility.Extensions.CommonExtensions;
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif

namespace PathfinderAutoBuff.Menu.QueuesComponents
{
    //Action item viewer
    class ActionItemView
    {
        private readonly QueuesController queuesController;
        private readonly QueueController selectedQueue;
        private CommandQueueItem commandQueueItem;
        private int actionEditStage = -1;
        private string[] castersUIArray;
        private int currentCasterIndex = -1;
        private int currentTargetTypeIndex = -1;
        private string[] targetTypeArray = { "Caster", "Character name", "Formation Position" };
        private Dictionary<int, bool> targetSelection;
        private List<string> partyNamesOrdered;


        //Constructor
        public ActionItemView(QueuesController queuesController, CommandQueueItem commandQueueItem)
        {
            this.queuesController = queuesController;
            this.selectedQueue = queuesController.queueController;
            this.commandQueueItem = commandQueueItem;
        }

        //GUI
        public void OnGUI()
        {
            try
            {
                //View mode
                if (this.commandQueueItem != this.selectedQueue.actionController?.CurrentAction())
                {
                    UI.Splitter(Color.grey);
                    UI.BeginHorizontal("box");
                    //Control Block
                    UI.Vertical(() =>
                    {
                        UI.Space(25f);
                        UI.ActionButton(Local["Menu_Queues_Up"], () => MoveUp(), DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                        UI.ActionButton(Local["Menu_Queues_Down"], () => MoveDown(), DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                        UI.ActionButton(Local["Menu_Queues_Edit"], () => Edit(), DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                        UI.ActionButton(Local["Menu_Queues_Delete"], () =>
                        {
                            Delete();
                            return;
                        }, DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                    });
                    UI.Vertical(() =>
                    {
                    //Ability name and status
                    AbilityNameStatusView();
                    }, GUILayout.Width((float)(Math.Max(250f, ummWidth * 0.15))), GUILayout.ExpandWidth(false));
                    UI.Vertical(() =>
                    {
                    //Caster
                    CasterView();
                    //Availability
                    AvailabilityView();

                    }, GUILayout.Width((float)(Math.Max(250f, ummWidth * 0.2))), GUILayout.ExpandWidth(false));
                    UI.Vertical(() =>
                    {
                    //Target
                    TargetView();

                    }, GUILayout.Width((float)(Math.Max(200, ummWidth * 0.15))), GUILayout.ExpandWidth(false));
                    UI.Vertical(() =>
                    {
                    //Ability description and duration
                    AbilityDescriptionView();
                    }, GUILayout.ExpandWidth(true));
                    UI.EndHorizontal();
                }
                else
                {
                    UI.Splitter(Color.blue);
                    EditMode();
                    UI.Splitter(Color.blue);
                }
            }
            catch (Exception ex)
            {
                Logger.Critical($"{ex}");
                throw ex;
            }
        }

        private void EditMode()
        {
            GUILayout.BeginVertical(GUILayout.MinHeight(150f));
            ActionController selectedActionController = this.selectedQueue.actionController;
            if (selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell)
            {
                string[] actionEditStrings = {Local["Menu_Queues_Caster"],
                                    Local["Menu_Queues_Spell"],
                                    Local["Menu_Queues_Target"],
                                    Local["Menu_Queues_Precast"],
                                    Local["Menu_Queues_PrecastActivatable"] };
                using (new GUILayout.HorizontalScope())
                {
                    Utility.UI.SelectionGrid(ref actionEditStage, actionEditStrings, 5, () =>
                    {
                        if (actionEditStage == 1)
                        {
                            AbilityFilteredList.Init(ref selectedActionController.abilityIDs,
                                queuesController,
                                selectedActionController.actionType,
                                (selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell) ? true : false,
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
                        , DefaultStyles.ButtonSelector());
                }
            }
            else
            {
                string[] actionEditStrings = {Local["Menu_Queues_Caster"],
                                    Local["Menu_Queues_Ability"],
                                    Local["Menu_Queues_Target"] };
                using (new GUILayout.HorizontalScope())
                {
                    Utility.UI.SelectionGrid(ref actionEditStage, actionEditStrings, 3, () =>
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
                        , DefaultStyles.ButtonSelector());
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
                if (GUILayout.Button(finishText, DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)) && editionComplete)
                {
                    CommandQueueItem commandQueueItemNew = new CommandQueueItem(
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
                    int index = this.selectedQueue.CurrentQueue().CommandList.FindIndex(ind => ind.Equals(commandQueueItem));
                    if (index > -1)
                        this.selectedQueue.CurrentQueue().CommandList[index] = commandQueueItemNew;
                    else
                        this.selectedQueue.CurrentQueue().CommandList.Add(commandQueueItemNew);
                    this.selectedQueue.Refresh();
                    return;
                }
                //Cancel action changes
                if (GUILayout.Button(Local["Menu_Queues_Cancel"], DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
                {
                    if (this.commandQueueItem.AbilityId == null)
                        this.selectedQueue.CurrentQueue().CommandList.Remove(this.commandQueueItem);
                    this.selectedQueue.Refresh();
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
                    GUILayout.Label(Local["Menu_Queues_CurrentCaster"], DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                    if (selectedActionController.casterName == null)
                        GUILayout.Label(Local["Menu_Queues_None"], DefaultStyles.LabelFixed120());
                    else
                    {
                        if (selectedActionController.casterName == "")
                        {
                            GUILayout.Label(Local["Menu_Queues_HighestCasterLevel"], DefaultStyles.LabelFixed120());
                            currentCasterIndex = 0;
                        }
                        else
                        {
                            GUILayout.Label(selectedActionController.casterName, DefaultStyles.LabelFixed120());
                            currentCasterIndex = Array.IndexOf(castersUIArray, selectedActionController.casterName);
                        }
                    }
                }
                Utility.UI.SelectionGrid(ref currentCasterIndex, castersUIArray, 4, () =>
                {
                    if (currentCasterIndex == 0 && selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell)
                    {
                        selectedActionController.casterName = "";
                    }
                    else
                    {
                        selectedActionController.casterName = castersUIArray[currentCasterIndex];
                    }
                }, DefaultStyles.ButtonSelector(), GUILayout.ExpandWidth(false));
            }
            //Spells and abilities
            if (actionEditStage == 1)
            {
                if (selectedActionController.actionType == CommandQueueItem.ActionTypes.Spell)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("All casters", DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
                        {
                            AbilityFilteredList.Init(ref selectedActionController.abilityIDs,
                                queuesController,
                                selectedActionController.actionType,
                                false,
                                false,
                                "");
                        }
                        if (GUILayout.Button("Selected caster", DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false)))
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
                GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_TargetSelectionType"]), DefaultStyles.LabelDefault());
                Utility.UI.SelectionGrid(ref currentTargetTypeIndex, targetTypeArray, 3, DefaultStyles.ButtonSelector(), GUILayout.ExpandWidth(false));
                //Target toggle selector
                if (currentTargetTypeIndex > 0 && targetSelection.Count == partyNamesOrdered.Count)
                {
                    UI.HorizontalScope();
                    for (int partyOrder = 0; partyOrder < partyNamesOrdered.Count; partyOrder++)
                    {
                        bool nameToggle = targetSelection[partyOrder];
                        Utility.UI.ToggleButton(
                        ref nameToggle, partyNamesOrdered[partyOrder],
                        DefaultStyles.ButtonFixed120(),GUILayout.ExpandHeight(true));
                        targetSelection[partyOrder] = nameToggle;

                    }
                    UI.EndHorizontal();
                }
                UI.HorizontalScope();
                if (GUILayout.Button(Local["Menu_Queues_ApplyTargetSelection"], DefaultStyles.ButtonFixed120()))
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
                if (GUILayout.Button(Local["Menu_Queues_ClearTargetSelection"], DefaultStyles.ButtonFixed120()))
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
                UI.EndHorizontal();
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

        private void AbilityDescriptionView()
        {
            if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Spell)
            {
                //Spell description
                UnitEntityData caster = commandQueueItem.GetCaster(queuesController.partySpellList);
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
                GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Description"]), DefaultStyles.LabelFixed120());
                GUILayout.Label(spellDescription.RemoveHtmlTags(), DefaultStyles.LabelDefault());
                GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Duration"]), DefaultStyles.LabelFixed120());
                GUILayout.Label(spellDuration, DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
            }
            else if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Ability)
            {
                //Ability description
                BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(commandQueueItem.AbilityId);
                string abilityDescription = blueprintAbility.Description;
                GUILayout.Label(DefaultStyles.TextHeader3(Local["Menu_Queues_Description"]), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
                GUILayout.Label(abilityDescription.RemoveHtmlTags(), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
            }
            //Mods
            if (commandQueueItem.AbilityMods != null)
            {
                GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_PreUseAbilities"]));
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
                GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_ToggleAbilities"]));
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

        //Ability name and status
        private void AbilityNameStatusView()
        {

            string abilityName = commandQueueItem.GetAbilityName();
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Spell"]), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            if (abilityName == "")
                GUILayout.Label(Local["Menu_Queues_NotAvailable"].Color(RGBA.red).Bold(), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            else
                GUILayout.Label(abilityName.RemoveHtmlTags().Color(RGBA.green).Bold(), DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Status"]), DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
            IEnumerable<string> actionStatus = commandQueueItem.GetStatus(queuesController.partySpellList);
            GUILayout.Label(StatusStyled(actionStatus, actionStatus != null ? string.Join(";\n", actionStatus).Bold() : "Undefined status"));
        }

        //Target determination and view
        private void TargetView()
        {
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Target"]), DefaultStyles.LabelFixed120(), GUILayout.ExpandWidth(false));
            switch (commandQueueItem.TargetType)
            {
                case CommandQueueItem.TargetTypes.Self:
                    GUILayout.Label("Self".Color(RGBA.green), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                    break;
                case CommandQueueItem.TargetTypes.Positions:
                    GUILayout.Label("(Formation positions:)".Italic(), DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
                    if (commandQueueItem.Positions != null)
                        foreach (int position in commandQueueItem.Positions)
                        {
                            UnitEntityData target = Target.GetTarget(position);
                            if (target != null)
                            {
                                GUILayout.Label($"#{position} ({target.CharacterName.Color(RGBA.green)})",
                                    DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                            }
                            else
                            {
                                GUILayout.Label($"#{position} (" + "N/A".Color(RGBA.red) + ")"
                                    , DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                            }
                        }
                    break;
                case CommandQueueItem.TargetTypes.CharacterNames:
                    GUILayout.Label("(Character names):".Italic(), DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
                    if (commandQueueItem.CharacterNames != null)
                    {
                        foreach (string characterName in commandQueueItem.CharacterNames)
                        {
                            List<UnitEntityData> targets = Target.GetTarget(characterName);
                            if (targets.Count > 0)
                            {
                                GUILayout.Label($"{characterName.Color(RGBA.green)}",
                                    DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
                            }
                            else
                            {
                                GUILayout.Label($"{characterName.Color(RGBA.red)}"
                                    , DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
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
                                        DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
                                }
                                else
                                {
                                    GUILayout.Label($"#{characterName} pet (" + "N/A".Color(RGBA.red) + ")"
                                        , DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
                                }
                            }
                        }
                    }
                    break;
            }
        }

        //Caster determination and view
        private void CasterView()
        {
            UnitEntityData caster = commandQueueItem.GetCaster(queuesController.partySpellList);
            //Caster
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Caster"]), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
            if (commandQueueItem.CasterName == "")
            {
                if (caster != null)
                    GUILayout.Label($"Highest Duration ({caster.CharacterName})".Color(RGBA.green), DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
                else
                    GUILayout.Label("Highest Duration (N /A)".Color(RGBA.red), DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
            }
            else
            {
                if (caster != null)
                    GUILayout.Label($"{caster.CharacterName}(Available)".Color(RGBA.green), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                else
                    GUILayout.Label($"Character name (N/A)".Color(RGBA.red), DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
            }
        }


        //Spell availability to casters view
        private void AvailabilityView()
        {
            GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Queues_Availableto"]), DefaultStyles.LabelFixed200());
            UnitEntityData caster = commandQueueItem.GetCaster(queuesController.partySpellList);
            BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(commandQueueItem.AbilityId);
            if (blueprintAbility == null)
            {
                GUILayout.Label(Local["Menu_Queues_NotAvailable"].Color(RGBA.red), DefaultStyles.LabelFixed200(), GUILayout.ExpandWidth(false));
            }
            else
            {
                string availableCastersString = "";
                IEnumerable<string> casterList = null;
                if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Spell)
                {
                    casterList = queuesController.partySpellList.GetAvailableCasters(blueprintAbility)?.Select(casterUnit => casterUnit.CharacterName);                   
                }
                if (commandQueueItem.ActionType == CommandQueueItem.ActionTypes.Ability)
                {
                    casterList = queuesController.partyAbilityList.GetAvailableCasters(blueprintAbility);
                }
                availableCastersString = casterList != null ? String.Join(",\n", casterList) : Local["Menu_Queues_NotAvailable"].Color(RGBA.red);
                GUILayout.Label(availableCastersString, DefaultStyles.LabelDefault());
            }
        }

        //Move action up the queue
        private void MoveUp()
        {
            this.selectedQueue.CurrentQueue().CommandList.MoveUpList(commandQueueItem);
            this.selectedQueue.Refresh();
        }

        //Move action down the queue

        private void MoveDown()
        {
            this.selectedQueue.CurrentQueue().CommandList.MoveDownList(commandQueueItem);
            this.selectedQueue.Refresh();
        }

        //Edit
        //TODO - cleanup
        private void Edit()
        {
            if (this.selectedQueue.actionController?.CurrentAction() != this.commandQueueItem)
            {
                this.selectedQueue.actionController = new ActionController(this.commandQueueItem);
                this.selectedQueue.actionController.abilityID = this.selectedQueue.actionController.CurrentAction().AbilityId;
                this.selectedQueue.actionController.abilityIDs.Clear();
                this.selectedQueue.actionController.abilityIDs.Add(this.selectedQueue.actionController.CurrentAction().AbilityId);
                this.selectedQueue.actionController.casterName = this.selectedQueue.actionController.CurrentAction().CasterName;
                this.selectedQueue.actionController.characterNames = this.selectedQueue.actionController.CurrentAction().CharacterNames;
                this.selectedQueue.actionController.petIndex = this.selectedQueue.actionController.CurrentAction().PetIndex;
                if (this.selectedQueue.actionController.CurrentAction().AbilityMods == null)
                    this.selectedQueue.actionController.abilityMods = new List<string>();
                else
                    this.selectedQueue.actionController.abilityMods = this.selectedQueue.actionController.CurrentAction().AbilityMods;
                if (this.selectedQueue.actionController.CurrentAction().ActivatableMods == null)
                    this.selectedQueue.actionController.activatableMods = new List<string>();
                else
                    this.selectedQueue.actionController.activatableMods = this.selectedQueue.actionController.CurrentAction().ActivatableMods;
                this.selectedQueue.actionController.positions = this.selectedQueue.actionController.CurrentAction().Positions;
                this.selectedQueue.actionController.actionType = this.selectedQueue.actionController.CurrentAction().ActionType;
                this.selectedQueue.actionController.targetSelf = this.selectedQueue.actionController.CurrentAction().TargetType == CommandQueueItem.TargetTypes.Self;
                ResetActionEdit();
                if (targetSelection == null)
                    targetSelection = new Dictionary<int, bool>();
                Target.GetTargetSelectionDict(ref targetSelection, this.selectedQueue.actionController.CurrentAction());
                switch (this.selectedQueue.actionController.CurrentAction().TargetType)
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
            }
        }

        //Destructor
        private void Delete()
        {
            this.selectedQueue.CurrentQueue().CommandList.Remove(this.commandQueueItem);
            this.selectedQueue.Refresh();
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
            castersUIArray = null;
            partyNamesOrdered = Target.GetPartyNamesOrder();
        }
    }
}
