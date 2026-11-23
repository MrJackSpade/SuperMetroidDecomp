using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The statue sequence's operations over the entrance statue and the unlock effects.</summary>
/// <param name="statue">The entrance statue.</param>
/// <param name="unlock">The unlock effects.</param>
/// <param name="presentation">How the room's enemies look, whose encounter looks the sprite statues' eyes show.</param>
internal sealed partial class TourianStatueActors(TourianEntranceStatueBehavior statue, TourianUnlockBehavior unlock, EnemyPresentation presentation)
    : ITourianStatueActors
{
    /// <summary>The unlock effects.</summary>
    internal TourianUnlockBehavior Unlock { get; } = unlock;

    /// <inheritdoc/>
    public TourianStatueAnimation Animation => statue.StatueRoom.Animation;

    /// <inheritdoc/>
    public void SpawnEyeGlow(TourianStatueEye eye) => Unlock.SpawnTourianEyeGlow(eye);

    /// <inheritdoc/>
    public void ShowSpriteStatuePicture(TourianSpriteStatue sprite, SuperMetroid.Core.Fx.Artwork.TourianStatueBlockPicture picture) =>
        presentation.ShowLook(TourianSpriteStatueEyeLooks.Of(sprite, picture));

    /// <inheritdoc/>
    public void SpawnSoul(TourianStatueEye eye) => Unlock.SpawnTourianSoul(eye);

    /// <inheritdoc/>
    public void SpawnDescentDust() => Unlock.SpawnTourianDescentDust();

    /// <inheritdoc/>
    public void ShowStatueLook(PictureShade look, bool drawnInRoomBackground) => statue.ShowStatueLook(look, drawnInRoomBackground);

    /// <inheritdoc/>
    public IGlowingEnemy SpriteStatueGreyOut(TourianSpriteStatue sprite) => new TourianSpriteStatueGreyOut(statue, sprite);
}
