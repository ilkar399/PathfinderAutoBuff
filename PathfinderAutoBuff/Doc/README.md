# Pathfinder Auto Buff
This is a mod project for Pathfinder: Kingmaker and Pathfinder Wrath of the Righteous.

It's an alternative to [BuffBot](https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/4). It provides a way to create (or record)
 a queue of actions (either the buff spells or ability usage) and execute it later.

## Installation
1. Download and install [Unity Mod Manager](https://www.nexusmods.com/site/mods/21)
2. Download the [mod](https://www.nexusmods.com/pathfinderkingmaker/mods/195)
3. Extract the archive and put the mod folder into 'Mods' folder of the Game
4. Open the interface (Ctrl+f10)


## Features
* You can create the "queue" of actions to execute. Each element in the queue describes
spell/ability, caster and the list of targets.
* You can also select to pre-activate an activatable ability/item (like Extend Metamagic
Rod) before casting spells
* You can record your own actions into the action queue
* There is small UI panel that allows a quick execution of one of your favorite action
queues (*Note: it's not very pretty and wasn't tested on different screen resolutions*)

There are issues with certain buffs that I'm not sure how to process yet
(like Acid Maw/Bless weapon that have some non-trivial targeting conditions)

*Notes: This mod might not work in combat because of the way action queueing is done.
Also, it might conflict with mods patching UnitCommand.OnEnded* 

## Usage notes
* I'd advice to first record some simple action queue to see how the mod handles them
and better understand the queue creation principles.
* It might not be very apparent, but you have to select caster, ability and targets
before you'll be able to finish creating new action. Don't forget to apply target
selection!
* Spell/Abilities are not refreshed automatically for the mod UI, use "Refresh data"
 button.
* Metamagic/multiple spellbooks spell selection - The highest level spell from the highest 
Caster Level spellbook is selected.
* The in-game UI should show itself if there's any queue selected as Favorite in the mod settings.

## Project compilation notes
[Github page (private repository atm)](https://github.com/ilkar399/PathfinderAutoBuff)

There're 6 build configurations:
* **Debug** - WoTR debug configuration
* **WoTRDebugInstall** - WoTR debug with installing the mod into the game
* **Release** - WoTR release configuration

* **KingmakerDebug** - Kingmaker debug configuration
* **KingmakerDebugInstall** - Kingmaker debug with installing the mod into the game
* **KingmakerRelease** - Kingmaker release configuration

Game lib references are handled through "TargetGame" property and conditional targeting.
Lib references use relative paths while game install paths are absolute.
Installed game paths are defined in "WoTRInstallPath" and "KingmakerInstallPath" properties.

#### Folder structure:
```
Repos
│
├── KingmakerLib (Kingmaker reference dlls)
│   └── *.dll
│
├── WoTRLib (Wrath reference dlls)
│   ├── UnityModManager
│   │   └── 0Harmony.dll.dll
│   └── *.dll
│
└── PathfinderAutoBuff
    ├── PathfinderAutoBuff
    │   └── PathfinderAutoBuff.csproj
    └── PathfinderAutoBuff.sln
```


## Credits to 
This mod is somewhat based on KingmakerAI by Holiic92 and contains some code used with his permission.
Credits to Spacehamster, Hambeard, Hsinyu Chan, Balkoth, Vek17 and Narria for the ideas, code snippets and overall help with making this mod.