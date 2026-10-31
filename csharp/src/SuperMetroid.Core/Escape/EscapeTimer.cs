using static SuperMetroid.Core.Escape.EscapeTimerDefinitions;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// The Ceres and Zebes escape countdown: a requested start installs whole minutes, the timer waits and then counts down
/// at the screen's centre, moves into its corner while counting, and reports expiry when the countdown reaches zero.
/// </summary>
/// <remarks>
/// The countdown is held in centiseconds and removes one or two each update by the cadence that makes 128 updates take
/// 2.13 seconds (<see cref="EscapeTimerCadence"/>). Counting below zero stops at zero.
/// </remarks>
public sealed class EscapeTimer
{
    /// <summary>The countdown a pending start installs.</summary>
    private int _pendingDuration;

    /// <summary>Updates since the start, counted through the two centre delays.</summary>
    private int _startupFrames;

    /// <summary>Which step of the timer's sequence the next <see cref="Process"/> call runs.</summary>
    public EscapeTimerPhase Phase { get; private set; }

    /// <summary>
    /// Whether the timer is running: set when a start installs the countdown, and cleared by a new request and by every
    /// clear.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>The countdown remaining, in centiseconds, 0 through <see cref="LongestTime"/>.</summary>
    public int RemainingCentiseconds { get; private set; }

    /// <summary>The minutes, seconds and centiseconds the timer shows.</summary>
    public EscapeTimeReadout Readout => EscapeTimeReadout.Of(RemainingCentiseconds);

    /// <summary>The timer's on-screen position, in 1/256 pixels.</summary>
    public Position16<ScreenSubpixels> Position { get; private set; }

    /// <summary>
    /// Whether the countdown has expired and Samus's timer handler only draws it: expiry installs the drawing-only
    /// handler, so the timer is never processed again until an escape restarts it.
    /// </summary>
    public bool CountdownExpired { get; private set; }

    /// <summary>Whole-pixel on-screen position the timer's label and digits are drawn from.</summary>
    public Position16<ScreenPixels> PixelPosition => Position.WholePixels();

    /// <summary>Requests the one-minute Ceres countdown; the next <see cref="Process"/> call starts it.</summary>
    public void RequestCeresStart() => RequestStart(CeresDuration);

    /// <summary>Requests the three-minute Mother Brain countdown; the next <see cref="Process"/> call starts it.</summary>
    public void RequestMotherBrainStart() => RequestStart(MotherBrainDuration);

    /// <summary>Runs one timer update and reports whether the countdown has reached zero.</summary>
    /// <param name="frameCounter">The game's frame counter, which selects this update's step of the countdown cadence.</param>
    /// <param name="preventEscapeTimeout">Host-only override: countdown continues normally until one second remains.</param>
    public bool Process(int frameCounter, bool preventEscapeTimeout = false)
    {
        if (CountdownExpired)
            return false;
        bool expired = Phase switch
        {
            EscapeTimerPhase.Inactive => false,
            EscapeTimerPhase.Starting => Start(),
            EscapeTimerPhase.InitialDelay => ProcessInitialDelay(),
            EscapeTimerPhase.CountingAtCentre => ProcessCentreDelay(frameCounter),
            EscapeTimerPhase.MovingIntoPlace => ProcessMovement(frameCounter),
            EscapeTimerPhase.InPlace => CountDown(frameCounter),
            _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerPhase)} {Phase}."),
        };

        // Apply the testing floor before publishing expiration to the gameplay dispatcher.
        // Startup delays and timer movement still run; inactive timers remain cleared.
        if (preventEscapeTimeout && IsActive && RemainingCentiseconds < TimeoutFloor)
        {
            RemainingCentiseconds = TimeoutFloor;
            return false;
        }
        CountdownExpired |= expired;
        return expired;
    }

    /// <summary>The time-up black-out stops the timer without clearing its countdown or position.</summary>
    public void ClearStatus() => SelectPhase(EscapeTimerPhase.Inactive);

    /// <summary>Sets the remaining countdown directly, for save-state loading and focused debugging.</summary>
    /// <param name="centiseconds">The remaining countdown, 0 through <see cref="LongestTime"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The countdown is negative or longer than the timer shows.</exception>
    public void SetRemaining(int centiseconds) =>
        RemainingCentiseconds = Ensure.BetweenInclusive(0, LongestTime, centiseconds);

    /// <summary>Stops the timer, empties its countdown and returns it to the screen's centre.</summary>
    public void Clear()
    {
        int centre = EightEight.FromWhole(ClearedPixel);
        Position = new((ushort)centre, (ushort)centre);
        _startupFrames = 0;
        _pendingDuration = 0;
        RemainingCentiseconds = 0;
        SelectPhase(EscapeTimerPhase.Inactive);
        CountdownExpired = false;
    }

    /// <summary>Requests a start that installs <paramref name="duration"/> centiseconds.</summary>
    private void RequestStart(int duration)
    {
        _pendingDuration = duration;
        SelectPhase(EscapeTimerPhase.Starting);
    }

    /// <summary>Clears the timer, installs the requested countdown, and enters the initial delay.</summary>
    private bool Start()
    {
        int duration = _pendingDuration;
        Clear();
        SetRemaining(duration);
        SelectRunningPhase(EscapeTimerPhase.InitialDelay);
        return false;
    }

    /// <summary>Waits at the centre without counting down.</summary>
    private bool ProcessInitialDelay()
    {
        _startupFrames++;
        if (_startupFrames >= InitialDelayFrames)
            AdvancePhase();

        // The initial delay never counts down, even on the update that ends it.
        return false;
    }

    /// <summary>Counts down at the centre until the movement delay has passed.</summary>
    private bool ProcessCentreDelay(int frameCounter)
    {
        _startupFrames++;
        if (_startupFrames >= MovementDelayFrames)
        {
            _startupFrames = 0;
            AdvancePhase();
        }

        return CountDown(frameCounter);
    }

    /// <summary>Moves toward the settled corner, clamping each axis there, while counting down.</summary>
    private bool ProcessMovement(int frameCounter)
    {
        int axesInPosition = 0;

        int settledX = EightEight.FromWhole(SettledX);
        int nextX = Position.X + RightwardSubpixelsPerFrame;
        if (nextX >= settledX)
        {
            axesInPosition++;
            nextX = settledX;
        }

        int settledY = EightEight.FromWhole(SettledY);
        int nextY = Position.Y - UpwardSubpixelsPerFrame;
        if (nextY < settledY)
        {
            axesInPosition++;
            nextY = settledY;
        }
        Position = new((ushort)nextX, (ushort)nextY);

        if (axesInPosition == 2)
            AdvancePhase();

        return CountDown(frameCounter);
    }

    /// <summary>Removes this update's step of the cadence, stopping at zero, and reports whether zero is reached.</summary>
    private bool CountDown(int frameCounter)
    {
        RemainingCentiseconds = Math.Max(0, RemainingCentiseconds - EscapeTimerCadence.Centiseconds(frameCounter));
        return RemainingCentiseconds == 0;
    }

    /// <summary>Selects the next step of the running sequence, keeping the timer active.</summary>
    private void AdvancePhase() => Phase = Phase switch
    {
        EscapeTimerPhase.InitialDelay
            or EscapeTimerPhase.CountingAtCentre
            or EscapeTimerPhase.MovingIntoPlace => EnumOrder.Next(Phase),
        EscapeTimerPhase.Inactive or
            EscapeTimerPhase.Starting or
            EscapeTimerPhase.InPlace => throw new InvalidOperationException($"The escape timer cannot advance from {Phase}."),
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerPhase)} {Phase}."),
    };

    /// <summary>Selects a step of the sequence with the timer stopped.</summary>
    private void SelectPhase(EscapeTimerPhase phase)
    {
        Phase = phase;
        IsActive = false;
    }

    /// <summary>Selects a step of the sequence with the timer running.</summary>
    internal void SelectRunningPhase(EscapeTimerPhase phase)
    {
        Phase = phase;
        IsActive = true;
    }
}
