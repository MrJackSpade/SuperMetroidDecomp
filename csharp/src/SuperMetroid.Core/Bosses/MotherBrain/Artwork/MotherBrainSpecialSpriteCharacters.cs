using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Pages of one installed Mother Brain special sprite sheet (legs, Baby, attack, exploded door).</summary>
/// <param name="Sheet">The sheet's index in <see cref="MotherBrainSpecialSpriteArtworkDefinitions.All"/>.</param>
/// <param name="Page">The first page uploaded.</param>
/// <param name="ByteCount">Bytes uploaded.</param>
internal sealed record MotherBrainSpecialSpriteCharacters(int Sheet, int Page, int ByteCount) : ObjCharacterSource
{
    /// <inheritdoc/>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver)
    {
        MotherBrainSpecialSpriteSheetDefinition sheet = MotherBrainSpecialSpriteArtworkDefinitions.All[Sheet];
        MotherBrainSpecialSpriteArtworkCatalog artwork = InstalledBossObjCharacterArtwork.Of(resolver).BossTiles.MotherBrainSpecialSprites ??
            throw new InvalidDataException($"Installed enemy artwork is missing {sheet.FileName}.");
        return artwork.Get(sheet.SourceAddress).Characters.Slice(Page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount, ByteCount);
    }

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"{MotherBrainSpecialSpriteArtworkDefinitions.All[Sheet].FileName} page {Page}, {ByteCount} bytes";
}
