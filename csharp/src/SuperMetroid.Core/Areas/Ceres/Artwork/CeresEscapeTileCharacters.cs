namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>A range of one installed Ceres escape character page, uploaded by the escape's transfer lists.</summary>
/// <param name="Page">The page: 0 for the warning text, 1 for the doors (<see cref="CeresEscapeTileArtworkDefinitions.All"/>).</param>
/// <param name="Offset">The first byte of the page uploaded.</param>
/// <param name="ByteCount">Bytes uploaded.</param>
internal sealed record CeresEscapeTileCharacters(int Page, int Offset, int ByteCount) : ObjCharacterSource
{

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The installed Ceres escape artwork holds no such page.</exception>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver) =>
        (InstalledAreaSpriteCharacterArtwork.Of(resolver).AreaTiles.CeresEscapeTiles ?? throw new InvalidDataException(
            "Installed Ceres escape has no editable character pages."))
        .TryResolve(Page, Offset, ByteCount, out ReadOnlyMemory<byte> characters)
            ? characters
            : throw new InvalidDataException($"Installed Ceres escape page {Page} has no {ByteCount} bytes at {Offset}.");

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Ceres escape page {Page} +{Offset}, {ByteCount} bytes";
}
