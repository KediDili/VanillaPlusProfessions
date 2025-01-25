# Features
VPP has a lot of features, but mainly divided in three groups:
- [Skill Changes](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#skill-changes)
  - [New Skill Levels](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#new-skill-levels)
  - [Skills Display](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#skills-display)
  - [Mastery Cave Changes](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#mastery-cave-changes)
  - [Skill Craftables](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#skill-craftables)
    - Farming (To be Implemented)
    - [Mining](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#mining)
    - Foraging (To be Implemented)
    - Fishing (To be Implemented)
    - Combat (To be Implemented)
- [Professions](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#professions)
  - [Farming Professions](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#farming-professions)
  - [Mining Professions](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#mining-professions)
  - [Foraging Professions](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#foraging-professions)
  - [Fishing Professions](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#fishing-professions)
  - [Combat Professions](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#combat-professions)
- [Talents](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#talents)
  - [Farming Talents](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#farming-talents)
  - [Mining Talents](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#mining-talents)
  - [Foraging Talents](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#foraging-talents)
  - [Fishing Talents](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#fishing-talents)
  - [Combat Talents](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#combat-talents)
  - [Daily Life Talents](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#daily-life-talents)

Please click one of the titles to jump between sections.

# Skill Changes
Following is the changes VPP adds for skills:
- Changes the skill leveling limit to be 20 instead of 10
- Adds an overlay in skills page to display your progress
- Locks Mastery Cave to be behind leveling up all skills to level 20. (Can be changed via config.json or GMCM menu)
- Adds craftables between levels 11-14 and 16-19 (Vanilla skills only, to be documented)

## New Skill Levels
VPP changes the highest skill level for vanilla skills to 20 from 10. This is not configurable due to a lot of mod features rely on this.
The vanilla and custom level experiences needed are listed below.

| Level | Experience Needed | Total Experience |
|:-----:|:-----------------:|:----------------:|
|1      | +100              | 100              |
|2      | +280              | 380              |
|3      | +390              | 770              |
|4      | +530              | 1300             |
|5      | +850              | 2150             |
|6      | +1150             | 3300             |
|7      | +1500             | 4800             |
|8      | +2100             | 6900             |
|9      | +3100             | 10000            |
|10     | +5000             | 15000            |
|11     | +6000             | 21000            |
|12     | +7000             | 28000            |
|13     | +8000             | 36000            |
|14     | +9000             | 45000            |
|15     | +10000            | 55000            |
|16     | +11000            | 66000            |
|17     | +12000            | 78000            |
|18     | +13000            | 91000            |
|19     | +14000            | 105000           |
|20     | +15000            | 120000           |

## Skills Display
It's available ONLY if you have at least one skill above level 10 (doesn't matter whether vanilla or modded). If you can't see it still, try resizing your window. If that doesn't work too, submit a bug report. It's accessed by clicking the little plus button that appears next to the menu's border. For striped versions, it comes with a color-blindness config.

TODO - ADD PICTURES HERE WITH CONFIG ON AND OFF

## Mastery Cave Changes
VPP changes Mastery Cave to unlock at when all skills are levelled up to 20, unless the config is turned off. It locks "Singular Mastery" and "Master of Five Ways" achievements behind that along with any and all Mastery experience to be gained.
Additionally, if you have Accessorise talent, you will be able to find trinkets even if you haven't obtained Combat Mastery yet, but you will still need the Combat Mastery to get any trinket slots.

## Skill Craftables

### Farming 
(To be Implemented)

### Mining
VPP adds a total of 9 unlockables for Mining skill. What they are and unlocking levels are listed below:

| Level | Unlockable Name | Type |
|:-----:|:---------------:|:----:|
| 11 | Glowing Crystal | Big Craftable |
| 12 | Programmable Drill | Big Craftable |
| 13 | Thermal Reactor | Big Craftable |
| 14 | Node Maker | Big Craftable |
| 16 | Minecart Chest</br>Minecart Repository | Big Craftable</br>Building |
| 17 | Machinery Collector | Big Craftable |
| 18 | Mineral Cavern | Building |
| 19 | Miner's Meal | Cooking |

#### Glowing Crystal
It's a lamp-like big craftable which is unlocked on level 11.
Requires 35 quartz, 30 stone and 1 coal to craft.

#### Programmable Drill
It's a special machine which is unlocked on level 12.
Requires a battery pack and one of any ore, gem, geode, coal or stone as input and works for 1 in-game week.
As output, it gives more of the input, counts are listed below:
| Type of Input | Count   |
|:-------------:|:-------:|
| Ores          | 35-70   |
| Gemstones     | 2-10    |
| Geodes        | 10-30   |
| Coal          | 100-300 |
| Stone         | 300-750 |

It will not work if its not in an indoor "mine" location. (in vanilla, that's only the mine entrance)
If any of the four adjacent tiles has a Machinery Collector; the drill will put any produced output into the collector, create extra drops (with %0.05 chance every 10 in-game minutes), pull inputs from the first possible collector detected. The initial input is prioritized while pulling input, if applicable. It can still take input even if batteries and ores/geodes/etc. are in different containers.
Requires 1 dwarf gadget, 10 copper bars and 5 iron bars to craft.

For Mod Authors: If you'd like programmable drills to work in your mine location, simply add the ``"KediDili.VanillaPlusProfessions_IsConsistentMineLocation": "true"`` in your location data's CustomFields. However, there currently isn't a way to exclude youre ores/geodes/etc. from being inputted into the drill.

#### Thermal Reactor
It's a solar panel-like machine unlocked on level 13.
It does not require any input, but it requires being placed into any location with lava (for vanilla, the Caldera is the only place it works.) Takes 7 days to give output, which is 21-49 batteries.
Requires 45 fire quartz, 1 Hematite and 3 frozen tears to craft.

For Mod Authors: If you'd like thermal reactors to work in your mine location, simply add the ``"KediDili.VanillaPlusProfessions_IsLavaLocation": "true"`` in your location data's CustomFields.
#### Node Maker
It's a regular machine unlocked on level 14.
It can take any ore/geode/gem as input along with 2 clay, as long as there's a corresponding stone node for it.
Outputs 2-6 of the corresponding stone node, which can be placed down in Mineral Cavern indoors.
Requires 1 Rusty Cog, 5 iron bars and 5 gold bars to craft.

For Mod Authors: If you'd like your nodes to be outputted with the node maker, add the "KediDili.VanillaPlusProfessions/NodeMakerData" key and the unqualified Ids of the nodes (I will find a better way later.)

#### Minecart Chest
It's a special chest unlocked on level 16, along with the minecart repository.
If there's a Minecart Repository built on the farm, warps any placed content to it. Otherwise has no function.
Requires 25 hardwood, 16 stone and 1 large oak resin to craft.
The large oak resin can be found via the Spring Thaw talent.

#### Minecart Repository
It's a special storage building unlocked on level 16, along with the minecart chest.
If you build one on your farm and place anything in a minecart chest, any of its content will warp into this building.
Only one of the repository can be built per save. If you want to build a new one, you need to demolish the existing one first.
Requires 20 copper bars, 10 iron bars and 300 stones to build. It can be built from Robin's Carpenter shop for 10000g.

#### Machinery Collector
It's a special type of chest unlocked on level 17.
It can work with programmable drills to collect output and put input into them.
Any and all Machinery Collectors are disabled from Automate's interactions by default.
Requirs 1 Dwarf gadget and 20 copper bars to craft.

#### Mineral Cavern
It's a special building unlocked on level 18.
The nodes from Node Maker can be placed indoors of this building, and any applicable combination of 2x2 (four in total) nodes have a %30 chance to grow into a boulder/clump overnight if there's a matching one with the nodes.
Requires 40 clay, 100 wood and 250 stones to build. It can be built from Robin's Carpenter shop for 20000g.

#### Miner's Meal
It's a cooked food unlocked on level 19.
When eaten, gives 
//TODO, COMPLETE THIS

### Foraging 
(To be Implemented)

### Fishing 
(To be Implemented)

### Combat 
(To be Implemented)

# Professions
VPP adds a total of 50 professions for all vanilla skills:
- 40 of these can be chosen at level 15 but your options will depend on what have you chosen at level 5 and 10. 
- The rest 10 of them (which are named Combined or Combo Professions) can be chosen at level 20, but will ignore what you chose at levels 5, 10 and 15.

Every profession you see with '(Lv15)' can be chosen at level 15, and '(Lv20)' means that it will appear in level 20.

## Farming Professions
```
 Level 10               Level 15      _____
                |-------> Nutritionist     \
 Coopmaster ----|                           |
                |-------> Breeder           |
                                            |
                |-------> Musterer          |        ____       Level 20
 Shepherd ------|                           |        \   \
                |-------> Caretaker         |         \   \     |--------> Farming-Foraging
                                            |==========\   \====|
                |-------> Machinist         |==========/   /====|
 Artisan -------|                           |         /   /     |--------> Farming-Mining
                |-------> Connoisseur       |        /___/
                                            |
                |-------> Horticulturist    |
 Agriculturist -|                           |
                |-------> Agronomist   ____/

```

### Nutritionist (Lv15)
Machines that take coop animal goods have a chance to double their output. (Machinery as in Mayonnaise machine or loom, but VPP doesn't try to keep track of wool comes from whether a sheep or a rabbit as that'd require effort that isn't worth it imho.)

For Mod Authors: If your animal(s) live in the coop and their products use machines that aren't Mayonnaise Machine or Loom, you should add ``StackModifiers`` to your rules to double the amount of the output. You can use the ``HasProfession`` token to detect if the player has this profession.
### Breeder (Lv15)
Coop animals are worth more when sold.

For Mod Authors: You don't need to do anything, as VPP will automatically detect your animal if it lives in any vanilla coop.
### Musterer (Lv15)
Machines that take barn animal goods work faster.
Loom, Mayonnaise machine (only for ostrich eggs) and cheese presses will work faster.

For Mod Authors: If your animal(s) live in the barn and their products use machines that aren't Loom or Cheese Press, or if they use Mayonnaise Machine you should add ``ReadyTimeModifiers`` to the said machinery to make the your produce faster. You can use the ``HasProfession`` token to detect if the player has this profession.
### Caretaker (Lv15)
Milk pail and shears take no energy. Chance for hay eaten by animals to not be consumed. The chance for animals to not eat hay is %35, and its evaluated for every animal individually.

For Mod Authors: For custom animals, you don't need to do anything as VPP will apply these effects automatically for any animal. If you add any custom tools for animals, you'll have to check if the player has a profession with the ID of ``33``.
### Machinist (Lv15)
Machines that take harvested crops work faster.
Preserves Jar, Keg and Oil Maker will process 25% faster when the inputs are harvested crops.

For Mod Authors: If you add machinery that accept crops, you should add a ReadyTimeModifier that applies the same effect when
### Connoisseur (Lv15)
Artisan goods are loved by NPCs.
Most NPCs will love most artisan goods, but it wont automatically make every artisan good loved. Exclusion cases are mentioned below.

For Mod Authors: VPP chooses to ignore the characters or items in the following cases:
1) The item is alcoholic (has the ``alcohol_item`` context tag)
2) The NPC has it as a hated or disliked gift
3) The NPC has an entry of ``"Kedi.VPP.ExcludeFromConnoisseur": "true"`` in their Data/Characters entry's CustomFields
So if you want to exclude an item for your NPC fully or just have it ignore your NPC/item, add any of those
### Horticulturist (Lv15)
Trees grown in greenhouses will give iridium-quality produce.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Agronomist (Lv15)
If a crop is grown outside of a greenhouse, chance for fertilizer to not be consumed.
Once you choose this profession, for every fertilizer you use outside of a greenhouse, each time its got a %30 chance to not be consumed. Gem dusts made with geode crushers are also affected by this profession.

For Mod Authors: Crop modders do not have to do anything, but for mods that add custom fertilizers, you might need to check if the player has a profession with the ID of ``37``.
### Farming-Mining (Lv20)
This profession allows you to use the Geode Crusher to produce Gem Dusts out of minerals, which you can use in place of fertilizers (gem dusts are affected by Agronomist profession as well.)
Which type of effect they give depends on their price and color:

| Color                        | Effect             |
|:----------------------------:|:------------------:|
| Green, black<br/>dark brown  | Tree Fertilizer    |
| Red<br/>Purple & Pink        | Speed-Gro          |
| Orange<br/>Yellow & Brown    | Quality Fertilizer |
| Blue, Cyan<br/>White, Gray   | Retaining Soil     |

| Price Range      | Strength Of Effect |
|:----------------:|:------------------:|
| < 120g           | Low[^3]            |
| >= 120g & <=280g | Medium[^3]         |
| > 280g           | High[^3]           |

[^3]: Doesn't affect Tree Fertilizer Gem Dusts

For Mod Authors: If you add custom minerals, they should have an appropriate color tag and be in the minerals category. If both of those are met, VPP will make gem dusts of your custom mineral too.
### Farming-Foraging (Lv20)
Fruit trees and giant crops are tappable.
You can now tap any giant crop and fruit tree with any tapper. Every 5 days that the tapper isn't removed, it will yield either a custom item defined by the game data (goes only for custom giant crops and fruit tree) or a Fruit Syrup flavored with the crop or fruit of the giant crop/fruit tree. 

- For Mod Authors: If you want your fruit tree or giant crop to give a custom item instead of a Fruit Syrup, you should add an entry like ``"Kedi.VPP.DoesUseCustomItem": "Example.ModID_UnqualifiedItemID"`` to the fruit tree/giant crop data's CustomFields. Otherwise you don't have to do anything other than giving your fruit tree or giant crop at least one fruit/crop.
## Fishing Professions

```
 Level 10               Level 15      _____
                |-------> Oceanologist     \
 Angler --------|                           |
                |-------> Artificer         |
                                            |
                |-------> Plunderer         |        ____       Level 20
 Pirate --------|                           |        \   \
                |-------> Buccaneer         |         \   \     |--------> Fishing-Farming
                                            |==========\   \====|
                |-------> Aquaculturalist   |==========/   /====|
 Mariner -------|                           |         /   /     |--------> Fishing-Mining
                |-------> Trawler           |        /___/
                                            |
                |-------> Recycler          |
 Luremaster ----|                           |
                |-------> Hydrologist   ____/

```

### Oceanologist (Lv15)
Attaching bait to a fishing rod will prevent trash from being caught.
If the first catch is a trash item, the catch determination process is re-run up to 100 times, then the mod lets it go to prevent infinite loops even if its a trash.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Artificer (Lv15)
Tackles last longer before breaking.
Tackles will have now 40 uses instead of 20.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Plunderer (Lv15)
Guaranteed treasure chest with every fishing rod cast.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Buccaneer (Lv15)
Treasure chests contain rarer and more valuable items.
Some drops will be switched with others upon fishing these treasures with a rod. The artifact drops will be replaced only if you have donated them to the museum already. Full drops are explained below:

|                Old Loot                | New Loot                                  |
|:--------------------------------------:|:-----------------------------------------:|
| Fire Quartz                            | Fire Opal                                 |
| Earth Crystal<br/>Glass Shards         | Refined Quartz                            |
| Stone                                  | Ocean Stone                               |
| Wood                                   | Hardwood                                  |
| Copper Ore                             | Gold Ore                                  |
| Iron Ore                               | Iridium Ore                               |
| Coal<br/>Rare Disc                     | Neptunite                                 |
| Small Glow Ring                        | Glow Ring                                 |
| Small Magnet Ring                      | Magnet Ring                               |
| Geode<br/>Frozen Geode<br/>Magma Geode | Omni Geode                                |
| Regular Bait                           | Deluxe Bait                               |
| Dwarf Scrolls                          | One of Ruby, Emerald, Aquamarine or Topaz |
| Chipped Amphora                        | Junimo Pot (Furniture)                    |
| Arrowhead                              | Magic Quiver[^1]                          |
| Ancient/Strange Dolls                  | Ancient/Strange Doll Shirt                |
| Chewing Stick                          | Magic Rock Candy                          |
| Ornamental Fan                         | Fairy Box[^1]                             |
| Ancient Sword<br/>Broken Trident       | Wicked Kris                               |
| Rusty Spoon                            | Energy Tonic                              |
| Rusty Spur                             | Golden Spur[^1]                           |
| Rusty Cog                              | Copper Bar                                |
| Chicken Statue                         | Parrot Egg[^1]                            |
| Prehistoric Tool                       | Ice Rod[^1]                               |
| Dried Starfish                         | Junimo Star (furniture)                   |
| Anchor (the artifact)                  | Anchor (the wall decor)                   |
| Bone Flute                             | Flute Block                               |
| Prehistoric Handaxe                    | Miner's Crest (wall decor)                |
| Dwarvish Helm                          | Wearable Dwarf Helmet                     |
| Dwarf Gadget                           | Battery Pack                              |
| Ancient Drum                           | Drum Block                                |
| Prehistoric Scapula                    | Deluxe Fertilizer                         |
| Prehistoric Tibia                      | Bone Sword                                |
| Prehistoric Skull                      | Skeleton Shirt                            |
| Skeletal Hand                          | Basilisk Paw[^1]                          |
| Prehistoric Rib                        | Tree Fertilizer                           |
| Prehistoric Vertebra                   | Deluxe Retaining Soil                     |
| Skeletal Tail                          | Hyper Speed-Gro                           |
| Nautilus Fossil                        | Nautilus Shell                            |
| Amphibian Fossil                       | Frog Egg[^1]                              |
| Palm Fossil                            | Golden Coconut                            |
| Triobite                               | Crab                                      |
| Sneakers                               | Mermaid Boots                             |
| Rubber Boots                           | Dragonscale Boots                         |
| Leather Boots                          | Crystal Shoes                             |
| Work Boots                             | Pirate Hat                                |
| Combat Boots                           | Eye Patch (hat)                           |
| Tundra Boots                           | Swashbuckler Hat                          |
| Thermal Boots                          | Cinderclown Shoes                         |
| Amethyst Ring                          | Lucky Ring                                |
| Topaz Ring                             | Hot Java Ring                             |
| Aquamarine Ring                        | Protection Ring                           |
| Jade Ring                              | Soul Sapper Ring                          |
| Emerald Ring                           | Phoenix Ring                              |
| Ruby Ring                              | Immunity Band                             |
| Neptune's Glaive                       | Obsidian Edge                             |
 
[^1]: if you aren't eligible for obtaining trinkets yet, you'll still get the artifact even if you donated it.
 
For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Aquaculturalist (Lv15)
Chance for fish ponds to produce double roe.

For Mod Authors: There's nothing you should do to add compatibility with this profession even if you add custom fish.
### Trawler (Lv15)
Adding bait to a crab pot guarantees a high-quality catch.
There's a %30 chance the output will be iridium quality, and %70 chance to be gold quality.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Recycler (Lv15)
Chance for recycled trash to produce various tackles.
There's a chance that putting trash in recycling machine will output tackles. Full drops are explained below:
| Type of Trash   | Possible Drop                               |
|:---------------:|:-------------------------------------------:|
| Trash           | Lead Bobber (%25)<br/>Sonar Bobber (%10)    |
| Soggy Newspaper | Trap Bobber (%25)<br/>Treasure Hunter (%10) |
| Broken Glasses  | Trap Bobber (%25)<br/>Treasure Hunter (%10) |
| Broken CD       | Spinner (%25)<br/>Dressed Spinner (%10)     |
| Driftwood       | Cork Bobber (%25)<br/>Quality Bobber (%10)  |

For Mod Authors: If you aren't adding custom tackles, this shouldn't matter for your mod. If so, you should add machine rules to Recycling Machine to give your tackle as output. VPP adds rules for vanilla tackles, with a chance of %25 for common ones and %10 for rarer ones. You can use the ``HasProfession`` token to detect if the player has this profession.
### Hydrologist (Lv15)
Chance for crab pots to produce a double catch.
There's a %30 chance your crab pot catch to be doubled in stack.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Fishing-Farming (Lv20)
Full fish ponds will continue making item requests. If completed, double yield for the rest of the week.
Fish ponds that are full will ask for quest items, just as they did while wanting to expand the population. If you bring the item to them, they'll give double items for the rest of the week.

For Mod Authors: Your fish must have at least one ``PopulationGate`` entry to be compatible with this profession.
### Fishing-Mining (Lv20)
Gems can be used as tackles.
Every vanilla gem will act like a certain vanilla tackle when placed to the fishing rod[^2]. The matches are:
| Gem           | Tackle it Acts Like |
|:-------------:|:-------------------:|
|Prismatic Shard| Curiosity Lure      |
|Diamond        | Quality Bobber      |
|Ruby           | Treasure Hunter     |
|Emerald        | Barbed Hook         |
|Jade           | Trap Bobber         |
|Aquamarine     | Lead Bobber         |
|Amethyst       | Dressed Spinner     |
|Topaz          | Cork Cobber         |

[^2]: After being used at least once, they WILL NOT stack with other gems, even if they're of the same type!

For Mod Authors: There's nothing you should do to add compatibility with this profession.
## Foraging Professions

```
 Level 10               Level 15      _____
                |-------> Arborist          \
 Lumberjack ----|                           |
                |-------> Shaker            |
                                            |
                |-------> Sapper            |        ____       Level 20
 Tapper --------|                           |        \   \
                |-------> Orchardist        |         \   \     |--------> Foraging-Fishing
                                            |==========\   \====|
                |-------> Ranger            |==========/   /====|
 Botanist ------|                           |         /   /     |--------> Foraging-Combat
                |-------> Adventurer        |        /___/
                                            |
                |-------> Gleaner           |
 Tracker -------|                           |
                |-------> Wayfarer      ____/

```
### Arborist (Lv15)
Wild trees grow faster.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Shaker (Lv15)
Chance for wild trees to drop items when shaken.

For Mod Authors: You can add items as drops for your custom wild trees' ``ShakeItems`` and ``SeedDropItems`` fields. You can use the ``HasProfession`` token to see if the player has the profession.
### Sapper (Lv15)
Tappers work faster.
All tappers will output %20 faster.

For Mod Authors: If you add custom tappers, add ``tapper_multiplier_<speed>`` context tag to your item. Vanilla takes the speed bit and sets the minutes-until-ready to ``daysUntilReady x 1 / speed`` . You can use the ``HasProfession`` token to see if the player has the profession.
### Orchardist (Lv15)
Tappers give double harvest.

For Mod Authors: There's nothing you need to do to add compatibility with this profession.
### Ranger (Lv15)
Forest forage worth more. The prices will be doubled.

For Mod Authors: You can exclude your custom forage if you give it the ``kedi_vpp_banned_ranger`` context tag.
### Adventurer (Lv15)
Non-forest forage worth more. The prices will be doubled.

For Mod Authors: You can exclude your custom forage if you give it the ``kedi_vpp_banned_adventurer`` context tag.
### Gleaner (Lv15)
Crop seeds can be found as forage.

Any vanilla seed may be found during their growing seasons in Forest, Mountain and Backwoods. (VPP will only affect vanilla seeds, but other mods can add theirs too.)

For Mod Authors: If you add custom crops, you can add your seeds as forage with a chance of 0.1, if you wish.

### Wayfarer (Lv15)
Chance for forage to be found out of season.

Any vanilla forage may be found OUT OF their regular seasons as well, for example a Holly may appear in Summer, Fall and Spring as well with this profession.
VPP affects only vanilla forage on its own, but other mods may add theirs too.

For Mod Authors: If you add custom forage, you can add your forage with a chance of 0.1 out of their regular seasons, if you wish.

### Foraging-Fishing (Lv20)
Every four days, a randomly-selected forage item will be able to summon fishing bubbles when tossed in the water.

For Mod Authors: If you want your forage to be included by this, make sure it has the ``forage_item`` context tag.
If you want your forage to be excluded, make sure it has ``vpp_forageThrowGame_banned`` context tag.

### Foraging-Combat (Lv20)
Off-screen monsters can be tracked.
Similar to the Tracker Profession in vanilla, when you choose this profession you'll be shown small and moving red arrows that points at monsters.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
## Mining Professions

```
 Level 10               Level 15      _____
                |-------> Metallurgist     \
 Blacksmith ----|                           |
                |-------> Ironmonger        |
                                            |
                |-------> Ignitor           |        ____       Level 20
 Prospector ----|                           |        \   \
                |-------> Crafter           |         \   \     |--------> Mining-Combat
                                            |==========\   \====|
                |-------> Archeologist      |==========/   /====|
 Excavator -----|                           |         /   /     |--------> Mining-Foraging
                |-------> Mineralogist      |        /___/
                                            |
                |-------> Appraiser         |
 Gemologist ----|                           |
                |-------> Enchanter    ____/

```

### Metallurgist (Lv15)
Metal bars require less materials to produce.
For all vanilla bars and for Furnace and Heavy Furnaces, the amount of ores are reduced by %20

For Mod Authors: If you add custom metal bars, then you should also reduce the material amount by %20.
### Ironmonger (Lv15)
Ores are worth more.

For Mod Authors: If you add custom ores, then you should also double your ore's sell price.
### Ignitor (Lv15)
Furnaces work faster.
Makes it so that any metal bar is processed %20 faster in both Furnace and Heavy Furnaces.

For Mod Authors: If you add custom ores and metal bars, you should also make furnaces faster.
### Crafter (Lv15)
Crafting recipes gained via the mining skill require less materials.
The recipe changes are explained below:
| Craftable Name | Old Recipe | New Recipe |
|:--------------:|:----------:|:----------:|
| Cherry Bomb    | 4 Copper Ore<br/>1 Coal | 3 Copper Ore<br/>1 Coal |
| Staircase      | 99 Stone | 66 stone |
| Glowstone Ring | 5 Solar Essence<br/>5 iron bars | 3 solar essence<br/>4 iron bars |
| Bomb           | 4 iron ore<br/>1 Coal | 2 iron ore<br/>1 coal |
| Mega Bomb      | 4 gold ore<br/>1 Solar Essence<br/>1 Void Essence | 3 gold ore<br/>1 solar essence<br/>1 void essence |
| Crystalarium   | 99 stone<br/>5 gold bars<br/>2 iridium bars<br/>1 battery pack |60 stone<br/>3 gold bars<br/>1 iridium bars<br/>1 battery pack|
| Transmute (Fe) | 3 copper bars | 5 copper bars[^4] |
| Transmute (Au) | 2 iron bars | 3 iron bars[^4] |

[^4]: The output is doubled.


For Mod Authors: If you add custom crafting recipes to unlock with Mining skill, you should lower the materials required.
### Archeologist (Lv15)
Artifacts can be recycled.
Allows artifacts to be recycled via Recycling Machine. The full outputs are explained below:

| Artifact                          | Output                                                           |
|:---------------------------------:|:----------------------------------------------------------------:|
| Chipped Amphora                   | 3 Clay                                                           |
| Arrowhead<br/>Prehistoric Headaxe | 3 Stone                                                          |
| Chewing Stick                     | 1 Wood                                                           |
| Rusty Spoon                       | 3 Iron Ore                                                       |
| Rusty Spur<br/>Rusty Cog          | 3 Copper Ore                                                     |
| Glass Shards                      | 1 Refined Quartz                                                 |
| Anchor                            | 3 Iron Bar                                                       |
| Ornamental Fan                    | 2 Wood                                                           |
| Golden Mask<br/>Golden Relic      | 2 Gold Bar                                                       |
| Non-Fossil Bones                  | 5 Bone Fragment                                                  |
| Prehistoric Tool                  | 1 Stone                                                          |
| Dried Starfish                    | 1 Petrified Slime                                                |
| Nautilus Fossil                   | 1 Nautilus Shell                                                 |
| Fossils                           | 3 Clay or 2 Opal(%20 chance)                                     |
| Elven Jewelry                     | 1 Aquamarine (%20 chance) or 2 Gold Bars                         |
| Dwarvish Helm<br/>Dwarf Gadget    | 1 Star Shards (%20 chance) or 2 Iron Bar                         |
| Dwarvish Scrolls                  | 1 Ruby/Aquamarine/Topaz/Emerald/Jade (%20 chance)<br/>or 1 Cloth |
| Ancient & Strange Dolls           | 1 Cloth (%20 chance) or 2 Wool                                   |
| Rare Disc                         | 1 Helvite (%20 chance) or 2 Bixite                               |
| Chicken Statue                    | 1-2 Copper Bar                                                   |
| Ancient Drum                      | 1 Cloth (%20 chance) or 2 Wood                                   |
| Ancient Sword                     | 2 Iron Ore (%20 chance) or 1 Copper Bar                          |

For Mod Authors: If you add custom artifacts, then you can also add machine rules to Recycling Machine to process your artifacts. (or you can make your own recycling machine for them, up to you)

### Mineralogist (Lv15)
All vanilla geodes hold the same items as omni-geodes.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Appraiser (Lv15)
Cinder shard nodes drop more cinder shards.
In vanilla, cinder shard nodes drop 1-3 shards each. This profession changes them to drop 2-5 instead.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Enchanter (Lv15)
Cheaper enchantment of weapons and tools.
It'll allow you to enchant your weapons and tools using 4 fire quartzes for each enchant attempt, but you still can use prismatic shards too.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Mining-Combat (Lv20)
Gain combat buffs for every 100 stones broken that day via pickaxe or bombs.
Every 100 stones you break will grant you ``stone amount / 100``+ Defense and attack buffs. It doesn't matter whether they're destroyed by hand or bombs, but it will reset the next day.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Mining-Foraging (Lv20)
Increased possibility for<br/>nMushroom level spawn.
In vanilla, there are "mushroom levels" which spawn in the Mine, below level 80, has colorful lights on the walls and a lot of mushrooms.
The chance for their appaearance is now at %15, while in vanilla this is %0.8.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
## Combat Professions

```
 Level 10               Level 15      _____
                |-------> Warrior          \
 Brute ---------|                           |
                |-------> Berserker         |
                                            |
                |-------> Survivalist       |        ____       Level 20
 Defender ------|                           |        \   \
                |-------> Healer            |         \   \     |--------> Combat-Farming
                                            |==========\   \====|
                |-------> Technician        |==========/   /====|
 Acrobat -------|                           |         /   /     |--------> Combat-Fishing
                |-------> Speedster         |        /___/
                                            |
                |-------> Assassin          |
 Desperado  ----|                           |
                |-------> Assailant    ____/
```

### Warrior (Lv15)
Invincible monsters can be damaged.
Monsters like armored bugs, rock crabs, mummies, or pupating grubs can be damaged.

For Mod Authors: If you add custom monsters that aren't added via FTM and not named ``Grub``, ``RockCrab``, ``Mummy`` or ``Bug`` in the C# code (or is not a subclass of any of them), contact me for compatibility with your mod. Otherwise you don't need to do anything.
### Berserker (Lv15)
Damage dealt increases at low health.
When your health is below 1/4 of your max health, your damage is doubled.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Survivalist (Lv15)
Length of positive buffs increased.
The length of the positive buffs are doubled.

For Mod Authors: If you're adding a custom buff that's not a debuff, make sure you set its IsDebuff field to false.
### Healer (Lv15)
Length of negative buffs decreased.
The length of the negative buffs are halved.

For Mod Authors: If you're adding a custom buff that's a debuff, make sure you set its IsDebuff field to true.
### Technician (Lv15)
Grants invulnerability during special weapon cooldown.
You will temporarily be invulnerable to any enemy attack while any of your weapons are on cooldown.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Speedster (Lv15)
Increased speed of weapon attacks and cooldowns.
Your weapon attacks will be double as fast, but cooldowns will also take twice as much.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Assassin (Lv15)
Guaranteed critical hits against monsters with high defense.
You will be dealing guaranteed crits against Metal Heads, Hot Heads, Dwarvish Sentries, Stickbugs and Rock Crabs.

For Mod Authors: If you add custom monsters that aren't added via FTM content packs and not named ``RockCrab``, ``MetalHead``, ``HotHead``, ``DwarvishSentry`` in the game code (or isn't subclasses of any of these), contact me for compatibility with your mod. Otherwise you don't need to do anything.
### Assailant (Lv15)
No cooldowns after critical hits.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Combat-Farming (Lv20)
Slimes in the slime hutch also produce valuable items.
When you leave your slimes without water, they'll produce Colored Petrified Slimes or Prismatic Slimes, which are valuable.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Combat-Fishing (Lv20)
Fish can be used as slingshot ammo.
You read it right. Fish on slingshots. The damage the fish will do depends on how expensive it is.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
# Talents
## What even is this?
VPP adds a skill point system named 'talents' which is built from ground to the top. Every talent is a perk purchasable with 'talent points', and each of them change a different aspect of the game.
Its said to be alike Skyrim's skill trees, but to be quite honest I haven't played it so I can't confirm nor deny.

## How do I get talent points?

|         Way to obtain          |  Talent points obtainable |
|:------------------------------:|:-------------------------:|
| Levelling up in any skill      | 1 (each level up)         |
| Completing Achievements        | 1 (each achievement)      |
| Succeeding in Qi's Challenges  | 1 (only once per SO)      |
| Achieving Perfection           | 10                        |

Once obtained, all talent points are shared across all skill trees so for example if you gained one point from levelling up Farming, you can use it in the Foraging or Combat trees etc. with no restrictions.

## Can I reset my talents?
You are allowed to reset the trees as often as you want, but you have to use one of:
- Stardrop Tea: To reset the tree you're currently viewing
- Prismatic Shard: To reset all trees at once

VPP wont allow you to reset if you haven't used any points yet (at all or since the last reset),
and after requesting the reset you have to confirm or abort so you can't accidentally waste items or initiate using both items.

## What do each talent do exactly? I'd like to spoil myself.
It adds a grand total of 141 talents divided across 6 skill trees, 25 for each vanilla skill and 16 extras for one tree that isn't spesific to any skills.

## Farming Talents
![FarmingTalents](https://github.com/user-attachments/assets/4cc761a5-321a-4ffe-ba9b-ea9be6116b3c)

### Resurgence
Watering can automatically partially refills every 90 minutes.

### Refreshing Waters
Watering can is refilled every morning upon waking.

### Good Soaking
Soil is automatically watered the day after rain.

### Nourishing Rain
For each rainy day, growth time for outdoor crops is reduced by 1 day.[^5]

### Wild Growth
Chance for animals to produce both deluxe & normal items.

### Overcrowding
Coops & barns can house more animals.
Vanilla coops and barns will increase in size to house more animals by %20.

Note: In case you're using a barn/coop replacer mod, there's an option in the CP part of the mod to turn the map edits off so that you can use the replacer.

### Bio-Engineering
Large animal produce will result in doubled artisan goods.

### Pastoralism
Quality animal produce will result in quality artisan goods.

### Breed Like Rabbits
Rabbits are able to reproduce.

### Brimming With Life
Double chance for animal pregnancy.

In vanilla, the base chance is ``currentAnimalAmount * 0.0055`` every night, the talent changes it to ``currentAnimalAmount * 0.011``

### Cycle Of Life
50% chance to receive back a seed of a harvested non-regrowable crop.

### Effloresence
Reduces time flowers take to grow.[^5]

### Harmonious Blooming
Receive additional honey depending on the number of flowers around a bee house (honey type from a random flower)

### Abundance
Chance for honey to increase in quality for each day not harvested.

### Fairy's Kiss
Every full-grown fairy rose increases the chance for crop fairy to appear.

### Fae Blessings
The crop fairy will instantly grow a giant crop if it's possible for one to grow.

### Fine Grind
Milled rice/flour/sugar matches the ingredient quality.

### Drift Fencing
Crafting fences produces double items.

### Storm Surge
Lightning rods protect more crops and fruit trees even when producing.

### Cold Press
Oil makers produce additional types of oils.

You can use the oil maker to make oils from:
- Tomato
- All seasonal wild seed packs
- Summer Squash
- Mango
- Apricot
- Pine Cone
- Orange
- Apple
- Amaranth
- Carrot
- Pomegranate
- Hazelnut
- Coconut
- Artichoke
- Pumpkin
- Poppy
- Wheat
- Unmilled Rice
- Radish

All of these will have names such as Tomato Oil but will act like the vanilla regular oil and can be used in such recipes.

### Upcycling
Higher-tier sprinklers require lower ones as materials.

This talent will changes the recipes of Iridium sprinkler and Quality Sprinkler, the old and new recipes are listed below

|     Sprinkler     |                    Old Recipe                   |                       New Recipe                      |
|:-----------------:|:-----------------------------------------------:|:-----------------------------------------------------:|
| Quality Sprinkler | 1 Iron Bar<br/>1 Gold Bar<br/>1 Refined Quartz  | 1 Gold Bar<br/>1 Coal<br/>1 Sprinkler                 |
| Iridium Sprinkler | 1 Gold Bar<br/>1 Iridium Bar<br/>1 Battery Pack | 1 Iridium Bar<br/>1 Battery Pack<br/>1 Quality Sprinkler |

### Fertigation
Both pressure nozzles and enrichers can be added to sprinklers.

Upon purchasing the talent, you will get a new crafting recipe for an item named "Fertigator" which requires one Pressure Nozzle and one Enricher.
When you place it to the sprinkler, you will get effects of both.

### Trickster
Crows wont eat full-grown crops anymore.

### Tropical Bliss
Summer & multi-season crops grow faster on Ginger Island.[^5]

[^5]: This talent works in sync with the other two of Nourishing Rain, Efflorescence and Tropical Bliss; only one of them will apply to one crop at a time.

### Harvest Season
Junimos harvest faster.

They will be twice as fast.

## Mining Talents
![MiningTalents](https://github.com/user-attachments/assets/623adbc6-587c-4c45-b6e9-34ede8dfadcf)

### Down In The Depths
If no ladder has spawned, guaranteed chance of finding one after breaking 8 stones.

### Shadow Buddies
If befriended, Krobus will gift you 1-3 geodes via mail each week.

### Dwarven Buddies
If befriended, the Dwarf will gift you 1-3 metal bars via mail each week.

### Essence Infusion
Each day, Sign of the Vessels will produce Solar Essence and Wicked Statues will produce Void Essence.

### Elder Scrolls
If the Dwarvish Translation Guide has been obtained, dwarf scrolls can be read for mining experience.

### Speed Of Darkness
Receive a speed boost after 12 AM.

### Ancestral Weaponry
If Ginger Island has been unlocked,\nthe Dwarf sells dwarven weaponry.

Note: The Dwarf that sells it is the one in the mines, not Ginger Island Volcano.

### Shared Focus
Time is slower in any mines.

### Museum Piece
Double chance of receiving\nundonated artifacts or minerals\nfrom geodes & artifact troves.

Note: This talent will automatically disable itself when you complete the vanilla museum.

### Explosive Personality
Rocks destroyed by bombs have increased chance to drop additional geodes.

### Synthesis
Crystalariums can copy geodes.

### Upheaval
Low chance for all stones on a mine floor to be replaced by geode nodes.

For Mod Authors: If you want your custom mines to be affected by this talent, add the ``Kedi.VPP.Upheaval`` key to your Data/Locations entry's CustomFields... field. Optionally, you can add a space-delimited list of UNQUALIFIED object IDs for the geode nodes you want your regular stones to be replaced with as value. If you don't set it, VPP will just use the vanilla ones instead. As a bonus, if you have custom nodes as well, add them the ``Kedi_VPP_Bland_Stone_Node`` context tag to your "stone items".

### Matryoshka
Chance for geodes to contain another geode.

For Mod Authors: If you add custom geodes and want your geodes to not be dropped by this talent, add the ``kedi_vpp_banned_from_being_dropped`` context tag to them, if you don't want your geode to not drop other geodes, add the ``kedi_vpp_banned_from_dropping`` context tag.

### X-ray
View what item the next geode contains. Geodes worth more.

For Mod Authors: If you add custom geodes and want your geodes' content to not be seen by this talent, add them the ``geode_crusher_ignored`` tag. But know that adding the tag will also prevent your geodes to be crushed by the Geode Crusher.

### Geometry
Crystalariums work faster.

### Dazzle
Chance for crystalariums to improve item quality by one.
This affects minerals, and geodes if you have Synthesis talent. 

### Straight Run
Robin sells staircases for 2000g each.

### Detonation Dampener
Bombs won't destroy forage, machinery, or artifact spots.

### Volatility
Chance for ore nodes to drop higher tier ore as well.
There's a %50 chance for a copper node to additionally drop iron ore,
an iron node to drop gold, etc.

### Alchemic Reversal
Higher tier metal bars can be converted into lower ones.
The talent will add two crafting recipes, similar to the Transmute bars added by Mining skill.
You will be able to have 5 gold bars for 2 iridium bars
and 5 iron bars for 2 gold bars.

### Room And Pillar
All ore nodes can spawn in the quarry mine.

### Fallout
Radioactive ore nodes spawn in the non-dangerous mines and Skull Cavern.
The chance is ``0.0001 * mineLevel`` per stone.

### Everyone's Best Friend
Diamond, tigerseye, opal, fire opal, jasper, and star shards will become universal loved gifts.
Note: This talent only makes these gems into universal loves. Due to how gift taste logic works, if a character's own gift data lists any of these as loved/hated/disliked/neutral/liked, their own gift data will take priority over universals (Such as Haley hating prismatic shards despite prismatic shards being universal loves). **This is not a bug with VPP. It's a vanilla feature.**

### Crystal Cavern
Low chance for all stones on a mine floor\nto be replaced by gem nodes.

For Mod Authors: If you want your custom mines to be affected by this talent, add the ``Kedi.VPP.CrystalCavern`` key to your Data/Locations entry's CustomFields... field. Optionally, you can add a space-delimited list of UNQUALIFIED object IDs for the gem nodes you want your regular stones to be replaced with as value. If you don't set it, VPP will just use the vanilla ones instead. As a bonus, if you have custom nodes as well, add them the ``Kedi_VPP_Bland_Stone_Node`` context tag to your "stone objects".

### Over The Rainbow
Add prismatic shard crafting recipe.
The recipe will require one of every gem.

## Foraging Talents
![ForagingTalents](https://github.com/user-attachments/assets/c77b4061-72e1-4f15-9abb-f9f20c115dfa)

### Nature Secrets
Chance for chopped tree stumps to drop forage items.

For Mod Authors: You can exclude your custom forage if you give it the ``kedi_vpp_banned_naturesecrets`` context tag.
### Primrose Path
Flowers give more friendship when gifted.

### Berry-mania
Berry seasons last for one week.

### Diamond of the Kitchen
Truffles spawn in forests.

### Local Knowledge
Forage is reset every day.

### Sea Change
Once per week, beach forage is doubled.

### Starfall
Chance for star shards to spawn as forage in the evening for 1-7 days.
Note: [Airyn's Star Fragments](https://www.nexusmods.com/stardewvalley/mods/30842) may spawn with this talent too!

### Desert Bloom
Chance for flowers to spawn in Calico Desert on rainy days in Pelican Town.

### Bountiful Boletes
High chance for mushrooms to spawn after rainy days.

For Mod Authors: If you add custom mushrooms and want them to be affected by this talent, you need to add one of these context tags to them; ``edible_mushroom`` or ``Kedi_VPP_Poisonous_Mushroom``, but the edible mushroom tag will make them available for Dehydrators' Dried Mushrooms as well which is not intended.

### Renewing Mist
Double forage spawns on the first sunny day after rain.

### Reforestation
Hand-planted wild tree seeds produce double harvest.

### Sap Sipper
Sap is edible and recovers health.

### Spring Thaw
Large tapper produce can be harvested from tappers.

This will only affect the vanilla pine tars, maple syrups and oak resins and turn them into their large versions added by VPP.
This talent may be needed for some skill craftables added by VPP as well.

### Accumulation
Chance for tapper products to increase\nin quality if left uncollected overnight.

### Exotic Tapping
Palm trees can be tapped for Oil. Mahogany trees can be tapped for Sugar.

### Grove Tending
Double effects of Tree Fertilizer.

### Surge Protection
Wild trees become coal trees when struck by lightning.

### Welcome to the Jungle
Coconuts can be planted to grow a palm tree. Golden coconuts can drop anywhere.

### Clear as Mud
High chance to find clay when tilling.

### Sleep Under the Stars
No penalty for falling asleep at 2AM if at the beach, mountain, or forest.

### Camp Spirit
Food made with a Cookout Kit restores more energy and health than food made in a kitchen.

### Survival Cooking
Adds unique recipes to Cookout Kits.
There will be new 10 cooking recipes that will be only available from Cookout Kits. So you wont be able to cook them in regular kitchens. Most of VPP-spesific ones use forage as ingredients because they're supposed to heal the player a middle amount while they're out in the wild.

The new recipes and their ingredients are listed below:
|        Food         |             Recipe             |
|:-------------------:|:------------------------------:|
| Grilled Leek        | 1 Leek                         |
| Seaweed Salad       | 2 Seaweed                      |
| Trail Mix           | 1 Hazelnut<br/>1 Spice Berry   |
| Roasted Fiddleheads | 1 Fiddlehead Fern              |
| Wild Fruit Salad    | 1 Blackberry<br/>1 Salmonberry |
| Grilled Fish        | 1 Any Fish                     |
| Baked Tubers        | 1 Snow Yam<br/>1 Winter Root   |
| Mushroom Kebab      | 2 Common Mushroom              |
| Fried Bug Steak     | 1 Bug Steak                    |
| Steamed Clams       | 2 Clam                         |

Note: Cooking with this talent activated will result in Qi Seasoning not being consumed and the cooked foods will be gold quality automatically. This is intended because when two independent items stacks are combined into one, the other loses all of its custom price, edibility and modData entries (which is something mods often use to add extra data to anything in the game), and so much other data. The gold quality intends to prevent the stacking so prevent the data loss.

For Mod Authors: If you have such recipes and want them to be added in this, add them the ``kedi_vpp_survival_cooking_food`` context tag.

### Pyrolysis
Charcoal Kilns will be more efficient.

### Static Charge
Lightning Rods can accumulate two charges at once. Solar Panels take 2 less days to produce a battery.

### Eye Spy
Increased chance of finding Living Hats and similar items.

## Fishing Talents
![FishingTalents](https://github.com/user-attachments/assets/c6011b80-a076-4ae7-b4f8-a237a1610c9a)

### Fishery Grant
Fish ponds are cheaper to build.
The old and new building requirements are listed below:
| Old           | New           |
|:-------------:|:-------------:|
| 5000g         | 2000g         |
| 200 Stone     | 100 Stones    |
| 5 Seaweed     | 5 Seaweed     |
| 5 Green Algae | 5 Green Algae |

### In the Weeds
Seaweed and Green Algae can be added to fish ponds.

Seaweed may only produce more seaweed and Clay,
Green Algae may only produce Green Algae and White Algae, with all of them having %50 chance.

### Roe-mance
Roe, Aged Roe, and Caviar worth more.

### Legendary Variety
Allows legendary fish to give non-roe drops when put in a fish pond.
The full drops are listed below:

| Fish                               | Drops                                                       |
|:----------------------------------:|:-----------------------------------------------------------:|
| Legend</br>Legend II               | 20-50 Hardwood (%20), 5-10 Hyper Quality Fertilizer (%15)   |
| Angler</br>Ms. Angler              | 1 Pearl (%10), 2-10 Magic Bait (%15)                        |
| Crimsonfish</br>Son of Crimsonfish | 2-5 Ruby (%10), 10-20 Deluxe Speed Gro (%15)                |
| Glacierfish</br>Glacierfish Jr.    | 2-5 Aquamarine (%10), 10-20 Deluxe Retaining Soil (%20)     |
| Mutant Carp</br>Radioactive Carp   | 5-10 Radioactive Ore (%15), 25-30 Seaweed/Green Algae (%30) |

For mod authors: Give non-roe drops for your legendary fish locked behind this talent normally. But you should use "BigFishSmallPond" while addressing this talent with GSQs and CP tokens, because this talent's name had to change due to 1.6.9

### Spawning Season
Chance for roe yield in fish ponds is increased.

If there's no output for the day or if it's not roe, there's a %50 chance the next might be roe.

### Ex-squid-site
Chance for roe to increase in quality if left uncollected overnight. Roe quality will apply to Aged Roe, Caviar, and Squid Ink.

The chance for this is %24.

### Double Hook
Worm bins will produce twice in a day.
This applies only to the regular worm bins.

### Clickbait
Worm Bin has a 50% chance of producing Wild Bait instead of normal Bait.

### Can It
Recycling machines produce an increased variety of items.
The extra drops are explained below.

| Type of Trash   | Possible Drops                         |
|:---------------:|:--------------------------------------:|
| Trash           | 1-3 Copper Ore<br/>1-3 Gold Ore        |
| Driftwood       | 1 Hardwood (%10)                       |
| Soggy Newspaper | Squid Ink (%20)<br/>Driftwood          |
| Broken Cd       | Prismatic Shard (%10)                  |
| Broken Glasses  | Aerinite (%10)<br/>Ghost Crystal (%10) |

### Smokehouse
Fish can be smoked multiple times to increase quality.

So if you have a fish with lowest quality, 
Then you can smoke it once to turn it into a smoked fish with lowest quality,
If you smoke it again, you will get a silver quality one. If you smoke it again, it'll be gold quality and so on.

### Fit for a Czar
Aged Roe can be aged in a cask to increase quality.
This also affects Caviar as well.

### Take it Slow
Fishing casting bar will move more slowly if you have the Snail Tackle equipped.

After you purchase the talent, a new crafting recipe will be available which is named Snail Tackle.
It will require one Snail and one iron bar. After equipped to the rod, it'll slow the casting bar.

### A-fish-ionado
Receive a random tackle via mail every week.

### It Was This Big
If catching a new size record for a fish, quality will increase.

### Dead Man's Chest
Chance for crab pots to produce artifact troves.
If your crab pots are empty, there's a %10 chance your crab pot will have an artifact trove.

### Diversification
Wild Bait now doubles Crab Pot produce.
(This doesn't apply to Dead Man's Chest, obviously)

### Fish Trap
Crab pots will sometimes catch regular fish.
If your crab pots are empty or they have trash, there's a %25 chance your crab pot will have a non-crab-pot fish.

### Bait and Switch
Chance for crab pot fish to increase in quality if left uncollected overnight.

### Crab Rave
Increased chance to find crabs, crayfish and lobsters if wearing a Crabshell Ring.

### Fish's Wishes
Allows using Magic Bait on crab pots. If you have Luremaster, basic Bait acts like Magic Bait.

### Here Fishy Fishy
Failing to catch any fish 3 times in a row greatly increases the bobber bar height for the next fish.
If you fail more than 3 times, the bobber bar height will also keep increasing in size until you catch at least one fish.

### Vast Domain
Legendary fish can be caught in an increased location range.

### One Fish, Two Fish
Challenge Bait can be used to catch four fish at once.

### Take a Break
Fish do not run away when reeling in a treasure chest.
The "progress bar" just stops until you're complete collecting the chest or stop reeling it.

### Bubble Trouble
Bubbles will always be within 4 tiles of land.
In other words, bubbles wont be further than 4 tiles away from the land.

## Combat Talents
![Combat Talents](https://github.com/user-attachments/assets/fb961c55-72cc-48a2-91cb-53a540b53f71)

### Slimeshot
Slime can be used as slingshot ammo. If slime hits a monster, it will be slowed down.
But the effect will be applied only once.

### Triple Shot
Gives slingshots a special attack that shoots three projectiles at once.

Note: The talent automatically disables itself if the slingshot has the auto-fire enchantment from the Enchanting talent, for the sake of balance.
Note #2: It's intended that you can "slide" around while casting the triple shot. It's supposed to make using slingshots slightly easier and be a lil funny bit.

### Bullseye
Slingshots can do critical damage.
In vanilla, slingshots are blocked from dealing critical damage (for unknown reasons. It just is but it makes no sense.)

### Dazzling Strike
Gems can be used as slingshot ammo.<br/>nHigh chance for gem ammo<br/>nto not be destroyed.

### Enchanting
Slingshots have custom enchantments called Auto-Fire, Rapid, Bat Killer, and Thrifty.
The effects are listed below:

| Name       | Effect |
|:----------:|:------:|
| Auto-Fire  | Slingshot will automatically<br/>fire while you aim |
| Rapid      | Slingshot ammo will<br/>move faster |
| Bat Killer | Double damage<br/>done to bats |
| Thrifty    | Slingshot might not use<br/>ammo at times |

### Accessorise
Trinkets can be converted into rings.
A new big craftable recipe is added into your crafting recipes, which is named "Trinket Workbench" and is craftable for 50 hardwood & 3 iron bars.

Note: Said rings spawned from CJB or SMAPI console will not be functional because both of those wont do some necessary changes VPP does. Trinket rings can't be combined with any ring since version 1.0.3.

For Mod Authors: If you add custom trinkets, you need to add a custom ring item, and add an entry like ``"Kedi.VPP.AccessoriseRing": "ExampleAuthor.ExampleMod_UnqualifiedItemIDForMyRing"`` to your trinket's CustomFields field. This is needed for VPP to match your trinket to your ring.

### Consolidation
Rings of the same type can be forged together.
There is only one exception to this. Combined Rings still can't be combined again.

### Hidden Benefits
All vanilla trinkets will get a second purpose.

There are 8 trinkets in vanilla, and one extra purpose for each.
In case there are people who want to learn what it is themselves, only the hint locations are listed below:

| Trinket name   | Location          |
|:--------------:|:-----------------:|
| Golden Spur    | Marnie's Shop     |
| Frog Egg       | Sebastian's Room  |
| Parrot Egg     | Emily's Room      |
| Fairy Box      | Jas' Room         |
| Ice Rod        | Pierre's Shop     |
| Basilisk Paw   | Wizard's Basement |
| Magic Quiver   | Adventurer Guild  |
| Magic Hair Gel | Alex's Room       |

### Rending Strike
4 consecutive strikes against a monster will reduce damage dealt by that monster.

### Debiliating Stab
Damaging monsters with a dagger will reduce their speed.

### Severing Swipe
Damaging monsters with a sword will reduce their attack.

### Concussive Impact
Damaging monsters with a club will make them take additional damage for 3 seconds following the hit.

### Aftershock
Club attacks hit a second time. The second strike deals reduced damage.

### Champion
Grants +2 Defense for 6 seconds after striking an enemy with a sword.

### Flurry
Consecutive dagger strikes against the same target deal half additional damage, stacking up to 5 times.

### Ferocity
Critical hits add a short 10% crit damage buff.

The buff lasts 10 seconds.

### Starburst
Adds a melee weapon enchantment named Starburst which throws projectiles when swung.

### Grit
Increases damage immunity period after taking damage.

Increases the damage invulneravility period by %20.

### Sidestep
Chance to dodge enemy attacks.

Every time a monster hits you, there's a %10 chance that you wont be damaged and the talent will give you a temporary invulnerability. The farmer will flash, but its to show the invulnerability.
If you have slime charmer ring, the talent wont be effectful against slimes.

### Slippery
Changes the Slimy debuff into a +1 Speed buff.

### Fortified
For every monster in the current location, gain +1 Defense to a limit of +8.
(aka capped at 8, so if there is 15 monsters there, the buff will still be +8.)

### Full Knowledge
Gives full exp for monsters killed on the farm.

In vanilla, slain farm monsters give you only 1/3 of the original XP (since 1.6). This talent restores it to full.

### Monster Specialist
Choose one of four pre-defined group of monsters and deal additional damage to them.

When you choose this talent in talent menu, you will get another menu with four more choices, the options and which monsters they affect are listed below.

|Branch Name| Pre-defined Group of Monsters                                    |
|-----------|:----------------------------------------------------------------:|
| Ground    | Slime, Grub, Dust Sprite<br/>Duggy, Lava Lurk, Spider            |
| Humanoid  | Golem, Skeleton<br/>Mummy, Shadow                                |
| Flying    | Bat, Ghost, Serpent, Magma Sprite<br/>Blue Squid, Squid kid, Fly |
| Armoured  | Dwarvish Sentry, Hot Head<br/>Metal Head, Rock Crab              |

For Mod Authors: If you add custom monsters via C# (excluding FTM monsters), and if they're not a subclass of any vanilla monsters, contact me for compatibility.

### Sugar Rush
Foods cooked with sugar will remove the Nauseated debuff. If you have Survival Cooking, its recipes will remove Nauseated as well.
For Vanilla, VPP will only cover anything that contains sugar in their recipe.

For Mod Authors: If you add custom desserts, you should add the ``ginger_item`` context tag ONLY if the farmer has the talent.

### Meditation
Passive health regeneration while standing still.
Every 10 in-game minutes, you will gain 15 points of health.

## Daily Life Talents
![DailyLifeTalents](https://github.com/user-attachments/assets/1eb3e255-dbdd-456e-985c-a8254f98d73b)

### Mate's Rates
Friendship with shopkeepers reduces prices.
Affects all shops except Desert Festival shops and will be triggered only when you have 6 hearts with the shopkeeper. Prices will be reduced by %20.

For Mod Authors: There is nothing you should do. Even if you add custom items to vanilla shops, they will be affected automatically.
### Monumental Discount
Obelisks are cheaper to purchase.
All vanilla obelisks are re-priced as 100,000g.

For Mod Authors: If you add custom obelisks, you should lower your money prices of your obelisks when this talent is acquired. 
### Craft Supplies
New craftables gained by skill level-up are sold at shops after being unlocked. There's nothing you should do for compatibiliy with this talent.

### Admiration
Halves the friendship decay due to not interacting with characters, doesn't affect things like disliked gifts or negative event choices. 
There's nothing you should do for compatibiliy with this talent.

### Insider Info
The price increases for the vanilla NPCs are listed below:
| Villager  | Items                                                                        |
|:---------:|:----------------------------------------------------------------------------:|
| Abigail   | Amethyst, Pumpkin<br/>Pumpkin Pie, Blackberry Cobbler                        |
| Alex      | Complete Breakfast<br/>Salmon, Hashbrowns, Pancakes                          |
| Caroline  | Summer Spangle<br/>Green Tea & Tea Leaves                                    |
| Clint     | All ores and bars                                                            |
| Demetrius | Red Mushroom<br/>Purple Mushroom, Orange, Apple                              |
| Dwarf     | All Dwarf items, Cave Carrot<br/>All Bombs                                   |
| Elliott   | All Beach Forage                                                             |
| Emily     | Cloth & Wool                                                                 |
| Evelyn    | Tulip, Blue Jazz & Cookie                                                    |
| George    | Leek, Earth Crystal<br/>Fried Mushroom                                       |
| Gus       | Bread & Spaghetti<br/>Fish Taco                                              |
| Haley     | Coconut & Sunflower                                                          |
| Harvey    | Pickles, Coffee<br/>Energy Tonic & Muscle Remedy                             |
| Jas       | Fairy Rose & Pink Cake                                                       |
| Jodi      | Chocolate Cake, Rhubarb Pie<br/>Eggplant Parmesan<br/>Crispy Bass, Fried Eel |
| Kent      | Fiddlehead Fern<br/>Fiddlehead Risotto<br/>Hazelnut & Roasted Hazelnuts      |
| Krobus    | Void Egg & Void Mayonnaise<br/>Strange Bun                                   |
| Leah      | Dandelion & Morel<br/>Common Mushroom & Chanterelle                          |
| Lewis     | Autumn's Bounty & Glazed Yams<br/>Vegetable Medley & Hot Pepper              |
| Leo       | Ginger & Taro Root<br/>Mango & Duck Feather                                  |
| Linus     | Salmonberry & Blackberry<br/>Spice Berry & Wild Plum                         |
| Marnie    | All milks and cheeses                                                        |
| Maru      | Strawberry & Battery Pack                                                    |
| Penny     | Treasure Appraisal Guide<br/>Book Of Stars & Poppy                           |
| Pierre    | Vinegar, Oil, Wheat<br/>Flour, Sugar, Miner's Treat                          |
| Robin     | Stone, Wood, Peach<br/>Hardwood                                              |
| Sam       | Pizza, Cactus Fruit & Tigerseye                                              |
| Sandy     | Crocus, Daffodil & Sweet Pea                                                 |
| Sebastian | Frozen Tear & Sashimi<br/>Pumpkin Soup & Obsidian                            |
| Shane     | All regular chicken eggs                                                     |
| Vincent   | Snail & Periwinkle<br/>Cranberry Candy, Spring Onion                         |
| Willy     | All baits and tackles                                                        |
| Wizard    | Solar Essence & Void Essence<br/>Super Cucumber                              |

For Mod Authors: If you're a custom NPC author and want your NPC(s) affected by this talent, you need to add an entry to Insider Info's data. The format DOES NOT accept context tags or category IDs. You can get the target path for the data by using the ``ContentPaths`` token. The key must be the NPC's internal name, and the value should be a space-delimited list of UNQUALIFIED item IDs. For exp: ``"Abigail": "123 456 789 102 425"``

### Gift of Friendship
The villagers will start giving you gifts after you're close enough to them to show their gratitude. The gifts are usually the villagers' loved and liked gifts.

For Mod Authors: If you're a custom NPC author and want your NPC(s) affected by this talent, you should add a CT response for ``VPP.GiftOfFriendship`` in their dialogue file, with a when condition if they have 8 hearts or above with the farmer. You can to make it less or more than 8 hearts depending on how your character is like, its just what I set vanilla NPCs to react to.
### Haute Cuisine
Increases sell prices of cooked food.
Doubles the sell price of cooking category objects, there's nothing you need to do add compatibility with this talent.

Note: Cooking with this talent activated will result in Qi Seasoning not being consumed and the cooked foods will be silver quality automatically. This is intended because when two independent items stacks are combined into one, the other loses all of its custom price, edibility and modData entries (which is something mods often use to add extra data to anything in the game), and so much other data. The silver quality intends to prevent the stacking so prevent the data loss.

### Mini Fridge, Big Space
Increase storage space for mini-fridges.
Makes it so that mini-fridges will act like big chests.

For Mod Authors: There's nothing you need to do add compatibility with this talent.
### Good Eats
Allows for two food buffs and two drink buffs simultaneously.

For Mod Authors: There's nothing you need to do add compatibility with this talent.
### Lost And Found
Reading lost books after they've been found gives you daily buffs, reading another book while one will override the previous book buff and the buffs will be removed next day. Here are the buffs for vanilla lost books:

|Starts With                  | Type of Buff | Strength of Buff |
|:---------------------------:|:------------:|:----------------:|
|Tips on Farming              | Farming      | +1               |
|...This is a book by Marnie. | Farming      | +2               |
|On Foraging                  | Foraging     | +2               |
|The Fisherman, Act I         | Fishing      | +1               |
|How deep do the mines go?    | Mining       | +1               |
|An Old Farmer's Journal      | Magnetism    | +40              |
|Scarecrows                   | Farming      | +3               |
|The Secret of the Stardrop   | Max Stamina  | +40              |
|Journey of the Prairie King  | Attack       | +2               |
|A Study on Diamond Yields    | Mining       | +3               |
|Brewmaster's Guide           | Farming      | +4               |
|Mysteries of the Dwarves     | Mining       | +3               |
|From The Book of Yoba        | Foraging     | +4               |
|Marriage Guide for Farmers   | Luck         | +3               |
|The Fisherman, Act II        | Fishing      | +3               |
|Technology Report!           | Mining       | +4               |
|Secrets of the Legendary Fish| Fishing      | +4               |
|The Bus Tunnel               | Luck         | +3               |
|Note From Gunther            | Speed        | +4               |
|'Goblins' by M. Jasper       | Defense      | +4               |
|Solok Ulan Paa Eno Ra        | Speed        | +4               |

For Mod Authors: There's nothing you need to do add compatibility with this talent.
### Bookclub Bargains
Reduces the bookseller's prices.

### Cycle of Knowledge
Extra books can be recycled.
Recycle outputs are listed below:

| Book Name              | Output           |
|:----------------------:|:----------------:|
| Book of Stars          | 1-3 Stardrop Tea |
| Stardew Valley Almanac | 10-20 Grass Starter</br>5-10 Blue Grass Starter (%10)</br>1-2 Golden Animal Cracker (%10, requires Farming Mastery) |
| Woodcutter Weekly      | 10-20 Hardwood   |
| Bait And Bobber        | 5-10 Deluxe Bait or Wild Bait or Magic Bait</br>1 of any tackle (%5) |
| Mining Monthly         | 5-10 of any ore  |
| Combat Quarterly       | 3-5 Life Elixir or Oil of Garlic or Monster Musk |
| Jewels Of The Sea      | 3-5 Roe</br>2-4 Treasure Chest |
| Woody's Secret         | 3-5 of any wild tree seed</br>1 of any vanilla fruit tree sapling |
| Monster Compendium     | 5-10 of Any Monster Loot |
| Jack Be Nimble,</br>Jack Be Thick | 5-10 Eggplant Parmesan or Stuffing</br>or Pumpkin Soup or Crab Cakes or Banana Pudding</br>or Mango Sticky Rice or Autumn's Bounty |
| Treasure Appraisal Guide | 5-10 Artifact Trove |
| Book Of Mysteries | 15-25 Mystery Box</br>15-25 Golden Mystery Box (%10) |

### Narrow Escape
Reduces the amount of gold lost on death.
In any case you die or pass out, you'll only lose 1000g at most.

For Mod Authors: There's nothing you need to do add compatibility with this talent.
### Trashed Treasure
More valuable items can be found in trash cans.

For Mod Authors: There's nothing you need to do add compatibility with this talent.
### Butterfly Effect
On bad luck days, a void butterfly spawns in distant locations (sewers, swamp, secret woods, etc) and gives a +10 luck buff when found.

For Mod Authors: There's nothing you need to do add compatibility with this talent.
### Gift of The Talented
Gives you a stardrop. This stardrop doesn't count towards perfection.

For Mod Authors: There's nothing you need to do add compatibility with this talent.
