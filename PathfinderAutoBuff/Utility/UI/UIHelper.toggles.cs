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
* Toggle buttons
*/
{
    public static partial class UI
    {
        public static string DefaultFormatOn = "✔".Bold().Color(RGBA.lime) + " - {0}";
        public static string DefaultFormatOff = "✖".Bold().Color(RGBA.red) + " - {0}";

        public static string GetToggleText(bool toggle, string text, string formatOn, string formatOff)
        {
            return string.Format(toggle ? formatOn : formatOff, text);
        }

        public static bool ToggleButton(
            bool toggle,
            string text,
            string formatOn,
            string formatOff,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            ToggleButton(ref toggle, text, formatOn, formatOff, style, options);
            return toggle;
        }

            public static void ToggleButton(
            ref bool toggle,
            string text,
            string formatOn,
            string formatOff,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {

            if (GUILayout.Button(GetToggleText(toggle, text, formatOn, formatOff), style ?? GUI.skin.button, options))
                toggle = !toggle;
        }

        public static bool ToggleButton(bool toggle, string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            ToggleButton(ref toggle, text, style, options);
            return toggle;
        }

        public static void ToggleButton(ref bool toggle, string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(GetToggleText(toggle, text, DefaultFormatOn, DefaultFormatOff), style ?? GUI.skin.button, options))
                toggle = !toggle;
        }

        public static bool ToggleButton(bool toggle, string text, Action on, Action off, GUIStyle style = null, params GUILayoutOption[] options)
        {
            ToggleButton(ref toggle, text, on, off, style, options);
            return toggle;
        }

        public static void ToggleButton(ref bool toggle, string text, Action on, Action off, GUIStyle style = null, params GUILayoutOption[] options)
        {
            bool old = toggle;
            ToggleButton(ref toggle, text, style, options);
            if (toggle != old)
            {
                if (toggle)
                    on?.Invoke();
                else
                    off?.Invoke();
            }
        }

        public static void ToggleButton(ref bool toggle, string text, ref float minWidth, GUIStyle style = null, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(GetToggleText(toggle, text, DefaultFormatOn, DefaultFormatOff));
            style = style ?? GUI.skin.button;
            minWidth = Math.Max(minWidth, style.CalcSize(content).x);
            if (GUILayout.Button(content, style, options?.Concat(new[] { GUILayout.Width(minWidth) }).ToArray() ?? new[] { GUILayout.Width(minWidth) }))
                toggle = !toggle;
        }

        public static void ToggleButton(ref bool toggle, string text, ref float minWidth, Action on, Action off, GUIStyle style = null, params GUILayoutOption[] options)
        {
            bool old = toggle;
            ToggleButton(ref toggle, text, ref minWidth, style, options);
            if (toggle != old)
            {
                if (toggle)
                    on?.Invoke();
                else
                    off?.Invoke();
            }
        }

        public static bool ToggleTypeList(bool toggle, string text, HashSet<string> selectedTypes, HashSet<Type> allTypes, GUIStyle style = null, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();

            ToggleButton(ref toggle, text, style, options);

            if (toggle)
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Select All"))
                        {
                            foreach (Type type in allTypes)
                            {
                                selectedTypes.Add(type.FullName);
                            }
                        }
                        if (GUILayout.Button("Deselect All"))
                        {
                            selectedTypes.Clear();
                        }
                    }

                    foreach (Type type in allTypes)
                    {
                        ToggleButton(selectedTypes.Contains(type.FullName), type.Name.ToSentence(),
                            () => selectedTypes.Add(type.FullName),
                            () => selectedTypes.Remove(type.FullName),
                            style, options);
                    }
                }
            }
            GUILayout.EndHorizontal();
            return toggle;
        }
    }
}
