namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>NTSC Boyon bounce curve and native eight-bit hardware multiplication.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>$A2:86DF, BoyonData.speedMultipliers: parameter one's low byte selects a hardware multiplier.</summary>
    /// <remarks>
    /// #625 / #645 retained table: all eight unsigned words at $A2:86DF-$86EE
    /// match the pinned NTSC J/U v1.0 ROM and bank-A2 disassembly. Values double
    /// through index 3, then rise by 2, 3, 3, and 4; a piecewise rounded formula
    /// would obscure these eight authored choices. The initializer selects indices
    /// 0..7 and rejects 8, which would read the adjacent jump-height table.
    /// VerifyCompiledBoyonSpeeds covers all eight real initializer selections.
    /// </remarks>
    internal static readonly ushort[] InitialMultipliers = [1, 2, 4, 8, 10, 13, 16, 20];

    /// <summary>$A2:86EF, BoyonData.jumpHeights: the number of 8.8 heights parameter one's high byte selects from.</summary>
    internal const int InitialHeightCount = 9;
    /// <summary>The lowest 8.8 jump height, selected by high byte zero.</summary>
    internal const int LowestInitialHeight = 12288;
    /// <summary>How much higher each successive 8.8 jump height is.</summary>
    internal const int InitialHeightStep = 4096;

    /// <summary>$A2:875E/$880F/$885B clamp indices at or above 23 to $FF before multiplication.</summary>
    public const int StoredSampleCount = 23;
}
