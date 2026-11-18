using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The statue sequence's operations over the entrance statue and the unlock effects.</summary>
/// <param name="statue">The entrance statue.</param>
/// <param name="unlock">The unlock effects.</param>
[RoomLifetime]
internal sealed class TourianStatueActors(TourianEntranceStatueBehavior statue, TourianUnlockBehavior unlock) : ITourianStatueActors
{
    /// <inheritdoc/>
    public TourianStatueAnimation Animation => statue.TourianEntranceStatueAnimation;

    /// <inheritdoc/>
    public bool Finished
    {
        get => statue.TourianEntranceStatueAnimation.Finished;
        set => statue.TourianEntranceStatueAnimation.Finished = value;
    }

    /// <inheritdoc/>
    public void SpawnEyeGlow(TourianStatueEye eye) => unlock.SpawnTourianEyeGlow(eye);

    /// <inheritdoc/>
    public void SpawnSoul(TourianStatueEye eye) => unlock.SpawnTourianSoul(eye);

    /// <inheritdoc/>
    public void SpawnDescentDust() => unlock.SpawnTourianDescentDust();

    /// <inheritdoc/>
    public void ShowStatueLook(PictureShade look) => statue.ShowStatueLook(look);

    /// <inheritdoc/>
    public IGlowingEnemy SpriteStatueGreyOut(TourianSpriteStatue sprite) => new TourianSpriteStatueGreyOut(statue, sprite);
}
