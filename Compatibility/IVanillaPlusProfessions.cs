using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using VanillaPlusProfessions.Talents;

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
        public void RegisterCustomSkillTree(string skillID, Func<string> displayTitle, List<Talent> talents, Texture2D treeTexture, Rectangle sourceRect, int bundleID = -1, Color? tintColor = null);

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
        /// <param name="key">The "Key" to provide the path of.</param>
        public string GetPathForAsset(string key);

        public void DrawProfessionIcon(SpriteBatch b, Vector2 position, int profession);

        public List<int> GetNextTierProfessions(Farmer who, int level, string skill);

        public ClickableTextureComponent[] CustomSkillBars { get; }
 
        public int[] LevelExperiences { get; }

        public bool MasteryCaveChanges { get; }

        public bool ColorBlindnessChanges { get; }

        public bool ProfessionsOnly { get; }

        public bool StaminaCostAdjustments { get; }
    }
}
