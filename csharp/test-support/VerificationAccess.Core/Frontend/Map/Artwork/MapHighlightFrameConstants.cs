namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Cartridge values and helpers of <see cref="MapHighlightFrame"/> that only verification reads.</summary>
internal static class MapHighlightFrameConstants
{
    /// <summary>The cycle's first frame, which every cycle holds.</summary>
    public static MapHighlightFrame First { get; } = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Frontend.Map.Artwork.MapHighlightFrame>([typeof(int)], new object?[] { 0 });
}
