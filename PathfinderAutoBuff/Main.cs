using System;
using System.Linq;
using System.Reflection;
using Kingmaker;
using UnityModManagerNet;
using PathfinderAutoBuff.Controllers;
using PathfinderAutoBuff.QueueOperattions;
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
 //       public static UnityModManager.ModEntry modEntry = null;
        public static Settings Settings;
        public static bool Enabled;
        static bool harmonyDebug = false;
        public static bool IsInGame { get { return Game.Instance.Player.Party.Any(); } }
        public static float ummWidth = 960f;
        public static LocalizationController<DefaultLanguage> Local;
        public static GUIController uiController { get; internal set; }
        public static RecordController recordQueue { get; internal set; }
        public static Controllers.MenuController Menu;

        //        public static ModManager<Core, Settings> Mod;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            if (harmonyDebug)
                HarmonyLib.Harmony.DEBUG = true;
            try
            {
                modId = modEntry.Info.Id;
                //                Mod = new ModManager<Core, Settings>();
                //            Menu = new ModMaker.MenuController();
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
                    ModPath = modEntry.Path;
                    uiController = new GUIController();
                    recordQueue = new RecordController();
                    uiController.Enable();
                    recordQueue.ModEnable();
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
