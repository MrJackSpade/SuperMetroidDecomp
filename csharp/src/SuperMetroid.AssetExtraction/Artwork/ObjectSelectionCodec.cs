namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Decodes OBSEL's object size selection into the sizes a screen's objects draw at (#1358).</summary>
internal static class ObjectSelectionCodec
{
    /// <summary>The eight size pairs, by OBSEL bits 5-7.</summary>
    private static readonly ScreenObjectSizes[] SizePairs =
    [
        new(new(8, 8), new(16, 16)),
        new(new(8, 8), new(32, 32)),
        new(new(8, 8), new(64, 64)),
        new(new(16, 16), new(32, 32)),
        new(new(16, 16), new(64, 64)),
        new(new(32, 32), new(64, 64)),
        new(new(16, 32), new(32, 64)),
        new(new(16, 32), new(32, 32)),
    ];

    /// <summary>The object sizes <paramref name="objectSelection"/> selects.</summary>
    public static ScreenObjectSizes Sizes(int objectSelection)
    {
        return SizePairs[(checked((byte)objectSelection) >> 5) & 7];
    }
}
