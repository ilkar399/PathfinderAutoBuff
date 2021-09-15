# WOTR Auto Buff
Mouse Wheel fix for UMM:
https://discord.com/channels/645948717400064030/791053285657542666/830334240930660373

## TODO
Converted spells and abilities
* Code cleanup
* GUI position adjustments in settings
* Enhance logging
* Move scripting patches to HarmonyPatches
* Change Kingmaker-Wotr extensions to conditional wrappers or compatibility class?
* Remake the game UI (check hamberd's mod) (scaling, positioning in options (up/down, left/right buttons + save position))
* Change FindApplyBuffActionAll to be more robust
* Readme
* Moving localization to Language files

## Updated Queue UI Structure:

---

|Control block +|Caster +    |Target  |SpellName  |Description | 
| ---           |:---:       | ---    | :---:     | ---        | 
|Up             |Available to|        | Status    | Duration   |
|Down           |            |        |           |            |
|Edit           |            |        |           |            |
|Delete         |            |        |           |            |


---
