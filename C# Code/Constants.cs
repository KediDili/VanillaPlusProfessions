using System.Collections.Generic;

namespace VanillaPlusProfessions
{
    public static class Constants
    {
        public const string Key_HasFoundForage = "Kedi.VPP.HasFoundForageGame";
        public const string Key_DaysLeftForForageGuess = "Kedi.VPP.GuessForageGameDaysLeft";
        public const string Key_ForageGuessItemID = "Kedi.VPP.GuessForageID";

        public const string Key_TFTapperDaysLeft = "Kedi.VPP.ProduceDaysLeft";
        public const string Key_TFTapperID = "Kedi.VPP.TapperID";
        public const string Key_TFHasTapper = "Kedi.VPP.IsTapped";
        public const string Key_FruitTreeOrGiantCrop = "Kedi.VPP.DoesUseCustomItem";

        public const string Key_IsSlimeHutchWatered = "Kedi.VPP.IsWatered";
        public const string Key_SlimeWateredDaysSince = "Kedi.VPP.SlimeWateredDaysSince";
        public const string Key_FishRewardOrQuestDayLeft = "Kedi.VPP.FishQuestOrRewardDuration";

        public const string GlobalInventoryID_RingTrinkets = "KediDili.VanillaPlusProfessions-RingTrinkets";
        public const string GlobalInventoryId_Minecarts = "KediDili.VanillaPlusProfessions-Minecarts";
        public const string Key_RingTrinkets = "Kedi.VPP.RingTrinketId";

        public const string Key_IsLavaLocation = "KediDili.VanillaPlusProfessions/IsLavaLocation";
        public const string Key_IsConsistentMineLocation = "KediDili.VanillaPlusProfessions/IsConsistentMineLocation";
        public const string Key_LastInput = "KediDili.VanillaPlusProfessions/LastInput";
        public const string Key_NodeMakerData = "KediDili.VanillaPlusProfessions/NodeMakerData";
        public const string Key_IsLavaLocation2 = "KediDili.VanillaPlusProfessions_IsLavaLocation";
        public const string Key_IsConsistentMineLocation2 = "KediDili.VanillaPlusProfessions_IsConsistentMineLocation";
        public const string Key_LastInput2 = "KediDili.VanillaPlusProfessions_LastInput";
        public const string Key_NodeMakerData2 = "KediDili.VanillaPlusProfessions_NodeMakerData";

        public const string Key_TalentPoints = "Kedi.VPP.TalentPointCount";
        public const string Key_PointsCalculated = "Kedi.VPP.TalentPointsCalculated";
        public const string Key_DisabledTalents = "Kedi.VPP.DisabledTalents";

        public const string Key_XrayDrop = "Kedi.VPP.XrayDrop";
        public const string Key_AccessoriseRing = "Kedi.VPP.AccessoriseRing";
        public const string Key_CrystalCavern = "Kedi.VPP.CrystalCavern";
        public const string Key_Upheaval = "Kedi.VPP.Upheaval";
        public const string Key_SharedFocus = "Kedi.VPP.SharedFocus";
        public const string Key_Resurgence = "Kedi.VPP.Resurgence";
        public const string Key_SlowerSliming = "Kedi.VPP.SlowerSliming";
        public const string Key_FaeBlessings = "Kedi.VPP.FaeBlessings";
        public const string Key_LocalKnowledge = "Kedi.VPP.LocalKnowledge";
        public const string Key_WasRainingHere = "Kedi.VPP.WasRainingHere";
        public const string Key_DownInTheDepths = "Kedi.VPP.DownInTheDepths";
        public const string Key_WildGrowth = "Kedi.VPP.WildGrowth";
        public const string Key_StaticCharge = "Kedi.VPP.StaticCharge";
        public const string Key_Reforestation = "Kedi.VPP.Reforestation";
        public const string Key_Flurry = "Kedi.VPP.Flurry";
        public const string Key_RendingStrike = "Kedi.VPP.RendingStrike";
        public const string Key_HiddenBenefit_FairyBox = "Kedi.VPP.HiddenBenefit";
        public const string Key_HiddenBenefit_Crop = "Kedi.VPP.FairyBox";
        public const string Key_HiddenBenefit_FrogEggs = "Kedi.VPP.FrogEgg";
        public const string Key_ElderScrolls = "Kedi.VPP.ElderScrolls";

        public const string ContextTag_PoisonousMushroom = "Kedi_VPP_Poisonous_Mushroom";
        public const string ContextTag_BlandStone = "Kedi_VPP_Bland_Stone_Node";
        public const string ContextTag_SurvivalCooking = "kedi_vpp_survival_cooking_food";
        public const string ContextTag_Matryoshka_Banned_FromDropping = "kedi_vpp_banned_from_dropping";
        public const string ContextTag_Matryoshka_Banned_FromBeingDropped = "kedi_vpp_banned_from_being_dropped";
        public const string ContextTag_Banned_Node = "kedi_vpp_banned_node";
        public const string ContextTag_Banned_NatureSecrets = "kedi_vpp_banned_naturesecrets";
        public const string ContextTag_Banned_Adventurer = "kedi_vpp_banned_adventurer";
        public const string ContextTag_Banned_Ranger = "kedi_vpp_banned_ranger";
        public const string ContextTag_Banned_WildTotem = "kedi_vpp_banned_wildtotem";

        public const string Key_ResourceClumpName = "KediDili.VanillaPlusProfessions/ResourceClumpName";
        public const string Key_MossyFertilizer = "KediDili.VanillaPlusProfessions/MossyFertilizer";
        public const string Key_WildTotem = "KediDili.VanillaPlusProfessions/WildTotem";
        public const string Key_BirdFeederTime = "KediDili.VanillaPlusProfessions_BirdFeederTime";
        public const string Key_VPPDeluxeForage = "KediDili.VanillaPlusProfessions/DeluxeForage";

        public readonly static string[] GemNodes = new string[] { "2", "4", "6", "8", "10", "12", "14" };
        public readonly static string[] GeodeNodes = new string[] { "75", "76", "77" };
        public readonly static string[] OreNodes = new string[] { "290", "751", "764", "765", "95" };
        public readonly static string[] BlandStones = new string[] { "32", "668", "670", "34", "36", "38", "40", "42", "48", "50", "52", "54", "56", "58", "343", "450", "760", "845", "846", "844" };

        public readonly static string[] ValidTalentStatuses = new string[] { "Enabled", "Refunded", "Disabled" };

        public const string Id_MineralCavern = "KediDili.VPPData.CP_MineralCavern";
        public const string Id_SnowTotem = "KediDili.VPPData.CP_SnowTotem";
        public const string Id_SunTotem = "KediDili.VPPData.CP_SunTotem";
        public const string Id_WildTotem = "KediDili.VPPData.CP_WildTotem";
        public const string Id_MossyFertilizer = "KediDili.VPPData.CP_MossyFertilizer";
        public const string Id_FruitSyrup = "Kedi.VPP.FruitSyrup";
        public const string Id_GemDust = "Kedi.VPP.GemDust";
        public const string Id_BeeHouse = "(BC)10";
        public const string Id_BirdFeeder = "(BC)KediDili.VPPData.CP_BirdFeeder";

        public readonly static List<string> VoidButterfly_Locations = new() { "WitchSwamp", "BugLair", "Sewers", "PirateCove", "Railroad", "BusTunnel", "UndergroundMines121", "Caldera" };
        public readonly static List<string> Fertilizer_Ids = new() { "(O)368", "(O)369", "(O)919", "(O)370", "(O)371", "(O)920", "(O)465", "(O)466", "(O)918" };
        public readonly static List<List<string>> Fertilizer_Color_Tags = new() {
            new() { "color_orange", "color_yellow", "color_brown", "color_sand", "color_poppyseed", "color_dark_orange", "color_dark_yellow", "color_gold", "color_copper" },
            new() { "color_blue", "color_cyan", "color_aquamarine", "color_light_cyan", "color_dark_blue", "color_dark_cyan", "color_white", "color_gray" },
            new() { "color_red", "color_purple", "color_pink", "color_pale_violet_red", "color_salmon", "color_dark_red", "color_dark_purple", "color_dark_pink", "color_iridium" },
            new() { "color_green", "color_sea_green", "color_lime", "color_yellow_green", "color_jade", "color_dark_green", "color_black", "color_dark_brown" }
        };
    }
}
