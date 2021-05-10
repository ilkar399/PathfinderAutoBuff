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
* Selectors
*/
{
    public static partial class UI
    {
        public static void SelectionGrid(ref int selected, string[] texts, int xCount, GUIStyle style = null, params GUILayoutOption[] options)
        {
            selected = GUILayout.SelectionGrid(selected, texts, xCount, style ?? GUI.skin.button, options);
        }

        public static void SelectionGrid(ref int selected, string[] texts, int xCount, Action onChanged, GUIStyle style = null, params GUILayoutOption[] options)
        {
            int old = selected;
            SelectionGrid(ref selected, texts, xCount, style, options);
            if (selected != old)
            {
                onChanged?.Invoke();
            }
        }

/*
* DropDown list. 
* showNull - shows the 'Non Selected' option returning -1
*/
        public static void DropDownList(ref bool toggle, ref int selected, List<string> texts, Action onChange,
            bool showNull = true, GUIStyle style = null, params GUILayoutOption[] options)
        {
            using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(false)))
            {
                string caption = "Not Selected";
                int old = selected;
                if (texts.ElementAtOrDefault(selected) != null)
                    caption = texts[selected].Color(RGBA.yellow);
                string formatOn = "◀".Bold().Color(RGBA.yellow) + " - {0}";
                string formatOFF = "▼".Bold() + " - {0}";
                UI.ToggleButton(ref toggle, caption, style, GUILayout.ExpandWidth(false));
                if (toggle)
                {
                    using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(false)))
                    {
                        if (showNull)
                        {
                            if (GUILayout.Button("Not Selected", style, options))
                            {
                                selected = -1;
                                if (selected != old)
                                    onChange.Invoke();
                                toggle = false;
                                return;
                            }
                        }
                        for (int index = 0; index < texts.Count; index++)
                        {
                            if (GUILayout.Button(texts[index], style, options))
                            {
                                selected = index;
                                if (selected != old)
                                    onChange.Invoke();
                                toggle = false;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}