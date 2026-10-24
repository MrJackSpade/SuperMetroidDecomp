using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One page of the installed Mother Brain escape text.</summary>
/// <param name="Page">The page.</param>
internal sealed record MotherBrainEscapeTextCharacters(int Page) : ObjCharacterSource
{
    /// <inheritdoc/>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver) =>
        (InstalledBossObjCharacterArtwork.Of(resolver).BossTiles.MotherBrainEscapeText ?? throw new InvalidDataException(
            "Installed enemy artwork is missing mother-brain-escape-text-tiles.png.")).Characters.Slice(
            checked((int)EscapeTextArtworkDefinitions.PageSource(Page)) - EscapeTextArtworkDefinitions.SourceAddress,
            EscapeTextArtworkDefinitions.PageByteCount(Page));

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Mother Brain escape text page {Page}";
}
