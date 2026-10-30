namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The cartridge's gameplay sprite character table, which the import composes sprite looks over (#1392, #1466).</summary>
public static class SpriteCharacterTableLayout
{
    /// <summary>Bytes one sprite character of installed artwork holds, the unit the artwork definitions count in.</summary>
    public const int CharacterByteCount = 32;

    /// <summary>The gameplay sprite characters artwork can be placed over.</summary>
    public const int CharacterCount = 512;

    /// <summary>The bytes every sprite character holds together.</summary>
    internal const int TableByteCount = CharacterCount * CharacterByteCount;
}
