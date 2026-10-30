namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Verification's record of which sprite look shows over which of the cartridge's OBJ characters (#1392, #1463, #1466): an
/// ordered record of placements, a later placement showing over an earlier one where they overlap. Checks replay the
/// cartridge's uploads and gameplay's shown looks through it; the import composes from the looks directly.
/// </summary>
public sealed class SpriteArtworkPlacements
{
    /// <inheritdoc cref="SpriteCharacterTableLayout.CharacterByteCount"/>
    internal const int CharacterByteCount = SpriteCharacterTableLayout.CharacterByteCount;

    /// <inheritdoc cref="SpriteCharacterTableLayout.CharacterCount"/>
    internal const int CharacterCount = SpriteCharacterTableLayout.CharacterCount;

    /// <inheritdoc cref="SpriteCharacterTableLayout.TableByteCount"/>
    internal const int TableByteCount = SpriteCharacterTableLayout.TableByteCount;

    /// <summary>The placements in order; a later one shows over an earlier one where they overlap.</summary>
    internal readonly List<SpriteArtworkPlacement> placements = [];
}
