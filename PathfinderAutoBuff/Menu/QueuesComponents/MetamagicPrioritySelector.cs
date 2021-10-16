using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif


namespace PathfinderAutoBuff.Menu.QueuesComponents
{
    //Metamagic priority pick selector
    public class MetamagicPrioritySelector
    {
        private List<Metamagic> metamagicPriority;
        private List<Metamagic> metamagicExcluded;
        //Constructor

        public MetamagicPrioritySelector(List<Metamagic> metamagicPriority)
        {
            this.metamagicPriority = metamagicPriority;
            this.metamagicExcluded = Enum.GetValues(typeof(Metamagic)).Cast<Metamagic>().Except(metamagicPriority).ToList();
            if (!metamagicPriority.Contains(0))
                metamagicExcluded.Add(0);
        }

        public void OnGUI()
        {
            UI.Vertical(() =>
            {
                //Accept priority
                UI.Label("Metamagics priority");
                for (int index = 0; index < metamagicPriority.Count; index++)
                {
                    UI.Horizontal(() =>
                    {
                        GUILayout.Label(metamagicPriority[index].ToString(),DefaultStyles.LabelFixed120());
                        UI.ActionButton(Local["Menu_Queues_Up"], () => {
                            metamagicPriority.MoveUpList(metamagicPriority[index]);
                            return;
                            }, DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                        UI.ActionButton(Local["Menu_Queues_Down"], () => {
                            if (metamagicPriority[index] == metamagicPriority.Last())
                            {
                                metamagicExcluded.Insert(0, metamagicPriority[index]);
                                metamagicPriority.Remove(metamagicPriority[index]);
                                return;
                            }
                            else
                            {
                                metamagicPriority.MoveDownList(metamagicPriority[index]);
                                return;
                            }
                        }, DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                    });
                }
                //Ignore metamagics
                UI.Label("Ignore metamagics");
                for (int index = 0; index < metamagicExcluded.Count; index++)
                {
                    UI.Horizontal(() =>
                    {
                        GUILayout.Label(metamagicExcluded[index].ToString(), DefaultStyles.LabelFixed120());
                        UI.ActionButton(Local["Menu_Queues_Up"], () => {
                            if (metamagicExcluded.IndexOf(metamagicExcluded[index]) == 0)
                            {
                                metamagicPriority.Add(metamagicExcluded[index]);
                                metamagicExcluded.Remove(metamagicExcluded[index]);
                                return;
                            }
                            else
                            {
                                metamagicExcluded.MoveUpList(metamagicExcluded[index]);
                                return;
                            }
                        }, DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                        UI.ActionButton(Local["Menu_Queues_Down"], () => {
                            metamagicExcluded.MoveDownList(metamagicExcluded[index]);
                            return;
                        }, DefaultStyles.ButtonFixed120(), GUILayout.ExpandWidth(false));
                    });
                }
            });
        }

    }
}
