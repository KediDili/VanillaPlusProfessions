# Compatibility

**Warning: This guide might assume you're already familiar with JSON format, Content Patcher packs, Game State Queries and/or C# based mods depending on the part of the guide.**

## Content Packs (Mostly CP, though)
VPP offers three custom CP tokens and custom GSQs for compatibility purposes at the moment.
Additionally, if you need to know whether your custom X is compatible with Y feature of VPP, every talent and profession section in [``features.md``](https://github.com/KediDili/VanillaPlusProfessions/blob/main/compatibility.md) tells how to add compatibility or do they need anything at all!

The queries/tokens, and the formats are listed below:

| Token Name    | Details |
|:-------------:|:--------|
| ContentPaths  | Accepts one of ``ItemSpritesheet``, ``ProfessionIcons``, ``TalentBG``, ``TalentSchema``, ``BundleIcons`` and ``SkillBars``.<br/><br/>``ItemSpritesheet`` will give you sprites of any item VPP adds, but it also contains a few misc things that don't fit anywhere else.<br/>``ProfessionIcons`` contains all and only profession icons meant to be used by VPP.<br/>``TalentSchema`` contains an image consisting of all "lines" and an icon of all VPP base talent trees.<br/>``BundleIcons`` is only the colored smaller bundles and greyscale bundles only meant for VPP to use.<br/>``SkillBars`` are the skill bars from VPP's Color Blindness config and skill overlay.<br/> depending on the input, it'll return a path to be used in the Target field.
| HasProfessions | Returns a list of the professions the farmer currently has. |
| HasTalents | Returns a list of the talents the farmer currently has. Respects the Professions Only config and disabled talents feature. |

An example patch for both of them is written below:
```json
{
    "LogName": "Changing VPP's profession icons"
    "Action": "EditImage",
    "Target": "{{KediDili.VanillaPlusProfessions/ContentPaths:ProfessionIcons}}",
    "FromFile": "assets/MyIcons.png",
}
```
```json
{
    "LogName": "Changing my custom ore's price depending on if the player has Ironmonger"
    "Action": "EditImage",
    "Target": "Data/Objects",
    "Fields": {
        "ExampleAuthor.ModId_MyCustomOre": {
            "Price": 500
        }
    },
    "When": {
        "KediDili.VanillaPlusProfessions/HasProfession|contains= Ironmonger": "true",
    }
}
```

### Game State Queries
VPP adds three GSQs to be used with VPP compatibility, their names, parameters and what they do are listed below:
|                                Format                                  |                         Details                      |
|:----------------------------------------------------------------------|:----------------------------------------------------|
| ``KediDili.VanillaPlusProfessions_WasRainingHereYesterday <location>`` | Checks if it was raining the said location YESTERDAY.<br/>This can be used for compatibility with talents such as Bountiful Boletes or Renewing Mist that need this check. |
| ``KediDili.VanillaPlusProfessions_PlayerHasTalent <player> <talentName>`` | Checks if the ``<player>`` has purchased ``<talentName>``. It's highly recommended to use this instead of mail flag checks to track talents, since this query also accounts for ``ProfessionsOnly`` config and the new disabled talents feature. |
| ``KediDili.VanillaPlusProfessions_PlayerHasProfession <player> <professionName>`` | Checks if the ``<player>`` has obtained ``<professionName>``. It's recommended to use in fields such as ``Condition`` since they require GSQs. |

### What you can/need to add compatibility depending on what your mod adds:
| Added by mods          | VPP Feature                                     |
|:-----------------------|:------------------------------------------------|
| Artifacts              | [Archaeologist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#archeologist-lv15) |
| Animals                | [Breed Like Rabbits](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#breed-like-rabbits), [Nutritionist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#nutritionist-lv15), [Musterer](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#musterer-lv15), [Pastoralism](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#pastoralism), [Bio-Engineering](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#bio-engineering) |
| Animal Prod. Machinery | [Nutritionist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#nutritionist-lv15), [Musterer](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#musterer-lv15), [Pastoralism](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#pastoralism), [Bio-Engineering](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#bio-engineering) |
| Buffs                  | [Healer](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#healer-lv15), [Survivalist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#survivalist-lv15) |
| Crafting recipes       | [Crafter](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#crafter-lv15), [Drift Fencing](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#drift-fencing), [Upcycling](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#upcycling), [Alchemic Reversal](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#alchemic-reversal) |
| Crops                  | [Gleaner](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#gleaner-lv15) |
| Crop Machinery         | [Machinist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#machinist-lv15), [Cold Press](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#cold-press) |
| Crystalariums          | [Dazzle](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#dazzle), [Geometry](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#geometry), [Synthesis](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#synthesis) |
| Cooked foods           | [Survival Cooking](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#survival-cooking), [Sugar Rush](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#sugar-rush) |
| Fruit Trees            | [Farming-Foraging](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#farming-foraging-lv20) |
| Forage                 | [Ranger](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#ranger-lv15), [Adventurer](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#adventurer-lv15), [Wayfarer](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#wayfarer-lv15),<br/>[Foraging-Fishing](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#foraging-fishing-lv20), [Bountiful Boletes](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#bountiful-boletes), [Nature Secrets](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#nature-secrets)  |
| Furnaces               | [Metallurgist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#metallurgist-lv15), [Ignitor](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#ironmonger-lv15) |
| Fish                   | [Fishing-Farming](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#fishing-farming-lv20), [Vast Domain](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#vast-domain), [A-fish-ionado](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#a-fish-ionado) [Legendary Variety](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#legendary-variety) |
| Garbage Cans/Trash Cans | [Trashed Treasure](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#trashed-treasure) |
| Geodes                 | [Matryoshka](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#matryoshka), [X-ray](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#x-ray) |
| Giant Crops            | [Farming-Foraging](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#farming-foraging-lv20) |
| Locations              | [Ranger](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#ranger-lv15), [Adventurer](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#adventurer-lv15), [Gleaner](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#gleaner-lv15), [Wayfarer](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#wayfarer-lv15), [Bountiful Boletes](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#bountiful-boletes),<br/>[Crystal Cavern](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#crystal-cavern), [Upheaval](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#upheaval), [Starfall](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#starfall), [Shared Focus](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#shared-focus),<br/>[Diamond of the Kitchen](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#diamond-of-the-kitchen), [Vast Domain](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#vast-domain) Programmable Drill, Thermal Reactor|
| Minerals               | [Farming-Mining](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#farming-mining-lv20) |
| Mill Produce           | [Fine Grind](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#fine-grind) |
| NPCs                   | [Gift Of Friendship](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#gift-of-friendship), [Insider Info](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#insider-info), [Connoisseur](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#connoisseur-lv15) |
| Ores/Metals            | [Ironmonger](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#ironmonger-lv15), [Metallurgist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#metallurgist-lv15), [Alchemic Reversal](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#alchemic-reversal)     |
| Other Machinery        | [Pyrolysis](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#pyrolysis), [Static Charge](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#static-charge), [Double Hook](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#double-hook), [Clickbait](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#clickbait) |
| Ore Nodes              | [Crystal Cavern](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#crystal-cavern), [Upheaval](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#upheaval) |
| Obelisks               | [Monumental Discount](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#monumental-discount) |
| Readable Books         | [Cycle of Knowledge](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#cycle-of-knowledge), [Bookclub Bargains](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#bookclub-bargains) |
| Shops                  | [Mate's Rates](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#mates-rates), [Bookclub Bargains](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#bookclub-bargains) |
| Tappers                | [Sapper](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#sapper-lv15) |
| Trash                  | [Recycler](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#recycler-lv15), [Can It](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#can-it) |
| Weapons                | [Ancestral Weaponry](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#ancestral-weaponry) |
| Wild Trees             | [Exotic Tapping](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#exotic-tapping), [Welcome to the Jungle](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#welcome-to-the-jungle), [Spring Thaw](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#spring-thaw) |

## C# Mods
VPP offers an API to do things like adding custom talent trees for SpaceCore skill mods, getting VPP's config values for Mastery Cave Changes and Color Blindness Changes, and getting VPP professions a player currently has.

If you want to use the VPP API:
1) Copy [``IVanillaPlusProfessions.cs``](https://github.com/KediDili/VanillaPlusProfessions/blob/main/Compatibility/IVanillaPlusProfessions.cs) in Compatibility folder to your project.
2) Delete the elements that you don't need, as the API may change anytime this will help your version to be compatible as long as possible.
3) Request it through SMAPI's ModRegistry.

If your add any new things of these following things, you might need to use the API for compatibility with these features:
| Mod Feature     | VPP Feature                   |
|:---------------:|:-----------------------------:|
| Fertilizers     | [Agronomist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#agronomist-lv15) |
| Fishing Tackles | [Recycler](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#recycler-lv15) |
| Lost Books      | [Lost And Found](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#lost-and-found) |
| Monsters        | [Slimeshot](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#slimeshot), [Monster Specialist](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#monster-specialist) |
| Trinkets        | [Accessorise](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#accessorise), [Hidden Benefits](https://github.com/KediDili/VanillaPlusProfessions/blob/main/features.md#hidden-benefits) |

### Skill Mods by SpaceCore
VPP will recognize your custom skill for features like the skill overlay, but will NOT try to manage your professions or add new ones for levels 15 and 20.
Creating a talent tree or expanding your skill's levelling and adding profesions for levels 15 and/or 20 is optional, VPP will be compatible with your skill mod even if you do none of these.

***If you DO want to add professions to your skill for levels 15 and 20***
- Increase the ``ExperienceLimits``'s length through SpaceCore API
- Add your extra professions as if you're adding regular level 5 or 10 professions, with the only difference being that the Level field needs to be 15 or 20.
- Relax because we all need rest and this isn't a bad thing.

There are also some guidelines to keep in mind that aren't necessarily technical, if you'd like to follow VPP's convention on your new professions:
- Level 15 professions are always a continuation of level 5 and 10 professions
- Level 20 professions are "combo" professions, which are special professions that do not care what you chose at level 15 and that are always a crossover of another skill and your skill. (or in VPP's case, two different vanilla skills).

***If you DO want to create a talent tree***
1) You will need access some of VPP's inner code and just copying the IVanillaPlusProfessions.cs will not work. This requires making VPP a hard dependency through your .csproj file, so making a seperate "bridge mod" is a preferable approach than adding it to the base .dll (similar to PFMAutomate, used pre-1.6 to make PFM (Producer Framework Mod) machinery compatible with Automate, which was the proper way to add custom machines back then.)
Copy this line to your .csproj file:
``<Reference Include="VanillaPlusProfessions" HintPath="$(GameModsPath)\VanillaPlusProfessions\VanillaPlusProfessions.dll" Private="False" />``

2) Register it through ``RegisterCustomSkillTree(string skillID, Func<string> displayTitle, List<Talent> talents, Texture2D treeTexture, Rectangle sourceRect, int bundleID = -1, Color? tintColor = null)`` method found in the VPP API: [``IVanillaPlusProfessions.cs``](https://github.com/KediDili/VanillaPlusProfessions/blob/main/Compatibility/IVanillaPlusProfessions.cs) file.

<br/><br/>

There are also a couple of guidelines to keep in mind that aren't necessarily technical:
- VPP awards talent points for every level up, including SpaceCore skills. This means there's at least ``[insert your level limit]`` talent points the player may gain just from your skill so it's highly recommended your talent tree has at least a few more than the limit, otherwise a player can max your talent tree and still be left with extra points and have nothing more to experience your tree, which reduces replayability for your mod, for VPP itself and for other skill mods. (For exp: VPP adds 25 talents for every vanilla skill, since the max limit is now 20 and not 10)
- Talents are much more powerful and accessible than professions, due to talents being immediately purchasable but professions have level limits. This means having a talent do an exact same thing as your professions will lead the player to prefer talents instead of your professions because they're just easier and basically kill your professions or enable min-maxing by trying to benefit from both at once.<br/><br/>TLDR; Do not make your talents do the same thing as your professions or your skill's other extra features.

### Skill mods that don't use SpaceCore's API to add their skills
Unfortunately I haven't got any feature that is for such type of mods, I am very sorry for any inconvenience this may cause.
