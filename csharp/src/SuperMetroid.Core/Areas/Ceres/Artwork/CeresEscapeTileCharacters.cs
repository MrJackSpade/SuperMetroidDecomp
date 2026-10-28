namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>A range of one installed Ceres escape character page, uploaded by the escape's transfer lists.</summary>
/// <param name="Page">The page: 0 for the warning text, 1 for the doors (<see cref="CeresEscapeTileArtworkDefinitions.All"/>).</param>
/// <param name="Offset">The first byte of the page uploaded.</param>
/// <param name="ByteCount">Bytes uploaded.</param>
internal sealed record CeresEscapeTileCharacters(int Page, int Offset, int ByteCount) : SpriteArtworkSource
{

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Ceres escape page {Page} +{Offset}, {ByteCount} bytes";
}
