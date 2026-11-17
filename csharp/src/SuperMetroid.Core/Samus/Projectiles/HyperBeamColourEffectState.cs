using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Hyper Beam colour effect, the projectile colour cycle
/// started when Samus receives the Hyper Beam.
/// </summary>
/// <remarks>
/// The effect keeps its place in the beams' colour animation and the hold of the shade it shows; the installed colours
/// change only what each shade looks like, never when it shows.
/// </remarks>
public sealed class HyperBeamColourEffectState
{
    /// <summary>The colours the Hyper Beam's colour animation names, which the effect checks the animation against.</summary>
    private const ColourEffectColours AnimationColours = ColourEffectColours.HyperBeam;

    /// <summary>Where the Hyper Beam's colour animation resumes between updates.</summary>
    internal AnimationPlace Place { get; private set; }

    /// <summary>True once equipping the Hyper Beam has started the effect.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The current shade's remaining hold. Spawn initializes it to one; the effect
    /// decrements before testing, so the first shade shows on its first update.
    /// </summary>
    public int FrameHold { get; private set; }

    /// <summary>The shade of the beams' cycle last shown, by its place in the cycle, or none before the first update.</summary>
    public int? ShownCycleShade { get; private set; }

    /// <summary>
    /// Starts the Hyper Beam's colour effect. No colour is shown until its first update
    /// (<see cref="Step"/>).
    /// </summary>
    public void Spawn()
    {
        // The effect starts at its animation's first shade with a hold of one update, and shows nothing yet.
        IsActive = true;
        FrameHold = 1;
        ShownCycleShade = null;
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
        if (definition.Start != SamusShadeDefinitions.HyperBeamFx.Start ||
            definition.Colours != AnimationColours)
        {
            throw new InvalidDataException(
                $"Compiled Hyper Beam colour effect definition is inconsistent: start " +
                $"{definition.Start}, colours {definition.Colours}.");
        }

        // The effect decrements before testing. A timer of two therefore displays a frame
        // on the starting update and one following update, then advances on the third update.
        FrameHold--;
        if (FrameHold != 0)
        {
            return;
        }

        AnimationPlace place = Place;
        AnimationShown<ColourEffectId, int> next = ColourEffectAnimations.Steps.PlayToNextFrame(
            definition.Animation, ref place,
            cue => throw new InvalidDataException($"The Hyper Beam's colour animation raised {cue}, which its object does not perform."));
        Place = place;
        if (next is not AnimationShown<ColourEffectId, int>.Frame shown)
            throw new InvalidDataException($"The Hyper Beam's colour animation reached {next}; its cycle only shows frames.");
        // The cycle's frames are the beams' cycle colourings in order.
        if (shown.Segment != ColourEffectId.HyperBeam)
            throw new InvalidDataException($"The Hyper Beam's colour animation showed frame {shown.Shown} of {shown.Segment}, outside its cycle.");
        int frameIndex = shown.Shown;

        colours.LayOverBeams(SamusShades.HyperBeamCycle(frameIndex));

        FrameHold = shown.Duration;
        ShownCycleShade = frameIndex;
    }

    /// <summary>The colour effect definition the Hyper Beam spawns.</summary>
    private static ColourEffectDefinition HyperBeamDefinition() =>
        ColourEffectAnimations.Definition(ColourEffectId.HyperBeam);

}
