# Cities: Skylines 2 Mod - LandValueRemake

Remaked and improved landvalue-rent system.

# Features and Usage

- Remaked LV System:
    Resumed and Improved LandValue System Realistic Simulation:
   1. More realistic landvalue simulation base on original vanilla game (by fixed the vanilla bug of incorrectly calculating landvalue), based on factors such as company profitability, household income level, and building density, which was removed completely since v1.1.x update. ( Its understandably, it will takes more effort to tweak the deep simulation balance than it does to rework a simple system)
   2. The landvalue is calculated according to the different zoning and different density of residential, industrial, commercial, office, etc., which makes it closer to the reality.
   3. Mixed new landvalue feature that service coverage will affect landvalues since v1.1.x update.
   Note: 
   1. In established cities, it may take a considerable amount of time to stabilize landvalues.
   2. The "fake" landvalue tooltip since v1.1.x update of the mechanism has been removed to avoid misleading planning.
   3. Recommand to use ExtendedTooltip mod to observe the real landvalue of each building.
   4. It is advisable to build high-density buildings in areas with higher landvalue (usually renewed in built-up areas) rather than low-value areas, otherwise buildings may be easily abandoned. (This is also similar to the real world)

- One-click removal of low-density residential building restrictions.
   Low-density residential building(including the beach house) are allowed to be built almost anywhere, no matter how high the landvalue is.
   Note: 
   1.When zoning, the road will still show red, ignore this, just zone it. The red color will gradually turn green as it grows out of the buildings.
   2.If you build a low-density building on high landvalue zones, it may be abandoned as soon as it is built, because the tenant family is relatively poor, and you have to wait for a richer family to move in, or wait for the house to drive down the surrounding land price. (Sounds a bit realistic :-)

- One-click removal of all the high-rent warnings for low-density residential buildings.
  Note:
  1.Other types of buildings are not supported at this time, as low-density residential buildings show that rent warnings are an obvious bug in the default or design of vanilla game, while others are relatively normal. If there are also high rent warnings for other types of buildings (after Remaked LV is enabled), bulldoze them :)
  2.It also reduces the likelihood of warnings for other types of buildings. The vanilla mechanism has a warning if the tenant cannot afford to pay the rent if it exceeds 30%, but it is now adjusted to 50%.
  3.Extremely high land prices (usually over $500) can also be used to build low-density housing, but very high-income families need to be brought in, otherwise no one will move in, or someone will rent for a short time and then flee. So plan carefully. (Keep it realistic :0)

- All features can be enabled or disabled via the option (may require a game restart)

- All job codes use burstcompile, which will improve performance that basically the same as when not using the mod. (Results vary from person to person. Tested in my own PC environment, the average CPU usage is reduced by 10-20% compared to the previous version of the mod, or the simulation speed is significantly improved at 100% CPU usage, which makes the PC significantly reduce heat and noise and save power)

- Try to keep minor code changes to the vanilla system to accommodate compatibility with other mods.

- Add more infomation in buildings of Devolopment UI.

# Compatibility and Known Issues

- Modified System: LandValueSystem, RentAdjustSystem, ZoneSpawnSystem. (Depending on the options)
- Compatibility : 
- Compatible with most mods (It is even possible to try it together with LandValueOverhaul but there is no guarantee that the economy will be normal). 
- If you have problems while using RealEco, recommand to turn off its prefab option. Other options are safe to use.
- Issues : welcome to submit issues in Github https://github.com/Noel-leoN/LandValueRemake

# Next Plan

- Manually adjust the cost of building levelingup separately by zoning building type.
- Manually adjust the factors that calculate landvalue by zoning type.
- Please let me know if you want to add other features that is related to landvalue or rent.

## Changelog

## Notice
- Strongly recommand save your game data before use this mod. (Although most of the time there will be no problems)

# Credits
 - Thanks to Cities Skylines 2 Unofficial Modding Discord(https://discord.gg/nJBfTzh7),  Cities Skylines 2 Modding Discord (https://discord.gg/HTav7ARPs2), and thanks to Infixo , Jimmyokok , 89Pleasure,  algeron , yanyang ,krzychu124 and other kind people (In no particular order, please forgive me for not being able to list all) for getting reference and help.
 - Thanks to [CSLBBS](https://www.cslbbs.net): Cities: Skylines 2 community ( in chinese )
