using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// ROM-driven translation of palette-FX object <c>$8D:E1F0</c>, the projectile palette
/// cycle installed when Samus receives the Hyper Beam.
/// </summary>
/// <remarks>
/// This is intentionally a small, object-specific interpreter rather than a copied array
/// of attractive-looking colors. The state retains the native instruction pointer and
/// instruction timer, reads every BGR555 word from bank <c>$8D</c>, and recognizes the
/// exact three commands used by this object. That makes malformed ROM data fail loudly
/// instead of silently turning into a different animation.
/// </remarks>
public sealed class HyperBeamPaletteFxState
{
    // `$8D:E1F0` is the two-word object definition. Its first word is a no-op setup
    // routine and its second word points at `$D900`, the instruction program below.
    private const int ObjectDefinitionAddress = 0x8de1f0;

    // The first program command sets the palette-buffer byte index to `$01C2`. Dividing
    // by two gives CGRAM color `$E1`, i.e. OBJ palette six colors one through eight.
    private const ushort ExpectedPaletteByteIndex = 0x01c2;
    private const int DestinationColorIndex = ExpectedPaletteByteIndex / 2;

    // These are the actual bank-$8D instruction addresses stored in the ROM stream.
    // Bit 15 distinguishes commands from nonnegative frame timers in the native parser.
    private const ushort ColorIndexInYInstruction = 0xc655;
    private const ushort DoneInstruction = 0xc595;
    private const ushort GotoYInstruction = 0xc61e;

    // Hyper Beam has ten records and each record writes exactly eight colors. Keeping
    // these structural facts explicit lets verification prove the whole 20-call loop.
    public const int FrameCount = 10;
    public const int ColorsPerFrame = 8;

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
        _instructionPointer = 0xd900;
    }

    /// <summary>Runs one call of <c>PaletteFXObject_Handler</c> for the Hyper Beam object.</summary>
    public HyperBeamPaletteFxStepResult Step(ISnesAddressSpace bus, SnesCgram cgram)
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

        // Validate the object header as part of every real execution path. The setup word
        // is `$C685` (RTS), while the second word is the instruction list address installed
        // by `$8D:C50C`. This guards against accidentally running the NTSC offsets against
        // a different ROM revision or an incorrectly mapped bus.
        ushort setupPointer = ReadWord(bus, ObjectDefinitionAddress);
        ushort initialListPointer = ReadWord(bus, ObjectDefinitionAddress + 2);
        if (setupPointer != 0xc685 || initialListPointer != 0xd900)
        {
            throw new InvalidDataException(
                $"Hyper Beam palette-FX header changed: setup ${setupPointer:X4}, list ${initialListPointer:X4}.");
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

        ushort pointer = _instructionPointer;
        ushort word;
        while (true)
        {
            word = ReadBank8dWord(bus, pointer);
            if ((word & 0x8000) == 0)
                break;

            switch (word)
            {
                case ColorIndexInYInstruction:
                {
                    // `$8D:C655` stores a byte offset, not a color number. This object is
                    // expected to own precisely E1..E8; accepting another destination
                    // would corrupt an unrelated background or sprite palette.
                    ushort colorByteIndex = ReadBank8dWord(bus, unchecked((ushort)(pointer + 2)));
                    if (colorByteIndex != ExpectedPaletteByteIndex)
                    {
                        throw new InvalidDataException(
                            $"Hyper Beam palette-FX selected byte index ${colorByteIndex:X4}, expected $01C2.");
                    }

                    pointer = unchecked((ushort)(pointer + 4));
                    continue;
                }

                case GotoYInstruction:
                {
                    // The terminal `$C61E,$D904` pair jumps directly to frame zero, not
                    // to `$D900`; the color-index command consequently executes only once.
                    ushort target = ReadBank8dWord(bus, unchecked((ushort)(pointer + 2)));
                    if (target != 0xd904)
                    {
                        throw new InvalidDataException(
                            $"Hyper Beam palette-FX loop target changed to ${target:X4}.");
                    }

                    CompletedCycles = unchecked((ushort)(CompletedCycles + 1));
                    pointer = target;
                    continue;
                }

                default:
                    throw new InvalidDataException(
                        $"Unsupported Hyper Beam palette-FX command ${word:X4} at $8D:{pointer:X4}.");
            }
        }

        // A frame begins with its duration, followed by eight literal BGR555 colors and
        // `$C595` (done). The native done trampoline advances the saved instruction pointer
        // to the word after that command, which is exactly twenty bytes after this timer.
        ushort frameTimer = word;
        if (frameTimer != 2)
        {
            throw new InvalidDataException(
                $"Hyper Beam palette-FX frame at $8D:{pointer:X4} has timer {frameTimer}, expected 2.");
        }

        int frameIndex = (pointer - 0xd904) / 20;
        if ((uint)frameIndex >= FrameCount || pointer != 0xd904 + frameIndex * 20)
        {
            throw new InvalidDataException(
                $"Hyper Beam palette-FX frame pointer $8D:{pointer:X4} is outside the ten-record program.");
        }

        for (int color = 0; color < ColorsPerFrame; color++)
        {
            ushort bgr555 = ReadBank8dWord(
                bus,
                unchecked((ushort)(pointer + 2 + color * 2)));
            cgram.SetColor(DestinationColorIndex + color, bgr555);
        }

        ushort done = ReadBank8dWord(
            bus,
            unchecked((ushort)(pointer + 2 + ColorsPerFrame * 2)));
        if (done != DoneInstruction)
        {
            throw new InvalidDataException(
                $"Hyper Beam palette-FX frame {frameIndex} ends in ${done:X4}, expected $C595.");
        }

        InstructionTimer = frameTimer;
        CurrentFrameIndex = frameIndex;
        _instructionPointer = unchecked((ushort)(pointer + 20));
        return new HyperBeamPaletteFxStepResult(
            Active: true,
            PaletteWritten: true,
            FrameIndex: CurrentFrameIndex,
            InstructionTimer,
            _instructionPointer,
            CompletedCycles);
    }

    private static ushort ReadBank8dWord(ISnesAddressSpace bus, ushort address) =>
        ReadWord(bus, 0x8d0000 | address);

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
