using System;
using Microsoft.Xna.Framework;

namespace VanillaPlusProfessions.Talents
{
    public class Talent
    {
        //Internal name of the talent
        public string Name = "";

        //Skill ID of the talent - mostly intended for VPP's own talents but you can use it too if you want to make a giant .json file that contains all the talent data
        public string Skill = "";

        //Internal name(s) of the talents that at least one of them should be purchased before this one is unlocked.
        //DO NOT NULLIFY! Nullifying will cause a lot of errors. If omitted, AmountToBuyFirst will be checked or the talent will be unlocked immediately.
        public string[] Requirements = Array.Empty<string>();

        //Amount of talents **from the tree it is in** before this talent is unlocked. Ignored if Requirements field is filled.
        public int AmountToBuyFirst = 0;

        //The mail flag given to player immediately when this talent is bought.
        public string MailFlag = "";

        //Branch data to make the player choose only one. This is for talents that may affect a wide variety of things but need specialization to be balanced (hence choosing ONE branch). If you're stuck anything, check VPP's MonsterSpecialist talent.
        public Branch[] Branches = null;

        //Translatable name of this talent
        public Func<string, string> DisplayName = null;

        //Translatable description of this talent
        public Func<string, string> Description = null;

        //X and Y coordinates on Talent Menu for this talent to be put on. MULTIPLIED WITH 4 BEFORE USE. So if your coords are (100, 100), VPP will use it as (400, 400).
        public Vector2 Position = new();

        //Method ran when the talent is purchased. If you need to do special operations on addition, this is the field for it.
        //The string is the internal name of the talent, the bool parameter is whether it has been removed or added.
        public Action<string, bool> OnTalentAdded = null;

        //Method ran when the talent is refunded. If you need to do special operations on removal, this is the field for it.
        //The string is the internal name of the talent, the bool parameter is whether it has been removed or added.
        public Action<string, bool> OnTalentRemoved = null;

        public class Branch
        {
            //Internal name of the branch
            public string Name;

            //Translated name of the branch
            public Func<string, string> DisplayName;

            //Translated description of the branch
            public Func<string, string> Desc;

            //The mail flag immediately added upon choosing this branch
            public string Flag;

            //The position on the menu (No multiplying with 4)
            public Vector2 Position;
        }
    }
}