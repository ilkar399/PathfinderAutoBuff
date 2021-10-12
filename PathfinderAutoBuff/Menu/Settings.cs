﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using PathfinderAutoBuff.Controllers;
using PathfinderAutoBuff.Utility;
using PathfinderAutoBuff.Utility.Extensions;
using static PathfinderAutoBuff.Main;
using PathfinderAutoBuff.QueueOperattions;
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
        private List<string> m_QueueList;
        bool [] m_favoriteToggles = new bool [] { false, false, false, false, false };
        private int favorite_count = 5;
        private GUIStyle buttonFixed120, labelDefault;
        private bool styleInit = false;

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
            if (this.m_QueueList == null)
            {
                this.m_QueueList = new List<string>();
                ReloadQueues();
            }
            using (new GUILayout.VerticalScope())
            {
                //TODO Styling
                UI.Label(Local["Menu_Settings_MechanicsLabel"]);
                IgnoreModifiiers = UI.ToggleButton(IgnoreModifiiers, Local["Menu_Settings_IgnoreModifiiers"], labelDefault, UI.AutoWidth());
                RefreshShort = UI.ToggleButton(RefreshShort, Local["Menu_Settings_RefreshShort"], labelDefault, UI.AutoWidth());
                UI.Label(string.Format(Local["Menu_Settings_RefreshLabel"], RefreshTime));
                RefreshTime = (int)Utility.UI.RoundedHorizontalSlider(RefreshTime, 1, 30f, 90f, GUILayout.Width(200f), UI.AutoWidth());
                /*string activeScene = SceneManager.GetActiveScene().name;
                if (Game.Instance?.Player == null || activeScene == "MainMenu" || activeScene == "Start")
                {
                    GUILayout.Label(Local["Menu_All_Label_NotInGame"].Color(RichTextExtensions.RGBA.red));
                    return;
                }*/
                //GUI settings
                UI.Label(Local["Menu_Settings_GUILabel"]);
                UIEnabled = UI.ToggleButton(UIEnabled, Local["Menu_Settings_UIEnabled"], labelDefault, UI.AutoWidth());
                GUIFavoriteOnly = UI.ToggleButton(GUIFavoriteOnly, Local["Menu_Settings_FavoriteQueuesOnly"], labelDefault, UI.AutoWidth());
                UI.Label(Local["Menu_Settings_FavoriteQueuesLabel"]);
                //GUI Scale
                UI.Label("GUI Scale: " + ABToolbarScale);
                ABToolbarScale = Utility.UI.RoundedHorizontalSlider(ABToolbarScale, 2, 0.5f, 2.0f, GUILayout.Width(200f), UI.AutoWidth());
                UI.ActionButton("Apply", () => {
                    Main.uiController.AutoBuffGUI.RefreshView();
                }, buttonFixed120);
                if (GUILayout.Button(Local["Menu_Settings_ReloadQueues"], GUILayout.ExpandWidth(false)))
                {
                    ReloadQueues();
                }
                if (GUILayout.Button(Local["Menu_Settings_RefreshGUI"], GUILayout.ExpandWidth(false)))
                {
                    Main.uiController.Update();
                }
                if (GUILayout.Button(Local["Menu_Settings_ResetGUI"], GUILayout.ExpandWidth(false)))
                { 
                    SettingsWrapper.ABToolbarScale = 1;
                    SettingsWrapper.GUIPosX = 0;
                    SettingsWrapper.GUIPosY = 0;
                    Main.uiController.Update();
                }
            }
        }

        private void ReloadQueues()
        {
            //Load queues for favorite selectors
            string loadPath = Path.Combine(ModPath, "scripts");
            this.m_QueueList.Clear();
            if (!Directory.Exists(loadPath))
            {
                Directory.CreateDirectory(loadPath);
            }
            try
            {
                CommandQueue commandQueue = new CommandQueue();
                string[] queueFiles = Directory.GetFiles(loadPath, "*.json");
                foreach (string queueFile in queueFiles)
                {
                    string queueName = Path.GetFileName(queueFile);
                    if (commandQueue.LoadFromFile(queueName))
                    {
                        m_QueueList.Add(queueName.Substring(0, queueName.Length - 5));
                    }
                }
            }
            catch (Exception e)
            {
#if (DEBUG)
                Logger.Log(e.StackTrace);
                throw e;
#endif
            }
        }
    }
}
