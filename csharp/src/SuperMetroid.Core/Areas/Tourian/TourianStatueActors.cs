using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The statue sequence's operations over the entrance statue and the unlock effects.</summary>
/// <param name="statue">The entrance statue.</param>
/// <param name="unlock">The unlock effects.</param>
internal sealed class TourianStatueActors(TourianEntranceStatueBehavior statue, TourianUnlockBehavior unlock) : ITourianStatueActors
{
    /// <inheritdoc/>
    public TourianStatueAnimation Animation => statue.TourianEntranceStatueAnimation;

    /// <inheritdoc/>
    public int VerticalOffset
    {
        get => statue.TourianEntranceStatueAnimation.VerticalOffset;
        set => statue.TourianEntranceStatueAnimation.VerticalOffset = value;
    }

    /// <inheritdoc/>
    public bool Finished
    {
        get => statue.TourianEntranceStatueAnimation.Finished;
        set => statue.TourianEntranceStatueAnimation.Finished = value;
    }

    /// <inheritdoc/>
    public Position16<RoomPixels> WaterSurface { get => unlock.TourianStatueWaterSurface; set => unlock.TourianStatueWaterSurface = value; }

    /// <inheritdoc/>
    public void SpawnEyeGlow(TourianStatueEye eye) => unlock.SpawnTourianEyeGlow(eye);

    /// <inheritdoc/>
    public void SpawnSoul(TourianStatueEye eye) => unlock.SpawnTourianSoul(eye);

    /// <inheritdoc/>
    public void SpawnDescentDust() => unlock.SpawnTourianDescentDust();

    /// <inheritdoc/>
    public void ShowStatueLook(PictureShade look) => statue.ShowStatueLook(look);
}
