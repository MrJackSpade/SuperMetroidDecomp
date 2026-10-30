namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Artwork the import pipeline composes background pictures from (#1422): what Core binds over background characters,
/// with the bytes the artwork holds. Core's bindings name the artwork and count its characters; the bytes stay here.
/// </summary>
/// <param name="Identity">The artwork bound.</param>
/// <param name="Set">The characters it shows over.</param>
/// <param name="FirstCharacter">The first character it shows at.</param>
/// <param name="Bytes">Its bytes, whole characters of the set.</param>
internal sealed record BgCharacterUpload(BgArtworkIdentity Identity, BgCharacterSet Set, int FirstCharacter, ReadOnlyMemory<byte> Bytes)
{
    /// <summary>The characters the artwork covers.</summary>
    /// <exception cref="InvalidDataException">The bytes are not whole characters of the set.</exception>
    internal int CharacterCount => Set.CharactersIn(Bytes.Length);

    /// <summary>The binding Core makes of the artwork: its name, place and character count.</summary>
    internal BgCharacterPlacement Placement => new(Identity, Set, FirstCharacter, CharacterCount);

    /// <summary>Whether the artwork covers <paramref name="character"/> of <paramref name="set"/>.</summary>
    internal bool Covers(BgCharacterSet set, int character) => Placement.Covers(set, character);

    /// <summary>What shows at <paramref name="character"/>, which the artwork covers: it, from that character's place in it.</summary>
    internal BgArtworkShown ShownAt(int character) => Placement.ShownAt(character);

    /// <summary>Binds the artwork.</summary>
    internal void BindTo(BgArtworkBindings bindings) => Placement.BindTo(bindings);
}
