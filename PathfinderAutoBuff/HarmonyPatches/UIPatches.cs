using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using UnityModManagerNet;

namespace PathfinderAutoBuff.HarmonyPatches
/*
* Based on Hsinyu Chan KingmakerModMaker
* https://github.com/hsinyuhcan/KingmakerModMaker
* And Cabarius ToyBox
* https://github.com/cabarius/ToyBox/
* UMM UI Patches
*/
{
    static class UIUPatches
    {
        [HarmonyPatch(typeof(UnityModManager.UI), "Update")]
        internal static class UnityModManager_UI_Update_Patch
            /*
             * Getting UMM window parameters
             */
        {
            static Dictionary<int, float> scrollOffsets = new Dictionary<int, float> { };
            private static void Postfix(UnityModManager.UI __instance, ref Rect ___mWindowRect, ref Vector2[] ___mScrollPosition, ref int ___tabId)
            {
                Main.ummWidth = ___mWindowRect.width;
            }
        }
    }
}