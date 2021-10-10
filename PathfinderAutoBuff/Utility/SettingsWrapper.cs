using System.Collections.Generic;
using UnityEngine;
using static PathfinderAutoBuff.Main;

namespace PathfinderAutoBuff.Utility
{
    public static class SettingsWrapper
    {

        public static string ModPath
        {
            get => Main.Settings.modPath;
            set => Main.Settings.modPath = value;
        }

        public static string LocalizationFileName
        {
            get => Main.Settings.localizationFileName;
            set => Main.Settings.localizationFileName = value;
        }

        public static bool IgnoreModifiiers
        {
            get => Main.Settings.ignoreModifiiers;
            set => Main.Settings.ignoreModifiiers = value;
        }
        public static bool RefreshShort
        {
            get => Main.Settings.refreshShort;
            set => Main.Settings.refreshShort = value;
        }
        public static int RefreshTime
        {
            get => Main.Settings.refreshTime;
            set => Main.Settings.refreshTime = value;
        }

        public static bool UIEnabled
        {
            get => Main.Settings.uIEnabled;
            set
            {
                Main.Settings.uIEnabled = value;
            }
        }
        public static bool GUIFavoriteOnly
        {
            get => Main.Settings.guiFavoriteOnly;
            set
            {
                Main.Settings.guiFavoriteOnly = value;
            }
        }
        public static float ABToolbarWidth
        {
            get => Main.Settings.aBToolbarWidth;
            set => Main.Settings.aBToolbarWidth = value;
        }

        public static float ABToolbarScale
        {
            get => Main.Settings.aBToolbarScale;
            set => Main.Settings.aBToolbarScale = value;
        }

        public static SerializableDictionary<int,string> FavoriteQueues
        {
            get => Main.Settings.favoriteQueues;
            set
            {
                Main.Settings.favoriteQueues = value;
            }
        }

        public static List<string> FavoriteQueues2
        {
            get => Main.Settings.favoriteQueues2;
            set => Main.Settings.favoriteQueues2 = value;
        }

        public static float UIScale
        {
            get => Main.Settings.uIScale;
        }

        public static float GUIPosX
        {
            get => Main.Settings.gUIPosX;
            set => Main.Settings.gUIPosX = value;
        }

        public static float GUIPosY
        {
            get => Main.Settings.gUIPosY;
            set => Main.Settings.gUIPosY = value;
        }

    }

}
