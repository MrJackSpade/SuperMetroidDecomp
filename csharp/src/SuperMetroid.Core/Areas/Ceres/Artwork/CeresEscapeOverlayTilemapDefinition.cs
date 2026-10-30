namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One fixed visual tilemap source used during the Ceres escape warning.</summary>
/// <param name="Name">The page's installed name, which names its installed words.</param>
/// <param name="WordCount">The page's words.</param>
/// <param name="Cell">The BG1 overlay cell its first word is written at (#1406).</param>
internal readonly record struct CeresEscapeOverlayTilemapDefinition(
    string Name, int WordCount, int Cell)
{
    /// <summary>
    /// The name of the page's picture in the room's written picture sheet (#1466): the getaway writes the picture by it,
    /// and extraction pictures the page's words under it.
    /// </summary>
    internal string Picture => $"CeresWarning.{Name}";
}
