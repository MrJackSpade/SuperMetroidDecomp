namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Cartridge values and helpers of <see cref="ChootFallingPathDefinitions"/> that only verification reads.</summary>
internal static class ChootFallingPathDefinitionsConstants
{
    /// <summary>Returns the exact physical sample selected by a native path pointer and frame.</summary>
    internal static ChootFallingPathPoint At(ushort pointer, int frameIndex) =>
        ChootFalls.At(ChootNativeWords.ChootFallingPaths.Decode(pointer, "Choot falling-pattern stream"), frameIndex);
}
