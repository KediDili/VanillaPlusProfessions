# Features
VPP has a lot of features, but mainly divided in three groups:
- Skill Changes
- Professions
- Talents

Please click one of the titles to jump between sections.

## Skill Changes
Following is the changes VPP adds for skills:
- Changes the skill leveling limit to be 20 instead of 10
- Adds an overlay in skills page to display your progress (Shown only if you have at least one vanilla skill above level 10)
- Adds craftables between levels 11-14 and 16-19 (Vanilla skills only)
- Locks Mastery Cave to be behind leveling up all skills to level 20.

## Professions
VPP adds a total of 50 professions for all vanilla skills:
- 40 of these can be chosen at level 15 but your options will depend on what have you chosen at level 5 and 10. 
- The rest 10 of them (which are named Combined or Combo Professions) can be chosen at level 20, but will ignore what you chose at levels 5, 10 and 15.

Every profession you see with '(Lv15)' might be chosen at level 15, and '(Lv20)' means that it will appear in level 20.

### Farming
#### Nutritionist (Lv15)
Machines that take coop animal goods have a chance to double their output. (Machinery as in Mayonnaise machine or loom, but VPP doesn't try to keep track of wool comes from whether a sheep or a rabbit as that'd require effort that isn't worth it imho.)

For Mod Authors: If your animal(s) live in the coop and their products use machines that aren't Mayonnaise Machine or Loom, you should add ``StackModifiers`` to your rules to double the amount of the output. You can use the ``HasProfession`` token to detect if the player has this profession.
#### Breeder (Lv15)
Coop animals are worth more when sold.

For Mod Authors: You don't need to do anything, as VPP will automatically detect your animal if it lives in any vanilla coop.
#### Musterer (Lv15)
Machines that take barn animal goods work faster.
Loom, Mayonnaise machine (only for ostrich eggs) and cheese presses will work faster.

For Mod Authors: If your animal(s) live in the barn and their products use machines that aren't Loom or Cheese Press, or if they use Mayonnaise Machine you should add ``ReadyTimeModifiers`` to the said machinery to make the your produce faster. You can use the ``HasProfession`` token to detect if the player has this profession.
#### Caretaker (Lv15)
Milk pail and shears take no energy. Chance for hay eaten by animals to not be consumed. The chance for animals to not eat hay is %35, and its evaluated for every animal individually.

For Mod Authors: For custom animals, you don't need to do anything as VPP will apply these effects automatically for any animal. If you add any custom tools for animals, you'll have to check if the player has a profession with the ID of ``33``.
#### Machinist (Lv15)
Machines that take harvested crops work faster.
Preserves Jar, Keg and Oil Maker will process 25% faster when the inputs are harvested crops.

For Mod Authors: If you add machinery that accept crops, you should add a ReadyTimeModifier that applies the same effect when
#### Connoisseur (Lv15)
Artisan goods are loved by NPCs.
Most NPCs will love most artisan goods, but it wont automatically make every artisan good loved. Exclusion cases are mentioned below.

For Mod Authors: VPP chooses to ignore the characters or items in the following cases:
1) The item is alcoholic (has the ``alcohol_item`` context tag)
2) The NPC has it as a hated or disliked gift
3) The NPC has an entry of ``"Kedi.VPP.ExcludeFromConnoisseur": "true"`` in their Data/Characters entry's CustomFields
So if you want to exclude an item for your NPC fully or just have it ignore your NPC/item, add any of those
#### Horticulturist (Lv15)
Trees grown in greenhouses will give iridium-quality produce.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Agronomist (Lv15)
If a crop is grown outside of a greenhouse, chance for fertilizer to not be consumed.
Once you choose this profession, for every fertilizer you use outside of a greenhouse, each time its got a %30 chance to not be consumed. Gem dusts made with geode crushers are also affected by this profession.

For Mod Authors: Crop modders do not have to do anything, but for mods that add custom fertilizers, you might need to check if the player has a profession with the ID of ``37``.
#### Farming-Mining (Lv20)
This profession allows you to use the Geode Crusher to produce Gem Dusts out of minerals, which you can use in place of fertilizers (gem dusts are affected by Agronomist profession as well.)
Which type of effect they give depends on their price and color:
- Greenish colors, black and dark brown: Tree fertilizer behaivor
- Red, purple and pink colors: Speed-Gro behaivor
- Orange, yellow, brown: Quality Fertilizer behaivor
- Blue, cyan, white and gray: Retaining Soil behaivor
- Price lower than 120g: Low-level fertilizer behaivor (doesn't affect green gem dusts)
- Price between 120g and 280g: Mid-level fertilizer behaivor (doesn't affect green gem dusts)
- Price higher than 280g: High-level fertilizer behaivor (doesn't affect green gem dusts)

For Mod Authors: If you add custom minerals, they should have an appropriate color tag and be in the minerals category. If both of those are met, VPP will make gem dusts of your custom mineral too.
#### Farming-Foraging (Lv20)
Fruit trees and giant crops are tappable.
You can now tap any giant crop and fruit tree with any tapper. Every 5 days that the tapper isn't removed, it will yield either a custom item defined by the game data (goes only for custom giant crops and fruit tree) or a Fruit Syrup flavored with the crop or fruit of the giant crop/fruit tree. 

- For Mod Authors: If you want your fruit tree or giant crop to give a custom item instead of a Fruit Syrup, you should add an entry like ``"Kedi.VPP.DoesUseCustomItem": "Example.ModID_UnqualifiedItemID"`` to the fruit tree/giant crop data's CustomFields. Otherwise you don't have to do anything other than giving your fruit tree or giant crop at least one fruit/crop.
### Fishing
#### Oceanologist (Lv15)
Attaching bait to a fishing rod will prevent trash from being caught.
If the first catch is a trash item, the catch determination process is re-run up to 100 times, then the mod lets it go to prevent infinite loops even if its a trash.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Artificer (Lv15)
Tackles last longer before breaking.
Tackles will have now 40 uses instead of 20.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Plunderer (Lv15)
Guaranteed treasure chest with every fishing rod cast.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Buccaneer (Lv15)
Treasure chests contain rarer and more valuable items.
Some drops will be switched with others upon fishing these treasures with a rod. The artifact drops will be replaced only if you have donated them to the museum already. Full drops are explained below:
- Fire Quartz -> Fire Opal
- Earth Crystal & Glass Shards -> Refined Quartz
- Stone -> Ocean Stone
- Wood -> Hardwood
- Copper Ore -> Gold Ore
- Iron Ore -> Iridium Ore
- Coal and Rare Disc -> Neptunite
- Small Glow Ring -> Glow Ring
- Small Magnet Ring -> Magnet Ring
- Geode, Frozen Geode & Magma Geode -> Omni Geode
- Regular Bait -> Deluxe Bait
- Dwarf Scrolls -> One of Ruby, Emerald, Aquamarine or Topaz, depending on the color
- Chipped Amphora -> Junimo Pot (furniture from junimo catalogue)
- Arrowhead -> Magic Quiver**
- Ancient/Strange Dolls -> Ancient/Strange Doll Shirt
- Chewing Stick -> Magic Rock Candy
- Ornamental Fan -> Fairy Box**
- Ancient Sword & Broken Trident -> Wicked Kris
- Rusty Spoon -> Energy Tonic
- Rusty Spur -> Golden Spur**
- Rusty Cog -> Copper Bar
- Chicken Statue -> Parrot Egg**
- Prehistoric Tool -> Ice Rod**
- Dried Starfish -> Junimo Star (furniture from junimo catalogue)
- Anchor (the artifact) -> Anchor (the wall decor)
- Bone Flute -> Flute Block
- Prehistoric Handaxe -> Miner's Crest (wall decor)
- Dwarvish Helm -> Wearable Dwarf Helmet
- Dwarf Gadget -> Battery Pack
- Ancient Drum -> Drum Block
- Prehistoric Scapula -> Deluxe Fertilizer
- Prehistoric Tibia -> Bone Sword
- Prehistoric Skull -> Skeleton Shirt
- Skeletal Hand -> Basilisk Paw**
- Prehistoric Rib -> Tree Fertilizer
- Prehistoric Vertebra -> Deluxe Retaining Soil
- Skeletal Tail -> Hyper Speed-Gro
- Nautilus Fossil -> Nautilus Shell
- Amphibian Fossil -> Frog Egg**
- Palm Fossil -> Golden Coconut
- Triobite -> Crab
- Sneakers -> Mermaid Boots
- Rubber Boots -> Dragonscale Boots
- Leather Boots -> Crystal Shoes
- Work Boots -> Pirate Hat
- Combat Boots -> Eye Patch (hat)
- Tundra Boots -> Swashbuckler Hat
- Thermal Boots -> Cinderclown Shoes
-	Amethyst Ring -> Lucky Ring
-	Topaz Ring -> Hot Java Ring
- Aquamarine Ring -> Protection Ring
-	Jade Ring -> Soul Sapper Ring
-	Emerald Ring -> Phoenix Ring
-	Ruby Ring -> Immunity Band
-	Neptune's Glaive -> Obsidian Edge

** if you aren't eligible for obtaining trinkets yet, you'll still get the artifact even if you donated it.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Aquaculturalist (Lv15)
Chance for fish ponds to produce double roe.

For Mod Authors: There's nothing you should do to add compatibility with this profession even if you add custom fish.
#### Trawler (Lv15)
Adding bait to a crab pot guarantees a high-quality catch.
There's a %30 chance the output will be iridium quality, and %70 chance to be gold quality.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Recycler (Lv15)
Chance for recycled trash to produce various tackles.
There's a chance that putting trash in recycling machine will output tackles. Full drops are explained below:
- Trash -> Lead Bobber (%25) or Sonar Bobber (%10)
- Soggy Newspaper -> Trap Bobber (%25) or Treasure Hunter (%10)
- Broken Glasses -> Barbed Hook (%25) or Curiosity Lure (%10)
- Broken CD -> Spinner (%25) or Dressed Spinner (%10)
- Driftwood -> Cork Bobber (%25) or Quality Bobber (%10)

For Mod Authors: If you aren't adding custom tackles, this shouldn't matter for your mod. If so, you should add machine rules to Recycling Machine to give your tackle as output. VPP adds rules for vanilla tackles, with a chance of %25 for common ones and %10 for rarer ones. You can use the ``HasProfession`` token to detect if the player has this profession.
#### Hydrologist (Lv15)
Chance for crab pots to produce a double catch.
There's a %30 chance your crab pot catch to be doubled in stack.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Fishing-Farming (Lv20)
Full fish ponds will continue making item requests. If completed, double yield for the rest of the week.
Fish ponds that are full will ask for quest items, just as they did while wanting to expand the population. If you bring the item to them, they'll give double items for the rest of the week.

For Mod Authors: Your fish must have at least one ``PopulationGate`` entry to be compatible with this profession.
#### Fishing-Mining (Lv20)
Gems can be used as tackles.
Every vanilla gem will act like a certain vanilla tackle when placed to the fishing rod. The matches are:
- Prismatic shard -> Curiosity Lure
- Diamond -> Quality Bobber
- Ruby -> Treasure Hunter
- Emerald -> Barbed Hook
- Jade -> Trap Bobber
- Aquamarine -> Lead Bobber
- Amethyst -> Dressed Spinner
- Topaz -> Cork Bobber

After being used at least once, they WILL NOT stack with other gems, even if they're of the same type!

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Foraging
#### Arborist (Lv15)
#### Shaker (Lv15)
#### Sapper (Lv15)
#### Orchardist (Lv15)
#### Ranger (Lv15)
#### Adventurer (Lv15)
#### Gleaner (Lv15)
#### Wayfarer (Lv15)
#### Foraging-Fishing (Lv20)
Every four days, a randomly-selected forage item will be able to summon fishing bubbles when tossed in the water.

For Mod Authors: If you want your forage to be included by this, make sure it has the ``forage_item`` context tag. If you want your forage to be excluded, make sure it has ``vpp_forageThrowGame_banned`` context tag.
#### Foraging-Combat (Lv20)
Off-screen monsters can be tracked.
Similar to the Tracker Profession in vanilla, when you choose this profession you'll be shown small and moving red arrows that points at monsters.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Mining
#### Metallurgist (Lv15)
Metal bars require less materials to produce.
For all vanilla bars and for Furnace and Heavy Furnaces, the amount of ores are reduced by %20

For Mod Authors: If you add custom metal bars, then you should also reduce the material amount by %20.
#### Ironmonger (Lv15)
Ores are worth more.

For Mod Authors: If you add custom ores, then you should also double your ore's sell price.
#### Ignitor (Lv15)
Furnaces work faster.
Makes it so that any metal bar is processed %20 faster in both Furnace and Heavy Furnaces.

For Mod Authors: If you add custom ores and metal bars, you should also make furnaces faster.
#### Crafter (Lv15)
Crafting recipes gained via the mining skill require less materials.
The recipe changes are explained below:
- 
- 
-
-
- 

For Mod Authors: If you add custom crafting recipes to unlock with Mining skill, you should lower the materials required.
#### Archeologist (Lv15)
Artifacts can be recycled.
Allows artifacts to be recycled via Recycling Machine. The full outputs are explained below:
- Chipped Amphora -> 3 Clay
- Arrowhead & Prehistoric Headaxe -> 3 Stone
- Chewing Stick -> 1 Wood
- Rusty Spoon -> 3 Iron Ore
- Rusty Spur & Rusty Cog -> 3 Copper Ore
- Glass Shards -> 1 Refined Quartz
- Anchor -> 3 Iron Bar
- Ornamental Fan -> 2 Wood
- Golden Mask & Golden Relic -> 2 Gold Bar
- Non Fossil Bones -> 5 Bone Fragment
- Prehistoric Tool -> 1 Stone
- Dried Starfish
- Fossils (Trilobite, Palm Fossil, etc.) -> Nautilus Shell (if input is Nautilus Fossil) / 3 Clay or 2 Opal(%20 chance) (Applies only if input isnt Nautilus Fossil)
- Elven Jewelry -> 1 Aquamarine (%20 chance) or 2 Gold Bars
- Dwarvish Helm & Dwarf Gadget -> 1 Star Shards (%20 chance) or 2 Iron Bar
- Dwarvish Scrolls -> One of Ruby, Aquamarine, Topaz, Emerald or Jade depending on the scroll's color (%20 chance) / 1 Cloth
- Ancient & Strange Dolls -> 1 Cloth (%20 chance) or 2 Wool
- Rare Disc -> 1 Helvite (%20 chance) or 2 Bixite
- Chicken Statue -> 1-2 Copper Bar
- Ancient Drum -> 1 Cloth (%20 chance) or 2 Wood
- Ancient Sword -> 2 Iron Ore (%20 chance) or 1 Copper Bar

For Mod Authors: If you add custom artifacts, then you should also add machine rules to Recycling Machine to process your artifacts.
#### Mineralogist (Lv15)
All vanilla geodes hold the same items as omni-geodes.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Appraiser (Lv15)
Cinder shard nodes drop more cinder shards.
In vanilla, cinder shard nodes drop 1-3 shards each. This profession changes them to drop 2-5 instead.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Enchanter (Lv15)
Cheaper enchantment of weapons and tools.
It'll allow you to enchant your weapons and tools using 4 fire quartzes for each enchant attempt, but you still can use prismatic shards too.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Mining-Combat (Lv20)
Gain combat buffs for every 100 stones broken that day via pickaxe or bombs.
Every 100 stones you break will grant you ``stone amount / 100``+ Defense and attack buffs. It doesn't matter whether they're destroyed by hand or bombs, but it will reset the next day.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Mining-Foraging (Lv20)
Increased possibility for\nMushroom level spawn.
In vanilla, there are "mushroom levels" which spawn in the Mine, below level 80, has colorful lights on the walls and a lot of mushrooms.
The chance for their appaearance is now at %15, while in vanilla this is %0.8.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
### Combat
#### Warrior (Lv15)
Invincible monsters can be damaged.
Monsters like armored bugs, rock crabs, or pupating grubs can be damaged.

For Mod Authors: If you add custom monsters that aren't added via FTM and not named ``Grub``, ``RockCrab`` or ``Bug`` in the C# code (or is not a subclass of any of them), contact me for compatibility with your mod. Otherwise you don't need to do anything.
#### Berserker (Lv15)
Damage dealt increases at low health.
When your health is below 1/4 of your max health, your damage is doubled.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Survivalist (Lv15)
Length of positive buffs increased.
The length of the positive buffs are doubled.

For Mod Authors: If you're adding a custom debuff, make sure you set its IsDebuff field to false.
#### Healer (Lv15)
Length of negative buffs decreased.
The length of the negative buffs are halved.

For Mod Authors: If you're adding a custom debuff, make sure you set its IsDebuff field to true.
#### Technician (Lv15)
Grants invulnerability during special weapon cooldown.
You will temporarily be invulnerable to any enemy attack while any of your weapons are on cooldown.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Speedster (Lv15)
Increased speed of weapon attacks and cooldowns.
Your weapon attacks will be double as fast, but cooldowns will also take twice as much.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Assassin (Lv15)
Guaranteed critical hits against monsters with high defense.
You will be dealing guaranteed crits against Metal Heads, Hot Heads, Dwarvish Sentries, Stickbugs and Rock Crabs.

For Mod Authors: If you add custom monsters that aren't added via FTM content packs and not named ``RockCrab``, ``MetalHead``, ``HotHead``, ``DwarvishSentry`` in the game code (or isn't subclasses of any of these), contact me for compatibility with your mod. Otherwise you don't need to do anything.
#### Assailant (Lv15)
No cooldowns after critical hits.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Combat-Farming (Lv20)
Slimes in the slime hutch also produce valuable items.
When you leave your slimes without water, they'll produce Colored Petrified Slimes or Prismatic Slimes, which are valuable.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Combat-Fishing (Lv20)
Fish can be used as slingshot ammo.
You read it right. Fish on slingshots. The damage the fish will do depends on how expensive it is.

For Mod Authors: There's nothing you should do to add compatibility with this profession.
## Talents
### What even is this?
VPP adds a skill point system named 'talents' which is built from ground to the top. Every talent is a perk purchasable with 'talent points', and each of them change a different aspect of the game.
Its said to be alike Skyrim's skill trees, but to be quite honest I haven't played it so I can't confirm nor deny.

### How do I get talent points?
There are four sources of them:
- Levelling up in any skill, vanilla or not (1 point for each level up)
- Completing Achievements (1 point for each achievement)
- Succeeding in Qi's Challenges (1 point for each special order)
- Achieving Perfection (10 points given at once)

### Can I reset my talents?
You are allowed to reset the trees as often as you want, but you have to use one of:
- Stardrop Tea: To reset the tree you're currently viewing
- Prismatic Shard: To reset all trees at once

Not to worry, VPP wont allow you to reset if you haven't used any points yet (at all or since the last reset),
and after requesting the reset you have to confirm or abort so you can't accidentally waste items or initiate using both items.

### What do each talent do exactly? I'd like to spoil myself.
It adds a grand total of 141 talents divided across 6 skill trees, 25 for each vanilla skill and 16 extras for one tree that isn't spesific to any skills.

### Farming

