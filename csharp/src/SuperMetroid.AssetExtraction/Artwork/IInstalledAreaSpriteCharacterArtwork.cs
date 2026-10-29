using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The installed area artwork that area sprite character sources name.</summary>
internal interface IInstalledAreaSpriteCharacterArtwork
{
    /// <summary>The installed area artwork.</summary>
    /// <exception cref="InvalidOperationException">No area artwork is installed.</exception>
    AreaSourceArtwork AreaTiles { get; }
}
