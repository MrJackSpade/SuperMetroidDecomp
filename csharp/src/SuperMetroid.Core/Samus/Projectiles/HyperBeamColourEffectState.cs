using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Hyper Beam colour effect, the projectile colour cycle
/// started when Samus receives the Hyper Beam.
/// </summary>
/// <remarks>
/// The state retains the animation position and timer. Fixed animation control is
/// compiled separately from the live colours so presentation replacement cannot
/// silently change engine cadence or route restored state into adjacent game data.
/// </remarks>
public sealed class HyperBeamColourEffectState
{
    /// <summary>The colours the Hyper Beam's colour animation names, which the effect checks the animation against.</summary>
    private const ColourEffectColours AnimationColours = ColourEffectColours.HyperBeam;

    /// <summary>Where the Hyper Beam's colour animation resumes between updates.</summary>
    internal AnimationPlace Place { get; private set; }

    /// <summary>True after equipping the Hyper Beam has spawned the object.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The current shade's remaining hold. Spawn initializes it to one; the effect
    /// decrements before testing, so the first shade shows on its first update.
    /// </summary>
    public int FrameHold { get; private set; }

    /// <summary>The cycle position of the last shade shown, or none before its first update.</summary>
    public int? CurrentFrameIndex { get; private set; }

    /// <summary>
    /// Starts the Hyper Beam's colour effect. No colour is shown until its first update
    /// (<see cref="Step"/>).
    /// </summary>
    public void Spawn()
    {
        // Allocation clears the effect's colour step and auxiliary timer, installs timer
        // one, then calls the object's no-op setup. This specialized state has no
        // reusable slot fields to clear, but preserves every observable field and delay.
        IsActive = true;
        FrameHold = 1;
        CurrentFrameIndex = null;
        Place = AnimationPlace.StartOfAnimation;
    }

    /// <summary>Runs one update of the Hyper Beam's colour effect.</summary>
    public void Step(SamusColours colours)
    {
        Ensure.NotNull(colours);

        if (!IsActive)
        {
            return;
        }

        ColourEffectDefinition definition = HyperBeamDefinition();
        if (definition.Start != SamusPaletteDefinitions.HyperBeamFx.Start ||
            definition.InitialAnimation != ColourEffectAnimation.HyperBeam)
        {
            throw new InvalidDataException(
                $"Compiled Hyper Beam colour effect definition is inconsistent: start " +
                $"{definition.Start}, program {definition.InitialAnimation}.");
        }

        // The effect decrements before testing. A timer of two therefore displays a frame
        // on the starting update and one following update, then advances on the third update.
        FrameHold--;
        if (FrameHold != 0)
        {
            return;
        }

        AnimationPlace place = Place;
        // The Hyper Beam's cycle counts no repeats.
        WrappingCountdown noRepeats = WrappingCountdown.Start(0);
        AnimationShown<ColourEffectAnimation, int> next = ColourEffectAnimations.Steps.PlayToNextFrame(
            ColourEffectAnimations.Of(definition.InitialAnimation), ref place, ref noRepeats,
            cue =>
            {
                if (cue != new ColourEffectCue.SetColours(AnimationColours))
                    throw new InvalidDataException($"The Hyper Beam's colour animation raised {cue}, which its object does not perform.");
            });
        Place = place;
        if (next is not AnimationShown<ColourEffectAnimation, int>.Frame shown)
            throw new InvalidDataException($"The Hyper Beam's colour animation reached {next}; its cycle only shows frames.");
        // The cycle's frames are the beams' cycle colourings in order.
        if (shown.Segment != ColourEffectAnimation.HyperBeam)
            throw new InvalidDataException($"The Hyper Beam's colour animation showed frame {shown.Shown} of {shown.Segment}, outside its cycle.");
        int frameIndex = shown.Shown;

        colours.LayOverBeams(SamusShades.HyperBeamCycle(frameIndex));

        FrameHold = shown.Duration;
        CurrentFrameIndex = frameIndex;
    }

    /// <summary>The colour effect definition the Hyper Beam spawns.</summary>
    private static ColourEffectDefinition HyperBeamDefinition() =>
        ColourEffectSelection.Get(ColourEffectId.HyperBeam);

}
