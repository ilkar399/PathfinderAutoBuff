#if (DEBUG)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using PathfinderAutoBuff.Controllers;
using PathfinderAutoBuff.Utility.Extensions;
using static PathfinderAutoBuff.Main;
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.UnitLogic;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using PathfinderAutoBuff.Tests;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;


namespace PathfinderAutoBuff.Menu
{
    class Tests : IMenuSelectablePage
    /*
     * Tests menu tab. Tests themselves are in PathfinderAutobuff.Tests
     * Tests are called with Testname(), giving detailed info if detailed is set to true
     * Removed from the release configuration
     */

    {
        public string Name => Local["Menu_Tab_Tests"];
        public int Priority => 400;
        public static bool detailed = true;

        public void OnGUI(UnityModManager.ModEntry modentry)
        {
            if (!Main.Enabled) return;
            //string activeScene = SceneManager.GetActiveScene().name;
            //if (Game.Instance?.Player == null || activeScene == "MainMenu" || activeScene == "Start")
            if (!Main.IsInGame)
            {
                GUILayout.Label(Local["Menu_All_Label_NotInGame"].Color(RichTextExtensions.RGBA.red));
                return;
            }
            //Spells/Ability tests
            GUILayout.Label("Spell/Ability lists:".Color(RichTextExtensions.RGBA.yellow));
            //Test Party Spellist forming
            if (GUILayout.Button("Test Party Spellist Getter", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                SpellAbilityLists.SpellLists();
            }
            //Test party abilities list forming
            if (GUILayout.Button("Test party abilities list formation", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                SpellAbilityLists.AbilityLists();
            }
            //TODO party activatable list forming
            //Test party abilities list forming
            if (GUILayout.Button("Test party activatable list formation", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                SpellAbilityLists.ActivatableLists();
            }
            //Test parsing the action queue
            GUILayout.Label("Base queue tests".Color(RichTextExtensions.RGBA.yellow));
            if (GUILayout.Button("Test action queue parsing", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                QueueBase.QueueParsing();
            }
            //Test executing the action queue
            if (GUILayout.Button("Test action queue execution", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                QueueBase.QueueExecution();
            }
            //Test executing the action queue
            if (GUILayout.Button("Test pre-cast ability/activatable queue execution", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                QueueBase.PreCastExecution();
            }
            if (GUILayout.Button("Reset script executors", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                ScriptController.Reset();
            }
            //Test Queue application status
            if (GUILayout.Button("Test Queue application status (only detailed log)", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                QueueBase.QueueApplication();
            }
            //Test Queue saving
            if (GUILayout.Button("Test queue save", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                QueueBase.QueueSave();
            }
            //Test Queue loading
            if (GUILayout.Button("Test queue load", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                QueueBase.QueueLoad();
            }
            //Queue Recording
            //TODO

            //Queue UI component test
            if (GUILayout.Button("Queue UI component test", DefaultStyles.ButtonDefault(), GUILayout.ExpandWidth(false)))
            {
                QueueUI.AbilityFilterComponentTest("TestBard", "TestBard", "TestBard");
            }

        }
    }
}
#endif