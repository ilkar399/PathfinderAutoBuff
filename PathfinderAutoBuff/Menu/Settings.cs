using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using Kingmaker.UnitLogic.Abilities;
using PathfinderAutoBuff.Controllers;
using PathfinderAutoBuff.Utility;
using PathfinderAutoBuff.Utility.Extensions;
using static PathfinderAutoBuff.Main;
using PathfinderAutoBuff.QueueOperations;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using Kingmaker;


namespace PathfinderAutoBuff.Menu
{
    class Settings: IMenuSelectablePage
        /*
         * Settings Menu Page
         * 
         */
    {
        public string Name => Local["Menu_Tab_Settings"];
        public int Priority => 500;
        private QueuesComponents.MetamagicPrioritySelector metamagicPrioritySelector;
        private GUIStyle buttonFixed120, labelDefault;
        private bool styleInit = false;
        private bool metamagicInit = false;

        public void OnGUI(UnityModManager.ModEntry modentry)
        {
            if (!Main.Enabled) return;
            if (!Main.IsInGame)
            {
                GUILayout.Label(Local["Menu_All_Label_NotInGame"].Color(RichTextExtensions.RGBA.red));
                return;
            }
            if (!styleInit)
            {
                buttonFixed120 = DefaultStyles.ButtonFixed120();
                labelDefault = DefaultStyles.LabelDefault();
                styleInit = true;
            }
            using (new GUILayout.VerticalScope())
            {
                UI.Label(Local["Menu_Settings_MechanicsLabel"]);
                //Ignore Modifiers
                IgnoreModifiiers = UI.ToggleButton(IgnoreModifiiers, Local["Menu_Settings_IgnoreModifiiers"], labelDefault, UI.AutoWidth());
                //Refresh short buffs
                RefreshShort = UI.ToggleButton(RefreshShort, Local["Menu_Settings_RefreshShort"], labelDefault, UI.AutoWidth());
                UI.Label(string.Format(Local["Menu_Settings_RefreshLabel"], RefreshTime));
                RefreshTime = (int)Utility.UI.RoundedHorizontalSlider(RefreshTime, 1, 30f, 90f, GUILayout.Width(200f), UI.AutoWidth());
                //Continue 
                ContinueCastOnFail = UI.ToggleButton(ContinueCastOnFail, Local["Menu_Settings_ContinueCastOnFail"], labelDefault, UI.AutoWidth());
                /*string activeScene = SceneManager.GetActiveScene().name;
                if (Game.Instance?.Player == null || activeScene == "MainMenu" || activeScene == "Start")
                {
                    GUILayout.Label(Local["Menu_All_Label_NotInGame"].Color(RichTextExtensions.RGBA.red));
                    return;
                }*/
                UI.Splitter(Color.grey);
                //Default Metadata Settings
                UI.Label(Local["Menu_Settings_MetadataLabel"]);
#if (WOTR)
                MetadataMythicSpellbookPriority = UI.ToggleButton(
                    MetadataMythicSpellbookPriority,
                    Local["Menu_Settings_MythicFirst"],
                    labelDefault,
                    UI.AutoWidth()
                    );
#endif
                MetadataInverseCasterLevelPriority = UI.ToggleButton(
                    MetadataInverseCasterLevelPriority,
                    Local["Menu_Settings_LowCLFirst"],
                    labelDefault,
                    UI.AutoWidth()
                    );
                MetadataIgnoreMetamagic = UI.ToggleButton(
                    MetadataIgnoreMetamagic,
                    Local["Menu_Settings_IgnoreMMPriority"],
                    labelDefault,
                    UI.AutoWidth()
                    );
                MetadataLowestSlotFirst = UI.ToggleButton(
                    MetadataLowestSlotFirst,
                    Local["Menu_Settings_LowSpelslotFirst"],
                    labelDefault,
                    UI.AutoWidth()
                    );
                //Metamagic priority
                if (!metamagicInit)
                {
                    metamagicPrioritySelector = new QueuesComponents.MetamagicPrioritySelector(SettingsWrapper.MetamagicPriority);
                    metamagicInit = true;
                }
                metamagicPrioritySelector.OnGUI();
                UI.Splitter(Color.grey);
                //GUI settings
                UI.Label(Local["Menu_Settings_GUILabel"]);
                UIEnabled = UI.ToggleButton(UIEnabled, Local["Menu_Settings_UIEnabled"], labelDefault, UI.AutoWidth());
                GUIFavoriteOnly = UI.ToggleButton(GUIFavoriteOnly, Local["Menu_Settings_FavoriteQueuesOnly"], labelDefault, UI.AutoWidth());
                UI.Label(Local["Menu_Settings_FavoriteQueuesLabel"]);
                //GUI Scale
                UI.Label("GUI Scale: " + ABToolbarScale);
                ABToolbarScale = Utility.UI.RoundedHorizontalSlider(ABToolbarScale, 2, 0.5f, 2.0f, GUILayout.Width(200f), UI.AutoWidth());
                UI.ActionButton("Apply", () => {
                    if (UIEnabled)
                        Main.uiController.AutoBuffGUI.RefreshView();
                }, buttonFixed120);
                if (GUILayout.Button(Local["Menu_Settings_RefreshGUI"], GUILayout.ExpandWidth(false)))
                {
                    if (UIEnabled)
                        Main.uiController.Update();
                }
                if (GUILayout.Button(Local["Menu_Settings_ResetGUI"], GUILayout.ExpandWidth(false)))
                {
                    if (UIEnabled)
                    {
                        SettingsWrapper.ABToolbarScale = 1;
                        SettingsWrapper.GUIPosX = 0;
                        SettingsWrapper.GUIPosY = 0;
                        Main.uiController.Update();
                    }
                }
            }
        }
    }
}
