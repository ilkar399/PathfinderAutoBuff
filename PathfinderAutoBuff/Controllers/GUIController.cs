using Kingmaker;
using Kingmaker.PubSubSystem;
using PathfinderAutoBuff.Utility;
using System.Reflection;
#if (WOTR)
using PathfinderAutoBuff.GUIWoTR;
#elif (KINGMAKER)
using PathfinderAutoBuff.GUIKingmaker;
#endif
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
        public ABQueuesToolbar ABQueuesToolbar { get; private set; }
        public int Priority => 800;

        public void Attach()
        {
            if (!ABQueuesToolbar)
            {
                Logger.Log("Attach");
                ABQueuesToolbar = ABQueuesToolbar.CreateObject();
            }
        }

        public void Detach()
        {
            Logger.Log("Detach");
            if (ABQueuesToolbar)
            {
//                ABQueuesToolbar.Clear();
                ABQueuesToolbar.SafeDestroy();
                ABQueuesToolbar = null;
            }
        }

#if (DEBUG)
        public void Clear()
        {
            Transform abQueuesToolbar;
            while (abQueuesToolbar = Game.Instance.UI.Common.transform.Find("Formations/ToggleGroup/"))
                abQueuesToolbar.SafeDestroy();
            ABQueuesToolbar = null;
        }

#endif

        public void Update()
        {
            Detach();
            Attach();
        }

        //Event handlers

        public void Enable()
        {
            if (UIEnabled)
            {
                Logger.Debug(MethodBase.GetCurrentMethod());
                Main.uiController = this;
                Attach();
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
        }
    }
}
