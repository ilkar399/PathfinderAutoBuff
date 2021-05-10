using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using PathfinderAutoBuff.Utility;
using PathfinderAutoBuff.Controllers;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;
using static PathfinderAutoBuff.Utility.Extensions.CommonExtensions;
using static PathfinderAutoBuff.Main;
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.UnitLogic;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace PathfinderAutoBuff.Menu
{
    class Recording : IMenuSelectablePage
    {
        /*
         * Queue Recording Menu. 
         * 
         */
        public string Name => Local["Menu_Tab_Recording"];
        public int Priority => 200;
        private float uiScale = 1f;
        private string queueName = "Recorded queue";
        private bool groupActions = true;
        private bool replaceNames = false;
        private string saveMessage = null;
        private bool saveResult = false;

        public void OnGUI(UnityModManager.ModEntry modentry)
        {
            if (!Main.Enabled) return;
            if (!Main.IsInGame)
            {
                GUILayout.Label(Local["Menu_All_Label_NotInGame"].Color(RGBA.red));
                return;
            }
            string activeScene = SceneManager.GetActiveScene().name;
            //Overall layout
            GUILayout.BeginVertical();
            GUILayout.Label(Local["Menu_Recording_Note0"]);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(Local["Menu_Recording_Start"], DefaultStyles.ButtonFixed120()))
                { 
                    if (Main.recordQueue == null)
                    {
                        Main.recordQueue = new RecordController();
                        Main.recordQueue.Enable();
                    }
                    else
                    {
                        Main.recordQueue.Clear();
                        Main.recordQueue.Enable();
                    }
                }
                if (GUILayout.Button(Local["Menu_Recording_Stop"], DefaultStyles.ButtonFixed120()))
                {
                    if (Main.recordQueue != null)
                    {
                        string recordedActionsNumber = Main.recordQueue.RecordedQueue == null ? "NULL" : Main.recordQueue.RecordedQueue.Count.ToString();
                        Logger.Debug($"Recorded actions: {recordedActionsNumber}");
                        Main.recordQueue.Disable();
                    }
                }

            }
            //Recorded actions list
            if (Main.recordQueue.RecordedQueue != null)
            {
                GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Recording_ActionsLabel"]));
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Recording_Caster"]), GUILayout.Width(150f * uiScale));
                    GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Recording_Target"]), GUILayout.Width(150f * uiScale));
                    GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Recording_Action"]), GUILayout.Width(150f * uiScale));
                    GUILayout.Label(DefaultStyles.TextHeader2(Local["Menu_Recording_Type"]), GUILayout.Width(150f * uiScale));
                }
                foreach (RecordController.RecordedAction recordedAction in Main.recordQueue.RecordedQueue)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(recordedAction.Caster.CharacterName.Color(RGBA.white), GUILayout.Width(150f * uiScale));
                        string targetString = recordedAction.Target != null ? recordedAction.Target.CharacterName : recordedAction.Caster.CharacterName;
                        GUILayout.Label(targetString.Color(RGBA.white), GUILayout.Width(150f * uiScale));
                        GUILayout.Label(recordedAction.Ability.Name.Color(RGBA.white).RemoveHtmlTags(), GUILayout.Width(150f * uiScale));
                        GUILayout.Label($"{recordedAction.RecordedActionType}".Color(RGBA.white), GUILayout.Width(150f * uiScale));
#if (DEBUG)
                        GUILayout.Label(recordedAction.Ability.name.Color(RGBA.white), GUILayout.Width(150f * uiScale));
                        GUILayout.Label(recordedAction.Ability.AssetGuid.Color(RGBA.white), GUILayout.Width(150f * uiScale));
#endif
                        var test = recordedAction.RecordedActionType;
                    }
                }
                UI.Space(10f);
                //Saving the resulting block
                //Queue Name
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Local["Menu_Queues_QueueName"],
                        DefaultStyles.LabelDefault(),GUILayout.ExpandWidth(false));
                    Utility.UI.TextField(ref queueName, DefaultStyles.TextField200());
                }
                //Group actions toggle
                Utility.UI.ToggleButton(ref groupActions, Local["Menu_Recording_GroupActions"], DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                //Replace names with positions toggle
                Utility.UI.ToggleButton(ref replaceNames, Local["Menu_Recording_UsePositions"], DefaultStyles.LabelDefault(), GUILayout.ExpandWidth(false));
                if (saveMessage != null)
                {
                    if (saveResult)
                        GUILayout.Label(saveMessage.Color(RGBA.green));
                    else
                        GUILayout.Label(saveMessage.Color(RGBA.red));
                }
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(Local["Menu_Recording_Save"], DefaultStyles.ButtonFixed120()))
                    {
                        (saveResult, saveMessage) = Main.recordQueue.SaveRecordQueue(queueName, groupActions, replaceNames);

                    }
                    if (GUILayout.Button(Local["Menu_Recording_Cancel"], DefaultStyles.ButtonFixed120()))
                    {
                        saveMessage = null;
                        Main.recordQueue.Clear();
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }
}
