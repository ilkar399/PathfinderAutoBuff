using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PathfinderAutoBuff.Controllers;
using Kingmaker.EntitySystem.Persistence.JsonUtility;

namespace PathfinderAutoBuff
{
    [JsonObject(MemberSerialization.OptOut)]
    public class DefaultLanguage: ILanguage
    {
        [JsonProperty]
        public string Language { get; set; } = "English (Default)";

        [JsonProperty]
        public Version Version { get; set; }

        [JsonProperty]
        public string Contributors { get; set; }

        [JsonProperty]
        public string HomePage { get; set; }

        [JsonProperty]
        public Dictionary<string, string> Strings { get; set; } = new Dictionary<string, string>()
        {
            { "Menu_Tab_Queues", "Queue management" },
            { "Menu_Tab_Settings", "Settings" },
            { "Menu_Tab_Tests", "Tests" },
            { "Menu_Tab_Recording", "Queue recording" },
            { "Menu_Settings_IgnoreModifiiers", "Ignore Ability/Activatable ability modifiers while processing"},
            { "Menu_Settings_RefreshShort", "Refresh buffs when they have a short duration"},
            { "Menu_Settings_UIEnabled", "Enable in-game UI"},
            { "Menu_Settings_ReloadQueues", "Reload Queues"},
            { "Menu_Settings_RefreshLabel", "Refresh in seconds: {0}"},
            { "Menu_Settings_FavoriteQueuesLabel", "Favorite queues:"},
            { "Menu_Queues_ReloadData", "Reload data"},
            { "Menu_Queues_NewQueue", "New Queue"},
            { "Menu_Queues_QueueList", "Queue List:"},
            { "Menu_Queues_QueueName", "Current queue name:"},
            { "Menu_Queues_ExecuteQueue", "Execute queue"},
            { "Menu_Queues_SaveQueue", "Save queue"},
            { "Menu_Queues_ErrorSaving", "Error saving queue {0}"},
            { "Menu_Queues_CancelChanges", "Cancel/Close"},
            { "Menu_Queues_DeleteQueue", "Delete queue"},
            { "Menu_Queues_ErrorDeleting", "Error deleting queue {0}"},
            { "Menu_Queues_ActionList", "Action List:"},
            { "Menu_Queues_NewSpell", "New Spell"},
            { "Menu_Queues_NewAbility", "New Ability"},
            { "Menu_Queues_Caster", "Caster"},
            { "Menu_Queues_Spell", "Spell"},
            { "Menu_Queues_Ability", "Ability"},
            { "Menu_Queues_Target", "Target"},
            { "Menu_Queues_Status", "Status"},
            { "Menu_Queues_Precast", "Pre-use ability"},
            { "Menu_Queues_PrecastActivatable", "Pre-use activatable"},
            { "Menu_Queues_Finish", "Finish"},
            { "Menu_Queues_Cancel", "Cancel"},
            { "Menu_Queues_CurrentCaster", "Current caster: "},
            { "Menu_Queues_None", "None"},
            { "Menu_Queues_HighestCasterLevel", "Highest caster level"},
            { "Menu_Queues_CurrentSpell", "Current spell: "},
            { "Menu_Queues_SpellSelection", "Spell selection: "},
            { "Menu_Queues_AbilitySelection", "Ability selection: "},
            { "Menu_Queues_Level", "Level: "},
            { "Menu_Queues_ErrorSpellData", "Error getting spell/ability data"},
            { "Menu_Queues_Description", "Description: "},
            { "Menu_Queues_Duration", "Duration: "},
            { "Menu_Queues_Availableto", "Available to: "},
            { "Menu_Queues_NotAvailable", "Not available"},
            { "Menu_Queues_NotMemorized", "Spell not memorized/fully used"},
            { "Menu_Queues_TargetSelectionType", "Target selection type:"},
            { "Menu_Queues_ClearTargetSelection", "Clear target selection"},
            { "Menu_Queues_ApplyTargetSelection", "Apply target selection"},
            { "Menu_Queues_PreUseAbilities", "Pre-use abilities: "},
            { "Menu_Queues_ClearMods", "Clear ability mods"},
            { "Menu_Queues_SelectedAbilityName", "Selected ability name: {0}"},
            { "Menu_Queues_ItemSource", "Item source: {0}"},
            { "Menu_Queues_ToggleAbilities", "Toggle abilities (they're untoggled after cast):"},
            { "Menu_Queues_SelectedActivatableName", "Selected Activatable name: {0}"},
            { "Menu_Queues_ModNote", "Note: ability modifiers are activated only if they are on the same character as the spell caster"},
            { "Menu_Queues_Up", "Up"},
            { "Menu_Queues_Down", "Down"},
            { "Menu_Queues_Edit", "Edit"},
            { "Menu_Queues_Delete", "Delete"},
            { "Menu_Queues_StatusNoErrors", "No errors"},
            { "Menu_Queues_StatusFatal", "Fatal error"},
            { "Menu_Queues_StatusNoAbility", "Error getting ability data for {0}"},
            { "Menu_Queues_StatusNoCasterName", "No caster available with the name"},
            { "Menu_Queues_StatusNoCasterSpell", "No caster available with the chosen action"},
            { "Menu_Queues_StatusNoTargets", "No available targets"},
            { "Menu_Queues_StatusNotMemorized", "Not enough spells memorized on the caster"},
            { "Menu_Queues_StatusNoAbilityResources","Not enough ability resources left on the caster"},
            { "Menu_Queues_StatusNotAllTargets", "Some targets are not available"},
            { "Menu_Recording_Note0", "*Only 100 BUFF actions are recorded. Toggling abilities/item usages are not recorded here"},
            { "Menu_Recording_Start", "Start recording"},
            { "Menu_Recording_Stop", "Stop recording"},
            { "Menu_Recording_Save", "Save queue"},
            { "Menu_Recording_Cancel", "Cancel and clear"},
            { "Menu_Recording_ActionsLabel", "Recorded actions list:"},
            { "Menu_Recording_Caster", "Caster:"},
            { "Menu_Recording_Target", "Target:"},
            { "Menu_Recording_Action", "Action:"},
            { "Menu_Recording_Type", "Type:"},
            { "Menu_Recording_GroupActions", "Group actions:"},
            { "Menu_Recording_UsePositions", "Replace names with party positions:"},
            { "Menu_All_Label_NotInGame", "Not in the game. Please start or load the game first." },

        };

        public T Deserialize<T>(TextReader reader)
        {
            DefaultJsonSettings.Initialize();
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }

        public void Serialize<T>(TextWriter writer, T obj)
        {
            DefaultJsonSettings.Initialize();
            writer.Write(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
