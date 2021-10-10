using System.Collections.Generic;
using UnityEngine;
using PathfinderAutoBuff.Utility;
using UnityModManagerNet;


namespace PathfinderAutoBuff
{
    public class Settings : UnityModManager.ModSettings
    {
        //Filename
        public string localizationFileName;
        //Mod folder
        public string modPath;
        //Ignore item/skill mods on queue execution
        public bool ignoreModifiiers = true;
        //refresh buffs with the short remaining time
        public bool refreshShort = true;
        //remaining time to refresh in seconds. Don't forget to use TimeSpan.FromSeconds()
        public int refreshTime = 60;
        //Favorite queues
        public SerializableDictionary<int, string> favoriteQueues = new SerializableDictionary<int, string> { 
            { 1, "" }, { 2, "" }, { 3, "" }, { 4, "" }, { 5, "" }, 
        };
        //Favorite queues - new version
        public List<string> favoriteQueues2 = new List<string>();
        //Enable in-game UI
        public bool uIEnabled = true;
        //Show only favorite queues in GUI
        public bool guiFavoriteOnly = false;
        //Button/UI Toolbar width
        public float aBToolbarWidth = 60f;
        //GUI scale
        public float aBToolbarScale = 0.5f;
        //UI scale
        public float uIScale = 1f;

        //GUI pos X
        public float gUIPosX = 0f;
        //GUI pos Y
        public float gUIPosY = 0f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
