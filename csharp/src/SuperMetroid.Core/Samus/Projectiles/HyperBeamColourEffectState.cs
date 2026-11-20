using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Hyper Beam colour effect, the projectile colour cycle started when Samus receives the Hyper Beam (#2529): equipping
/// the Hyper Beam creates it, and it plays from then on.
/// </summary>
/// <remarks>
/// The effect plays the beams' colour cycle through the shared animation player (#2458), which counts each shade's
/// updates; the installed colours change only what each shade looks like, never when it shows.
/// </remarks>
public sealed class HyperBeamColourEffectState
{
    /// <summary>
    /// The play of the Hyper Beam's colour cycle: the shade it shows and the updates that shade has left. No colour is
    /// shown until the effect's first update (<see cref="Step"/>), which shows the cycle's first shade.
    /// </summary>
    private AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> Cycle { get; } =
        SamusGlowPlay.Starting(SamusGlowSegment.HyperBeamCycle);

    /// <summary>
    /// Runs one update of the Hyper Beam's colour effect: once the shade shown has held its updates, the next shade is laid
    /// over Samus's beams.
    /// </summary>
    public void Step(SamusColours colours)
    {
        Ensure.NotNull(colours);
        if (Cycle.StepToNewFrame() is { } shade)
            colours.LayOverBeams(shade);
    }
}
