using DG.Tweening;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UI.Formation;
using Kingmaker.UI.Constructor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Utility.StatusWrapper;
using static PathfinderAutoBuff.Utility.SettingsWrapper;


namespace PathfinderAutoBuff.GUIWoTR
{
    /*
     * Toolbar to execute one of the defined in settings queues
     * WoTR version (they're different cause of the game UI difference)
     * 
     */
    public class ABQueuesToolbar : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private RectTransform _body;
        private HorizontalLayoutGroupWorkaround _toggleLayout;
        private List<ButtonWrapper> queueButtons;
        private float m_scale;
        private float m_width;
        private bool m_enabled;
        internal int toggleInstanceID;
        private int m_buttonCount = 5;

        public static ABQueuesToolbar CreateObject()
        {
            //Getting base object
            UICommon uiCommon = Game.Instance.UI.Common;
            GameObject hudLayout = uiCommon?.transform.Find("HUDLayout")?.gameObject;
            GameObject togglePanel = uiCommon?.transform.Find("EscMenuView/Window/ButtonBlock")?.gameObject;
            if (!hudLayout || !togglePanel)
                return null;
            //Initialize windows
            GameObject aBQueuesToolbar = new GameObject("ABQueuesToolbar", typeof(RectTransform), typeof(CanvasGroup));
            aBQueuesToolbar.transform.SetParent(hudLayout.transform);
            aBQueuesToolbar.transform.SetSiblingIndex(0);
            RectTransform rectABQueuesToolbar = (RectTransform)aBQueuesToolbar.transform;
            rectABQueuesToolbar.anchorMin = new Vector2(0.95f, 0.8f);
            rectABQueuesToolbar.anchorMax = new Vector2(0.95f, 0.8f);
            rectABQueuesToolbar.pivot = new Vector2(0.5f, 0.5f);
            rectABQueuesToolbar.localPosition += rectABQueuesToolbar.forward * hudLayout.transform.position.z;
            rectABQueuesToolbar.rotation = Quaternion.identity;
            //Initialize body
            GameObject body = Instantiate(togglePanel, rectABQueuesToolbar.transform, false);
            body.name = "AutoBuffBody";
            //Panel background
            Image imgBackground = body.AddComponent<Image>();
            Image imgSource = uiCommon.transform.Find("ServiceWindowConfig/SpellBookView/Background")?.gameObject.GetComponent<Image>();
            if (imgSource && imgBackground)
            {
                imgBackground.sprite = imgSource.sprite;
            }
            RectTransform rectBody = (RectTransform)body.transform;
            rectBody.anchorMin = new Vector2(1f, 1f);
            rectBody.anchorMax = new Vector2(1f, 1f);
            rectBody.pivot = new Vector2(1f, 1f);
            rectBody.localPosition = new Vector3(0f, 0f, 0f);
            rectBody.rotation = Quaternion.identity;

            //Fitter and Layout definition
            ContentSizeFitter contentSizeFitter = body.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            VerticalLayoutGroup verticalLayoutGroup = body.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childAlignment = TextAnchor.UpperRight;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childForceExpandWidth = true;
            verticalLayoutGroup.childForceExpandHeight = false;

            //UI Button objects creating/pointing
            void SetQueueButtons(OwlcatButton button, string name)
            {
                button.name = name;
                button.transform.SetParent(verticalLayoutGroup.transform, false);
                RectTransform rect = (RectTransform)button.transform;
                rect.anchoredPosition = new Vector2(0f, 0f);
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.rotation = Quaternion.identity;
            }
            // toggleFormation_00
            OwlcatButton toggleAB_00 = body.transform.Find("SaveButton").gameObject.GetComponent<OwlcatButton>();
            SetQueueButtons(toggleAB_00, "ToggleAB_00");
            // toggleFormation_01
            OwlcatButton toggleAB_01 = body.transform.Find("LoadButton").gameObject.GetComponent<OwlcatButton>();
            SetQueueButtons(toggleAB_01, "ToggleAB_01");
            // toggleFormation_02
            OwlcatButton toggleAB_02 = body.transform.Find("OptionsButton").gameObject.GetComponent<OwlcatButton>();
            SetQueueButtons(toggleAB_02, "ToggleAB_02");
            // toggleFormation_03
            OwlcatButton toggleAB_03 = body.transform.Find("MainMenuButton").gameObject.GetComponent<OwlcatButton>();
            SetQueueButtons(toggleAB_03, "ToggleAB_03");
            // toggleFormation_04
            OwlcatButton toggleAB_04 = body.transform.Find("QuitButton").gameObject.GetComponent<OwlcatButton>();
            SetQueueButtons(toggleAB_04, "ToggleAB_04");
            /*// Settings
            GameObject toggleABSettingsObject = Instantiate(toggleAB_02.gameObject, body.transform, false);
            Button toggleABSettings = toggleABSettingsObject.GetComponent<Button>();
            SetQueueButtons(toggleABSettings, "ToggleABSettings");
            */
            // clear unused child
            for (int i = body.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = body.transform.GetChild(i).gameObject;
                if (child.name != "ToggleAB_00" && child.name
                    != "ToggleAB_01" && child.name
                    != "ToggleAB_02" && child.name
                    != "ToggleAB_03" && child.name
                    != "ToggleAB_04" && child.name
                    != "ToggleABSettings")
                {
                    child.SafeDestroy();
                }
            }
            Logger.Debug("Created object");
            return aBQueuesToolbar.AddComponent<ABQueuesToolbar>();
        }

        public void Awake()
        {
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _body = (RectTransform)transform.Find("AutoBuffBody");
            _toggleLayout = _body.GetComponent<HorizontalLayoutGroupWorkaround>();
            queueButtons = new List<ButtonWrapper>();
            queueButtons.Add(new ButtonWrapper(
                _body.Find("ToggleAB_00").gameObject.GetComponent<OwlcatButton>(),
                "#1", 1));
            queueButtons.Add(new ButtonWrapper(
                _body.Find("ToggleAB_01").gameObject.GetComponent<OwlcatButton>(),
                "#2", 2));
            queueButtons.Add(new ButtonWrapper(
                _body.Find("ToggleAB_02").gameObject.GetComponent<OwlcatButton>(),
                "#3", 3));
            queueButtons.Add(new ButtonWrapper(
                _body.Find("ToggleAB_03").gameObject.GetComponent<OwlcatButton>(),
                "#4", 4));
            queueButtons.Add(new ButtonWrapper(
                _body.Find("ToggleAB_04").gameObject.GetComponent<OwlcatButton>(),
                "#5", 5));
            /*
            ABSettings = new ButtonWrapper(
                _body.Find("ToggleABSettings").gameObject.GetComponent<Button>(),
                "Edit", -1);
            */
            UpdateButtonStatus();
        }

        public void Hide()
        {

        }

        void Update()
        {
            if (IsValidMode(Game.Instance.CurrentMode) & IsHUDShown() && UIEnabled())
            {
                ResizeScale(ABToolbarScale);
                ResizeWidth(ABToolbarWidth);
                if (!m_enabled)
                {
 //                   UpdateButtonStatus();
                    m_enabled = true;
                    //                   Logger.Debug("m_enabled "  + m_enabled);
                    _canvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
                    _body.DOAnchorPosY(0f, 0.5f, false).SetUpdate(true);
                }
            }
            else
            {
                if (m_enabled)
                {
 //                   UpdateButtonStatus();
                    m_enabled = false;
                    //                   Logger.Debug("m_enabled "  + m_enabled);
                    _canvasGroup.DOFade(0f, 0.5f).SetUpdate(true);
                    _body.DOAnchorPosY(_body.rect.height, 0.5f, false).SetUpdate(true);
                }
            }
        }

        void OnEnable()
        {
            Logger.Debug(MethodBase.GetCurrentMethod());
            //TODO: bind hotkeys
        }

        void OnDisable()
        {
            Logger.Debug(MethodBase.GetCurrentMethod());
            //TODO: unbind hotkeys
            _canvasGroup.DOKill();
            _body.DOKill();
        }

        private void ResizeScale(float scale)
        {
            if (m_scale != scale)
            {
                m_scale = scale;
                _body.localScale = new Vector3(scale, scale, scale);
            }
        }

        private void ResizeWidth(float width)
        {
            if (m_width != width)
            {
                m_width = width;
                _body.sizeDelta = new Vector2(width, _body.sizeDelta.y);
            }
        }

        public void UpdateButtonStatus()
        {
           Dictionary<int,string> favoriteQueues = new Dictionary<int, string>(FavoriteQueues);
            for (int i = 0; i < m_buttonCount; i++)
            {
                if (favoriteQueues[i+1] != "")
                {
                    queueButtons[i].Activate();
                }
                else
                {
                    queueButtons[i].Deactivate();
                }
            }
        }

        //Toggle button wrapper/constructor
        private class ButtonWrapper
        {
            private readonly Color _enableColor = Color.white;
            private readonly Color _disableColor = new Color(0.7f, 0.8f, 1f);
            private readonly OwlcatButton _button;
            private readonly TextMeshProUGUI _textMesh;
            private readonly Image _image;

            //Handle toggle value change
            private void onClick(int queueID)
            {
                if (queueID == -1)
                {
                    //TODO: invoke edit UI
                }
                if (Main.Settings.favoriteQueues[queueID] != "")
                {
                    ScriptController.Reset();
                    try
                    {
                        CommandQueue executingQueue = new CommandQueue();
                        bool result1 = executingQueue.LoadFromFile($"{Main.Settings.favoriteQueues[queueID]}.json");
                        ScriptController.CreateFromQueue(executingQueue.CommandList, Main.Settings.favoriteQueues[queueID]);
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

            public void Activate()
            {
                this._button.gameObject.SetActive(true);
            }

            public void Deactivate()
            {
                this._button.gameObject.SetActive(false);
            }

            public ButtonWrapper(OwlcatButton button, string text, int queueID)
            {
                _button = button;
                Logger.Log(queueID);
                _button.m_OnLeftClick = new Button.ButtonClickedEvent();
                _button.m_OnLeftClick.AddListener(() => {
                    onClick(queueID);
                });
                _textMesh = _button.GetComponentInChildren<TextMeshProUGUI>();              
                _textMesh.fontSize = 20;
                _textMesh.fontSizeMax = 72;
                _textMesh.fontSizeMin = 18;
                _textMesh.text = text;
                _textMesh.color = _button.isActiveAndEnabled ? _enableColor : _disableColor;
                _image = _button.GetComponentInChildren<Image>();
                GameObject styleSource = Game.Instance.UI.Common?.transform.Find("HUDLayout/CombatLog_New/TooglePanel/ToogleAll")?.gameObject;
                _image.sprite = styleSource.GetComponentInChildren<Image>().sprite;
            }
        }
    }
}
