using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Translation of palette-FX object <c>$8D:E1F0</c>, the projectile palette cycle
/// installed when Samus receives the Hyper Beam.
/// </summary>
/// <remarks>
/// The state retains the native instruction pointer and timer. Fixed program control is
/// compiled separately from the live BGR555 payloads so presentation replacement cannot
/// silently change engine cadence or route restored state into adjacent cartridge data.
/// </remarks>
public sealed class HyperBeamPaletteFxState
{
    /// <summary>CGRAM destination for the Hyper Beam projectile cycle, converted from its byte offset.</summary>
    private const int DestinationColorIndex =
        SamusPaletteRomData.HyperBeamFx.DestinationByteIndex / sizeof(ushort);

    /// <summary>Where the object's typed program resumes between palette handler calls.</summary>
    internal AnimationProgramCursor<PaletteFxProgram> _cursor;

    /// <summary>The cycle's ten records in the order <see cref="PaletteFxProgram.HyperBeam1"/> shows them; a record's place is its colour frame.</summary>
    private static readonly int[] CycleRecords = [.. PaletteFxAnimations.Programs.Steps(PaletteFxProgram.HyperBeam1)
        .Where(step => step.Kind == AnimationProgramStepKind.Show)
        .Select(step => step.Frame.Record)];

    /// <summary>True after controller function three has spawned object <c>$E1F0</c>.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Native per-object instruction timer. Spawn initializes it to one; the handler
    /// decrements before testing, so the first palette record executes on its first call.
    /// </summary>
    public NativeCountdown InstructionTimer { get; private set; }

    /// <summary>Last palette record written, or -1 before the first handler call.</summary>
    public int CurrentFrameIndex { get; private set; } = -1;

    /// <summary>
    /// Number of completed ten-frame loops, a port-side debugger count. No cartridge word holds it and nothing in
    /// gameplay reads it, so it is an ordinary count that starts at zero on spawn and grows by one per loop.
    /// </summary>
    public int CompletedCycles { get; private set; }

    /// <summary>
    /// Performs <c>Spawn_PaletteFXObject</c>'s state initialization for object
    /// <c>$8D:E1F0</c>. No colors are copied until the global handler runs.
    /// </summary>
    public void Spawn()
    {
        // Native allocation clears the color index and auxiliary timer, installs timer
        // one, then calls the object's no-op setup routine. This specialized state has no
        // reusable slot fields to clear, but preserves every observable field and delay.
        IsActive = true;
        InstructionTimer = NativeCountdown.Start(1);
        CurrentFrameIndex = -1;
        CompletedCycles = 0;
        _cursor = AnimationProgramCursor<PaletteFxProgram>.Start(HyperBeamDefinition().InitialProgram);
    }

    /// <summary>Runs one call of <c>PaletteFXObject_Handler</c> for the Hyper Beam object.</summary>
    public HyperBeamPaletteFxStepResult Step(SnesCgram cgram,
        HyperBeamFxColorCatalog? presentationColors = null)
    {
        Ensure.NotNull(cgram);

        if (!IsActive)
        {
            return new HyperBeamPaletteFxStepResult();
        }

        RoomPaletteFxDefinition definition = HyperBeamDefinition();
        if (definition.SetupCallback != SamusPaletteRomData.HyperBeamFx.SetupCallback ||
            definition.InitialProgram != PaletteFxProgram.HyperBeam0)
        {
            throw new InvalidDataException(
                $"Compiled Hyper Beam palette-FX definition is inconsistent: setup " +
                $"${(int)definition.SetupCallback:X4}, program {definition.InitialProgram}.");
        }

        // `$8D:C552` decrements before testing. A timer of two therefore displays a frame
        // on the load call and one following call, then advances on the third handler call.
        InstructionTimer = InstructionTimer.Decrement().Remaining;
        if (!InstructionTimer.IsZero)
        {
            return new HyperBeamPaletteFxStepResult();
        }

        NativeCountdown unusedCounter = default;
        AnimationProgramAdvance<PaletteFxFrame> advance = PaletteFxAnimations.Programs.Advance(ref _cursor, ref unusedCounter,
            (cue, _) => cue == new PaletteFxCue(PaletteFxCueKind.SetColorIndex, ColorByteIndex: (ushort)SamusPaletteRomData.HyperBeamFx.DestinationByteIndex)
                ? (PaletteFxProgram?)null
                : throw new InvalidDataException($"The Hyper Beam palette program published {cue}, which its object does not carry out."));
        if (advance.Kind != AnimationProgramAdvanceKind.Show)
            throw new InvalidDataException($"The Hyper Beam palette program reached a {advance.Kind}; its cycle only shows records.");
        int frameIndex = Array.IndexOf(CycleRecords, advance.Frame.Record);
        if (frameIndex < 0)
            throw new InvalidDataException($"The Hyper Beam palette program showed record {advance.Frame.Record}, outside its cycle.");
        // The cycle's goto returns to its first record: showing it again completes a cycle.
        if (frameIndex == 0 && CurrentFrameIndex >= 0)
            CompletedCycles++;

        (presentationColors ?? throw new InvalidOperationException(
            "Hyper Beam palette FX requires installed colors."))
            .Apply(cgram, frameIndex, DestinationColorIndex);

        InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
        CurrentFrameIndex = frameIndex;
        return new HyperBeamPaletteFxStepResult();
    }

    /// <summary>The palette-FX object definition the Hyper Beam spawns.</summary>
    private static RoomPaletteFxDefinition HyperBeamDefinition() =>
        RoomPaletteFxDefinitions.Get(unchecked((ushort)SamusPaletteRomData.HyperBeamFx.ObjectDefinition));

}
