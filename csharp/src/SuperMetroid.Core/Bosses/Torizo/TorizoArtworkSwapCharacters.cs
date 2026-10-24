using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The installed characters one Torizo artwork swap shows.</summary>
/// <param name="Entry">The swap's <see cref="TorizoArtworkSwapDefinitions"/> entry.</param>
internal sealed record TorizoArtworkSwapCharacters(int Entry) : ObjCharacterSource
{
    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The Torizo artwork is not installed.</exception>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver)
    {
        TorizoArtworkSwapDefinition definition = TorizoArtworkSwapDefinitions.Entries[Entry];
        return (InstalledBossObjCharacterArtwork.Of(resolver).BossTiles.TorizoInstructionVram ?? throw new InvalidDataException(
                "Torizo artwork swaps require installed artwork."))
            .Characters(definition.Page, definition.PageByte, definition.ByteCount);
    }

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Torizo artwork swap {Entry}";
}
