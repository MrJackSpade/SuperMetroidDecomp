namespace SuperMetroid.AssetExtraction.Video;

/// <summary>The extent of each background character set (#1406).</summary>
public static class BgCharacterSets
{
    /// <summary>The bytes one character of <paramref name="set"/> holds: 32 for four-bit characters, 16 for two-bit.</summary>
    public static int CharacterBytes(this BgCharacterSet set) => set switch
    {
        BgCharacterSet.Tiles => 32,
        BgCharacterSet.Layer3 => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(set), set, "Unknown background character set."),
    };

    /// <summary>The whole characters of <paramref name="set"/> that <paramref name="byteCount"/> bytes hold.</summary>
    /// <exception cref="InvalidDataException">The bytes are not whole characters of the set.</exception>
    public static int CharactersIn(this BgCharacterSet set, int byteCount) => byteCount % set.CharacterBytes() == 0
        ? byteCount / set.CharacterBytes()
        : throw new InvalidDataException($"{byteCount} bytes are not whole {set} characters.");

    /// <summary>The characters <paramref name="set"/> holds.</summary>
    public static int CharacterCount(this BgCharacterSet set) => set switch
    {
        BgCharacterSet.Tiles => 1024,
        BgCharacterSet.Layer3 => 256,
        _ => throw new ArgumentOutOfRangeException(nameof(set), set, "Unknown background character set."),
    };
}
