using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Artwork bound over background characters (#1392, #1406, #1422): what it is, the set and first character it shows over
/// and how many characters it covers. A room's load binds its tileset this way, and extraction pictures each block over
/// each artwork that can show under it.
/// </summary>
/// <param name="Identity">The artwork bound.</param>
/// <param name="Set">The characters it shows over.</param>
/// <param name="FirstCharacter">The first character it shows at.</param>
/// <param name="CharacterCount">The characters it covers.</param>
public sealed record BgCharacterPlacement(BgArtworkIdentity Identity, BgCharacterSet Set, int FirstCharacter, int CharacterCount)
{
    /// <summary>Whether the artwork covers <paramref name="character"/> of <paramref name="set"/>.</summary>
    public bool Covers(BgCharacterSet set, int character) =>
        set == Set && character >= FirstCharacter && character < FirstCharacter + CharacterCount;

    /// <summary>What shows at <paramref name="character"/>, which the artwork covers: it, from that character's place in it.</summary>
    public BgArtworkShown ShownAt(int character) => new(Identity, (character - FirstCharacter) * Set.CharacterBytes() / sizeof(ushort));

    /// <summary>Binds the artwork.</summary>
    public void BindTo(BgArtworkBindings bindings) => Ensure.NotNull(bindings).Bind(Identity, Set, FirstCharacter, CharacterCount);
}
