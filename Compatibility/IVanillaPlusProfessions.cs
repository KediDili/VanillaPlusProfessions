using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace VanillaPlusProfessions.Compatibility
{
    public interface IVanillaPlusProfessions
    {
        /// <summary>
        /// Registers a custom skill talent tree for a custom skill added via SpaceCore.
        /// </summary>
        /// <param name="skillID">Your custom skill's SpaceCore ID.</param>
        /// <param name="displayTitle">Your custom skill's translated title for the page.</param>
        /// <param name="bundleID">Which bundle animation you want your custom skill to have, all bundle shapes are on <c>LooseSprites\\JunimoNote</c>. Green one uses 0, dark blue uses 6.</param>
        /// <param name="treeTexture">The line image for your custom skill tree. The exact dimensions should be 320x180, for ease of positioning.</param>
        /// <param name="rectangle">The rectangle for the 32x32 icon to display onyour custom skill tree, which is required to be on JunimoNote.</param>
        /// <remarks>Note: You must first register all of your talents using <see cref="RegisterCustomSkillTalent(string, string, Func{string}, Func{string}, Vector2, string, string)"/>, then call this method afterwards.</remarks>
        public void RegisterCustomSkillTree(string skillID, Func<string> displayTitle, int bundleID, Texture2D treeTexture, Rectangle rectangle, int lockedID);

        /// <summary>
        /// Registers a custom skill talent for a custom skill added via SpaceCore.
        /// </summary>
        /// <param name="skillID">Your custom skill's SpaceCore ID.</param>
        /// <param name="name">The internal name of your talent. Make sure this is unique amongst all mods.</param>
        /// <param name="displayName">Function for your talent's translated name.</param>
        /// <param name="tooltip">Function for your talent's translated description.</param>
        /// <param name="requiresTalent1">First other talent your current talent requires, do not nullify if you wont use it, use <c>string.Empty</c> instead.</param>
        /// <param name="requiresTalent2">Second other talent your current talent requires, do not nullify if you wont use it, use <c>string.Empty</c> instead. This is ignored if <paramref name="requiresTalent1"/> is empty.</param>
        /// <param name="position">Where should the talent's button be on its tree, keep in mind X and Y of the vector will be multiplied with 4, for ease of positioning. The button will automatically be created by this mod.</param>
        /// <remarks>Note: You must call this method for registering all of your talents first, then call <see cref="RegisterCustomSkillTree(string, Func{string}, int, Texture2D, Rectangle, int)"/> to initialize your SkillTree without bugs.</remarks>
        public void RegisterCustomSkillTalent(string skillID, string name, Func<string> displayName, Func<string> tooltip, Vector2 position, string requiresTalent1 = "", string requiresTalent2 = "");

        /// <summary>
        /// Method for checking whether a specified player has gained the named talent.
        /// </summary>
        /// <param name="name">The internal talent name to be checked.</param>
        /// <param name="farmerID">The farmer multiplayer ID to have whether if they have the talent, if applicable.</param>
        /// <param name="who">The Farmer instance to have whether if they have the talent, if applicable.</param>
        /// <returns>Whether the specified or Game1.player has gained the named talent.</returns>
        public bool IsTalentGained(string name, long farmerID = -1, Farmer who = null);
    }
}
