namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable control words for Wrecked Ship's powered green-light palette loop.
/// </summary>
/// <remarks>
/// Palette-FX definitions <c>$F76D</c> and <c>$F771</c> share this program. Its sixteen
/// BGR555 colors remain live presentation data; only setup, timing, waits, and loop control
/// are compiled here.
/// $8D:EAE2 selects CGRAM byte $0098. Records f=0..7 begin at
/// $EAE6 + 8*f, last ten frames, write two live colors, and end in
/// $C595 wait. The $C61E goto at $EB26 returns to $EAE6 after an
/// 80-frame cycle; f=8 reaches control. All 20 mechanics words match
/// the pinned NTSC J/U v1.0 ROM for both definition entries.
/// </remarks>
public static class WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions
{
    /// <summary>Powered Wrecked Ship palette-FX definition at $8D:F76D.</summary>
    public const ushort PoweredDefinition = 0xf76d;

    /// <summary>Alternate caller of the powered-light program at $8D:F771.</summary>
    public const ushort PoweredDefinitionAlternate = 0xf771;

    /// <summary><c>InstList_PaletteFXObject_WreckedShip1_0</c> at $8D:EAE2.</summary>
    public const ushort ProgramStart = 0xeae2;

    /// <summary>First timed record, <c>InstList_PaletteFXObject_WreckedShip1_1</c>.</summary>
    public const ushort FirstFramePointer = 0xeae6;

    /// <summary>Terminal <c>goto</c> command at $8D:EB26.</summary>
    public const ushort LoopInstructionPointer = 0xeb26;

    /// <summary>The first powered-light destination byte in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x0098;

    /// <summary>Eight ten-frame records form the powered-light cycle.</summary>
    public const int FrameCount = 8;

    /// <summary>Two BGR555 colors are presentation-owned by each record.</summary>
    public const int ColorsPerFrame = 2;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 8;

    /// <summary>Reads one fixed control word while excluding BGR555 presentation words.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (pointer == ProgramStart)
        {
            value = PaletteFxInstructionCodes.SetColorIndex;
            return true;
        }
        if (pointer == unchecked((ushort)(ProgramStart + sizeof(ushort))))
        {
            value = ColorByteIndex;
            return true;
        }
        if (pointer == LoopInstructionPointer)
        {
            value = PaletteFxInstructionCodes.Goto;
            return true;
        }
        if (pointer == unchecked((ushort)(LoopInstructionPointer + sizeof(ushort))))
        {
            value = FirstFramePointer;
            return true;
        }

        int frameOffset = pointer - FirstFramePointer;
        if (frameOffset >= 0 && frameOffset < FrameCount * FrameByteCount)
        {
            int inFrame = frameOffset % FrameByteCount;
            if (inFrame == 0)
            {
                value = 10;
                return true;
            }
            if (inFrame == FrameByteCount - sizeof(ushort))
            {
                value = PaletteFxInstructionCodes.Wait;
                return true;
            }
        }

        value = 0;
        return false;
    }

    /// <summary>Returns the timed-record pointer for one zero-based cycle frame.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one contiguous presentation-color address within a frame.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
