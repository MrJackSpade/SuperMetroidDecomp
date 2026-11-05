namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One fixed visual tilemap source used during the Ceres escape warning.</summary>
/// <param name="Name">The page's installed name, which names its installed words.</param>
/// <param name="Cell">The foreground cell its top-left corner is written at (#1406, #1514).</param>
internal readonly record struct CeresEscapeOverlayTilemapDefinition(
    string Name, Position16<TilemapCells> Cell)
{
    /// <summary>
    /// The name of the page's picture in the room's written picture sheet (#1466): the getaway writes the picture by it,
    /// and extraction pictures the page's words under it.
    /// </summary>
    internal string Picture => $"CeresWarning.{Name}";
}
