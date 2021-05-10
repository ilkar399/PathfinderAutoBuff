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
        private GUIStyle buttonFixed120, labelDefault;
        private bool styleInit = false;

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
                styleInit = true;
            }
            string activeScene = SceneManager.GetActiveScene().name;
            //Overall layout
            GUILayout.BeginVertical();
            GUILayout.Label(Local["Menu_Recording_Note0"]);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(Local["Menu_Recording_Start"], buttonFixed120))
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
                if (GUILayout.Button(Local["Menu_Recording_Stop"], buttonFixed120))
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
                        GUILayout.Label(recordedAction.Ability.Name.Color(RGBA.white), GUILayout.Width(150f * uiScale));
#if (DEBUG)
                        GUILayout.Label(recordedAction.Ability.name.Color(RGBA.white), GUILayout.Width(150f * uiScale));
                        GUILayout.Label(recordedAction.Ability.AssetGuid.Color(RGBA.white), GUILayout.Width(150f * uiScale));
#endif
                        GUILayout.Label($"{recordedAction.RecordedActionType}".Color(RGBA.white), GUILayout.Width(150f * uiScale));
                        var test = recordedAction.RecordedActionType;
                    }
                }
                GUILayout.Space(10f);
                //Saving the resulting block
                //Queue Name
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Local["Menu_Queues_QueueName"],
                        new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fixedHeight = 25f, fixedWidth = 120f });
                    Utility.UI.TextField(ref queueName, new GUIStyle(GUI.skin.box) { fixedWidth = 200f, wordWrap = true });
                }
                //Group actions toggle
                Utility.UI.ToggleButton(ref groupActions, Local["Menu_Recording_GroupActions"], labelDefault, GUILayout.ExpandWidth(false));
                //Replace names with positions toggle
                Utility.UI.ToggleButton(ref replaceNames, Local["Menu_Recording_UsePositions"], labelDefault, GUILayout.ExpandWidth(false));
                using (new GUILayout.HorizontalScope())
                {
                    string saveMessage = null;
                    bool saveResult = false;
                    if (GUILayout.Button(Local["Menu_Recording_Save"], buttonFixed120))
                    {
                        (saveResult, saveMessage) = Main.recordQueue.SaveRecordQueue(queueName, groupActions, replaceNames);

                    }
                    if (saveMessage != null)
                    {
                        if (saveResult)
                            GUILayout.Label(saveMessage.Color(RGBA.green));
                        else
                            GUILayout.Label(saveMessage.Color(RGBA.red));
                    }
                    if (GUILayout.Button(Local["Menu_Recording_Cancel"], buttonFixed120))
                    {
                        Main.recordQueue.Clear();
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }
}
