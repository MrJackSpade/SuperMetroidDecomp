using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Escape;

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
    /// <summary>Which step of the timer's sequence the next <see cref="Process"/> call runs.</summary>
    public EscapeTimerState State { get; private set; }

    /// <summary>
    /// Whether the timer is running: set when a start request installs the countdown, and cleared by a new request and
    /// by every clear.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>Packed-BCD centiseconds at WRAM <c>$0945</c>.</summary>
    public PackedDecimal Centiseconds { get; private set; }

    /// <summary>Packed-BCD seconds at WRAM <c>$0946</c>.</summary>
    public PackedDecimal Seconds { get; private set; }

    /// <summary>Packed-BCD minutes at WRAM <c>$0947</c>.</summary>
    public PackedDecimal Minutes { get; private set; }

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
    public void RequestCeresStart() => SelectStatus(EscapeTimerState.CeresStart);

    /// <summary>
    /// Requests the three-minute Mother Brain sequence by selecting dispatch state 2.
    /// </summary>
    public void RequestMotherBrainStart() => SelectStatus(EscapeTimerState.MotherBrainStart);

    /// <summary>
    /// Runs one timer update and returns the carry-style result: true once all three BCD
    /// fields have reached zero.
    /// </summary>
    /// <param name="frameCounter">
    /// The game's frame counter. Only its low seven bits select the periodic correction schedule.
    /// </param>
    /// <param name="preventEscapeTimeout">Host-only override: countdown continues normally until one second remains.</param>
    public bool Process(int frameCounter, bool preventEscapeTimeout = false)
    {
        if (CountdownExpired)
            return false;
        // $80:9DEC masks status to eight bits before indexing a table of function pointers.
        // A switch expresses the same dispatch while leaving useful named frames in a C#
        // call stack. An invalid state throws where the original would jump through data.
        bool expired = State switch
        {
            EscapeTimerState.Inactive => false,
            EscapeTimerState.CeresStart => Start(PackedDecimal.FromDecimal(1)),
            EscapeTimerState.MotherBrainStart => Start(PackedDecimal.FromDecimal(3)),
            EscapeTimerState.InitialDelay => ProcessInitialDelay(),
            EscapeTimerState.RunningMovementDelayed => ProcessMovementDelay(frameCounter),
            EscapeTimerState.RunningMovingIntoPlace => ProcessMovement(frameCounter),
            EscapeTimerState.RunningInPlace => Decrement(frameCounter),
            _ => throw new InvalidOperationException($"Escape timer state {State} has no dispatch entry."),
        };

        // Apply the testing floor before publishing expiration to the gameplay dispatcher.
        // Startup delays and timer movement still run; inactive timers remain cleared.
        if (preventEscapeTimeout && IsActive && Minutes == PackedDecimal.Zero && Seconds == PackedDecimal.Zero)
        {
            SetTime(PackedDecimal.Zero, PackedDecimal.FromDecimal(1), PackedDecimal.Zero);
            return false;
        }
        CountdownExpired |= expired;
        return expired;
    }

    /// <summary><c>$82:8476</c>: the time-up black-out clears only the timer's status word.</summary>
    public void ClearStatus() => SelectStatus(EscapeTimerState.Inactive);

    /// <summary>
    /// Directly sets valid packed-BCD time for save-state loading and focused debugging.
    /// The ROM's <c>$80:9E8C</c> entry point only sets whole minutes, but explicit fields
    /// make boundary cases inspectable without advancing thousands of frames.
    /// </summary>
    public void SetTime(PackedDecimal minutes, PackedDecimal seconds, PackedDecimal centiseconds)
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
        Centiseconds = PackedDecimal.Zero;
        Seconds = PackedDecimal.Zero;
        Minutes = PackedDecimal.Zero;
        SelectStatus(EscapeTimerState.Inactive);
        CountdownExpired = false;
    }

    /// <summary>Clears timer RAM, installs the selected whole-minute duration, and enters the initial delay.</summary>
    private bool Start(PackedDecimal minutes)
    {
        // Both start functions clear all timer RAM, set a whole-minute value through the
        // seconds/minutes adjacent word, then select the active initial delay ($8003).
        Clear();
        SetTime(minutes, PackedDecimal.Zero, PackedDecimal.Zero);
        SelectRunningStatus(EscapeTimerState.InitialDelay);
        return false;
    }

    /// <summary>Advances the X-fraction alias to sixteen without decrementing the countdown.</summary>
    private bool ProcessInitialDelay()
    {
        IncrementXLowByte();
        if ((byte)Position.X >= 16)
            AdvanceState();

        // State 3 never decrements time, even on the update that advances to state 4.
        return false;
    }

    /// <summary>Counts down while the aliased X counter advances to its movement threshold.</summary>
    private bool ProcessMovementDelay(int frameCounter)
    {
        IncrementXLowByte();
        if ((byte)Position.X >= 96)
        {
            // STZ TimerXPosition-1 clears only the aliased counter/subpixel byte. The
            // visible high-byte X coordinate remains $80 until movement begins.
            Position = Position.WithX((ushort)(SnesLittleEndian.HighByteInPlace(Position.X)));
            AdvanceState();
        }

        return Decrement(frameCounter);
    }

    /// <summary>Moves the 8.8 timer origin toward pixel (220, 48) while continuing the countdown.</summary>
    private bool ProcessMovement(int frameCounter)
    {
        int axesInPosition = 0;

        // X advances by 224/256 (0.875) pixel and clamps at pixel 220.
        int settledX = (ushort)EightEight.FromWhole(220);
        int nextX = unchecked((ushort)(Position.X + 224));
        if (nextX >= settledX)
        {
            axesInPosition++;
            nextX = settledX;
        }
        Position = Position.WithX(checked((ushort)nextX));

        // Y moves upward by 193/256 pixel and clamps at pixel 48. The unsigned comparison
        // reproduces CMP/BCS after 16-bit wrapped subtraction.
        int settledY = (ushort)EightEight.FromWhole(48);
        int nextY = unchecked((ushort)(Position.Y - 193));
        if (nextY < settledY)
        {
            axesInPosition++;
            nextY = settledY;
        }
        Position = Position.WithY(checked((ushort)nextY));

        if (axesInPosition == 2)
        {
            AdvanceState();
        }

        return Decrement(frameCounter);
    }

    /// <summary>Subtracts this NMI's BCD correction and reports expiration at exactly zero.</summary>
    private bool Decrement(int frameCounter)
    {
        PackedDecimal correction = PackedDecimal.FromDecimal(EscapeTimerCadenceDefinitions.Centiseconds(frameCounter));
        PackedDecimal one = PackedDecimal.FromDecimal(1);
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
                    Centiseconds = PackedDecimal.Zero;
                    Seconds = PackedDecimal.Zero;
                    Minutes = PackedDecimal.Zero;
                }
                else
                {
                    // A successful minute decrement wraps seconds to 59. Centiseconds
                    // already wrapped to 98 or 99 according to this frame's correction.
                    Seconds = PackedDecimal.FromDecimal(59);
                }
            }
        }

        // The assembly ORs centiseconds with the adjacent seconds/minutes word and returns
        // carry set only when all three are zero.
        return Centiseconds == PackedDecimal.Zero && Seconds == PackedDecimal.Zero && Minutes == PackedDecimal.Zero;
    }

    /// <summary>Increments the X fraction used as a startup counter while preserving visible X.</summary>
    private void IncrementXLowByte()
    {
        int nextCounter = unchecked((byte)((byte)Position.X + 1));
        Position = Position.WithX((ushort)((SnesLittleEndian.HighByteInPlace(Position.X)) | nextCounter));
    }

    /// <summary>Selects the next step of the running sequence, keeping the timer active.</summary>
    private void AdvanceState() => State = State switch
    {
        EscapeTimerState.InitialDelay
            or EscapeTimerState.RunningMovementDelayed
            or EscapeTimerState.RunningMovingIntoPlace => EnumOrder.Next(State),
        EscapeTimerState.Inactive or
            EscapeTimerState.CeresStart or
            EscapeTimerState.MotherBrainStart or
            EscapeTimerState.RunningInPlace => throw new InvalidOperationException($"The escape timer cannot advance from {State}."),
        _ => throw new InvalidOperationException($"The escape timer cannot advance from {State}."),
    };

    /// <summary>Selects a step of the sequence with the timer stopped.</summary>
    internal void SelectStatus(EscapeTimerState state)
    {
        State = state;
        IsActive = false;
    }

    /// <summary>Selects a step of the sequence with the timer running.</summary>
    internal void SelectRunningStatus(EscapeTimerState state)
    {
        State = state;
        IsActive = true;
    }

    /// <summary>Rejects a timer byte whose high or low nibble is not a decimal digit.</summary>
    private static void ValidateBcd(PackedDecimal value, string parameterName)
    {
        if (!value.IsValid)
            throw new ArgumentOutOfRangeException(parameterName, value.Raw, "Value must contain two valid packed-BCD digits.");
    }
}
