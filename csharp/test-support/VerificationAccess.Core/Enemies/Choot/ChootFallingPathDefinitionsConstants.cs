namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Cartridge values and helpers of <see cref="ChootFallingPathDefinitions"/> that only verification reads.</summary>
internal static class ChootFallingPathDefinitionsConstants
{
    /// <summary>Returns the loop's offsets of the path a native path pointer selects.</summary>
    internal static ReadOnlyMemory<Offset16<RoomPixels>> LoopOf(ushort pointer) =>
        ChootFalls.LoopOf(ChootNativeWords.ChootFallingPaths.Decode(pointer, "Choot falling-pattern stream"));
}
