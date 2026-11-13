using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Hyper Beam palette-FX object, the projectile palette cycle
/// installed when Samus receives the Hyper Beam.
/// </summary>
/// <remarks>
/// The state retains the animation position and timer. Fixed animation control is
/// compiled separately from the live colours so presentation replacement cannot
/// silently change engine cadence or route restored state into adjacent game data.
/// </remarks>
public sealed class HyperBeamPaletteFxState
{
    /// <summary>The colours the Hyper Beam palette animation names, which its object checks the animation against.</summary>
    private const PaletteFxColours AnimationColours = PaletteFxColours.HyperBeam;

    /// <summary>Where the Hyper Beam's colour animation resumes between updates.</summary>
    internal AnimationPlace Place { get; private set; }

    /// <summary>True after controller function three has spawned the object.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Per-object frame hold. Spawn initializes it to one; the handler
    /// decrements before testing, so the first palette record executes on its first call.
    /// </summary>
    public int FrameHold { get; private set; }

    /// <summary>The cycle position of the last palette record written, or none before the first handler call.</summary>
    public int? CurrentFrameIndex { get; private set; }

    /// <summary>
    /// Number of completed ten-frame loops, a count this model keeps. No game value holds it and nothing in
    /// gameplay reads it, so it is an ordinary count that starts at zero on spawn and grows by one per loop.
    /// </summary>
    public int CompletedCycles { get; private set; }

    /// <summary>
    /// Performs the palette effect's spawn-time state initialization for
    /// the Hyper Beam palette object. No colors are copied until the global handler runs.
    /// </summary>
    public void Spawn()
    {
        // Allocation clears the color index and auxiliary timer, installs timer
        // one, then calls the object's no-op setup. This specialized state has no
        // reusable slot fields to clear, but preserves every observable field and delay.
        IsActive = true;
        FrameHold = 1;
        CurrentFrameIndex = null;
        CompletedCycles = 0;
        Place = AnimationPlace.StartOfAnimation;
    }

    /// <summary>Runs one call of the palette effect handler for the Hyper Beam object.</summary>
    public HyperBeamPaletteFxStepResult Step(SamusColours colours)
    {
        Ensure.NotNull(colours);

        if (!IsActive)
        {
            return new HyperBeamPaletteFxStepResult();
        }

        RoomPaletteFxDefinition definition = HyperBeamDefinition();
        if (definition.Start != SamusPaletteDefinitions.HyperBeamFx.Start ||
            definition.InitialAnimation != PaletteFxSegment.HyperBeam0)
        {
            throw new InvalidDataException(
                $"Compiled Hyper Beam palette-FX definition is inconsistent: start " +
                $"{definition.Start}, program {definition.InitialAnimation}.");
        }

        // The handler decrements before testing. A timer of two therefore displays a frame
        // on the load call and one following call, then advances on the third handler call.
        FrameHold--;
        if (FrameHold != 0)
        {
            return new HyperBeamPaletteFxStepResult();
        }

        AnimationPlace place = Place;
        // The Hyper Beam's cycle counts no repeats.
        WrappingCountdown noRepeats = WrappingCountdown.Start(0);
        AnimationShown<PaletteFxSegment, int> next = PaletteFxSegments.All.PlayToNextFrame(
            PaletteFxAnimations.StartingWith(definition.InitialAnimation), ref place, ref noRepeats,
            cue =>
            {
                if (cue != new PaletteFxCue.SetColours(AnimationColours))
                    throw new InvalidDataException($"The Hyper Beam's colour animation raised {cue}, which its object does not perform.");
            });
        Place = place;
        if (next is not AnimationShown<PaletteFxSegment, int>.Frame shown)
            throw new InvalidDataException($"The Hyper Beam's colour animation reached {next}; its cycle only shows frames.");
        // The cycle's frames are the beams' cycle colourings in order.
        if (shown.Segment != PaletteFxSegment.HyperBeam1)
            throw new InvalidDataException($"The Hyper Beam's colour animation showed frame {shown.Shown} of {shown.Segment}, outside its cycle.");
        int frameIndex = shown.Shown;
        // The cycle's jump returns to its first frame: showing it again completes a cycle.
        if (frameIndex == 0 && CurrentFrameIndex is not null)
            CompletedCycles++;

        colours.LayOverBeams(SamusShades.HyperBeamCycle(frameIndex));

        FrameHold = shown.Duration;
        CurrentFrameIndex = frameIndex;
        return new HyperBeamPaletteFxStepResult();
    }

    /// <summary>The palette-FX object definition the Hyper Beam spawns.</summary>
    private static RoomPaletteFxDefinition HyperBeamDefinition() =>
        RoomPaletteFxSelection.Get(PaletteFxObject.HyperBeam);

}
