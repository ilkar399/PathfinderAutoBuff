﻿# Pathfinder Auto Buff
This is a mod project for Pathfinder: Kingmaker and Pathfinder Wrath of the Righteous.

It's an alternative to (insert BuffBot link). It provides a way to create (or record)
 a queue of actions (either the buff spells or ability usage) and execute it later.

## Install
1. Download and install [Unity Mod Manager](https://www.nexusmods.com/site/mods/21)
2. Download the [mod](https://www.nexusmods.com/pathfinderkingmaker/mods/195)
3. Extract the archive and put the mod folder into 'Mods' folder of the Game
4. Open the interface (Ctrl+f10)


## Features

## Project compilation notes
Github page:

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
Spacehamster, Hambeard, Holic92, Hsinyu Chan and Narria/Cabarius