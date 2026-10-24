namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A set of background characters a layer reads (#1406): what a tile word's character number names. Artwork binds over a
/// set's characters, and a layer's pictures name what is bound under the characters they read.
/// </summary>
public enum BgCharacterSet
{
    /// <summary>
    /// The characters BG1's and BG2's blocks and tile words name: the room tileset's, the common room elements', the
    /// collectibles' and what bosses bind among them.
    /// </summary>
    Tiles,

    /// <summary>The characters layer 3 names: the HUD's, the minimap's and the room effects' glyphs.</summary>
    Layer3,

    /// <summary>
    /// The copy of layer 3's characters Kraid's library background makes, which layer 3 reads while his body covers the
    /// standard ones.
    /// </summary>
    KraidLayer3,
}

/// <summary>The extent of each background character set (#1406).</summary>
public static class BgCharacterSets
{
    /// <summary>The bytes one character of <paramref name="set"/> holds: 32 for four-bit characters, 16 for two-bit.</summary>
    public static int CharacterBytes(this BgCharacterSet set) => set switch
    {
        BgCharacterSet.Tiles => 32,
        BgCharacterSet.Layer3 or BgCharacterSet.KraidLayer3 => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(set), set, "Unknown background character set."),
    };

    /// <summary>The characters <paramref name="set"/> holds.</summary>
    public static int CharacterCount(this BgCharacterSet set) => set switch
    {
        BgCharacterSet.Tiles => 1024,
        BgCharacterSet.Layer3 or BgCharacterSet.KraidLayer3 => 256,
        _ => throw new ArgumentOutOfRangeException(nameof(set), set, "Unknown background character set."),
    };
}
