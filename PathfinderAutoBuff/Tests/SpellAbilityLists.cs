﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static PathfinderAutoBuff.Main;
using PathfinderAutoBuff.Scripting;
using PathfinderAutoBuff.UnitLogic;
using static PathfinderAutoBuff.Utility.SettingsWrapper;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace PathfinderAutoBuff.Tests
{
    static class SpellAbilityLists
    {
        //SpellLists forming test
        public static void SpellLists()
        {
            PartySpellList testPartySpellList = new PartySpellList();
            for (int i = 1; i < 11; i++)
            {
                var output = testPartySpellList.PartySpellByLevel(i);
                if (output != null)
                {
                    foreach (BlueprintAbility blueprintAbility in output.Keys)
                    {
                        PartySpellData longestPartySpellData = testPartySpellList.LongestDurationSpellCaster(blueprintAbility, output[blueprintAbility]);
                        if (longestPartySpellData == null)
                        {
                            TestHelpers.DetailedLog($"No caster capable of casting {blueprintAbility.name}");
                            continue;
                        }
                        string durationString = PartySpellList.GetSpellDuration(longestPartySpellData).Seconds.ToString("c");
                        TestHelpers.DetailedLog("Name: " + blueprintAbility.Name + "; Duration: " + durationString +
                            " CastSelf " + longestPartySpellData.CanTargetSelf + "; CastAllies " + longestPartySpellData.CanTargetFriends);
                        foreach (PartySpellData partySpellData in output[blueprintAbility])
                        {
                            TestHelpers.DetailedLog(partySpellData.Caster.CharacterName + "; " + testPartySpellList.GetAvailableCasts(partySpellData) + "; " +
                                PartySpellList.GetSpellDuration(partySpellData).Seconds.ToString("c"));
                        }
                    }
                }
            }
            TestHelpers.TestLog("SpellLists",testPartySpellList.m_AllSpells.Count.ToString());
        }

        //TODO List of Abilities forming tests
        public static void AbilityLists()
        {
            TestHelpers.TestLog("AbilityLists", "Test not implemented");
        }

        //TODO List of Activatables forming tests
        public static void ActivatableLists()
        {
            TestHelpers.TestLog("AbilityLists", "Test not implemented");
        }
    }
}