using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;


namespace PathfinderAutoBuff

{

    public static class BackgroundStyledBox
    /*
     * Background style color constructor
     */
    {
        private static GUIStyle style = new GUIStyle("box");
        private static Texture2D texture = new Texture2D(1, 1);


        public static GUIStyle Get(Color color)
        {
            texture.SetPixel(0, 0, color);
            texture.Apply();
            style.normal.background = texture;
            return style;
        }
    }

    static class DefaultStyles
    /*
    * UI Styles to gather them in one place
    * 
    */
    {
        //Font sizes
        public static readonly float SmallFontSize = 10f;
        public static readonly float BigFontSize = 20f;

        //Default UI widths
        public static float LeftBoxWidth = 300f;
        public static float RightBoxWidth = 600f;

        private static readonly GUIStyle
            buttonDefault,
            labelDefault,
            buttonFixed120,
            labelFixed120,
            textField120,
            buttonSelector;

        static DefaultStyles()
        {
            buttonFixed120 = new GUIStyle(GUI.skin.button) { fixedWidth = 120f, wordWrap = true, alignment = TextAnchor.MiddleCenter };
            labelFixed120 = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fixedHeight = 25f, fixedWidth = 120f };
            textField120 = new GUIStyle(GUI.skin.textField) { fixedWidth = 200f, wordWrap = true };
            labelDefault = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            buttonDefault = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            buttonSelector = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            buttonSelector.onNormal.textColor = Color.yellow;
        }

        //Default styles with variable parameters
        public static GUIStyle ButtonDefault() { return buttonDefault; }
        public static GUIStyle LabelDefault() { return labelDefault; }
        //Default styles with fixed parameters
        public static GUIStyle ButtonFixed120() { return buttonFixed120; }
        public static GUIStyle LabelFixed120() { return labelFixed120; }
        public static GUIStyle TextField200() { return textField120; }
        //Styles for certain elements
        public static GUIStyle ButtonSelector() {
            return buttonSelector;
        }
        public static GUIStyle ButtonSelectorLeft()
        {
            GUIStyle result;
            result = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, wordWrap = true };
            result.onNormal.textColor = Color.yellow;
            return result;
        }
        public static GUIStyle ButtonSelector120()
        {
            GUIStyle result;
            result = new GUIStyle(GUI.skin.button) {  alignment = TextAnchor.MiddleCenter, fixedWidth = 120f, wordWrap = true,  };
            result.onNormal.textColor = Color.yellow;
            return result;
        }

        public static GUIStyle ButtonEmpty()
        {
            GUIStyle result;
            result = new GUIStyle(GUIStyle.none) { alignment = TextAnchor.MiddleCenter, wordWrap = true,};
            return result;
        }

        public static GUIStyle ButtonWrapped() { return new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, wordWrap = true }; }

        //Text style wrappers
        //Header2
        public static string TextHeader2(string text) { return text.Bold().Yellow(); }
        public static string TextHeader2(string format, object arg0) { return String.Format(format, arg0).Bold().Yellow(); }
        public static string TextHeader2(string format, object arg0, object arg1) { return String.Format(format, arg0, arg1).Bold().Yellow(); }
        public static string TextHeader2(string format, object arg0, object arg1, object arg2) { return String.Format(format, arg0, arg1, arg2).Bold().Yellow(); }
        public static string TextHeader2(string format, object[] args) { return String.Format(format, args).Bold().Yellow(); }

        //Header3
        public static string TextHeader3(string text) { return text.Bold(); }
        public static string TextHeader3(string format, object arg0) { return String.Format(format, arg0).Bold(); }
        public static string TextHeader3(string format, object arg0, object arg1) {return String.Format(format, arg0, arg1).Bold();}
        public static string TextHeader3(string format, object arg0, object arg1, object arg2) { return String.Format(format, arg0, arg1, arg2).Bold(); }
        public static string TextHeader3(string format, object[] args) { return String.Format(format, args).Bold(); }

    }
}
