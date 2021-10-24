using Kingmaker;
using Kingmaker.PubSubSystem;
using PathfinderAutoBuff.Utility;
using System.Reflection;
using PathfinderAutoBuff.GUI;
using UnityEngine;
using static PathfinderAutoBuff.Main;
using static PathfinderAutoBuff.Utility.SettingsWrapper;

namespace PathfinderAutoBuff.Controllers
{
    //Controller for the in-game UI mod parts
    public class GUIController :
#if (WOTR)
        IAreaHandler
#elif (KINGMAKER)
        ISceneHandler 
#endif
    {
        internal GUIManager AutoBuffGUI { get; private set; }
        public int Priority => 400;

        public void Attach()
        {

            if (!AutoBuffGUI)
            {
                Logger.Debug("Attach");
                if (AutoBuffGUI == null)
                    AutoBuffGUI = GUIManager.CreateObject();
            }
        }

        public void Detach()
        {
            Logger.Debug("Detach");
            AutoBuffGUI.SafeDestroy();
            AutoBuffGUI = null;
        }

#if (DEBUG)
        public void Clear()
        {
            Transform transform;
            while (transform = Game.Instance.UI.Common.transform.Find(GUIManager.Source))
            {
                transform.SafeDestroy();
            }
            transform = null;
        }

#endif

        public void Update()
        {
            Detach();
            Attach();
            if (AutoBuffGUI != null)
                AutoBuffGUI.RefreshView();
        }

        //Event handlers
        public void Enable()
        {
            if (UIEnabled)
            {
                Logger.Debug(MethodBase.GetCurrentMethod());
                Attach();
                if (AutoBuffGUI != null)
                    AutoBuffGUI.RefreshView();
                EventBus.Subscribe(this);
            }
        }

        public void Disable()
        {
            Logger.Debug(MethodBase.GetCurrentMethod());
            EventBus.Unsubscribe(this);
            Detach();
            Main.uiController = null;
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            Logger.Debug(MethodBase.GetCurrentMethod());
            Attach();
            if (AutoBuffGUI != null)
                AutoBuffGUI.RefreshView();
        }
    }
}
