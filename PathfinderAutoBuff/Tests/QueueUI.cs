using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PathfinderAutoBuff.UnitLogic;
using PathfinderAutoBuff.Menu.QueuesComponents;

namespace PathfinderAutoBuff.Tests
{
    /*
     * Testing Queue UI components
     */
    static class QueueUI
    {
        /*
         * Testing if filter functions work
         */
        public static bool AbilityFilterComponentTest(
            string casterSpell = "",
            string casterAbility = "",
            string casterActivatable = "")
        {
            PartySpellList partySpellList = new PartySpellList();
            PartyAbilityList partyAbility = new PartyAbilityList();
            PartyActivatableList partyActivatableList = new PartyActivatableList();
            List<AbilityFilter> filterTestList = new List<AbilityFilter>(){
                //Party Spell list test data
                new AbilityFilter("SpellAllTest1",
                "",
                1,
                (caster, level) =>
                PartySpellList.SpellDataUIFilterLevels(partySpellList, caster,level)
                ),
                new AbilityFilter("SpellCasterTest1",
                casterSpell,
                1,
                (caster, level) =>
                PartySpellList.SpellDataUIFilterLevels(partySpellList, caster,level)
                ),
                //Party Ability test data
                new AbilityFilter("AbilityAllTest",
                "",
                1,
                (caster, level) =>
                PartyAbilityList.AbilityDataUIFilter(partyAbility, caster)
                ),
                new AbilityFilter("AbilityCasterTest",
                casterAbility,
                1,
                (caster, level) =>
                PartyAbilityList.AbilityDataUIFilter(partyAbility, caster)
                ),
                //Party Activatables test data
                new AbilityFilter("ActivatableAllTest",
                "",
                1,
                (caster, level) =>
                PartyActivatableList.ActivatableDataUIFilter(partyActivatableList, caster)
                ),
                new AbilityFilter("ActivatableCasterTest",
                casterActivatable,
                1,
                (caster, level) =>
                PartyActivatableList.ActivatableDataUIFilter(partyActivatableList, caster)
                ),
                };
            bool resultFlag = true;
            foreach (AbilityFilter afc in filterTestList)
            {
                afc.Update();
                TestHelpers.TestLog("AbilityFilterComponentTest", $"Filter {afc.Name} ({afc.FilteredData.Count})");
                resultFlag = resultFlag && afc.FilteredData.Count > 0;
            }
            return resultFlag;
        }

    }
}
