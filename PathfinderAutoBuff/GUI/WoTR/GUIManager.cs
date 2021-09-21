using DG.Tweening;
using Kingmaker;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using PathfinderAutoBuff.Utility;
using PathfinderAutoBuff.QueueOperattions;
using static PathfinderAutoBuff.Main;
using Kingmaker.UI.Selection;

namespace PathfinderAutoBuff.GUI.WoTR
{
    internal class GUIManager : MonoBehaviour
    {
        public const string Source = "pathfinderautobuffpanel";
        private Text _text;

        internal CommandQueue selectedQueue;

        [Header("DropList")]
        [SerializeField]
        private TMP_Dropdown m_Dropdown;

        [Header("Buttons")]
        [SerializeField]
        private OwlcatButton m_Execute;

        [SerializeField]
        private OwlcatButton m_RecordStop;

        [SerializeField]
        private OwlcatButton m_Remove;

        [SerializeField]
        private OwlcatButton m_Settings;

        [SerializeField]
        private bool m_IsInit = false;


        public static GUIManager CreateObject()
        {
            //This is the method that get's called when it is time to create the UI.  This happens every time a scene is loaded.

            try
            {
                if (!Game.Instance.UI.Canvas) return null;
                if (!BundleManger.IsLoaded(Source)) throw new NullReferenceException();

                //
                //Attempt to get the wrath objects needed to build the UI
                //
                var staticCanvas = Game.Instance.UI.Canvas.RectTransform;
                //
                //Attempt to get the objects loaded from the AssetBundles and build the window.
                //
                var window = Instantiate(BundleManger.LoadedPrefabs[Source].transform.Find("WeaponWindow")); //We ditch the TutorialCanvas as talked about in the Wiki, we will attach it to a different parent
                window.SetParent(staticCanvas, false); //Attaches our window to the static canvas
                window.SetAsFirstSibling(); //Our window will always be under other UI elements as not to interfere with the game. Top of the list has the lowest priority
                return window.gameObject.AddComponent<GUIManager>(); //This adds this class as a component so it can handle events, button clicks, awake, update, etc.
            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
            }
            return new GUIManager();
        }

        private void Awake()
        {
            //This is a unity message that runs once when the script activates (Check Unity documenation for the differences between Start() and Awake()

            //
            // Setup the listeners when the script starts
            //
            this.m_Dropdown = this.transform.Find("Container/DropDown")?.gameObject.GetComponent<TMP_Dropdown>();
            this.m_Execute = this.transform.Find("Container/Buttons/Execute")?.gameObject.GetComponent<OwlcatButton>();
            this.m_RecordStop = this.transform.Find("Container/Buttons/RecordStop")?.gameObject.GetComponent<OwlcatButton>();
            this.m_Remove = this.transform.Find("Container/Buttons/Remove")?.gameObject.GetComponent<OwlcatButton>();
            this.m_Settings = this.transform.Find("Container/Buttons/Settings")?.gameObject.GetComponent<OwlcatButton>();

            var button = this.transform.Find("Foreground/Button").GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new UnityAction(HandleButtonClick));
            button.gameObject.AddComponent<DraggableWindow>(); //Add draggable windows component allowing the window to be dragged when the button is pressed down

            _text = this.transform.Find("Foreground/Text").GetComponent<Text>(); //Find the text component so we can update later.
        }

        private void Update()
        {
            //This is a unity message that runs each frame.
        }

        //Selecting item from DropDown
        private void HandleSelectItem(int index)
        {
            if (Main.queueData.CommandQueues.ElementAtOrDefault(index) != null)
                selectedQueue = queueData.CommandQueues.ElementAtOrDefault(index);
        }

        //Starting/stop recording
        private void HandleRecordingToggleClick()
        {
            Logger.Log("OnRecordingToggle");
            Main.recordQueue.Toggle();
            HandleRecordingToggle(Main.recordQueue.Enabled);
        }

        //Handling Recording status change
        private void HandleRecordingToggle(bool isRecording)
        {

        }

        //Execuute selected queue
        private void HandleExecuteQueueClick()
        {
            Logger.Log("ExecuteQueue");
            if (selectedQueue != null)
            {
                ScriptController.Reset();
                try
                {
                    CommandQueue executingQueue = new CommandQueue();
                    bool result1 = executingQueue.LoadFromFile($"{Main.Settings.favoriteQueues[queueID]}.json");
                    ScriptController.CreateFromQueue(selectedQueue.CommandList,
                                                    selectedQueue);
                    ScriptController.Run();
                }
                catch (Exception ex)
                {
#if (DEBUG)
                    Logger.Log(ex.StackTrace);
                    throw ex;
#endif
                    Logger.Log($"Error executing queue {Main.Settings.favoriteQueues[queueID]}");
                }
            }
        }

        //Remove selected queue
        private void HandleRemoveQueueClick()
        {
            if (selectedQueue != null)
            {
                queueData.DeleteQueue(selectedQueue.QueueName);

            }
            Logger.Log("RemoveQueue");
        }

        //Open mod settings window
        private void HandleStartSettingsClick()
        {
            Kingmaker.Modding.OwlcatModificationsWindow.Show();
            Logger.Log("StartSettings");
        }


        private void HandleButtonClick()
        {
            //Display the equiped weapon wame of the selected character
            var selection = SelectionManager.Instance.SelectedUnits;
            var color = _text.color;
            _text.color = new Color(color.r, color.g, color.b, 0f);
            if ((selection != null) && (selection.Count == 1))
                _text.text = selection[0].Body.PrimaryHand.MaybeWeapon.Name;
            else if (selection.Count == 0)
                _text.text = "No one selected";
            else if (selection.Count > 1)
                _text.text = "Select only one";
            _text.DOFade(1f, 1.5f); //fade text alpha in using tweening
        }
    }
}
