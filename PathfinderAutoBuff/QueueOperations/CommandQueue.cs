using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Newtonsoft.Json;
using PathfinderAutoBuff.UnitLogic;
using PathfinderAutoBuff.Utility;
using PathfinderAutoBuff.Utility.Extensions;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif

namespace PathfinderAutoBuff.QueueOperations
{
    [Serializable]
    public class CommandQueueItem
    {
        //Action type. If null - assume it's a spell (for the backward compatibility)
        [CanBeNull]
        public ActionTypes ActionType;
        //Caster. If null or empty string, assume it's the highest duration
        [CanBeNull]
        public string CasterName;
        //Ability
        [NotNull]
        public string AbilityId;
        //Target if all nulls, assume it's self?
        [CanBeNull]
        public TargetTypes TargetType;
        [CanBeNull]
        public Dictionary<string, List<int>> PetIndex;
        [CanBeNull]
        public List<int> Positions { get; set; }
        [CanBeNull]
        public List<string> CharacterNames;
        //Modifiers
        [CanBeNull]
        public List<string> AbilityMods;
        [CanBeNull]
        public List<string> ActivatableMods;

        public enum TargetTypes
        {
            Self,
            Positions,
            CharacterNames,
            AOE
        }

        public enum ActionTypes
        {
            Spell,
            Ability,
            Activatable
        }

        //Constructors
        public CommandQueueItem()
        {
            this.AbilityId = null;
        }


        public CommandQueueItem(CommandQueueItem commandQueueItem)
        {
            this.CasterName = string.Copy(commandQueueItem.CasterName);
            this.AbilityId = string.Copy(commandQueueItem.AbilityId);
            this.TargetType = commandQueueItem.TargetType;
            this.ActionType = commandQueueItem.ActionType;
            this.CharacterNames = !(commandQueueItem.CharacterNames is null) ?  new List<string>(commandQueueItem.CharacterNames) :null;
            this.Positions = !(commandQueueItem.Positions is null) ? new List<int>(commandQueueItem.Positions) : null;
            this.PetIndex = !(commandQueueItem.PetIndex is null) ? new Dictionary<string, List<int>>() : null;
            if (commandQueueItem.PetIndex is null)
            {
                this.PetIndex = null;
            }
            else
            {
                this.PetIndex = new Dictionary<string, List<int>>();
                foreach (KeyValuePair<string, List<int>> entry in commandQueueItem.PetIndex)
                {
                    this.PetIndex.Add(entry.Key, new List<int>(entry.Value));
                }
            }
            this.AbilityMods = !(commandQueueItem.AbilityMods is null) ? new List<string>(commandQueueItem.AbilityMods) : null;
            this.ActivatableMods = !(commandQueueItem.ActivatableMods is null) ? new List<string>(commandQueueItem.ActivatableMods) : null;
        }

        public CommandQueueItem(string CasterName, string AbilityId,
            bool isSelf = false, List<string> CharacterNames = null, List<int> Positions = null, Dictionary<string, List<int>> PetIndex = null,
            List<string> AbilityMods = null, List<string> ActivatableMods = null, ActionTypes actionType = ActionTypes.Spell)
        {
            this.CasterName = CasterName;
            this.AbilityId = AbilityId;
            if (isSelf || (CharacterNames == null && Positions == null && PetIndex == null))
            {
                this.TargetType = TargetTypes.Self;
                this.CharacterNames = null;
                this.Positions = null;
            }
            else if (CharacterNames != null || PetIndex != null)
            {
                this.Positions = null;
                this.CharacterNames = CharacterNames;
                this.TargetType = TargetTypes.CharacterNames;
                if (PetIndex != null)
                    this.PetIndex = PetIndex;
            }
            else if (Positions != null)
            {
                this.Positions = Positions;
                this.CharacterNames = null;
                this.TargetType = TargetTypes.Positions;
            }
            if (AbilityMods?.Count < 1)
                this.AbilityMods = null;
            else
                this.AbilityMods = AbilityMods;
            if (ActivatableMods?.Count < 1)
                this.ActivatableMods = null;
            else
                this.ActivatableMods = ActivatableMods;
            this.ActionType = actionType;
        }

        //Get Command Caster
        public UnitEntityData GetCaster(PartySpellList partySpellList)
        {
#if (WOTR)
            var party = Kingmaker.Game.Instance?.Player?.PartyAndPets;
#elif (KINGMAKER)
            var party = Kingmaker.Game.Instance?.Player?.PartyAndPets();
#endif
            if (party == null || party.Count < 1)
            {
                return null;
            }
            UnitEntityData Result = null;
            if (this.CasterName != null && this.CasterName != "" || this.ActionType == ActionTypes.Ability)
            {
                Result = party.Where(unit1 => (unit1.CharacterName == this.CasterName)).FirstOrDefault();
            }
            else
            {
                BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(this.AbilityId);
                if (blueprintAbility == null)
                    return null;
                PartySpellData spellData = partySpellList.LongestDurationSpellCaster(blueprintAbility, null, false);
                if (spellData != null)
#if (WOTR)
                    Result = spellData.Caster;
#elif (KINGMAKER)
                    Result = spellData.Caster.Unit;
#endif
                else
                    return null;
            }
            return Result;
        }

        //Get number of targets
        public int GetRawTargetCount()
        {
            int targetCount = 0;
            //Target
            switch (this.TargetType)
            {
                case TargetTypes.Self:
                    targetCount = 1;
                    break;
                case TargetTypes.Positions:
                    if (this.Positions != null)
                        targetCount = this.Positions.Count;
                    break;
                case TargetTypes.CharacterNames:
                    if (this.CharacterNames != null)
                    {
                        targetCount += this.CharacterNames.Count;
                    }
                    if (this.PetIndex != null)
                    {
                        foreach (string characterName in this.PetIndex.Keys)
                        {
                            targetCount += this.PetIndex[characterName].Count;
                        }
                    }
                    break;
                default:
                    targetCount = 0;
                    break;
            }
            return targetCount;
        }

        //Ability Name
        public string GetAbilityName()
        {
            BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(this.AbilityId);
            if (blueprintAbility == null)
            {
                return "";
            }
            return blueprintAbility.Name;
        }

        //Parsing Command
        public List<CommandProvider> Parse(out string errorMessage, bool ignoreMods, bool refreshShort, PartySpellList partySpellList = null)
        {
            errorMessage = "";
            if (partySpellList == null)
                partySpellList = new PartySpellList();
            //Caster
            UnitEntityData caster = GetCaster(partySpellList);
            if (caster == null)
            {
                errorMessage = "Unable to get the Caster";
                return null;
            }
            List<UnitEntityData> targets = new List<UnitEntityData>();
            //Target
            switch (this.TargetType)
            {
                case TargetTypes.Self:
                    targets.Add(caster);
                    break;
                case TargetTypes.Positions:
                    foreach (int position in this.Positions)
                    {
                        targets.Add(Targets.GetTarget(position));
                    }
                    break;
                case TargetTypes.CharacterNames:
                    if (this.CharacterNames != null)
                    {
                        foreach (string characterName in this.CharacterNames)
                        {
                            targets.AddRange(Targets.GetTarget(characterName));
                        }
                    }
                    if (this.PetIndex != null)
                    {
                        foreach (string characterName in this.PetIndex.Keys)
                        {
                            foreach (int petIndex in this.PetIndex[characterName])
                                targets.AddIfNotNull(Targets.GetTargetPet(characterName, petIndex));
                        }
                    }
                    break;
                default:
                    targets.Add(caster);
                    break;
            }
            if (targets.Count < 1)
            {
                errorMessage = "Unable to get the targets";
                return null;
            }
            //Ability Mods
            List<BlueprintActivatableAbility> activatableAbilities = new List<BlueprintActivatableAbility>();
            List<BlueprintAbility> preCastAbilities = new List<BlueprintAbility>();
            if (!ignoreMods)
            {
                //Activatable abilities mods (togglable rods go here as well)
                if (this.ActivatableMods != null)
                {
                    foreach (string abilityID in this.ActivatableMods)
                    {
                        Logger.Debug($"Activatable ability {abilityID}");
                        BlueprintActivatableAbility blueprintActivatableAbility = caster.ActivatableAbilities.Enumerable.Select(ability => (ability.Blueprint))
                                                    .Where(blueprintAbilityTemp => blueprintAbilityTemp.AssetGuid == abilityID).FirstOrDefault();
                        if (blueprintActivatableAbility != null)
                        {
                            Logger.Debug("Added");
                            activatableAbilities.Add(blueprintActivatableAbility);
                        }
                    }
                }
                if (this.AbilityMods != null && this.ActionType != ActionTypes.Ability)
                {
                    foreach (string abilityID in this.AbilityMods)
                    {
                        Logger.Debug($"Pre-use ability {abilityID}");
                        BlueprintAbility blueprintAbility1 = caster.Abilities.Enumerable.Select(ability => (ability.Blueprint))
                                                    .Where(blueprintAbilityTemp => blueprintAbilityTemp.AssetGuid == abilityID).FirstOrDefault();
                     if (blueprintAbility1 != null)
                        {
                            Logger.Debug("Added");
                            preCastAbilities.Add(blueprintAbility1);
                        }
                    }
                }
            }
            BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(this.AbilityId);
            if (blueprintAbility == null)
            {
                errorMessage = "Unable to get Ability data";
                return null;
            }
            List<CommandProvider> Result = new List<CommandProvider>();
            foreach (UnitEntityData target in targets)
            {
                if ((refreshShort && LogicHelpers.UnitHasLowBuffDuration(target, blueprintAbility))
                    || !refreshShort)
                {
                    foreach (BlueprintActivatableAbility activatableAbility in activatableAbilities)
                    {
                        Result.Add(new ActivateAbilityProvider(caster,activatableAbility));
                    }
                    foreach (BlueprintAbility preCastAbility in preCastAbilities)
                    {
                        Result.Add(new UseAbilityProvider(caster, caster, preCastAbility,null, false));
                    }
                    Logger.Debug(string.Join(", ",caster.CharacterName,target.CharacterName,blueprintAbility.Name));
                    //Check if the ability is a variant
                    if (blueprintAbility.Parent == null)
                    {
                        Result.Add(new UseAbilityProvider(caster, target, blueprintAbility,null, this.ActionType == ActionTypes.Spell ? true : false));
                    }
                    else
                    {
                        Result.Add(new UseAbilityProvider(caster, target, blueprintAbility.Parent, blueprintAbility, this.ActionType == ActionTypes.Spell ? true : false));
                    }
                    foreach (BlueprintActivatableAbility activatableAbility in activatableAbilities)
                    {
                        Result.Add(new DeactivateAbilityProvider(caster, activatableAbility));
                    }
                }
            }
            return Result;
        }

        //Check how Queue item applies to the current party. 
        public IEnumerable<string> GetStatus(PartySpellList partySpellList)
        {
            List<string> Result = new List<string>();
#if (WOTR)
            var party = Kingmaker.Game.Instance?.Player?.PartyAndPets;
#elif (KINGMAKER)
            var party = Kingmaker.Game.Instance?.Player?.PartyAndPets();
#endif
            BlueprintAbility blueprintAbility = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(this.AbilityId);
            if (party == null || party.Count < 1 || blueprintAbility == null )
            {
                Result.Add("Fatal error");
                return Result;
            }
            UnitEntityData caster;
            //Caster checks
            if (this.CasterName != null && this.CasterName != "")
            {
                caster = party.Where(unit1 => (unit1.CharacterName == this.CasterName)).FirstOrDefault();
                if (caster == null)
                    Result.Add("No caster available with the name");
            }
            else
            {
                PartySpellData longestDuration = partySpellList.LongestDurationSpellCaster(blueprintAbility, null, false);
                if (longestDuration != null)
#if (WOTR)
                    caster = longestDuration.Caster;
#elif (KINGMAKER)
                    caster = longestDuration.Caster.Unit;
#endif
                else
                {
                    caster = null;
                    if (partySpellList.LongestDurationSpellCaster(blueprintAbility, null, true) == null)
                        Result.Add("No caster available with the chosen action");
                }
            }
            if (caster != null)
            {
                if (partySpellList.GetAvailableCasts(caster, blueprintAbility) == 0 && this.ActionType == ActionTypes.Spell)
                    Result.Add("Not enough spells memorized on the caster");
                
                 if (PartyAbilityList.GetAvailableUsages(caster, blueprintAbility) == 0 && this.ActionType == ActionTypes.Ability)
                   Result.Add("Not enough ability resources left on the caster");
            }
            //Target checks
            List<UnitEntityData> targets = new List<UnitEntityData>();
            bool targetNotAvailableFlag = false;
            //Target
            switch (this.TargetType)
            {
                case TargetTypes.Self:
                    if (caster != null)
                        targets.Add(caster);
                    break;
                case TargetTypes.Positions:
                    foreach (int position in this.Positions)
                    {
                        UnitEntityData target = Targets.GetTarget(position);
                        if (target == null)
                            targetNotAvailableFlag = true;
                        else
                            targets.Add(target);
                    }
                    break;
                case TargetTypes.CharacterNames:
                    if (this.CharacterNames != null)
                    {
                        foreach (string characterName in this.CharacterNames)
                        {
                            List<UnitEntityData> targets1 = Targets.GetTarget(characterName);
                            if (targets1.Count < 1)
                                targetNotAvailableFlag = true;
                            else
                                targets.AddRange(targets1);
                        }
                    }
                    if (this.PetIndex != null)
                    {
                        foreach (string characterName in this.PetIndex.Keys)
                        {
                            foreach (int petIndex in this.PetIndex[characterName])
                            {
                                UnitEntityData target = Targets.GetTargetPet(characterName, petIndex);
                                if (target == null)
                                    targetNotAvailableFlag = true;
                                else
                                    targets.Add(target);
                            }
                        }
                    }
                    break;
                default:
                    if (caster != null)
                        targets.Add(caster);
                    break;
            }
            if (targets.Count < 1)
                Result.Add("No available targets");
            else
                if (targetNotAvailableFlag)
                    Result.Add("Some targets are not available");
            if (Result.Count < 1)
                Result.Add("No errors");
            return Result;
        }

        //TODO: Destructor
    }

    //Process the command queue. Made for easier file&UI management
    public class CommandQueue
    {
        List<CommandQueueItem> m_List;

        public CommandQueue()
        {

        }

        public CommandQueue(List<CommandQueueItem> commandQueueItems)
        {
            this.m_List = commandQueueItems;
        }

        public List<CommandQueueItem> CommandList
        {
            get
            {
                return this.m_List;
            }
            set
            {
                this.m_List = value;
            }
        }

        //Import queue from file
        public bool LoadFromFile(string fileName)
        {
            string filePath = (Path.Combine(ModPath, "scripts"));
            filePath += $"/{fileName}";
            List<CommandQueueItem> commandQueueItems = null;
            if (!File.Exists(filePath))
                return false;
            try
            {
                using (StreamReader file = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    commandQueueItems = (List<CommandQueueItem>)serializer.Deserialize(file, typeof(List<CommandQueueItem>));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deserializing the queue {fileName}");
                Logger.Log(ex.StackTrace);
                return false;
            }
            if (commandQueueItems != null)
            {
                this.m_List = commandQueueItems;
                return true;
            }
            return false;
        }

        //Export queue into file
        public bool SaveToFile(string queueName)
        {
            string savepath = "";
            savepath = Path.Combine(ModPath,"scripts") + $"/{queueName}.json";
            try
            {
                if (!Directory.Exists(Path.Combine(ModPath, "scripts")))
                {
                    Directory.CreateDirectory(Path.Combine(ModPath, "scripts"));
                }
                var JsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };
                using (StreamWriter file = File.CreateText(savepath))
                {
                    Logger.Debug(this.m_List.Count);
                    JsonSerializer serializer = JsonSerializer.Create(JsonSettings);
                    serializer.Serialize(file, this.m_List);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Error saving the queue {queueName}");
                Logger.Log(e.StackTrace);
                return false;
            }

        }

        //TODO: Destructor
    }
}
