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
            GameObject draggableWindowObject = transform.Find("Container/Buttons/DragHandleLeft")?.gameObject;
            GUI.DraggableWindow draggableWindow = draggableWindowObject.GetComponent<GUI.DraggableWindow>();
            //DropDownIconShow
            GameObject dropdownItemTemplate = transform.Find("Template/Viewport/Content/Item").gameObject;
            GUI.DropDownIconShow dropdownIconShow = dropdownItemTemplate.GetComponent<GUI.DropDownIconShow>();
            //ToggleSpriteSwap
            GameObject recordToggleObject = transform.Find("Container/Buttons/RecordToggle")?.gameObject;
            GUI.ToggleImageSwap toggleImageSwap = recordToggleObject.GetComponent<GUI.ToggleImageSwap>();
            result = result && (draggableWindow != null) && (dropdownIconShow != null) && (toggleImageSwap != null);
            return result;
        }
    }
}
