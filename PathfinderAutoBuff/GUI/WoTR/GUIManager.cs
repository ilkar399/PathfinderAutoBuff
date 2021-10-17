using DG.Tweening;
using Kingmaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using PathfinderAutoBuff.Utility;
using PathfinderAutoBuff.QueueOperations;
using static PathfinderAutoBuff.Main;
using static PathfinderAutoBuff.Utility.StatusWrapper;

using Kingmaker.UI.Selection;

namespace PathfinderAutoBuff.GUI
{
    public class GUIManager : MonoBehaviour
    {
        public const string Source = "PathfinderAutoBuffActionPanel";
        private RectTransform rectTransform;
        bool m_enabled;

        internal CommandQueue selectedQueue;

        [Header("DropList")]
        [SerializeField]
        private TMP_Dropdown m_Dropdown;

        [Header("Buttons")]
        [SerializeField]
        private Button m_Execute;

        [SerializeField]
        private Button m_Remove;

        [Header("Toggles")]
        [SerializeField]
        private Toggle m_RecordToggle;

        [SerializeField]
        private Toggle m_Favorite;

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
                Transform _transform = BundleManger.LoadedPrefabs[Source].transform;
                Logger.Debug(_transform);
                var window = Instantiate(_transform); //We ditch the TutorialCanvas as talked about in the Wiki, we will attach it to a different parent
                window.SetParent(staticCanvas, false); //Attaches our window to the static canvas
                window.SetAsFirstSibling(); //Our window will always be under other UI elements as not to interfere with the game. Top of the list has the lowest priority
                RectTransform rectTransform = (RectTransform)window;
                //Scaling according to settings
                rectTransform.localScale = new Vector3(SettingsWrapper.ABToolbarScale, SettingsWrapper.ABToolbarScale, SettingsWrapper.ABToolbarScale);
                rectTransform.anchoredPosition = new Vector2(SettingsWrapper.GUIPosX, SettingsWrapper.GUIPosY);
                window.gameObject.SetActive(true);
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
            rectTransform = (RectTransform)this.transform;
            this.m_Dropdown = this.transform.Find("Container/DropDown")?.gameObject.GetComponent<TMP_Dropdown>();
            m_Dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
            m_Dropdown.onValueChanged.AddListener(new UnityAction<int>(HandleSelectItem));
            GameObject dropdownItemTemplate = m_Dropdown.transform.Find("Template/Viewport/Content/Item").gameObject;
            dropdownItemTemplate.AddComponent<DropDownIconShow>();
            this.m_Execute = this.transform.Find("Container/Buttons/Execute")?.gameObject.GetComponent<Button>();
            m_Execute.onClick = new Button.ButtonClickedEvent();
            m_Execute.onClick.AddListener(new UnityAction(HandleExecuteQueueClick));
            this.m_RecordToggle = this.transform.Find("Container/Buttons/RecordToggle")?.gameObject.GetComponent<Toggle>();
            m_RecordToggle.gameObject.AddComponent<ToggleImageSwap>();
            m_RecordToggle.onValueChanged.AddListener(HandleRecordingToggleChange);
            this.m_Remove = this.transform.Find("Container/Buttons/Remove")?.gameObject.GetComponent<Button>();
            m_Remove.onClick = new Button.ButtonClickedEvent();
            m_Remove.onClick.AddListener(new UnityAction(HandleRemoveQueueClick));
            this.m_Favorite = this.transform.Find("Container/Buttons/Favorite")?.gameObject.GetComponent<Toggle>();
            m_Favorite.gameObject.AddComponent<ToggleImageSwap>();
            m_Favorite.onValueChanged.AddListener(HandleFavoriteClick);
            //Add draggable windows component allowing the window to be dragged when the button is pressed down
            GameObject dragLeft = this.transform.Find("Container/Buttons/DragHandleLeft")?.gameObject;
            GameObject dragRight = this.transform.Find("Container/Buttons/DragHandleRight")?.gameObject;
            GameObject scrollbar = this.transform.Find("Container/DropDown/Template/Scrollbar")?.gameObject;
            dragLeft.AddComponent<DraggableWindow>();
            dragRight.AddComponent<DraggableWindow>();
        }

        private void Update()
        {
            //This is a unity message that runs each frame.
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            RectTransform body = gameObject.GetComponent<RectTransform>();
            if (IsValidMode(Game.Instance.CurrentMode) & IsHUDShown() && UIEnabled())
            {
                if (!m_enabled)
                {
                    m_enabled = true;
                    canvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
                }
            }
            else
            {
                if (m_enabled)
                {
                    m_enabled = false;
                    canvasGroup.DOFade(0f, 0.5f).SetUpdate(true);
                }
            }
        }

        //Selecting item from DropDown
        //TODO: Error Handling
        private void HandleSelectItem(int index)
        {
            Logger.Debug($"index {index} OptionName {this.m_Dropdown.options[index].text}");
            Logger.Debug($"Favorite queues {String.Join("; ", SettingsWrapper.FavoriteQueues2)}");
            int queueIndex = Array.IndexOf(Main.QueuesController.m_Queues, this.m_Dropdown.options[index].text);
            if (queueIndex > -1)
            {
                selectedQueue = new CommandQueue();
                string queueName = Main.QueuesController.m_Queues[queueIndex];
                if (selectedQueue.LoadFromFile($"{queueName}.json"))
                {
                    Main.QueuesController.CurrentQueueName = queueName;
                    Main.QueuesController.CurrentQueueIndex = queueIndex;
                    Main.QueuesController.queueController = new QueueController(selectedQueue);
                    m_Favorite.isOn = SettingsWrapper.FavoriteQueues2.Contains(Main.QueuesController.CurrentQueueName);
                }
                else
                {
                    Logger.Log($"Error loading queue {queueName}");
                }
            }
        }

        //Start/stop recording
        private void HandleRecordingToggleChange(bool state)
        {
            Logger.Debug("HandleRecordingToggleClick");
            Main.recordQueue.Toggle();
            HandleRecordingToggle(state);
        }

        public void HandleRecordingToggle(bool state)
        {
            m_RecordToggle.isOn = state;
        }

        //Execute selected queue
        private void HandleExecuteQueueClick()
        {
            Logger.Debug("ExecuteQueue");
            Logger.Debug(selectedQueue);
            if (selectedQueue != null)
            {
                ScriptController.Reset();
                try
                {
                    ScriptController.Reset();
                    ScriptController.CreateFromQueue(Main.QueuesController.queueController.CurrentQueue().CommandList,
                                                    Main.QueuesController.CurrentQueueName);
                    ScriptController.Run();
                }
                catch (Exception ex)
                {
#if (DEBUG)
                    Logger.Log(ex.StackTrace);
                    throw ex;
#endif
                    Logger.Log($"Error executing queue {Main.QueuesController.CurrentQueueName}");
                }
            }
            else
            {
                Logger.Log("No queue selected");
            }
        }

        //Remove selected queue
        private void HandleRemoveQueueClick()
        {
            if (selectedQueue != null)
            {
                bool queueDeletionFlag = IOHelpers.DeleteQueue(Main.QueuesController.CurrentQueueName);
                if (!queueDeletionFlag)
                    Logger.Log(string.Format(Local["Menu_Queues_ErrorDeleting"], Main.QueuesController.CurrentQueueName));
                else
                {
                    Main.QueuesController.ReloadQueues();
                    RefreshView();
                    return;
                }
            }
            Logger.Debug("RemoveQueue");
        }

        //Add/remove from Favorites
        private void HandleFavoriteClick(bool state)
        {
            Logger.Debug($"Favorite {Main.QueuesController.CurrentQueueName} {state}");
            if (!SettingsWrapper.FavoriteQueues2.Contains(Main.QueuesController.CurrentQueueName) && state)
            {
                SettingsWrapper.FavoriteQueues2.Add(Main.QueuesController.CurrentQueueName);
            }
            else if (!state)
            {
                SettingsWrapper.FavoriteQueues2.Remove(Main.QueuesController.CurrentQueueName);
            }
            this.m_Favorite.isOn = state;
            this.m_Dropdown.RefreshShownValue();
        }

        //Force refreshing view if data/scale changed
        public void RefreshView()
        {
            //Update Dropdown Options
            this.m_Dropdown.ClearOptions();
            List<string> list = new List<string>();
            if (Main.QueuesController == null)
            {
                Main.QueuesController = new QueuesController();
            }
            int favoriteIndex = 0;
            foreach (string queueName in Main.QueuesController.m_Queues)
            {
                if (SettingsWrapper.FavoriteQueues2.Contains(queueName))
                {
                    list.Insert(favoriteIndex, queueName);
                    favoriteIndex++;
                }
                else
                    if (!SettingsWrapper.GUIFavoriteOnly)
                        list.Add(queueName);
            }
            this.m_Dropdown.AddOptions(list);
            Logger.Debug($"RefreshView {list.Count} queues");
            //Rescale
            rectTransform.localScale = new Vector3(SettingsWrapper.ABToolbarScale, SettingsWrapper.ABToolbarScale, SettingsWrapper.ABToolbarScale);
        }

        //Options count for tests
        public int DropdownOptionsCount()
        {
            if (this.m_Dropdown != null)
                return this.m_Dropdown.options.Count;
            else
                return -1;
        }

        public bool Enabled
        {
            get => this.m_enabled;
            private set => this.m_enabled = value;
        }
    }
}
