using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Exact packed timer/index animation used by <c>HandleVisorPalette</c> at
/// <c>$91:D83F-$91:D8A4</c>.
/// </summary>
/// <remarks>
/// This is intentionally not a generic color tween. In rooms whose default layer-blending
/// configuration is <c>$28</c> or <c>$2A</c>, the ROM rotates only Samus OBJ palette-four
/// color four through the last three words at <c>$9B:A3C0</c>. The timer and table offset
/// are adjacent bytes in WRAM, so the native routine decrements them as one 16-bit word.
/// Keeping that alias visible makes unusual externally edited states debugger-faithful too.
/// </remarks>
public sealed class SamusVisorPaletteState
{
    private const int VisorColors = 0x9ba3c0;
    private const int SamusVisorCgramIndex = 192 + 4;
    private const ushort NormalRoomReset = 0x0601;

    /// <summary>
    /// Native word at WRAM <c>$0A72</c>: low byte is the countdown and high byte is an
    /// even byte offset into the six-word visor table.
    /// </summary>
    public ushort PackedTimerIndex { get; private set; } = NormalRoomReset;

    /// <summary>Low byte at WRAM <c>$0A72</c>.</summary>
    public byte Timer => unchecked((byte)PackedTimerIndex);

    /// <summary>High byte at WRAM <c>$0A73</c>; this is a byte offset, not an ordinal.</summary>
    public byte PaletteByteOffset => unchecked((byte)(PackedTimerIndex >> 8));

    /// <summary>
    /// Advances one palette-handler call and optionally writes CGRAM color 196.
    /// </summary>
    public SamusVisorPaletteStepResult Update(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        ushort specialSamusPaletteType,
        ushort layerBlendingDefaultConfig)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        ushort packedBefore = PackedTimerIndex;

        // X-ray handler eight owns this same visor color. `$91:D842` returns immediately,
        // preserving both packed bytes so ordinary room animation resumes where it stopped.
        if ((SamusSpecialPaletteType)specialSamusPaletteType == SamusSpecialPaletteType.Xray)
        {
            return new SamusVisorPaletteStepResult(
                SamusVisorPaletteAction.SuppressedByXray,
                packedBefore,
                PackedTimerIndex,
                layerBlendingDefaultConfig);
        }

        // Every other room writes `$0601` on every call. Besides disabling the animation,
        // this primes an immediate color write on the first call after entering a backdrop-
        // color-math room: 1 decrements to 0 before the table is consulted.
        if (!layerBlendingDefaultConfig.AnimatesVisor())
        {
            PackedTimerIndex = NormalRoomReset;
            return new SamusVisorPaletteStepResult(
                SamusVisorPaletteAction.ResetForNormalRoom,
                packedBefore,
                PackedTimerIndex,
                layerBlendingDefaultConfig);
        }

        // DEC is deliberately word-wide. Normally only the low timer changes, but if an
        // external debugger supplies timer zero the borrow also decrements the high offset,
        // exactly as the adjacent native WRAM bytes do.
        PackedTimerIndex = unchecked((ushort)(PackedTimerIndex - 1));
        if (Timer != 0)
        {
            return new SamusVisorPaletteStepResult(
                SamusVisorPaletteAction.Countdown,
                packedBefore,
                PackedTimerIndex,
                layerBlendingDefaultConfig);
        }

        // OR rather than assignment is literal `$91:D864`: the expired low byte is zero in
        // admitted state, so it becomes five while the high table offset is retained.
        PackedTimerIndex |= 0x0005;
        byte sourceOffset = PaletteByteOffset;
        ushort color = ReadWord(bus, VisorColors + sourceOffset);
        cgram.SetColor(SamusVisorCgramIndex, color);

        // Only offsets 6, 8, and 10 belong to the room-backdrop cycle. Offsets 0/2/4 are
        // X-ray widening colors and are selected by the separate X-ray palette handler.
        int nextOffset = sourceOffset + 2;
        byte storedNextOffset = unchecked((byte)(nextOffset < 12 ? nextOffset : 6));
        PackedTimerIndex = unchecked((ushort)(Timer | (storedNextOffset << 8)));

        return new SamusVisorPaletteStepResult(
            SamusVisorPaletteAction.ColorWritten,
            packedBefore,
            PackedTimerIndex,
            layerBlendingDefaultConfig,
            sourceOffset,
            color);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(address) |
        (bus.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8)));
}

/// <summary>Branch and packed-word witness from one native visor-palette call.</summary>
public readonly record struct SamusVisorPaletteStepResult(
    SamusVisorPaletteAction Action,
    ushort PackedBefore,
    ushort PackedAfter,
    ushort LayerBlendingDefaultConfig,
    byte? SourceByteOffset = null,
    ushort? WrittenColor = null);

/// <summary>Observable exits from <c>$91:D83F-$91:D8A4</c>.</summary>
public enum SamusVisorPaletteAction : byte
{
    None,
    SuppressedByXray,
    ResetForNormalRoom,
    Countdown,
    ColorWritten,
}
