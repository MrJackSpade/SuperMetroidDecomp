namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One fixed visual tilemap source used during the Ceres escape warning.</summary>
internal readonly record struct CeresEscapeOverlayTilemapDefinition(
    string Name, int SourceAddress, int WordCount);
