#if (DEBUG)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PathfinderAutoBuff.Tests
{
    /*
     * Testing GUI components
     */
    static class GUITests
    {
        /*
         * Panel and related object creation test
         */
        public static bool MainObjectTests()
        {
            bool result = true;
            //Existence of controller
            result = result && (Main.uiController != null);
            //Existence of manager
            result = result && (Main.uiController.AutoBuffGUI != null);
            //Queues filled in
            result = result && (Main.uiController.AutoBuffGUI.DropdownOptionsCount() > 0);
            return result;
        }

        /*
         * Components creation test
         */

        public static bool GUIComponentsTest()
        {
            Transform transform = Main.uiController?.AutoBuffGUI?.transform;
            if (!transform)
                return false;
            bool result = true;
            //DraggableWindow
            Transform draggableWindowObject = transform.Find("Container/Buttons/DragHandleLeft");
            //DropDownIconShow
            Transform dropdownItemTemplate = transform.Find("Container/DropDown/Template/Viewport/Content/Item");
            //ToggleSpriteSwap
            Transform recordToggleObject = transform.Find("Container/Buttons/RecordToggle");
            if (draggableWindowObject == null || dropdownItemTemplate == null || recordToggleObject == null)
                return false;
            GUI.DropDownIconShow dropdownIconShow = dropdownItemTemplate.gameObject.GetComponent<GUI.DropDownIconShow>();
            GUI.DraggableWindow draggableWindow = draggableWindowObject.gameObject.GetComponent<GUI.DraggableWindow>();
            GUI.ToggleImageSwap toggleImageSwap = recordToggleObject.gameObject.GetComponent<GUI.ToggleImageSwap>();
            result = result && (draggableWindow != null) && (dropdownIconShow != null) && (toggleImageSwap != null);
            return result;
        }
    }
}
#endif