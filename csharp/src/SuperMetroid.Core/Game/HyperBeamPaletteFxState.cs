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

    /// <summary>Number of timed color records in the Hyper Beam loop.</summary>
    public const int FrameCount = HyperBeamPaletteFxProgramDefinitions.FrameCount;

    /// <summary>Number of colors copied by each Hyper Beam record.</summary>
    public const int ColorsPerFrame = HyperBeamPaletteFxProgramDefinitions.ColorsPerFrame;

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

    /// <summary>Current bank-$8D instruction pointer, exposed for exact-step debugging.</summary>
    public ushort InstructionPointer => _instructionPointer;

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
            return new HyperBeamPaletteFxStepResult(
                Active: false,
                PaletteWritten: false,
                FrameIndex: CurrentFrameIndex,
                InstructionTimer,
                _instructionPointer,
                CompletedCycles);
        }

        RoomPaletteFxDefinition definition = RoomPaletteFxDefinitions.Get(
            unchecked((ushort)SamusPaletteRomData.HyperBeamFx.ObjectDefinition));
        if (definition.SetupCallback != SamusPaletteRomData.HyperBeamFx.SetupCallback ||
            definition.InitialInstructionList !=
                HyperBeamPaletteFxProgramDefinitions.InitialInstructionPointer)
        {
            throw new InvalidDataException(
                $"Compiled Hyper Beam palette-FX definition is inconsistent: setup " +
                $"${definition.SetupCallback:X4}, list ${definition.InitialInstructionList:X4}.");
        }

        // `$8D:C552` decrements before testing. A timer of two therefore displays a frame
        // on the load call and one following call, then advances on the third handler call.
        InstructionTimer = unchecked((ushort)(InstructionTimer - 1));
        if (InstructionTimer != 0)
        {
            return new HyperBeamPaletteFxStepResult(
                Active: true,
                PaletteWritten: false,
                FrameIndex: CurrentFrameIndex,
                InstructionTimer,
                _instructionPointer,
                CompletedCycles);
        }

        HyperBeamPaletteFxFrame frame =
            HyperBeamPaletteFxProgramDefinitions.ResolveFrame(
                _instructionPointer,
                out bool completedCycle);
        if (completedCycle)
            CompletedCycles = unchecked((ushort)(CompletedCycles + 1));

        if (presentationColors is not null)
            presentationColors.Apply(cgram, frame.Index, DestinationColorIndex);
        else
        {
            for (int color = 0; color < ColorsPerFrame; color++)
            {
                ushort bgr555 = ReadBank8dWord(
                    bus,
                    unchecked((ushort)(frame.FirstColorPointer + color * sizeof(ushort))));
                cgram.SetColor(DestinationColorIndex + color, bgr555);
            }
        }

        InstructionTimer = frame.Duration;
        CurrentFrameIndex = frame.Index;
        _instructionPointer = frame.NextInstructionPointer;
        return new HyperBeamPaletteFxStepResult(
            Active: true,
            PaletteWritten: true,
            FrameIndex: CurrentFrameIndex,
            InstructionTimer,
            _instructionPointer,
            CompletedCycles);
    }

    private static ushort ReadBank8dWord(ISnesAddressSpace bus, ushort address) =>
        ReadWord(bus, SamusPaletteRomData.Banks.PaletteFx | address);

    private static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Debugger-visible result of one Hyper Beam palette-object handler call.</summary>
public readonly record struct HyperBeamPaletteFxStepResult(
    bool Active,
    bool PaletteWritten,
    int FrameIndex,
    ushort InstructionTimer,
    ushort InstructionPointer,
    ushort CompletedCycles);
