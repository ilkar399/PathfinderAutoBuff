## [0.2.1]
## Fixed
- CompletelyNormal metamagic
- Recompiled for 1.1.0i
## [0.2.0]
## Fixed
- Some possible crash issues
- Some mod conflict crashes caused by json serializer conflict
- Fixed some abilities that weren't casted 
- Fixed free actions being sometimes applied too fast and causing queue execution interruption
## Reworked
- Reworked GUI to the prefab one. Should also fix some inconsistencies and crashes
- Reworked targeting UI (you can also prioretize targets now)
- Reworked favorite queues
## Added
- Per-queue settings
- Metamagic priority settings
- Pets are now available as casters as well

## [0.1.6] - 2021-09-15
## Fixed
- Fixed some ability context not applied on queue execution.
- Fixed Extend metamagic not being used for spellcasting 
(Priority for spells now is Spellbook with the highest CL-> 
Has Extend -> Lowest spell level slot (for cases when caster has 
the same spell on multiple spellevels)
## Added
- Action copy button (for the cases like Cackle/Chant)

## [0.1.5b] - 2021-09-07
## Fixed
- Improved error handling to reduce the chances of crashing the game/mod 
UI completely

## [0.1.5a] - 2021-09-04
## Fixed
- Initial ability execution fix

## [0.1.5] - 2021-09-02
## Fixed
- Preparing for Wrath of the Righteous Release
## [0.1.4] - 2021-06-09
## Fixed
- Fixed casting from Spontaneous&Mythic spellbooks
- Relaxed restrictions on stopping queue execution if one of the spells is not castable for some reason.

## [0.1.3] - 2021-05-30
## Fixed
- Fixed Chant and Cackle not executing in the queue if 'refresh buffs when they have a short duration' enabled in settings.
- Fixed some possible null exceptions in UI. Also added some exception wrappers for such cases
## Added
- Added automatic queue UI refresh on changing area

## [0.1.2] - 2021-05-22
## Fixed
- Fixed the target selection UI bug

## [0.1.1a] - 2021-05-15
### Fixed
- Fixed spontaneous casting bug introduced in 0.1.1

## [0.1.1] - 2021-05-15
### Fixed
- Fixed Cantrips not processed properly in the UI.
- Initial Metamagic fix.
- Mod repackaged to the proper folder structure.

## [0.1.0] - 2021-05-12
### Added
- Initial Release, see README for complete feature list.
