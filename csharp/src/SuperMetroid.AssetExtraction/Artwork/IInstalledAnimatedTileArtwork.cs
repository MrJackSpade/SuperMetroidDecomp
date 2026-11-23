using SuperMetroid.AssetExtraction.Fx;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed room-FX animated-tile pictures an encounter's looks can copy (#2625): the Tourian sprite statues' eye
/// pictures, which their animations copy over the statues' sprite characters.
/// </summary>
internal interface IInstalledAnimatedTileArtwork
{
    /// <summary>The room-FX atlas holding the animated-tile pictures.</summary>
    RoomFxBlockAnimationAtlas AnimatedTiles { get; }
}
