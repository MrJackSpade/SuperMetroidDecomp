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

    /// <summary>The object selection gameplay draws its sprites with: small sprites are 8 pixels and large ones 16, drawn from the gameplay sprite tiles.</summary>
    private const int GameplayObjectSelection = 3;

    /// <summary>The object sizes gameplay draws its sprites at.</summary>
    public static ScreenObjectSizes GameplaySizes => Sizes(GameplayObjectSelection);

    /// <summary>The object sizes <paramref name="objectSelection"/> selects.</summary>
    public static ScreenObjectSizes Sizes(int objectSelection)
    {
        return SizePairs[(checked((byte)objectSelection) >> 5) & 7];
    }

    /// <summary>The placement of the object characters, decoded from the selection byte (#2563).</summary>
    /// <param name="objectSelection">The object-selection byte, 0 through 255.</param>
    /// <returns>
    /// The first name table's base, in video words (bits 0-2 in $2000-word units), and the second name table's
    /// displacement past it, in video words (bits 3-4 plus one, in $1000-word units).
    /// </returns>
    public static (int NameBaseWord, int SecondTableDisplacementWords) CharacterPlacement(int objectSelection)
    {
        int selection = checked((byte)objectSelection);
        return ((selection & 7) << 13, (((selection >> 3) & 3) + 1) << 12);
    }
}
