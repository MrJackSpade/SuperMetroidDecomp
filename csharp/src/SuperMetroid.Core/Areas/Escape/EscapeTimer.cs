using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Areas.Escape;

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
    /// Raw state word at WRAM <c>$0943</c>. The dispatch index is only its low byte;
    /// active timers conventionally retain flag bit 15, producing values <c>$8003-$8006</c>.
    /// </summary>
    public ushort RawStatus { get; internal set; }

    /// <summary>Low-byte dispatch state used by the seven-entry pointer table.</summary>
    public EscapeTimerState State => (EscapeTimerState)(byte)RawStatus;

    /// <summary>Whether the conventional active flag in status bit 15 is set.</summary>
    public bool IsActive => (RawStatus & EscapeTimerStatusDefinitions.ActiveFlag) != 0;

    /// <summary>Packed-BCD centiseconds at WRAM <c>$0945</c>.</summary>
    public PackedBcd Centiseconds { get; private set; }

    /// <summary>Packed-BCD seconds at WRAM <c>$0946</c>.</summary>
    public PackedBcd Seconds { get; private set; }

    /// <summary>Packed-BCD minutes at WRAM <c>$0947</c>.</summary>
    public PackedBcd Minutes { get; private set; }

    /// <summary>
    /// Raw unsigned 8.8 on-screen position words at WRAM <c>$0948</c> (X) and <c>$094A</c> (Y).
    /// The low byte of X is also the status counter used by states 3 and 4.
    /// </summary>
    public Position16<ScreenSubpixels> Position { get; private set; }

    /// <summary>
    /// Whether the countdown has expired and Samus's timer hack handler only draws it:
    /// <c>$90:E0E6</c> installs <c>SamusTimerHackHandler_DrawTimer</c> on expiry, so the
    /// timer is never processed again until an escape restarts it.
    /// </summary>
    public bool CountdownExpired { get; private set; }

    /// <summary>Whole-pixel on-screen position consumed by <c>$80:9FB3</c>'s spritemap drawing.</summary>
    public Position16<ScreenPixels> PixelPosition => Position.WholePixels();

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
    /// Global NMI frame counter. Only its low seven bits select the periodic correction schedule.
    /// </param>
    /// <param name="preventEscapeTimeout">Host-only override: countdown continues normally until one second remains.</param>
    public bool Process(int nmiFrameCounter, bool preventEscapeTimeout = false)
    {
        if (CountdownExpired)
            return false;
        // $80:9DEC masks status to eight bits before indexing a table of function pointers.
        // A switch expresses the same dispatch while leaving useful named frames in a C#
        // call stack. An invalid state throws where the original would jump through data.
        bool expired = State switch
        {
            EscapeTimerState.Inactive => false,
            EscapeTimerState.CeresStart => Start(PackedBcd.FromDecimal(1)),
            EscapeTimerState.MotherBrainStart => Start(PackedBcd.FromDecimal(3)),
            EscapeTimerState.InitialDelay => ProcessInitialDelay(),
            EscapeTimerState.RunningMovementDelayed => ProcessMovementDelay(nmiFrameCounter),
            EscapeTimerState.RunningMovingIntoPlace => ProcessMovement(nmiFrameCounter),
            EscapeTimerState.RunningInPlace => Decrement(nmiFrameCounter),
            _ => throw new InvalidOperationException($"Timer status low byte ${(byte)State:X2} has no $80:9DE7 dispatch entry."),
        };

        // Apply the testing floor before publishing expiration to the gameplay dispatcher.
        // Startup delays and timer movement still run; inactive timers remain cleared.
        if (preventEscapeTimeout && IsActive && Minutes == PackedBcd.Zero && Seconds == PackedBcd.Zero)
        {
            SetTime(PackedBcd.Zero, PackedBcd.FromDecimal(1), PackedBcd.Zero);
            return false;
        }
        CountdownExpired |= expired;
        return expired;
    }

    /// <summary><c>$82:8476</c>: the time-up black-out clears only the timer's status word.</summary>
    public void ClearStatus() => RawStatus = 0;

    /// <summary>
    /// Directly sets valid packed-BCD time for save-state loading and focused debugging.
    /// The ROM's <c>$80:9E8C</c> entry point only sets whole minutes, but explicit fields
    /// make boundary cases inspectable without advancing thousands of frames.
    /// </summary>
    public void SetTime(PackedBcd minutes, PackedBcd seconds, PackedBcd centiseconds)
    {
        ValidateBcd(minutes, nameof(minutes));
        ValidateBcd(seconds, nameof(seconds));
        ValidateBcd(centiseconds, nameof(centiseconds));
        if (seconds.Value > 59)
            throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be packed BCD in the range 00-59.");

        Minutes = minutes;
        Seconds = seconds;
        Centiseconds = centiseconds;
    }

    /// <summary>
    /// Resets the exact timer-owned WRAM fields as <c>$80:9E93</c> does.
    /// </summary>
    public void Clear()
    {
        // Pixel $80 with zero subpixel fraction. During startup that low fraction byte is
        // immediately repurposed as a delay counter.
        Position = new(EscapeTimerStatusDefinitions.ClearedOrigin, EscapeTimerStatusDefinitions.ClearedOrigin);
        Centiseconds = PackedBcd.Zero;
        Seconds = PackedBcd.Zero;
        Minutes = PackedBcd.Zero;
        RawStatus = 0;
        CountdownExpired = false;
    }

    /// <summary>Clears timer RAM, installs the selected whole-minute duration, and enters the initial delay.</summary>
    private bool Start(PackedBcd minutes)
    {
        // Both start functions clear all timer RAM, set a whole-minute value through the
        // seconds/minutes adjacent word, then select the active initial delay ($8003).
        Clear();
        SetTime(minutes, PackedBcd.Zero, PackedBcd.Zero);
        RawStatus = EscapeTimerStatusDefinitions.ActiveFlag | (ushort)EscapeTimerState.InitialDelay;
        return false;
    }

    /// <summary>Advances the X-fraction alias to sixteen without decrementing the countdown.</summary>
    private bool ProcessInitialDelay()
    {
        IncrementXLowByte();
        if ((byte)Position.X >= 16)
            IncrementStatusLowByte();

        // State 3 never decrements time, even on the update that advances to state 4.
        return false;
    }

    /// <summary>Counts down while the aliased X counter advances to its movement threshold.</summary>
    private bool ProcessMovementDelay(int nmiFrameCounter)
    {
        IncrementXLowByte();
        if ((byte)Position.X >= 96)
        {
            // STZ TimerXPosition-1 clears only the aliased counter/subpixel byte. The
            // visible high-byte X coordinate remains $80 until movement begins.
            Position = Position.WithX((ushort)(SnesLittleEndian.HighByteInPlace(Position.X)));
            IncrementStatusLowByte();
        }

        return Decrement(nmiFrameCounter);
    }

    /// <summary>Moves the 8.8 timer origin toward pixel (220, 48) while continuing the countdown.</summary>
    private bool ProcessMovement(int nmiFrameCounter)
    {
        int axesInPosition = 0;

        // X advances by 224/256 (0.875) pixel and clamps at pixel 220.
        ushort settledX = (ushort)SnesEightEight.FromWhole(220);
        ushort nextX = unchecked((ushort)(Position.X + 224));
        if (nextX >= settledX)
        {
            axesInPosition++;
            nextX = settledX;
        }
        Position = Position.WithX(nextX);

        // Y moves upward by 193/256 pixel and clamps at pixel 48. The unsigned comparison
        // reproduces CMP/BCS after 16-bit wrapped subtraction.
        ushort settledY = (ushort)SnesEightEight.FromWhole(48);
        ushort nextY = unchecked((ushort)(Position.Y - 193));
        if (nextY < settledY)
        {
            axesInPosition++;
            nextY = settledY;
        }
        Position = Position.WithY(nextY);

        if (axesInPosition == 2)
        {
            // Here the assembly is in 16-bit accumulator mode and INC affects the complete
            // word. For the normal $8005 value this yields $8006.
            RawStatus = unchecked((ushort)(RawStatus + 1));
        }

        return Decrement(nmiFrameCounter);
    }

    /// <summary>Subtracts this NMI's BCD correction and reports expiration at exactly zero.</summary>
    private bool Decrement(int nmiFrameCounter)
    {
        PackedBcd correction = PackedBcd.FromDecimal(EscapeTimerCadenceDefinitions.Centiseconds(nmiFrameCounter));
        PackedBcd one = PackedBcd.FromDecimal(1);
        Centiseconds = Centiseconds.Subtract(correction, out bool borrowedCentiseconds);

        if (borrowedCentiseconds)
        {
            Seconds = Seconds.Subtract(one, out bool borrowedSeconds);
            if (borrowedSeconds)
            {
                Minutes = Minutes.Subtract(one, out bool borrowedMinutes);
                if (borrowedMinutes)
                {
                    // Underflow below 00:00.00 saturates at zero rather than wrapping to
                    // 99:99.99. This is the .clearTimer branch at $80:9ED7.
                    Centiseconds = PackedBcd.Zero;
                    Seconds = PackedBcd.Zero;
                    Minutes = PackedBcd.Zero;
                }
                else
                {
                    // A successful minute decrement wraps seconds to 59. Centiseconds
                    // already wrapped to 98 or 99 according to this frame's correction.
                    Seconds = PackedBcd.FromDecimal(59);
                }
            }
        }

        // The assembly ORs centiseconds with the adjacent seconds/minutes word and returns
        // carry set only when all three are zero.
        return Centiseconds == PackedBcd.Zero && Seconds == PackedBcd.Zero && Minutes == PackedBcd.Zero;
    }

    /// <summary>Increments the X fraction used as a startup counter while preserving visible X.</summary>
    private void IncrementXLowByte()
    {
        byte nextCounter = unchecked((byte)((byte)Position.X + 1));
        Position = Position.WithX((ushort)((SnesLittleEndian.HighByteInPlace(Position.X)) | nextCounter));
    }

    /// <summary>Advances the dispatch state while preserving the active high-byte flag.</summary>
    private void IncrementStatusLowByte()
    {
        byte nextState = unchecked((byte)((byte)RawStatus + 1));
        RawStatus = (ushort)((SnesLittleEndian.HighByteInPlace(RawStatus)) | nextState);
    }

    /// <summary>Rejects a timer byte whose high or low nibble is not a decimal digit.</summary>
    private static void ValidateBcd(PackedBcd value, string parameterName)
    {
        if (!value.IsValid)
            throw new ArgumentOutOfRangeException(parameterName, value.Raw, "Value must contain two valid packed-BCD digits.");
    }
}
