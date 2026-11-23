namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A statue projectile animation cue.</summary>
internal abstract partial record TourianStatueProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TourianStatueProjectileCue()
    {
    }

    /// <summary>Moves the projectile down by its cue's offset.</summary>
    /// <param name="Pixels">How far a <see cref="MoveDown"/> cue moves the projectile.</param>
    internal sealed partial record MoveDown(int Pixels) : TourianStatueProjectileCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(TourianStatueProjectileCueKind Kind) : TourianStatueProjectileCue;
}
