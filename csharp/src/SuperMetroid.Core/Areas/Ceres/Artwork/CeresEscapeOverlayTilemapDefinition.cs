namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One fixed visual tilemap source used during the Ceres escape warning.</summary>
/// <param name="Name">The page's installed name.</param>
/// <param name="SourceAddress">The page's cartridge source, which names its installed words.</param>
/// <param name="WordCount">The page's words.</param>
/// <param name="Cell">The BG1 overlay cell its first word is written at (#1406).</param>
internal readonly record struct CeresEscapeOverlayTilemapDefinition(
    string Name, int SourceAddress, int WordCount, int Cell);
