using Kingmaker.UI.Common;
using Kingmaker.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PathfinderAutoBuff.GUIWoTR
{
    /*
    * Showing icon of the dropdown item if queueName is in favorites
    * TODO: Remake into the general purpose component
    */
    public class DropDownIconShow: MonoBehaviour
    {
        private RectTransform _iconTransform;

        public void Start()
        {
            _iconTransform = (RectTransform)transform.Find("ItemIcon").gameObject.transform;
        }

        public void Update()
        {
            string queueName = gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
            if (queueName != null && queueName != "")
                if (Utility.SettingsWrapper.FavoriteQueues2.Contains(queueName))
                {
                    _iconTransform.gameObject.SetActive(true);
                }
                else
                {
                    _iconTransform.gameObject.SetActive(false);
                }
        }
    }
}
