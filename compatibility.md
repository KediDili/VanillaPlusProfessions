# Compatibility

**Warning: This guide might assume you're already familiar with JSON format, Content Patcher packs, Game State Queries and/or C# based mods depending on the part of the guide.**

## Content Packs (Mostly CP, though)
VPP offers two custom CP tokens and one custom GSQ for compatibility purposes at the moment.
The queries/tokens, and the formats are listed below:

| Token Name    | Details |
|:-------------:|:--------|
| ContentPaths  | Accepts one of ``ItemSpritesheet``, ``ProfessionIcons``, ``InsiderInfo``, ``TalentBG``, ``TalentSchema``, ``BundleIcons`` and ``SkillBars``.<br/><br/>``ItemSpritesheet`` will give you sprites of any item VPP adds, but it also contains a few misc things that don't fit anywhere else.<br/>``ProfessionIcons`` contains all and only profession icons meant to be used by VPP.<br/>``InsiderInfo`` is only meant to be used for compatibility with the InsiderInfo talent.<br/>``TalentBG`` has only the generic elements that's supposed to be in every talent tree. such as the backgrounds for icons, resetting items and talent point display.<br/>``TalentSchema`` contains an image consisting of all "lines" and an icon of all VPP base talent trees.<br/>``BundleIcons`` is only the colored smaller bundles and greyscale bundles only meant for VPP to use.<br/>``SkillBars`` are the skill bars from VPP's Color Blindness config and skill overlay.<br/> depending on the input, it'll return a path to be used in the Target field.
| HasProfession | Returns a list of the professions the farmer currently has. Best used in the When field. |

An example patch for both of them is written below:
```json
{
    "LogName": "Changing VPP's profession icons"
    "Action": "EditImage",
    "Target": "{{KediDili.VanillaPlusProfessions/ContentPaths:ProfessionIcons}}",
    "FromFile": "My_Assets/My_Icons.png",
}
```
```json
{
    "LogName": "Changing my custom ore's price depending on if the player has Ironmonger"
    "Action": "EditImage",
    "Target": "Data/Objects",
    "Fields": {
        "My_Custom_Ore": {
            "Price": 500
        }
    },
    "When": {
        "KediDili.VanillaPlusProfessions/HasProfession|contains= Ironmonger": "true",
    }
}
```

|                                Format                                  |                         Details                         |
|:----------------------------------------------------------------------:|:-------------------------------------------------------:|
| ``KediDili.VanillaPlusProfessions_WasRainingHereYesterday <location>`` | Checks if it was raining the said location YESTERDAY.<br/>This can be used for compatibility with talents such as Bountiful Boletes or Renewing Mist that need this check. |

## C# Mods
VPP offers an API ~~currently only to add talents which I'm going to completely revamp...~~

### Skill Mods by SpaceCore
VPP will recognize your custom skill for features like the skill overlay, but will NOT try to manage your professions or add new ones for levels 15 and 20.
You should add them yourself via SpaceCore's Skills API. Creating a talent tree is optional, VPP will be compatible with your skill mod even if you do not.

