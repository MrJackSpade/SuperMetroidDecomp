using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One page of the installed Mother Brain corpse tiles.</summary>
/// <param name="Page">The page.</param>
internal sealed record MotherBrainCorpseCharacters(int Page) : ObjCharacterSource
{
    /// <inheritdoc/>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver) =>
        (InstalledBossObjCharacterArtwork.Of(resolver).BossTiles.MotherBrainCorpse ?? throw new InvalidDataException(
            "Installed enemy artwork is missing mother-brain-corpse-tiles.png.")).Transfer.Slice(
            checked((int)MotherBrainCorpseArtworkDefinitions.PageSource(Page)) - MotherBrainCorpseArtworkDefinitions.SourceAddress,
            MotherBrainCorpseArtworkDefinitions.PageByteCount);

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Mother Brain corpse page {Page}";
}
