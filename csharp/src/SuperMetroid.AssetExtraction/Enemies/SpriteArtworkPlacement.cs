using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where a sprite look lands among the cartridge's OBJ characters (#1392, #1406, #1422, #1463): the look and the characters
/// it shows over, from its first. The import pipeline alone knows it; the pictures of frames drawn over those characters are
/// composed from it.
/// </summary>
/// <param name="Source">The look shown.</param>
/// <param name="FirstCharacter">The sprite character it shows from.</param>
/// <param name="CharacterCount">The characters it shows.</param>
public sealed record SpriteArtworkPlacement(SpriteArtworkSource Source, int FirstCharacter, int CharacterCount)
{
    /// <summary>The look shown.</summary>
    public SpriteArtworkSource Source { get; } = Ensure.NotNull(Source);

    /// <summary>The sprite character it shows from.</summary>
    public int FirstCharacter { get; } = Ensure.BetweenInclusive(0, SpriteCharacterTableLayout.CharacterCount - 1, FirstCharacter);

    /// <summary>The characters it shows.</summary>
    public int CharacterCount { get; } = Ensure.BetweenInclusive(1, SpriteCharacterTableLayout.CharacterCount - FirstCharacter, CharacterCount);

    /// <summary>The table byte the placement shows from.</summary>
    internal int FirstByte => FirstCharacter * SpriteCharacterTableLayout.CharacterByteCount;

    /// <summary>The table bytes the placement shows.</summary>
    internal int ByteCount => CharacterCount * SpriteCharacterTableLayout.CharacterByteCount;

    /// <summary>Whether the placement shows over every one of <paramref name="characterCount"/> characters from <paramref name="firstCharacter"/>.</summary>
    public bool Covers(int firstCharacter, int characterCount) =>
        FirstCharacter <= firstCharacter && firstCharacter + characterCount <= FirstCharacter + CharacterCount;
}
