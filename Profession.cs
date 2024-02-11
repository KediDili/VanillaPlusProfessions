namespace VanillaPlusProfessions
{
    public class Profession
    {
        //public string Name; //Unique name, not translated

        public int ID; //Unique name, not translated

        public int Requires = -2; //Vanilla profession, should be ignored for combo professions

        public int Skill = -1;

        public int FirstRequires
        {
            get
            {
                if (LevelRequirement is not 20)
                {
                    return Skill is 0 ? Requires is 2 or 3 ? 0 : 1 : Skill is 1 ? Requires is 8 or 9 ? 6 : 7 : Skill is 2 ? Requires is 14 or 15 ? 12 : 13 : Skill is 3 ? Requires is 20 or 21 ? 18 : 19 : Requires is 26 or 27 ? 24 : 25;
                }
                else
                {
                    return -2;
                }
            }        
        }

        public int LevelRequirement => Requires is -2 ? 20 : 15;
    }
}
