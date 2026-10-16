namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The statue sequence's operations over the entrance statue and the unlock effects.</summary>
/// <param name="statue">The entrance statue.</param>
/// <param name="unlock">The unlock effects.</param>
internal sealed class TourianStatueActors(TourianEntranceStatueBehavior statue, TourianUnlockBehavior unlock) : ITourianStatueActors
{
    /// <inheritdoc/>
    public ushort AnimationState
    {
        get => statue.TourianEntranceStatueAnimationState;
        set => statue.TourianEntranceStatueAnimationState = value;
    }

    /// <inheritdoc/>
    public short VerticalOffset
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
    public ushort WaterY { get => unlock.TourianStatueWaterY; set => unlock.TourianStatueWaterY = value; }

    /// <inheritdoc/>
    public void SpawnUnlockEffect(ushort parameter, bool soul) => unlock.SpawnTourianUnlockEffect(parameter, soul);

    /// <inheritdoc/>
    public void SpawnDescentDust() => unlock.SpawnTourianDescentDust();
}
