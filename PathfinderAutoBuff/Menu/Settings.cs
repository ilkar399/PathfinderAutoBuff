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
using PathfinderAutoBuff.Scripting;
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
                IgnoreModifiiers = UI.ToggleButton(IgnoreModifiiers, Local["Menu_Settings_IgnoreModifiiers"], labelDefault, UI.AutoWidth());
                RefreshShort = UI.ToggleButton(RefreshShort, Local["Menu_Settings_RefreshShort"], labelDefault, UI.AutoWidth());
                UIEnabled = UI.ToggleButton(UIEnabled, Local["Menu_Settings_UIEnabled"], labelDefault, UI.AutoWidth());
                UI.Label(string.Format(Local["Menu_Settings_RefreshLabel"], RefreshTime));
                RefreshTime = (int)Utility.UI.RoundedHorizontalSlider(RefreshTime, 1, 30f, 90f, GUILayout.Width(200f), UI.AutoWidth());
                /*string activeScene = SceneManager.GetActiveScene().name;
                if (Game.Instance?.Player == null || activeScene == "MainMenu" || activeScene == "Start")
                {
                    GUILayout.Label(Local["Menu_All_Label_NotInGame"].Color(RichTextExtensions.RGBA.red));
                    return;
                }*/
                //Favorite queue selection
                UI.Label(Local["Menu_Settings_FavoriteQueuesLabel"]);
                if (GUILayout.Button(Local["Menu_Settings_ReloadQueues"], GUILayout.ExpandWidth(false)))
                {
                    ReloadQueues();
                }
                using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
                {
                    for (int favoriteQueueNumber = 0; favoriteQueueNumber < favorite_count; favoriteQueueNumber++)
                    {
                        int selected = m_QueueList.IndexOf(FavoriteQueues[favoriteQueueNumber + 1]);
                        using (new GUILayout.VerticalScope(GUILayout.Width(120f)))
                        {
                            Utility.UI.DropDownList(ref m_favoriteToggles[favoriteQueueNumber], ref selected, m_QueueList, () =>
                             {
                                 if (selected == -1)
                                     FavoriteQueues[favoriteQueueNumber + 1] = "";
                                 else
                                     FavoriteQueues[favoriteQueueNumber + 1] = m_QueueList[selected];
                                 Main.uiController.ABQueuesToolbar.UpdateButtonStatus();
                             }
                            , true, buttonFixed120, GUILayout.ExpandWidth(false));
                        }
                    }
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