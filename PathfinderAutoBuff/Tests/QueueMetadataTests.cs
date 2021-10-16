using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using PathfinderAutoBuff.QueueOperations;
using PathfinderAutoBuff.Tests;


namespace PathfinderAutoBuff.Tests
{
    static class QueueMetadataTests
    /*
     * Queue metadata tests
     * Spellbook priority tests
     * 
     * Spellcasting priority tests
     * 
     * Required characters and abilities:
     * MC with Unbreakable Heart in both Mythic and Class spellbooks
     * dd38f33c56ad00a4da386c1afaa49967
     * TestBard, Spontaneous caster with Remove Fear
     * 55a037e514c0ee14a8e3ed14b47061de
     * Extend
     * Quicken
     * TestCleric, prepared caster with Remove Fear
     * 55a037e514c0ee14a8e3ed14b47061de
     * Extend
     * Quicken
     * 
     */
    {
        //Spellbook priority tests
        public static bool SpellbookPriority()
        {
            bool result = true;
            bool removeQueue = false;
            List<CommandQueueItem> testCommands = new List<CommandQueueItem>();
            QueueMetadata backupMetadata = Main.QueuesController?.queueController?.m_CurrentMetadata;
            if (Main.QueuesController.queueController == null)
            {
                Main.QueuesController.queueController = new QueueController(new CommandQueue(testCommands));
                Main.QueuesController.queueController.CurrentQueue().CommandList = testCommands;
                removeQueue = true;
            }
#if (WOTR)
            //MythicSpellbookPriority
            result = result & MythicSpellbookPriorityTest(testCommands);
            //ClassSpellbookPriority
            result = result & ClassSpellbookPriority(testCommands);
#endif
            //ReverseCasterLevel
            result = result & ReverseCasterLevel(testCommands);
            //ContinueCasting
            if (!removeQueue)
                Main.QueuesController.queueController.m_CurrentMetadata = backupMetadata;
            else
                Main.QueuesController.queueController = null;
            return result;
        }

        private static bool ReverseCasterLevel(List<CommandQueueItem> testCommands)
        {
            bool result = true;
            testCommands.AddRange(new List<CommandQueueItem>(){
                new CommandQueueItem("Airmed", "dd38f33c56ad00a4da386c1afaa49967",true),
            });
            QueueMetadata queueMetadata = new QueueMetadata();
            Main.QueuesController.queueController.m_CurrentMetadata = queueMetadata;
            queueMetadata.MetadataInverseCasterLevelPriority = true;
#if (WOTR)
            queueMetadata.MetadataMythicSpellbookPriority = false;
#endif
            QueueParser testParser = new QueueParser(testCommands, false, true);
            string testResult = "ERROR";
            foreach (UnitEntityData caster in testParser.commandProviders.Keys)
            {
                TestHelpers.DetailedLog("SpellbookPriority-ReverseCasterLevel: Caster: " + caster.CharacterName);
                foreach (CommandProvider command in testParser.commandProviders[caster])
                {
                    UnitUseAbility unitCommand = (UnitUseAbility)command.getCommand();
                    if (unitCommand != null)
                    {
#if (WOTR)
                        AbilityData abilityData = unitCommand.Ability;
#elif (KINGMAKER)
                        AbilityData abilityData = unitCommand.Spell;
#endif
                        TestHelpers.DetailedLog($"SpellbookPriority-ReverseCasterLevel: {abilityData.Spellbook.Blueprint.Name}");
                        if (abilityData.Spellbook.Blueprint.Name == "Azata")
                            testResult = "SUCCESS";
                        else
                            result = false;
                    }
                    else
                    {
                        TestHelpers.DetailedLog("SpellbookPriority-ReverseCasterLevel: Error getting command");
                    }
                }
            }
            testCommands.Clear();
            TestHelpers.TestLog("SpellbookPriority-ReverseCasterLevel - ", $"{testResult}");
            return result;
        }

        private static bool MythicSpellbookPriorityTest(List<CommandQueueItem> testCommands)
        {
            bool result = true;
            testCommands.AddRange(new List<CommandQueueItem>(){
                new CommandQueueItem("Airmed", "dd38f33c56ad00a4da386c1afaa49967",true),
            });
            QueueMetadata queueMetadata = new QueueMetadata();
            Main.QueuesController.queueController.m_CurrentMetadata = queueMetadata;
            queueMetadata.MetadataInverseCasterLevelPriority = false;
#if (WOTR)
            queueMetadata.MetadataMythicSpellbookPriority = true;
#endif
            QueueParser testParser = new QueueParser(testCommands, false, true);
            string testResult = "ERROR";
            foreach (UnitEntityData caster in testParser.commandProviders.Keys)
            {
                TestHelpers.DetailedLog("SpellbookPriority-MythicSpellbookPriority: Caster: " + caster.CharacterName);
                foreach (CommandProvider command in testParser.commandProviders[caster])
                {
                    UnitUseAbility unitCommand = (UnitUseAbility)command.getCommand();
                    if (unitCommand != null)
                    {
#if (WOTR)
                        AbilityData abilityData = unitCommand.Ability;
#elif (KINGMAKER)
                        AbilityData abilityData = unitCommand.Spell;
#endif
                        TestHelpers.DetailedLog($"SpellbookPriority-MythicSpellbookPriority: {abilityData.Spellbook.Blueprint.Name}");
                        if (abilityData.Spellbook.Blueprint.Name == "Azata")
                            testResult = "SUCCESS";
                        else
                            result = false;
                    }
                    else
                    {
                        TestHelpers.DetailedLog("SpellbookPriority-MythicSpellbookPriority: Error getting command");
                    }
                }
            }
            testCommands.Clear();
            TestHelpers.TestLog("SpellbookPriority-MythicSpellbookPriority - ", $"{testResult}");
            return result;
        }

        private static bool ClassSpellbookPriority(List<CommandQueueItem> testCommands)
        {
            bool result = true;
            testCommands.AddRange(new List<CommandQueueItem>(){
                new CommandQueueItem("Airmed", "dd38f33c56ad00a4da386c1afaa49967",true),
            });
            QueueMetadata queueMetadata = new QueueMetadata();
            Main.QueuesController.queueController.m_CurrentMetadata = queueMetadata;
            queueMetadata.MetadataInverseCasterLevelPriority = false;
#if (WOTR)
            queueMetadata.MetadataMythicSpellbookPriority = false;
#endif
            QueueParser testParser = new QueueParser(testCommands, false, true);
            string testResult = "ERROR";
            foreach (UnitEntityData caster in testParser.commandProviders.Keys)
            {
                TestHelpers.DetailedLog("SpellbookPriority-ClassSpellbookPriority: Caster: " + caster.CharacterName);
                foreach (CommandProvider command in testParser.commandProviders[caster])
                {
                    UnitUseAbility unitCommand = (UnitUseAbility)command.getCommand();
                    if (unitCommand != null)
                    {
#if (WOTR)
                        AbilityData abilityData = unitCommand.Ability;
#elif (KINGMAKER)
                        AbilityData abilityData = unitCommand.Spell;
#endif
                        TestHelpers.DetailedLog($"SpellbookPriority-ClassSpellbookPriority: {abilityData.Spellbook.Blueprint.Name}");
                        if (abilityData.Spellbook.Blueprint.Name == "Skald")
                            testResult = "SUCCESS";
                        else
                            result = false;
                    }
                    else
                    {
                        TestHelpers.DetailedLog("SpellbookPriority-ClassSpellbookPriority: Error getting command");
                    }
                }
            }
            testCommands.Clear();
            TestHelpers.TestLog("SpellbookPriority-ClassSpellbookPriority - ", $"{testResult}");
            return result;
        }

        //Spell slots priority tests
        public static bool SpellCastingPriority()
        {
            bool result = true;
            bool removeQueue = false;
            List<CommandQueueItem> testCommands = new List<CommandQueueItem>();
            QueueMetadata backupMetadata = Main.QueuesController?.queueController?.m_CurrentMetadata;
            if (Main.QueuesController.queueController == null)
            {
                Main.QueuesController.queueController = new QueueController(new CommandQueue(testCommands));
                Main.QueuesController.queueController.CurrentQueue().CommandList = testCommands;
                removeQueue = true;
            }
            //Lowest spell slot
            result = result & SpellslotTests(testCommands, true);
            //Highest spell slot
            result = result & SpellslotTests(testCommands, false);
            //MetamagicTest
            result = result & MetaMagicTests(testCommands);
            if (!removeQueue)
                Main.QueuesController.queueController.m_CurrentMetadata = backupMetadata;
            else
                Main.QueuesController.queueController = null;
            return result;
        }

        private static bool MetaMagicTests(List<CommandQueueItem> testCommands)
        {
            bool result = true;
            testCommands.AddRange(new List<CommandQueueItem>(){
                new CommandQueueItem("TestBard", "55a037e514c0ee14a8e3ed14b47061de",true),
                new CommandQueueItem("TestCleric", "55a037e514c0ee14a8e3ed14b47061de",true),
            });
            QueueMetadata queueMetadata = new QueueMetadata();
            Main.QueuesController.queueController.m_CurrentMetadata = queueMetadata;
            queueMetadata.MetadataInverseCasterLevelPriority = false;
#if (WOTR)
            queueMetadata.MetadataMythicSpellbookPriority = false;
#endif
            queueMetadata.MetadataIgnoreMetamagic = false;
            queueMetadata.MetamagicPriority = new List<Kingmaker.UnitLogic.Abilities.Metamagic>() {
                                            Kingmaker.UnitLogic.Abilities.Metamagic.Extend,
                                            Kingmaker.UnitLogic.Abilities.Metamagic.Heighten,
                                            0
                                            };
            QueueParser testParser = new QueueParser(testCommands, false, true);
            foreach (UnitEntityData caster in testParser.commandProviders.Keys)
            {
                TestHelpers.DetailedLog("SpellCastingPriority-SpellslotTests: Caster: " + caster.CharacterName);
                foreach (CommandProvider command in testParser.commandProviders[caster])
                {
                    UnitUseAbility unitCommand = (UnitUseAbility)command.getCommand();
                    if (unitCommand != null)
                    {
#if (WOTR)
                        AbilityData abilityData = unitCommand.Ability;
#elif (KINGMAKER)
                        AbilityData abilityData = unitCommand.Spell;
#endif
                        TestHelpers.DetailedLog($"SpellCastingPriority-MetaMagicTests: " +
                                                $"Spell - {abilityData.Blueprint.Name}, " + 
                                                $"Extend - {abilityData.HasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Extend)}, " +
                                                $" Heighten - {abilityData.HasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Heighten)}");
                        if (caster.CharacterName == "TestBard")
                            if (abilityData.HasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Extend) &&
                                abilityData.HasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Heighten))
                                result = result && true;
                            else
                                result = false;
                        if (caster.CharacterName == "TestCleric")
                            if (abilityData.HasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Extend))
                                result = result && true;
                            else
                                result = false;

                    }
                    else
                    {
                        TestHelpers.DetailedLog("SpellCastingPriority-MetaMagicTests: Error getting command");
                    }
                }
            }
            testCommands.Clear();
            string teststring;
            teststring = result ? "SUCCESS" : "ERROR";
            TestHelpers.TestLog("SpellCastingPriority-MetaMagicTests - ", $"{teststring}");
            return result;
        }
        private static bool SpellslotTests(List<CommandQueueItem> testCommands, bool lowest)
        {
            bool result = true;
            testCommands.AddRange(new List<CommandQueueItem>(){
                new CommandQueueItem("TestBard", "55a037e514c0ee14a8e3ed14b47061de",true),
                new CommandQueueItem("TestCleric", "55a037e514c0ee14a8e3ed14b47061de",true),
            });
            QueueMetadata queueMetadata = new QueueMetadata();
            Main.QueuesController.queueController.m_CurrentMetadata = queueMetadata;
            queueMetadata.MetadataInverseCasterLevelPriority = false;
#if (WOTR)
            queueMetadata.MetadataMythicSpellbookPriority = false;
#endif
            queueMetadata.MetadataIgnoreMetamagic = true;
            queueMetadata.MetadataLowestSlotFirst = lowest;
            QueueParser testParser = new QueueParser(testCommands, false, true);
            foreach (UnitEntityData caster in testParser.commandProviders.Keys)
            {
                TestHelpers.DetailedLog("SpellCastingPriority-SpellslotTests: Caster: " + caster.CharacterName);
                foreach (CommandProvider command in testParser.commandProviders[caster])
                {
                    UnitUseAbility unitCommand = (UnitUseAbility)command.getCommand();
                    if (unitCommand != null)
                    {
#if (WOTR)
                        AbilityData abilityData = unitCommand.Ability;
#elif (KINGMAKER)
                        AbilityData abilityData = unitCommand.Spell;
#endif
                        TestHelpers.DetailedLog($"SpellCastingPriority-SpellslotTests: Spelllevel - {abilityData.SpellLevel}");
                        if (lowest)
                        {
                            if (abilityData.SpellLevel == 1)
                            {
                                result = result & true;
                            }
                            else
                                result = false;
                        }
                        else
                        {
                            if (abilityData.SpellLevel > 1)
                            {
                                result = result & true;
                            }
                            else
                                result = false;
                        }
                    }
                    else
                    {
                        TestHelpers.DetailedLog("SpellCastingPriority-SpellslotTests: Error getting command");
                    }
                }
            }
            testCommands.Clear();
            string teststring;
            teststring = result ? "SUCCESS" : "ERROR";
            TestHelpers.TestLog("SpellCastingPriority-SpellslotTests - ", $"{teststring}");
            return result;
        }

        public static bool MetadataLoading()
        {
            bool result = true;
            //Testing how metadata is loaded from file
            return result;
        }

    }
}
