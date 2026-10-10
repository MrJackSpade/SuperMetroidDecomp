using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

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
    private const int DestinationColorIndex =
        SamusPaletteRomData.HyperBeamFx.DestinationByteIndex / sizeof(ushort);

    private ushort _instructionPointer;

    /// <summary>True after controller function three has spawned object <c>$E1F0</c>.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Native per-object instruction timer. Spawn initializes it to one; the handler
    /// decrements before testing, so the first palette record executes on its first call.
    /// </summary>
    public ushort InstructionTimer { get; private set; }

    /// <summary>Last palette record written, or -1 before the first handler call.</summary>
    public int CurrentFrameIndex { get; private set; } = -1;

    /// <summary>Number of completed ten-frame loops, useful in debugger watches.</summary>
    public ushort CompletedCycles { get; private set; }

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
        InstructionTimer = 1;
        CurrentFrameIndex = -1;
        CompletedCycles = 0;
        _instructionPointer = HyperBeamPaletteFxProgramDefinitions.InitialInstructionPointer;
    }

    /// <summary>Runs one call of <c>PaletteFXObject_Handler</c> for the Hyper Beam object.</summary>
    public HyperBeamPaletteFxStepResult Step(ISnesAddressSpace bus, SnesCgram cgram,
        HyperBeamFxColorCatalog? presentationColors = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        if (!IsActive)
        {
            return new HyperBeamPaletteFxStepResult();
        }

        RoomPaletteFxDefinition definition = RoomPaletteFxDefinitions.Get(
            unchecked((ushort)SamusPaletteRomData.HyperBeamFx.ObjectDefinition));
        if (definition.SetupCallback != SamusPaletteRomData.HyperBeamFx.SetupCallback ||
            definition.InitialInstructionList !=
                HyperBeamPaletteFxProgramDefinitions.InitialInstructionPointer)
        {
            throw new InvalidDataException(
                $"Compiled Hyper Beam palette-FX definition is inconsistent: setup " +
                $"${(int)definition.SetupCallback:X4}, list ${definition.InitialInstructionList:X4}.");
        }

        // `$8D:C552` decrements before testing. A timer of two therefore displays a frame
        // on the load call and one following call, then advances on the third handler call.
        InstructionTimer = unchecked((ushort)(InstructionTimer - 1));
        if (InstructionTimer != 0)
        {
            return new HyperBeamPaletteFxStepResult();
        }

        HyperBeamPaletteFxFrame frame =
            HyperBeamPaletteFxProgramDefinitions.ResolveFrame(
                _instructionPointer,
                out bool completedCycle);
        if (completedCycle)
            CompletedCycles = unchecked((ushort)(CompletedCycles + 1));

        (presentationColors ?? throw new InvalidOperationException(
            "Hyper Beam palette FX requires installed colors."))
            .Apply(cgram, frame.Index, DestinationColorIndex);

        InstructionTimer = frame.Duration;
        CurrentFrameIndex = frame.Index;
        _instructionPointer = frame.NextInstructionPointer;
        return new HyperBeamPaletteFxStepResult();
    }

}

/// <summary>Debugger-visible result of one Hyper Beam palette-object handler call.</summary>
public readonly record struct HyperBeamPaletteFxStepResult();
