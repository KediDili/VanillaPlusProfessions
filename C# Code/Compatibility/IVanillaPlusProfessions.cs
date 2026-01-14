using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using VanillaPlusProfessions.Talents.UI;

namespace VanillaPlusProfessions.Compatibility
{
    public interface IVanillaPlusProfessions
    {
        public enum MonsterType
        {
            Humanoid,
            Armoured,
            Flying,
            Ground
        }

        /// <summary>
        /// Registers a custom skill talent tree for a custom skill added via SpaceCore.
        /// </summary>
        /// <param name="skillID">Your custom skill's SpaceCore ID.</param>
        /// <param name="displayTitle">Your custom skill's translated title for the page.</param>
        /// <param name="treeTexture">The image for your custom skill tree's skill icon (displayed at bottom-right) and lines to draw to connect your talents. Width and height are strongly recommended to be 320 and 180, for ease of positioning.</param>
        /// <param name="sourceRect">The rectangle for the talent schema to display on your custom skill tree, width and height are strongly recommended to be 320 and 180.</param>
        /// <param name="bundleID">Which bundle animation you want your custom skill to have, all bundle shapes are on <c>LooseSprites\\JunimoNote</c>. Green one uses 0, dark blue uses 6.</param>
        /// <param name="tintColor">If you don't want to use a vanilla bundle type, fill this field with any color instance and VPP will use it to make custom colored icons.</param>
        public void RegisterCustomSkillTree(SkillTree tree);

        /// <summary>
        ///  A method to find out what professions a player has.
        /// </summary>
        /// <returns>A list of string names of the professions <paramref name="who"/> or current player has.</returns>
        /// <param name="who">The player to get the VPP professions of. If not filled, it'll default to the current player.</param>
        public IEnumerable<string> GetProfessionsForPlayer(Farmer who = null);

        /// <summary>
        /// A method to find out what talents a player has.
        /// </summary>
        /// <returns>A list of string names of the talents <paramref name="who"/> or current player has.</returns>
        /// <param name="who">The player to get the VPP professions of. If not filled, it'll default to the current player.</param>
        public IEnumerable<string> GetTalentsForPlayer(Farmer who = null);

        /// <summary>
        /// Registers a custom skill talent tree for a custom skill added via SpaceCore.
        /// </summary>
        /// <param name="monsterType">A <c>Type</c> instance of your custom monster.</param>
        /// <param name="isSlimy">Whether if this is a 'slimy' monster. Affects how will the Slimeshot talent apply to it.</param>
        /// <param name="type">The 'type' of your monster. Affects how will the Monster Specialist talent will apply to it</param>
        public void RegisterCustomMonster(Type monsterType, bool isSlimy, MonsterType type);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The asset path equivalent to the given key, otherwise null.</returns>
        /// <param name="key">The "Key" to provide the path of. The valid values are the same as <c>ContentPaths</c> CP token's, which is documented on GitHub compatibility document.</param>
        public string GetPathForAsset(string key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b">The spritebatch instance to use.</param>
        /// <param name="position">Where should the icon's top-left corner be?</param>
        /// <param name="profession">Which profession's icon should be drawn?</param>
        public void DrawProfessionIcon(SpriteBatch b, Vector2 position, int profession);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="who">Whose next tier professions do you need?</param>
        /// <param name="level">Which level's professions do you need? </param>
        /// <param name="skill">Which skill's professions do you need?</param>
        /// <returns>A list with two ID elements of VPP's next tier professions.</returns>
        /// <remarks>Since this method was originally for VPP's own purposes, it only will provide level 15 and 20's professions.</remarks>
        public List<int> GetNextTierProfessions(Farmer who, int level, string skill);

        /// <summary>
        /// Registers a method to run when the talent menu is closed and if any talent has been bought, disabled, enabled or refunded. It will exclude talents that change statuses that count while triggering methods. (ex: a talent was disabled but then enabled again, or a talent tree was refunded then some talents were bought again) Even if multiple talents you list are affected, your method will be run only once.
        /// </summary>
        /// <param name="talents">Internal names of the talents the <c>action</c> should be run for. If at least one is changed, your method will run. If the list is empty, it'll be run for every talent.</param>
        /// <param name="action">The method that should be run when any of these <c>talents</c> change 'status'. Dictionary's keys are the talent internal names and the values are the new 'statuses'.</param>
        /// <returns>Whether the method was successfully added.</returns>
        public bool RegisterTalentStatusAction(IEnumerable<string> talents, Action<Dictionary<string, string>> action);

        /// <summary>
        /// Property for VPP's own 'skill bars' it uses on skills-page (the longer ones that give profession pop-ups when you hover over them). It's already compatible with SpaceCore skills.
        /// </summary>
        public ClickableTextureComponent[] CustomSkillBars { get; }
 
        /// <summary>
        /// Exposes XP limits for VPP's new levels. Index 0 is total experience required for level 11.
        /// </summary>
        public int[] LevelExperiences { get; }

        /// <summary>
        /// The config value.
        /// </summary>
        public int MasteryCaveChanges { get; }

        /// <summary>
        /// The config value.
        /// </summary>
        public bool ColorBlindnessChanges { get; }

        /// <summary>
        /// The config value.
        /// </summary>
        public bool ProfessionsOnly { get; }

        /// <summary>
        /// The config value.
        /// </summary>
        public bool StaminaCostAdjustments { get; }
    }
}
