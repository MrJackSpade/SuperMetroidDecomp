using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Hyper Beam colour effect, the projectile colour cycle
/// started when Samus receives the Hyper Beam.
/// </summary>
/// <remarks>
/// The effect plays the beams' colour cycle through the shared animation player (#2458), which counts each shade's
/// updates; the installed colours change only what each shade looks like, never when it shows.
/// </remarks>
public sealed class HyperBeamColourEffectState
{
    /// <summary>
    /// The play of the Hyper Beam's colour cycle: the shade it shows and the updates that shade has left; null until
    /// equipping the Hyper Beam starts the effect.
    /// </summary>
    internal AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue>? Cycle { get; private set; }

    /// <summary>
    /// Starts the Hyper Beam's colour effect. No colour is shown until its first update
    /// (<see cref="Step"/>), which shows the cycle's first shade.
    /// </summary>
    public void Spawn() => Cycle = SamusGlowPlay.Starting(SamusGlowSegment.HyperBeamCycle);

    /// <summary>
    /// Runs one update of the Hyper Beam's colour effect: once the shade shown has held its updates, the next shade is laid
    /// over Samus's beams.
    /// </summary>
    /// <exception cref="InvalidOperationException">The cycle comes to rest, which a cycle that only shows shades never does.</exception>
    public void Step(SamusColours colours)
    {
        Ensure.NotNull(colours);
        if (Cycle is not { } cycle)
            return;

        if (cycle.StepToNewFrame() is { } shade)
            colours.LayOverBeams(shade);
        if (cycle.IsResting)
            throw new InvalidOperationException("The Hyper Beam's colour cycle came to rest; it only shows shades, for ever.");
    }
}
