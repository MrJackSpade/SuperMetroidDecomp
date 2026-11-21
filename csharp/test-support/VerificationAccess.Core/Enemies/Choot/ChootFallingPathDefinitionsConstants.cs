namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Cartridge values and helpers of <see cref="ChootFallingPathDefinitions"/> that only verification reads.</summary>
internal static class ChootFallingPathDefinitionsConstants
{
<<<<<<< ours
    /// <summary>Returns the offset selected by a native path pointer and frame, null at the loop's end.</summary>
    internal static Offset16<RoomPixels>? At(ushort pointer, int frameIndex) =>
        ChootFallLoopAccess.OffsetAt(ChootNativeWords.ChootFallingPaths.Decode(pointer, "Choot falling-pattern stream"), frameIndex);
||||||| base
    /// <summary>Returns the offset selected by a native path pointer and frame, null at the loop's end.</summary>
    internal static Offset16<RoomPixels>? At(ushort pointer, int frameIndex) =>
        ChootFalls.At(ChootNativeWords.ChootFallingPaths.Decode(pointer, "Choot falling-pattern stream"), frameIndex);
=======
    /// <summary>Returns the loop's offsets of the path a native path pointer selects.</summary>
    internal static ReadOnlyMemory<Offset16<RoomPixels>> LoopOf(ushort pointer) =>
        ChootFalls.LoopOf(ChootNativeWords.ChootFallingPaths.Decode(pointer, "Choot falling-pattern stream"));
>>>>>>> theirs
}
