using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker;
using System.Reflection;
#if (KINGMAKER)
using static KingmakerAutoBuff.Extensions.WoTRExtensions;
#endif

namespace PathfinderAutoBuff.QueueOperattions
{
    /*
     * Target detection for scripts
     * - UnitEntityData GetTarget(int) - Get target from party position
     * - UnitEntityData GetTarget(string) Target CharacterName
     * - UnitEntityData GetTargetPet(string targetParetyCharBame, opt string targetPetName)
     */
    static class Targets
    {
        //Unit party order as it's used in game 
        public static List<UnitEntityData> GetPartyOrder()
        {
            List<UnitEntityData> Result = new List<UnitEntityData>();
#if (WOTR)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets
#elif (KINGMAKER)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets()
#endif
                                             where u.IsDirectlyControllable
                                             select u).ToList<UnitEntityData>())
            {
                Result.Add(unit);
            }
            return Result;
        }

        //Unit party order as it's used in game 
        public static List<string> GetPartyNamesOrder()
        {
            List<string> Result = new List<string>();
#if (WOTR)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets
#elif (KINGMAKER)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets()
#endif
                                             where u.IsDirectlyControllable
                                             select u).ToList<UnitEntityData>())
            {
#if (WOTR)
                if (unit.IsPet)
                {
                    if (unit.Master != null)
                        Result.Add(unit.Master.CharacterName + "'s " + unit.CharacterName);
#elif (KINGMAKER)
                if (unit.IsPet())
                {
                    if (unit.Master() != null)

                        Result.Add(unit.Master().CharacterName + "'s " + unit.CharacterName);
#endif
                    else
                        Result.Add(unit.CharacterName);
                }
                else
                    Result.Add(unit.CharacterName);
            }
            return Result;
        }

        //Get a dictionary for target selector in UI
        public static void GetTargetSelectionDict(ref Dictionary<int, bool> targetSelection,CommandQueueItem commandQueueItem = null)
        {
            Dictionary<int, bool> result = new Dictionary<int, bool>();
            int index = 0;
            //Target checks
            List<UnitEntityData> targets = new List<UnitEntityData>();
            if (commandQueueItem == null)
            {
#if (WOTR)
                foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets
#elif (KINGMAKER)
                foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets()
#endif
                                                 where u.IsDirectlyControllable
                                                 select u).ToList<UnitEntityData>())
                {
                    targetSelection[index] = false;
                    index += 1;
                }
                return;
            }
            else
            {
                //Target
                switch (commandQueueItem.TargetType)
                {
                    case CommandQueueItem.TargetTypes.Self:
#if (WOTR)
                        foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets
#elif (KINGMAKER)
                        foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets()
#endif
                                                         where u.IsDirectlyControllable
                                                         select u).ToList<UnitEntityData>())
                        {
                            targetSelection[index] = false;
                            index += 1;
                        }
                        return;
                    case CommandQueueItem.TargetTypes.Positions:
                        foreach (int position in commandQueueItem.Positions)
                        {
                            UnitEntityData target = Targets.GetTarget(position);
                            if (target != null)
                                targets.Add(target);
                        }
                        break;
                    case CommandQueueItem.TargetTypes.CharacterNames:
                        if (commandQueueItem.CharacterNames != null)
                        {
                            foreach (string characterName in commandQueueItem.CharacterNames)
                            {
                                List<UnitEntityData> targets1 = Targets.GetTarget(characterName);
                                if (targets1.Count > 0)
                                    targets.AddRange(targets1);
                            }
                        }
                        if (commandQueueItem.PetIndex != null)
                        {
                            foreach (string characterName in commandQueueItem.PetIndex.Keys)
                            {
                                foreach (int petIndex in commandQueueItem.PetIndex[characterName])
                                {
                                    UnitEntityData target = Targets.GetTargetPet(characterName, petIndex);
                                    if (target != null)
                                        targets.Add(target);
                                }
                            }
                        }
                        break;
                }
            }
#if (WOTR)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets
#elif (KINGMAKER)
            foreach (UnitEntityData unit in (from u in Game.Instance.Player.PartyAndPets()
#endif
                                             where u.IsDirectlyControllable
                                             select u).ToList<UnitEntityData>())
            {
                targetSelection[index] = targets.Contains(unit);
                index += 1;
            }
            return;
        }

        //Get Unit from party position
        public static UnitEntityData GetTarget(int targetPartyPosition)
        {
            List<UnitEntityData> partyOrder = GetPartyOrder();
            if ((partyOrder.Count() -1 )< targetPartyPosition)
                return null;
            return partyOrder[targetPartyPosition];
        }

        //Get units with CharacterName
        public static List<UnitEntityData> GetTarget(string targetPartyCharName)
        {
            List<UnitEntityData> Result = new List<UnitEntityData>();
            foreach (UnitEntityData unit in (from u in Game.Instance?.Player?.Party
                                             where u.IsDirectlyControllable
                                             select u))
            {
                if (unit.CharacterName == targetPartyCharName)
                    Result.Add(unit);
            }
            return Result;
        }

        //Get #PetIndex of targetPartyCharName
        public static UnitEntityData GetTargetPet(string targetPartyCharName, int PetIndex)
        {
            foreach (UnitEntityData unit in (from u in Kingmaker.Game.Instance?.Player?.Party where u.IsDirectlyControllable select u))
            {
                if (unit.CharacterName == targetPartyCharName)
                {
#if (WOTR)
                    if ((unit.Pets.Count - 1) < PetIndex)
#elif (KINGMAKER)
                    if ((unit.Pets().Count - 1) < PetIndex)
#endif
                        return null;
                    else
#if (WOTR)
                        return unit.Pets[PetIndex];
#elif (KINGMAKER)
                        return unit.Pets()[PetIndex];
#endif
                }
            }
            return null;
        }
    }
}
