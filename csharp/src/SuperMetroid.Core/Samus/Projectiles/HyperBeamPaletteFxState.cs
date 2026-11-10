using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Translation of the Hyper Beam palette-FX object, the projectile palette cycle
/// installed when Samus receives the Hyper Beam.
/// </summary>
/// <remarks>
/// The state retains the program position and timer. Fixed program control is
/// compiled separately from the live colours so presentation replacement cannot
/// silently change engine cadence or route restored state into adjacent game data.
/// </remarks>
public sealed class HyperBeamPaletteFxState
{
    /// <summary>The colours the Hyper Beam palette program names, which its object checks the program against.</summary>
    private const PaletteFxColours ProgramColours = PaletteFxColours.HyperBeam;

    /// <summary>Where the object's typed program resumes between palette handler calls.</summary>
    private AnimationProgramCursor<PaletteFxProgram> cursor;
    /// <summary>Where the Hyper Beam palette program resumes.</summary>
    internal AnimationProgramCursor<PaletteFxProgram> Cursor { get => cursor; private set => cursor = value; }

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
    /// Number of completed ten-frame loops, a count the port keeps. No game value holds it and nothing in
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
        // one, then calls the object's no-op setup routine. This specialized state has no
        // reusable slot fields to clear, but preserves every observable field and delay.
        IsActive = true;
        FrameHold = 1;
        CurrentFrameIndex = null;
        CompletedCycles = 0;
        Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(HyperBeamDefinition().InitialProgram);
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
            definition.InitialProgram != PaletteFxProgram.HyperBeam0)
        {
            throw new InvalidDataException(
                $"Compiled Hyper Beam palette-FX definition is inconsistent: start " +
                $"{definition.Start}, program {definition.InitialProgram}.");
        }

        // The handler decrements before testing. A timer of two therefore displays a frame
        // on the load call and one following call, then advances on the third handler call.
        FrameHold--;
        if (FrameHold != 0)
        {
            return new HyperBeamPaletteFxStepResult();
        }

        WrappingCountdown unusedCounter = default;
        AnimationProgramAdvance<int> advance = PaletteFxAnimations.Programs.Advance(ref cursor, ref unusedCounter,
            (cue, _) => cue == new PaletteFxCue.SetColours(ProgramColours)
                ? null
                : throw new InvalidDataException($"The Hyper Beam palette program published {cue}, which its object does not carry out."));
        if (advance is not AnimationProgramAdvance<int>.Show shown)
            throw new InvalidDataException($"The Hyper Beam palette program reached a {advance}; its cycle only shows frames.");
        // A show runs on to the next step of its own program, so the cursor names the program that showed the frame; the
        // cycle's frames are the beams' cycle colourings in order.
        if (cursor.Program != PaletteFxProgram.HyperBeam1)
            throw new InvalidDataException($"The Hyper Beam palette program showed frame {shown.Frame} of {cursor.Program}, outside its cycle.");
        int frameIndex = shown.Frame;
        // The cycle's goto returns to its first frame: showing it again completes a cycle.
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
