using System;
using System.Linq;
using System.Reflection;
using Kingmaker;
using UnityModManagerNet;
using PathfinderAutoBuff.Controllers;
using PathfinderAutoBuff.QueueOperations;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using HarmonyLib;


namespace PathfinderAutoBuff
{
#if (DEBUG)
    [EnableReloading]
#endif

    static class Main
    {
        static Harmony HarmonyInstance;
        static string modId;
        public static Settings Settings;
        public static bool Enabled;
        static bool harmonyDebug = false;
        public static bool IsInGame { get { return Game.Instance.Player.Party.Any(); } }
        public static float ummWidth = 960f;
        public static LocalizationController<DefaultLanguage> Local;
        public static GUIController uiController { get; internal set; }
        public static RecordController recordQueue { get; internal set; }
        public static QueuesController QueuesController { get; internal set; }
        public static Controllers.MenuController Menu;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            if (harmonyDebug)
                HarmonyLib.Harmony.DEBUG = true;
            try
            {
                modId = modEntry.Info.Id;
                //Local
                Local = new LocalizationController<DefaultLanguage>();
                //Menu
                Menu = new Controllers.MenuController();
                //Settings
                Settings = Settings.Load<Settings>(modEntry);
                //Harmony Patching
                HarmonyInstance = new Harmony(modId);
                HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

                //Logger
                Logger.modLogger = modEntry.Logger;
                Logger.modEntryPath = modEntry.Path;
                modEntry.OnToggle = OnToggle;
                modEntry.OnSaveGUI = OnSaveGUI;
#if (DEBUG)
                modEntry.OnUnload = Unload;
#endif
                Enabled = modEntry.Enabled;
                ModPath = modEntry.Path;
                //Transferring settings from the old version
                Utility.VersionCompatibility.SettingsCompatibility();
                //Loading Assets
                Utility.BundleManger.AddBundle("pathfinderautobuffpanel"); //Load AssetBundle.
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw ex;
            }
            return true;
        }

#if (DEBUG)
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            Utility.BundleManger.RemoveBundle("pathfinderautobuffpanel");
            HarmonyInstance.UnpatchAll(modId);
            Menu = null;
            Local = null;
            return true;
        }
#endif
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            try
            {
                if (value)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    Local.Enable(modEntry);
                    Menu.Enable(modEntry, assembly);
                    uiController = new GUIController();
                    Logger.Debug("uiController created");
                    recordQueue = new RecordController();
                    Logger.Debug("RecordController created");
                    if (uiController != null && recordQueue != null)
                    {
                        uiController.Enable();
                        Logger.Debug("uiController Enabled");
                        recordQueue.ModEnable();
                        Logger.Debug("RecordController ModEnabled");
                    }
                }
                else
                {
                    uiController.Disable();
                    recordQueue.ModDisable();
                    Menu.Disable(modEntry);
                    Local.Disable(modEntry);
                    uiController = null;
                    recordQueue = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + ex.StackTrace);
            }
            return true;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }

    }

}
