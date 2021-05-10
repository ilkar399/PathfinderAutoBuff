using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;
using GL = UnityEngine.GUILayout;

namespace PathfinderAutoBuff.Utility
/*
* Based on Hsinyu Chan KingmakerModMaker
* https://github.com/hsinyuhcan/KingmakerModMaker
* And Cabarius ToyBox
* https://github.com/cabarius/ToyBox/
* Controls
*/
{
    public static partial class UI
    {

        public static void Label(String title, params GUILayoutOption[] options)
        {
            // var content = tooltip == null ? new GUIContent(title) : new GUIContent(title, tooltip);
            //  if (options.Length == 0) { options = new GUILayoutOption[] { GL.Width(150f) }; }
            GL.Label(title, options);
        }

        public static int AdjusterButton(int value, string text, int min = int.MinValue, int max = int.MaxValue)
        {
            AdjusterButton(ref value, text, min, max);
            return value;
        }

        public static void AdjusterButton(ref int value, string text, int min = int.MinValue, int max = int.MaxValue)
        {
            GUILayout.Label(text, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)) && value > min)
                value--;
            GUILayout.Label(value.ToString(), GUILayout.ExpandWidth(false));
            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)) && value < max)
                value++;
        }

        public static void Hyperlink(string url, Color normalColor, Color hoverColor, GUIStyle style)
        {
            Hyperlink(url, url, normalColor, hoverColor, style);
        }

        public static void Hyperlink(string text, string url, Color normalColor, Color hoverColor, GUIStyle style)
        {
            Color color = GUI.color;
            GUI.color = Color.clear;
            GUILayout.Label(text, style, GUILayout.ExpandWidth(false));
            Rect lastRect = GUILayoutUtility.GetLastRect();
            GUI.color = lastRect.Contains(Event.current.mousePosition) ? hoverColor : normalColor;
            if (GUI.Button(lastRect, text, style))
                Application.OpenURL(url);
            lastRect.y += lastRect.height - 2;
            lastRect.height = 1;
            GUI.DrawTexture(lastRect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = color;
        }

        public static void TextField(ref string value, GUIStyle style = null, params GUILayoutOption[] options)
        {
            value = GUILayout.TextField(value, style ?? GUI.skin.textField, options);
        }

        public static void TextField(ref string value, Action onChanged, GUIStyle style = null, params GUILayoutOption[] options)
        {
            TextField(ref value, null, onChanged, style, options);
        }

        public static void TextField(ref string value, Action onClear, Action onChanged, GUIStyle style = null, params GUILayoutOption[] options)
        {
            string old = value;
            TextField(ref value, style, options);
            if (value != old)
            {
                if (onClear != null && string.IsNullOrEmpty(value))
                    onClear();
                else
                    onChanged();
            }
        }
    }
}
