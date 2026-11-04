using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where a verification replay of the cartridge's native transfers lands among the OBJ characters (#1392, #1513): the
/// artwork copied and the characters it shows over, from its first. The game and the import name no such placement; only
/// checks that replay native transfers keep one.
/// </summary>
/// <param name="Source">The artwork copied.</param>
/// <param name="FirstCharacter">The sprite character it shows from.</param>
/// <param name="CharacterCount">The characters it shows.</param>
public sealed record SpriteArtworkPlacement(VerificationArtworkSource Source, int FirstCharacter, int CharacterCount)
{
    /// <summary>The artwork copied.</summary>
    public VerificationArtworkSource Source { get; } = Ensure.NotNull(Source);

    /// <summary>The sprite character it shows from.</summary>
    public int FirstCharacter { get; } = Ensure.BetweenInclusive(0, SpriteCharacterTableLayout.CharacterCount - 1, FirstCharacter);

    /// <summary>The characters it shows.</summary>
    public int CharacterCount { get; } = Ensure.BetweenInclusive(1, SpriteCharacterTableLayout.CharacterCount - FirstCharacter, CharacterCount);

    /// <summary>The table byte the placement shows from.</summary>
    internal int FirstByte => FirstCharacter * SpriteCharacterTableLayout.CharacterByteCount;

    /// <summary>The table bytes the placement shows.</summary>
    internal int ByteCount => CharacterCount * SpriteCharacterTableLayout.CharacterByteCount;
}
