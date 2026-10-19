using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed gameplay artwork that domain OBJ character sources name.</summary>
internal interface IInstalledObjCharacterArtwork
{
    /// <summary>The installed enemy and boss tile artwork.</summary>
    /// <exception cref="InvalidOperationException">No enemy artwork is installed.</exception>
    EnemyTileArtworkCatalog EnemyTiles { get; }

    /// <summary>The installed room-FX animated tiles, which hold the Tourian statues' pictures.</summary>
    /// <exception cref="InvalidOperationException">No map presentation is installed.</exception>
    RoomFxAnimatedTileAtlas AnimatedTiles { get; }
}
