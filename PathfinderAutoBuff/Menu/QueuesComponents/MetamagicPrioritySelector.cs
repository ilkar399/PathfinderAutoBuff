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
        private List<Metamagic> metamagicAll;
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
            if (metamagicAll == null)
            {
                metamagicAll = Enum.GetValues(typeof(Metamagic)).Cast<Metamagic>().ToList();
                metamagicAll.Add(0);
            }
            UI.Label("Metamagic priority:");
            UI.Label("Metamagic priority is used when selecting which spell slot to cast " +
                     "during queue execution. If the spell uses selected metamagic, it is " +
                     "used first (with the priority, i.e. with [0] Extended and [1] Quickened" +
                     "the spell that has Extended will be used first). 'None' prioritizes " +
                     "the slot that doesn't have any metamagic applied to.");
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int metamagicIndex = 0; metamagicIndex < metamagicAll.Count; metamagicIndex ++)
                {
                    bool metamagicButtonToggle = metamagicPriority.Contains(metamagicAll[metamagicIndex]);
                    string metamagicButtonLabel = "";
                    if (metamagicButtonToggle)
                        metamagicButtonLabel += string.Format("[{0}]", metamagicPriority.IndexOf(metamagicAll[metamagicIndex])).Color(RGBA.lime);
                    metamagicButtonLabel += metamagicAll[metamagicIndex] == 0 ? "None" : metamagicAll[metamagicIndex].ToString();
                    Utility.UI.ToggleButton(
                        ref metamagicButtonToggle,
                        metamagicButtonLabel,
                        () => {
                            metamagicPriority.Add(metamagicAll[metamagicIndex]);
                        }, () =>
                        {
                            metamagicPriority.Remove(metamagicAll[metamagicIndex]);
                        },
                        DefaultStyles.ButtonFixed120(), GUILayout.ExpandHeight(true)
                    );
                if ((metamagicIndex % 6) == 5)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            /*
            UI.BeginHorizontal();
            UI.Vertical(() =>
            {
                //Accept priority
                UI.Label("Prioritize metamagic:");
                for (int index = 0; index < metamagicPriority.Count; index++)
                {
                    string metamagicButtonLabel = metamagicPriority[index] == 0 ? "None" : metamagicPriority[index].ToString();
                    UI.Horizontal(() =>
                    {
                        GUILayout.Label(
                            metamagicButtonLabel,
                            DefaultStyles.LabelFixed120()
                            );
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
                UI.Label("Ignore metamagics:");
                for (int index = 0; index < metamagicExcluded.Count; index++)
                {
                    string metamagicButtonLabel = metamagicExcluded[index] == 0 ? "None" : metamagicExcluded[index].ToString();
                    UI.Horizontal(() =>
                    {
                        GUILayout.Label(
                            metamagicButtonLabel,
                            DefaultStyles.LabelFixed120());
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
            UI.Vertical(() =>
            {
                UI.Label("Some metamagic priority help text");
            });
            UI.EndHorizontal();
            */
        }

    }
}
