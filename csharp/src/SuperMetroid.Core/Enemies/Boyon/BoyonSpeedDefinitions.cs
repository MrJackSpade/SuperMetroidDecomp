namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>NTSC Boyon bounce curve and eight-bit hardware multiplication.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>Parameter one's low byte selects a speed multiplier.</summary>
    /// <remarks>
    /// #625 / #645 retained table: all eight multipliers
    /// are authored values. Values double
    /// through index 3, then rise by 2, 3, 3, and 4; a piecewise rounded formula
    /// would obscure these eight authored choices. The initializer selects indices
    /// 0..7 and rejects 8, which has no authored multiplier.
    /// VerifyCompiledBoyonSpeeds covers all eight real initializer selections.
    /// </remarks>
    internal static readonly int[] InitialMultipliers = [1, 2, 4, 8, 10, 13, 16, 20];

    /// <summary>The number of 8.8 heights parameter one's high byte selects from.</summary>
    internal const int InitialHeightCount = 9;
    /// <summary>The lowest 8.8 jump height, selected by high byte zero.</summary>
    internal const int LowestInitialHeight = 12288;
    /// <summary>How much higher each successive 8.8 jump height is.</summary>
    internal const int InitialHeightStep = 4096;

    /// <summary>The bounce clamps indices at or above 23 to 255 before multiplication.</summary>
    public const int StoredSampleCount = 23;
}
