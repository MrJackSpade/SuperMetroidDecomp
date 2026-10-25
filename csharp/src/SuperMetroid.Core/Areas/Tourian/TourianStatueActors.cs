namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The statue sequence's operations over the entrance statue and the unlock effects.</summary>
/// <param name="statue">The entrance statue.</param>
/// <param name="unlock">The unlock effects.</param>
internal sealed class TourianStatueActors(TourianEntranceStatueBehavior statue, TourianUnlockBehavior unlock) : ITourianStatueActors
{
    /// <inheritdoc/>
    public TourianStatueAnimationState AnimationState
    {
        get => statue.TourianEntranceStatueAnimationState;
        set => statue.TourianEntranceStatueAnimationState = value;
    }

    /// <inheritdoc/>
    public int VerticalOffset
    {
        get => statue.TourianEntranceStatueVerticalOffset;
        set => statue.TourianEntranceStatueVerticalOffset = value;
    }

    /// <inheritdoc/>
    public bool Finished
    {
        get => statue.TourianEntranceStatueFinished;
        set => statue.TourianEntranceStatueFinished = value;
    }

    /// <inheritdoc/>
    public Position16<RoomPixels> WaterSurface { get => unlock.TourianStatueWaterSurface; set => unlock.TourianStatueWaterSurface = value; }

    /// <inheritdoc/>
    public void SpawnEyeGlow(int parameter) => unlock.SpawnTourianEyeGlow(parameter);

    /// <inheritdoc/>
    public void SpawnSoul(int parameter) => unlock.SpawnTourianSoul(parameter);

    /// <inheritdoc/>
    public void SpawnDescentDust() => unlock.SpawnTourianDescentDust();
}
