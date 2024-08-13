using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        /// <param name="rectangle">The rectangle for the talent schema to display on your custom skill tree, width and height are required to be 320 and 180.</param>
        /// <remarks>Note: You must first register all of your talents using <see cref="RegisterCustomSkillTalent(string, string, Func{string,string}, Func{string,string}, Vector2, string[], int)"/>, then call this method afterwards.</remarks>
        public void RegisterCustomSkillTree(string skillID, Func<string> displayTitle, Texture2D treeTexture, Rectangle rectangle, int bundleID = -1,  Color? tintColor = null);

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
        /// <remarks>Note: You must call this method for registering all of your talents first, then call <see cref="RegisterCustomSkillTree(string, Func{string}, Texture2D, Rectangle, int, Color?)"/> to initialize your SkillTree without bugs.</remarks>
        public void RegisterCustomSkillTalent(string skillID, string name, Func<string, string> displayName, Func<string, string> tooltip, Vector2 position, string[] requirements, int amountToBuyFirst = 0);

        /// <summary>
        /// Constructs a skill tree of talents.
        /// </summary>
      /*  public class SkillTree
        {
            /// <summary>
            /// The ID of this tree's skill. This is expected to be your SpaceCore skill ID.
            /// </summary>
            public string SkillID;

            /// <summary>
            /// The texture you want to use, it should contain both of your branch lines and skill icon.
            /// </summary>
            public Texture2D TreeTexture;

            /// <summary>
            /// Source rect to use on <see cref="TreeTexture"/> while drawing the skill tree. For ease of positioning, width and height should be 320 and 180.
            /// </summary>
            public Rectangle Rectangle;

            /// <summary>
            /// The talents this tree should own.
            /// </summary>
            public List<Talent> Talents;

            /// <summary>
            /// The color code that you want to use for your tree, must be set if <see cref="BundleID"/> isn't set.
            /// </summary>
            public Color? BundleColor = null;

            /// <summary>
            /// 
            /// </summary>
            public int BundleID = -1;

            public class Talent
            {
                /// <summary>
                /// The ID of this tree's skill. This is expected to be your SpaceCore skill ID.
                /// </summary>
                public string SkillID;
                
                /// <summary>
                /// Internal name of your talent. It must be unique among all talents.
                /// </summary>
                public string Name;

                /// <summary>
                /// A function that returns a display name for the talent.
                /// </summary>
                public Func<string, string> DisplayName;

                /// <summary>
                /// A function that returns a translated tooltip for the talent.
                /// </summary>
                public Func<string, string> Tooltip;

                /// <summary>
                /// 
                /// </summary>
                public string MailFlag;

                /// <summary>
                /// The position for where this talent should appear on talent selection menu. For ease of positioning, this will be multiplied with 4.
                /// </summary>
                public Vector2 Position;

                /// <summary>
                /// 
                /// </summary>
                public Branch[] Branches = Array.Empty<Branch>();

                /// <summary>
                /// Required talents for this talent to be unlocked and purchasable. Keep in mind it is always an 'or', and never an 'and'. Elements should be match to their <see cref="Name"/> field.
                /// </summary>
                public string[] Requirements = Array.Empty<string>();

                /// <summary>
                /// Locks this talent behind buying specified number of talents from its own tree, regardless of what they are named.
                /// </summary>
                public int AmountToBuyFirst = 0;

                /// <summary>
                /// 
                /// </summary>
                public class Branch
                {
                    public string Name;

                    public Func<string, string> displayName;

                    public Func<string, string> tooltip;

                    public string MailFlag;
                }
            }
        }*/
    }
}
