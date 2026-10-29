using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The installed gameplay artwork that domain sprite character sources name.</summary>
internal interface IInstalledSpriteCharacterArtwork
{
    /// <summary>The installed enemy and boss tile artwork.</summary>
    /// <exception cref="InvalidOperationException">No enemy artwork is installed.</exception>
    EnemyTileSourceArtwork EnemyTiles { get; }

    /// <summary>The installed room-FX animated tiles, which hold the Tourian statues' pictures.</summary>
    /// <exception cref="InvalidOperationException">No map presentation is installed.</exception>
    RoomFxAnimatedTileAtlas AnimatedTiles { get; }
}
