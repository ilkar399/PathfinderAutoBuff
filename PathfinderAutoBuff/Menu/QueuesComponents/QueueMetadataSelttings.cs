using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using PathfinderAutoBuff.QueueOperations;
using PathfinderAutoBuff.UnitLogic;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Main;
using static PathfinderAutoBuff.Utility.IOHelpers;
using static PathfinderAutoBuff.Utility.Extensions.RichTextExtensions;
using static PathfinderAutoBuff.Utility.Extensions.CommonExtensions;

namespace PathfinderAutoBuff.Menu.QueuesComponents
{
    //Queue metadata settings
    //Loaded either from file or default
    class QueueMetadataSettings
    {
        private readonly QueuesController queuesController;
        private readonly QueueController selectedQueue;
        private QueueMetadata queueMetadata;
        private MetamagicPrioritySelector metamagicPrioritySelector;
        private bool metamagicInit;

        //Constructor
        public QueueMetadataSettings(QueuesController queuesController)
        {
            this.queuesController = queuesController;
            this.selectedQueue = queuesController.queueController;
            this.queueMetadata = queuesController.queueController.CurrentMetadata();
        }

        //GUI
        public void OnGUI()
        {
            UI.Label("Queue execution settings:");
            UI.ActionButton(
                "Reset to default",
                () => {
                    queuesController.queueController.LoadMetadata("");
                    this.queueMetadata = queuesController.queueController.CurrentMetadata();
                    metamagicInit = false;
                },
                DefaultStyles.ButtonFixed120(),
                GUILayout.ExpandWidth(false));
#if (WOTR)
            this.queueMetadata.MetadataMythicSpellbookPriority = UI.ToggleButton(
                this.queueMetadata.MetadataMythicSpellbookPriority,
                "Use Mythic Spellbook first",
                DefaultStyles.LabelDefault(),
                UI.AutoWidth()
                );
#endif
            this.queueMetadata.MetadataInverseCasterLevelPriority = UI.ToggleButton(
                this.queueMetadata.MetadataInverseCasterLevelPriority,
                "Lower caster level spellbook first",
                DefaultStyles.LabelDefault(),
                UI.AutoWidth()
                );
            this.queueMetadata.MetadataIgnoreMetamagic = UI.ToggleButton(
                this.queueMetadata.MetadataIgnoreMetamagic,
                "Ignore metamagic priority settings",
                DefaultStyles.LabelDefault(),
                UI.AutoWidth()
                );
            this.queueMetadata.MetadataLowestSlotFirst = UI.ToggleButton(
                this.queueMetadata.MetadataLowestSlotFirst,
                "Lower level spellslot first",
                DefaultStyles.LabelDefault(),
                UI.AutoWidth()
                );
            //Metamagic priority
            if (!metamagicInit)
            {
                metamagicPrioritySelector = new MetamagicPrioritySelector(this.queueMetadata.MetamagicPriority);
                metamagicInit = true;
            }
            metamagicPrioritySelector.OnGUI();
        }
    }
}
