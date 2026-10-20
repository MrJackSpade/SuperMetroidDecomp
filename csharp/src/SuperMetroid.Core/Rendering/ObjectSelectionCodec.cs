using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Decodes OBSEL's object size selection into the sizes a screen's objects draw at (#1358).</summary>
[PpuLayoutBoundary("codec: OBSEL bits 5-7, the small/large object size pair")]
public static class ObjectSelectionCodec
{
    /// <summary>The eight size pairs, by OBSEL bits 5-7.</summary>
    private static readonly (int SmallWidth, int SmallHeight, int LargeWidth, int LargeHeight)[] SizePairs =
    [
        (8, 8, 16, 16),
        (8, 8, 32, 32),
        (8, 8, 64, 64),
        (16, 16, 32, 32),
        (16, 16, 64, 64),
        (32, 32, 64, 64),
        (16, 32, 32, 64),
        (16, 32, 32, 32),
    ];

    /// <summary>The object sizes <paramref name="objectSelection"/> selects.</summary>
    public static ScreenObjectSizes Sizes(int objectSelection)
    {
        (int smallWidth, int smallHeight, int largeWidth, int largeHeight) = SizePairs[(checked((byte)objectSelection) >> 5) & 7];
        return new ScreenObjectSizes(new Extent16<ScreenPixels>((ushort)smallWidth, (ushort)smallHeight),
            new Extent16<ScreenPixels>((ushort)largeWidth, (ushort)largeHeight));
    }
}
