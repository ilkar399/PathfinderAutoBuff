#if (DEBUG)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using static PathfinderAutoBuff.Main;
using PathfinderAutoBuff.UnitLogic;
using PathfinderAutoBuff.Utility;
using static PathfinderAutoBuff.Utility.SettingsWrapper;

namespace PathfinderAutoBuff.Tests
{
    static class UtilitiesTests
    {
        public static void FlattenActions()
        {
            string abilityID;
            BlueprintAbility blueprintAbility;
            //Acid Maw
            abilityID = "75de4ded3e731dc4f84d978fe947dc67";
            blueprintAbility = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(abilityID);
            blueprintAbility = blueprintAbility?.GetComponent<AbilityEffectStickyTouch>()?.TouchDeliveryAbility != null ? blueprintAbility.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility : blueprintAbility;
            TestHelpers.TestLog("FlattenActionsTest", blueprintAbility.Name);
            TestHelpers.TestLog("FlattenActionsTest", string.Join(",", LogicHelpers.FlattenAllActions(blueprintAbility, true).Select(a => a.NameSafe())));
            TestHelpers.TestLog("FlattenActionsTest", LogicHelpers.FlattenAllActions(blueprintAbility, true).Where(action => (action as ContextActionApplyBuff) != null).FirstOrDefault().NameSafe());
            //Bane
            abilityID = "8bc64d869456b004b9db255cdd1ea734";
            blueprintAbility = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(abilityID);
            blueprintAbility = blueprintAbility?.GetComponent<AbilityEffectStickyTouch>()?.TouchDeliveryAbility != null ? blueprintAbility.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility : blueprintAbility;
            TestHelpers.TestLog("FlattenActionsTest", blueprintAbility.Name);
            TestHelpers.TestLog("FlattenActionsTest", string.Join(",", LogicHelpers.FlattenAllActions(blueprintAbility, true).Select(a => a.NameSafe())));
            TestHelpers.TestLog("FlattenActionsTest", LogicHelpers.FlattenAllActions(blueprintAbility, true).Where(action => (action as ContextActionApplyBuff) != null).FirstOrDefault().NameSafe());
            //Bless
            abilityID = "90e59f4a4ada87243b7b3535a06d0638";
            blueprintAbility = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(abilityID);
            blueprintAbility = blueprintAbility?.GetComponent<AbilityEffectStickyTouch>()?.TouchDeliveryAbility != null ? blueprintAbility.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility : blueprintAbility;
            TestHelpers.TestLog("FlattenActionsTest", blueprintAbility.Name);
            TestHelpers.TestLog("FlattenActionsTest", string.Join(",", LogicHelpers.FlattenAllActions(blueprintAbility, true).Select(a => a.NameSafe())));
            TestHelpers.TestLog("FlattenActionsTest", LogicHelpers.FlattenAllActions(blueprintAbility, true).Where(action => (action as ContextActionApplyBuff) != null).FirstOrDefault().NameSafe());
            //Bless Weapon
            abilityID = "831e942864e924846a30d2e0678e438b";
            blueprintAbility = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(abilityID);
            blueprintAbility = blueprintAbility?.GetComponent<AbilityEffectStickyTouch>()?.TouchDeliveryAbility != null ? blueprintAbility.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility : blueprintAbility;
            TestHelpers.TestLog("FlattenActionsTest", blueprintAbility.Name);
            TestHelpers.TestLog("FlattenActionsTest", string.Join(",", LogicHelpers.FlattenAllActions(blueprintAbility, true).Select(a => a.NameSafe())));
            TestHelpers.TestLog("FlattenActionsTest", LogicHelpers.FlattenAllActions(blueprintAbility, true).Where(action => (action as ContextActionApplyBuff) != null).FirstOrDefault().NameSafe());
            //Death Ward
            abilityID = "0413915f355a38146bc6ad40cdf27b3f";
            blueprintAbility = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(abilityID);
            blueprintAbility = blueprintAbility?.GetComponent<AbilityEffectStickyTouch>()?.TouchDeliveryAbility != null ? blueprintAbility.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility : blueprintAbility;
            TestHelpers.TestLog("FlattenActionsTest", blueprintAbility.Name);
            TestHelpers.TestLog("FlattenActionsTest", string.Join(",", LogicHelpers.FlattenAllActions(blueprintAbility, true).Select(a => a.NameSafe())));
            TestHelpers.TestLog("FlattenActionsTest", LogicHelpers.FlattenAllActions(blueprintAbility, true).Where(action => (action as ContextActionApplyBuff) != null).FirstOrDefault().NameSafe());
        }
    }
}
#endif