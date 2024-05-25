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
Machines that take coop animal goods have a chance to double their output. (Machinery as in Mayonnaise machine or loom, but VPP doesn't try to keep track of wool comes from whether a sheep or a rabbit as that'd require effort that isn't worth it.)
For Mod Authors: If your animal(s) live in the coop and their products use machines that aren't Mayonnaise Machine or Loom, you should add ``StackModifiers`` to your rules to double the amount of the output. You can use the ``HasProfession`` token to detect if the player has this profession.
#### Breeder (Lv15)
Coop animals are worth more when sold.
For Mod Authors: You don't need to do anything, as VPP will automatically detect your animal if it lives in any vanilla coop.
#### Musterer (Lv15)
Machines that take barn animal goods work faster.
Loom, Mayonnaise machine (only for ostrich eggs) and cheese presses will work faster.
For Mod Authors: If your animal(s) live in the barn and their products use machines that aren't Loom or Cheese Press, or if they use Mayonnaise Machine you should add ``ReadyTimeModifiers`` to the said machinery to make the your produce faster. You can use the ``HasProfession`` token to detect if the player has this profession.
#### Caretaker (Lv15)
Milk pail and shears take no energy. Chance for hay eaten by animals to not be consumed. The chance for animals to not eat hay is %, and its evaluated for every animal individually.

For Mod Authors: For custom animals, you don't need to do anything as VPP will apply these effects automatically for any animal. If you add any custom tools for animals, you'll have to check if the player has a profession with the ID of ``33``.
#### Machinist (Lv15)

#### Connoisseur (Lv15)
For Mod Authors: VPP doesn't automatically make any artisan good into a loved gift, and chooses to ignore the characters or items in the following cases:
1) The item is alcoholic (has the ``alcohol_item`` context tag)
2) The NPC has it as a hated or disliked gift
3) The NPC has an entry of ``"Kedi.VPP.ExcludeFromConnoisseur": "true"`` in their Data/Characters entry's CustomFields
#### Horticulturist (Lv15)
#### Agronomist (Lv15)
Once you choose this profession, for every fertilizer you use outside of a greenhouse. Gem dusts made with geode crushers & Farming-Mining combo profession are also affected by this profession.
For Mod Authors: Crop modders do not have to do anything, but for mods that add custom fertilizers, you might need to check if the player has a profession with the ID of ``37``.
#### Farming-Mining (Lv20)
This profession allows you to use the Geode Crusher to produce Gem Dusts out of minerals, which you can use in place of fertilizers (gem dusts are affected by Agronomist profession as well.)
Which type of effect they give depends on their price and color:
- Greenish colors: Tree fertilizer behaivor
- colors: Speed-Gro behaivor
- colors: Quality Fertilizer behaivor
- colors: Retaining Soil behaivor
- Price lower than : Low-level fertilizer behaivor
- Price between and : Mid-level fertilizer behaivor
- Price higher than : High-level fertilizer behaivor

For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Farming-Foraging (Lv20)
- For Mod Authors: There's a lot of things you should do but I'm lazy atm. dman.

### Fishing
#### Oceanologist (Lv15)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Artificer (Lv15)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Plunderer (Lv15)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Buccaneer (Lv15)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Aquaculturalist (Lv15)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Trawler (Lv15)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Recycler (Lv15)
For Mod Authors: If you aren't adding custom tackles, this shouldn't matter for your mod. If so, you should add machine rules to Recycling Machine to give your tackle as output. VPP adds rules for vanilla tackles, with a chance of %25 for common ones and %10 for rarer ones. You can use the ``HasProfession`` token to detect if the player has this profession.
#### Hydrologist (Lv15)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Fishing-Farming (Lv20)
For Mod Authors: Your fish must have at least one ``PopulationGate`` to be compatible with this profession. Nothing should error if it doesn't, though.
#### Fishing-Mining (Lv20)
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
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Foraging-Combat (Lv20)
For Mod Authors: There's nothing you should do to add compatibility with this profession.

### Mining
#### Metallurgist (Lv15)
#### Ironmonger (Lv15)
#### Ignitor (Lv15)
#### Crafter (Lv15)
#### Archeologist (Lv15)
#### Mineralogist (Lv15)
#### Appraiser (Lv15)
#### Enchanter (Lv15)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Mining-Combat (Lv20)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Mining-Foraging (Lv20)
For Mod Authors: There's nothing you should do to add compatibility with this profession.

### Combat
#### Warrior (Lv15)
#### Berserker (Lv15)
#### Survivalist (Lv15)
#### Healer (Lv15)
#### Technician (Lv15)
#### Speedster (Lv15)
#### Assassin (Lv15)
#### Assailant (Lv15)
#### Combat-Farming (Lv20)
For Mod Authors: There's nothing you should do to add compatibility with this profession.
#### Combat-Fishing (Lv20)
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
