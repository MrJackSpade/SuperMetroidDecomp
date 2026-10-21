using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The installed characters one Torizo instruction-time tile transfer copies.</summary>
/// <param name="Entry">The transfer's <see cref="TorizoInstructionVramTransferDefinitions"/> entry.</param>
internal sealed record TorizoTileTransferCharacters(int Entry) : ObjCharacterSource
{
    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The installed Torizo artwork holds no characters for the transfer.</exception>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver)
    {
        TileTransferDefinition definition = TorizoInstructionVramTransferDefinitions.Entries[Entry];
        return (InstalledBossObjCharacterArtwork.Of(resolver).BossTiles.TorizoInstructionVram ?? throw new InvalidDataException(
                "Torizo instruction-time tile transfers require installed artwork."))
            .TryResolve(definition.SourceAddress, definition.ByteCount, out ReadOnlyMemory<byte> characters)
            ? characters
            : throw new InvalidDataException($"Torizo tile transfer {Entry} has no installed art.");
    }

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Torizo tile transfer {Entry}";
}
