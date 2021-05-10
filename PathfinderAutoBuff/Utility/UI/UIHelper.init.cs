using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace PathfinderAutoBuff.Utility
{
    public static partial class UI
    {
        private static readonly GUIStyle splitterStyle;

        static UI()
        {
            GUISkin skin = GUI.skin;

            splitterStyle = new GUIStyle();
            splitterStyle.normal.background = Texture2D.whiteTexture;
            splitterStyle.margin = new RectOffset(0, 0, 4, 4);
            splitterStyle.fixedHeight = 1;
        }
    }
}
