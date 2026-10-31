namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Native OBJ tile anchors shared by the ending reward arm poses.</summary>
internal static class EndingRewardArmCellDefinitions
{
    /// <summary>Tile$EE, initial pose's lower large arm piece.</summary>
    internal const int InitialLower = 238;
    /// <summary>Tile$E9, initial pose's upper large arm piece.</summary>
    internal const int InitialUpper = 233;
    /// <summary>Tile$10D, top small piece of the split pose; next row$11D is below it.</summary>
    internal const int SplitSmallTop = 269;
    /// <summary>Tile$10E, large piece adjacent to the split pose's small column.</summary>
    internal const int SplitLarge = 270;
    /// <summary>Tile$109, first upper piece in the paired variants; lower piece
    /// is two rows later and the second variant is two columns to the right.</summary>
    internal const int PairedUpper = 265;
    /// <summary>Tile$149, left small shoulder cap; adjacent$14A is the right cap
    /// and the next atlas row$159 is the large shoulder body.</summary>
    internal const int ShoulderCap = 329;
}
