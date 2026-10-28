using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One Tourian statue's installed picture characters.</summary>
/// <param name="Statue">The statue.</param>
/// <param name="Picture">The picture shown.</param>
internal sealed record TourianStatuePictureCharacters(AnimatedTileObject Statue, TourianStatueTilePicture Picture) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"{Statue} {Picture}";
}
