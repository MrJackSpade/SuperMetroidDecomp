namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Cartridge values and helpers of <see cref="ChootFallingPathDefinitions"/> that only verification reads.</summary>
internal static class ChootFallingPathDefinitionsConstants
{
    /// <summary>Returns the offset selected by a native path pointer and frame, null at the loop's end.</summary>
    internal static Offset16<RoomPixels>? At(ushort pointer, int frameIndex) =>
        ChootFalls.At(ChootNativeWords.ChootFallingPaths.Decode(pointer, "Choot falling-pattern stream"), frameIndex);
}
