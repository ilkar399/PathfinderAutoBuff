using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;

namespace PathfinderAutoBuff.Utility
/*
* Based on Hsinyu Chan KingmakerModMaker
* https://github.com/hsinyuhcan/KingmakerModMaker
* And Cabarius ToyBox
* https://github.com/cabarius/ToyBox/
* Toolbars
*/
{
    public static partial class UI
    {
        public static void Toolbar(ref int selected, string[] texts, GUIStyle style = null, params GUILayoutOption[] options)
        {
            selected = GUILayout.Toolbar(selected, texts, style ?? UnityEngine.GUI.skin.button, options);
        }

        public static float RoundedHorizontalSlider(float value, int digits, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            if (digits < 0)
            {
                float num = (float)Math.Pow(10d, -digits);
                return (float)Math.Round(GUILayout.HorizontalSlider(value, leftValue, rightValue, options) / num, 0) * num;
            }
            else
            {
                return (float)Math.Round(GUILayout.HorizontalSlider(value, leftValue, rightValue, options), digits);
            }
        }

        public static void Splitter(Color color, float thickness = 2f)
        {
            Rect position = GUILayoutUtility.GetRect(
                GUIContent.none,
                splitterStyle,
                GUILayout.Height(thickness)
                );
            if (Event.current.type == EventType.Repaint)
            {
                Color restoreColor = UnityEngine.GUI.color;
                UnityEngine.GUI.color = color;
                splitterStyle.Draw(position, false, false, false, false);
                UnityEngine.GUI.color = restoreColor;
            }
        }
    }
}
