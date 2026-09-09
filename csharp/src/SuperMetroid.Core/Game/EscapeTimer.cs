namespace SuperMetroid.Core.Game;

/// <summary>
/// Exact state-machine port of Super Metroid's Ceres/Mother Brain escape timer at
/// <c>$80:9DE7-$80:9EEB</c>.
/// </summary>
/// <remarks>
/// Time bytes are packed binary-coded decimal (BCD): hexadecimal <c>$59</c> means decimal
/// 59, not decimal 89. Positions are unsigned 8.8 fixed point, with the high byte used as
/// the on-screen pixel coordinate. Most surprisingly, the low byte of X position is reused
/// as the pre-animation state counter. The original WRAM layout really aliases those jobs;
/// this class keeps that behavior visible rather than inventing a separate clean counter.
/// </remarks>
public sealed class EscapeTimer
{
    /// <summary>
    /// NTSC centisecond corrections copied from ROM <c>$80:9EEC-$80:9F6B</c>. Over 128
    /// video frames the timer removes 213 centiseconds: 43 table entries are 1 and 85 are
    /// 2. This compensates for the NTSC frame rate without using floating-point time.
    /// </summary>
    private static ReadOnlySpan<byte> CentisecondDecrements => [
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2,
    ];

    /// <summary>
    /// Raw state word at WRAM <c>$0943</c>. The dispatch index is only its low byte;
    /// active timers conventionally retain flag bit 15, producing values <c>$8003-$8006</c>.
    /// </summary>
    public ushort RawStatus { get; private set; }

    /// <summary>Low-byte dispatch state used by the seven-entry pointer table.</summary>
    public EscapeTimerState State => (EscapeTimerState)(byte)RawStatus;

    /// <summary>Whether the conventional active flag in status bit 15 is set.</summary>
    public bool IsActive => (RawStatus & 0x8000) != 0;

    /// <summary>Packed-BCD centiseconds at WRAM <c>$0945</c>.</summary>
    public byte CentisecondsBcd { get; private set; }

    /// <summary>Packed-BCD seconds at WRAM <c>$0946</c>.</summary>
    public byte SecondsBcd { get; private set; }

    /// <summary>Packed-BCD minutes at WRAM <c>$0947</c>.</summary>
    public byte MinutesBcd { get; private set; }

    /// <summary>
    /// Raw unsigned 8.8 X position at WRAM <c>$0948</c>. Its low byte is also the status
    /// counter used by states 3 and 4.
    /// </summary>
    public ushort XPositionFixed { get; private set; }

    /// <summary>Raw unsigned 8.8 Y position at WRAM <c>$094A</c>.</summary>
    public ushort YPositionFixed { get; private set; }

    /// <summary>Integer X coordinate consumed by <c>$80:9FB3</c>'s spritemap drawing.</summary>
    public byte XPixel => (byte)(XPositionFixed >> 8);

    /// <summary>Integer Y coordinate consumed by <c>$80:9FB3</c>'s spritemap drawing.</summary>
    public byte YPixel => (byte)(YPositionFixed >> 8);

    /// <summary>
    /// Requests the one-minute Ceres sequence by selecting dispatch state 1. The next call
    /// to <see cref="Process"/> performs the initialization, matching the main game's
    /// indirect state-machine convention.
    /// </summary>
    public void RequestCeresStart() => RawStatus = (ushort)EscapeTimerState.CeresStart;

    /// <summary>
    /// Requests the three-minute Mother Brain sequence by selecting dispatch state 2.
    /// </summary>
    public void RequestMotherBrainStart() => RawStatus = (ushort)EscapeTimerState.MotherBrainStart;

    /// <summary>
    /// Runs one timer update and returns the carry-style result: true once all three BCD
    /// fields have reached zero.
    /// </summary>
    /// <param name="nmiFrameCounter">
    /// Global NMI frame counter. Only its low seven bits index the correction table.
    /// </param>
    /// <param name="preventEscapeTimeout">Host-only override: countdown continues normally until one second remains.</param>
    public bool Process(ushort nmiFrameCounter, bool preventEscapeTimeout = false)
    {
        // $80:9DEC masks status to eight bits before indexing a table of function pointers.
        // A switch expresses the same dispatch while leaving useful named frames in a C#
        // call stack. An invalid state throws where the original would jump through data.
        bool expired = State switch
        {
            EscapeTimerState.Inactive => false,
            EscapeTimerState.CeresStart => Start(minutesBcd: 0x01),
            EscapeTimerState.MotherBrainStart => Start(minutesBcd: 0x03),
            EscapeTimerState.InitialDelay => ProcessInitialDelay(),
            EscapeTimerState.RunningMovementDelayed => ProcessMovementDelay(nmiFrameCounter),
            EscapeTimerState.RunningMovingIntoPlace => ProcessMovement(nmiFrameCounter),
            EscapeTimerState.RunningInPlace => Decrement(nmiFrameCounter),
            _ => throw new InvalidOperationException($"Timer status low byte ${(byte)State:X2} has no $80:9DE7 dispatch entry."),
        };

        // Apply the testing floor before publishing expiration to the gameplay dispatcher.
        // Startup delays and timer movement still run; inactive timers remain cleared.
        // Packed BCD compares naturally here because the threshold is one whole second.
        if (preventEscapeTimeout && IsActive && MinutesBcd == 0 && SecondsBcd == 0)
        {
            SetTime(0, 1, 0);
            return false;
        }
        return expired;
    }

    /// <summary>
    /// Directly sets valid packed-BCD time for save-state loading and focused debugging.
    /// The ROM's <c>$80:9E8C</c> entry point only sets whole minutes, but explicit fields
    /// make boundary cases inspectable without advancing thousands of frames.
    /// </summary>
    public void SetTime(byte minutesBcd, byte secondsBcd, byte centisecondsBcd)
    {
        ValidateBcd(minutesBcd, nameof(minutesBcd));
        ValidateBcd(secondsBcd, nameof(secondsBcd));
        ValidateBcd(centisecondsBcd, nameof(centisecondsBcd));
        if (secondsBcd > 0x59)
            throw new ArgumentOutOfRangeException(nameof(secondsBcd), "Seconds must be packed BCD in the range 00-59.");
        if (centisecondsBcd > 0x99)
            throw new ArgumentOutOfRangeException(nameof(centisecondsBcd));

        MinutesBcd = minutesBcd;
        SecondsBcd = secondsBcd;
        CentisecondsBcd = centisecondsBcd;
    }

    /// <summary>
    /// Resets the exact timer-owned WRAM fields as <c>$80:9E93</c> does.
    /// </summary>
    public void Clear()
    {
        // $8000 corresponds to pixel coordinate $80 with zero subpixel fraction. During
        // startup that low fraction byte is immediately repurposed as a delay counter.
        XPositionFixed = 0x8000;
        YPositionFixed = 0x8000;
        CentisecondsBcd = 0;
        SecondsBcd = 0;
        MinutesBcd = 0;
        RawStatus = 0;
    }

    /// <summary>
    /// Performs an 8-bit decimal-mode SBC and reports whether it borrowed. This helper is
    /// public so the processor-specific behavior can be verified independently.
    /// </summary>
    public static byte SubtractPackedBcd(byte number, byte value, out bool borrowed)
    {
        // This mirrors the native decomp's DecrementDecimal helper. Compute the low nibble
        // with the implicit input carry set, then apply the 65C816's decimal correction.
        int result = (number & 0x0f) + (~value & 0x0f) + 1;
        if (result < 0x10)
        {
            int correctedLow = result - 0x06;
            result = correctedLow & (correctedLow < 0 ? 0x0f : 0x1f);
        }

        // Add the high digits and correct a decimal borrow by subtracting six from the
        // upper nibble. Values may become negative here; the final byte cast intentionally
        // retains the processor's low eight result bits.
        result = (number & 0xf0) + (~value & 0xf0) + result;
        if (result < 0x100)
            result -= 0x60;

        borrowed = result < 0x100;
        return unchecked((byte)result);
    }

    private bool Start(byte minutesBcd)
    {
        // Both start functions clear all timer RAM, set a whole-minute value through the
        // seconds/minutes adjacent word, then select state $8003.
        Clear();
        SetTime(minutesBcd, secondsBcd: 0, centisecondsBcd: 0);
        RawStatus = 0x8003;
        return false;
    }

    private bool ProcessInitialDelay()
    {
        IncrementXLowByte();
        if ((byte)XPositionFixed >= 0x10)
            IncrementStatusLowByte();

        // State 3 never decrements time, even on the update that advances to state 4.
        return false;
    }

    private bool ProcessMovementDelay(ushort nmiFrameCounter)
    {
        IncrementXLowByte();
        if ((byte)XPositionFixed >= 0x60)
        {
            // STZ TimerXPosition-1 clears only the aliased counter/subpixel byte. The
            // visible high-byte X coordinate remains $80 until movement begins.
            XPositionFixed &= 0xff00;
            IncrementStatusLowByte();
        }

        return Decrement(nmiFrameCounter);
    }

    private bool ProcessMovement(ushort nmiFrameCounter)
    {
        int axesInPosition = 0;

        // X advances by $00E0 (0.875 pixels) and clamps at $DC00 (pixel 220).
        ushort nextX = unchecked((ushort)(XPositionFixed + 0x00e0));
        if (nextX >= 0xdc00)
        {
            axesInPosition++;
            nextX = 0xdc00;
        }
        XPositionFixed = nextX;

        // Y moves upward by $00C1 (193/256 pixel) and clamps at $3000 (pixel 48).
        // The unsigned comparison reproduces CMP/BCS after 16-bit wrapped subtraction.
        ushort nextY = unchecked((ushort)(YPositionFixed - 0x00c1));
        if (nextY < 0x3000)
        {
            axesInPosition++;
            nextY = 0x3000;
        }
        YPositionFixed = nextY;

        if (axesInPosition == 2)
        {
            // Here the assembly is in 16-bit accumulator mode and INC affects the complete
            // word. For the normal $8005 value this yields $8006.
            RawStatus = unchecked((ushort)(RawStatus + 1));
        }

        return Decrement(nmiFrameCounter);
    }

    private bool Decrement(ushort nmiFrameCounter)
    {
        byte correction = CentisecondDecrements[nmiFrameCounter & 0x7f];
        CentisecondsBcd = SubtractPackedBcd(CentisecondsBcd, correction, out bool borrowedCentiseconds);

        if (borrowedCentiseconds)
        {
            SecondsBcd = SubtractPackedBcd(SecondsBcd, 1, out bool borrowedSeconds);
            if (borrowedSeconds)
            {
                MinutesBcd = SubtractPackedBcd(MinutesBcd, 1, out bool borrowedMinutes);
                if (borrowedMinutes)
                {
                    // Underflow below 00:00.00 saturates at zero rather than wrapping to
                    // 99:99.99. This is the .clearTimer branch at $80:9ED7.
                    CentisecondsBcd = 0;
                    SecondsBcd = 0;
                    MinutesBcd = 0;
                }
                else
                {
                    // A successful minute decrement wraps seconds to 59. Centiseconds
                    // already wrapped to 98 or 99 according to this frame's correction.
                    SecondsBcd = 0x59;
                }
            }
        }

        // The assembly ORs centiseconds with the adjacent seconds/minutes word and returns
        // carry set only when all three are zero.
        return (CentisecondsBcd | SecondsBcd | MinutesBcd) == 0;
    }

    private void IncrementXLowByte()
    {
        byte nextCounter = unchecked((byte)((byte)XPositionFixed + 1));
        XPositionFixed = (ushort)((XPositionFixed & 0xff00) | nextCounter);
    }

    private void IncrementStatusLowByte()
    {
        byte nextState = unchecked((byte)((byte)RawStatus + 1));
        RawStatus = (ushort)((RawStatus & 0xff00) | nextState);
    }

    private static void ValidateBcd(byte value, string parameterName)
    {
        if ((value & 0x0f) > 9 || ((value >> 4) & 0x0f) > 9)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must contain two valid packed-BCD digits.");
    }
}

/// <summary>Low-byte entries in the dispatch table at <c>$80:9DFB</c>.</summary>
public enum EscapeTimerState : byte
{
    Inactive = 0,
    CeresStart = 1,
    MotherBrainStart = 2,
    InitialDelay = 3,
    RunningMovementDelayed = 4,
    RunningMovingIntoPlace = 5,
    RunningInPlace = 6,
}
