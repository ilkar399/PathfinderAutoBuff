using System;
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
using PathfinderAutoBuff.Tests;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace PathfinderAutoBuff.Tests
{
    static class QueueBase
    /*
     * Base queue functioning tests
     * Forming queue, executing (separately for spells and activatable abilities mods)
     * saving and loading
     * Required characters and abilities:
     * TestCleric
     *  Bless, Shield of Faith, Death Ward
     * TestHunter (with pet)
     * TestAlchemist
     *  Greater mutagen - Con
     * TestBard
     *  Haste, lesser extend wand
     * Any:
     *  Barskin (2+ usages)
     * TODO:
     * Kingmaker tests
     */
    {
        //Data used for queue tests
        private static readonly Dictionary<string, List<int>> petDict = new Dictionary<string, List<int>> { { "Seelah", new List<int>{0} } };
        private static readonly List<CommandQueueItem> testQueueItems1 = new List<CommandQueueItem>{
                //Bless             
                new CommandQueueItem("TestCleric", "90e59f4a4ada87243b7b3535a06d0638",true),
                //Shield of Faith
                new CommandQueueItem("TestCleric", "183d5bb91dea3a1489a6db6c9cb64445", true),
                new CommandQueueItem("TestCleric", "183d5bb91dea3a1489a6db6c9cb64445", false,new List<string>{ "TestCleric", "TestBard" }),
                new CommandQueueItem("TestCleric", "183d5bb91dea3a1489a6db6c9cb64445", false,null,null,petDict),
                //Death Ward
                new CommandQueueItem("TestCleric", "e9cc9378fd6841f48ad59384e79e9953",false,null,new List<int>{ 0,2}),
                //Domain spell (requires Cleric with Barskin)
                new CommandQueueItem("TestCleric", "5b77d7cc65b8ab74688e74a37fc2f553", false, new List<string>{"TestAlchemist"}, null,null),
                //Highest CL Barskin
                new CommandQueueItem("", "5b77d7cc65b8ab74688e74a37fc2f553", false, new List<string>{"TestCleric"}, null,null),
                new CommandQueueItem("", "5b77d7cc65b8ab74688e74a37fc2f553", false, new List<string>{ "TestBard"}, null, petDict),
            };
        private static readonly List<CommandQueueItem> testQueueItems2 = new List<CommandQueueItem> {
                //Ability mod test
                new CommandQueueItem("", "c60969e7f264e6d4b84a1499fdcf9039", false, new List<string>{ "TestCleric"}, null, null,null,new List<string>{ "605e64c0b4586a34494fc3471525a2e5"}),
                //Activatable mod test
                //NOTE: Abilities are actually activated after the parsing test
                new CommandQueueItem("TestBard", "486eaff58293f6441a5c2759c4872f98", false, new List<string>{ "TestBard"}, null, null,new List<string>{ "605e64c0b4586a34494fc3471525a2e5" }),
                //Ability test
                new CommandQueueItem("TestAlchemist", "c1e46599fcade78418ef1ada71c1f487", true,null, null, null,null,null,CommandQueueItem.ActionTypes.Ability)
            };

        //Testing queue parsing
        public static void QueueParsing()
        {
            QueueParser testParser = new QueueParser(testQueueItems1, false, true);
            int commandCount = 0;
            foreach (UnitEntityData caster in testParser.commandProviders.Keys)
            {
                TestHelpers.DetailedLog("Caster: " + caster.CharacterName);
                foreach (CommandProvider command in testParser.commandProviders[caster])
                {
                    string teststring1 = "null";
                    var unitCommand = command.getCommand();
                    if (unitCommand != null)
                    {
                        teststring1 = unitCommand.TargetUnit != null ? unitCommand.TargetUnit.CharacterName : "no target";
                        commandCount += 1;
                    }
                    TestHelpers.DetailedLog("Target " + teststring1 +
                                 "; Description " + command.getDescription());
                }
            }
            TestHelpers.TestLog("QueueParsing", $"Created {commandCount} commands");
        }
        
        //Testing  execution without precast/activations
        public static void QueueExecution()
        {
            ScriptController.CreateFromQueue(testQueueItems1, "TESTQUEUE");
            ScriptController.Run();
        }

        //Testing execution with precast/activations
        public static void PreCastExecution()
        {
            ScriptController.CreateFromQueue(testQueueItems2, "TESTQUEUE");
            ScriptController.Run();
        }

        //Testing queue buff application (only detailed log)
        public static void QueueApplication()
        {
            int i = 0;
            foreach (CommandQueueItem commandQueueItem in testQueueItems1)
            {
                PartySpellList testPartySpellList = new PartySpellList();
                TestHelpers.DetailedLog($"#{i} - CasterName: {commandQueueItem.CasterName}" +
                    $" - AbilityID: {commandQueueItem.AbilityId}" +
                    $" - TargetType: {Enum.GetName(typeof(CommandQueueItem.TargetTypes), commandQueueItem.TargetType)}");
                TestHelpers.DetailedLog(String.Join("; ", commandQueueItem.GetStatus(testPartySpellList)));
                i++;
            }
        }

        public static void QueueSave()
        {
            CommandQueue testQueue = new CommandQueue();
            testQueue.CommandList = testQueueItems1;
            bool result1 = testQueue.SaveToFile("test");
            TestHelpers.TestLog("QueueSave", $"Test successful: {result1}");
        }

        public static void QueueLoad()
        {
            CommandQueue testQueue = new CommandQueue();
            testQueue.CommandList = testQueueItems1;
            bool result1 = testQueue.LoadFromFile("test.json");
            TestHelpers.TestLog("QueueLoad",$"Test successful: {result1}");
            TestHelpers.TestLog("QueueLoad", $"Resulting queue Length: {testQueue.CommandList.Count}");
        }
    }
}
