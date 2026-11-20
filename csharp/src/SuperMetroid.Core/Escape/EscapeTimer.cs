using static SuperMetroid.Core.Escape.EscapeTimerDefinitions;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// The Ceres and Zebes escape countdown: a requested start installs whole minutes, the timer waits and then counts down
/// at the screen's centre, moves into its corner while counting, and reports expiry when the countdown reaches zero.
/// </summary>
/// <remarks>
/// The countdown is held in centiseconds and removes one or two each update by the timer's cadence
/// (<see cref="EscapeTimerCadence"/>). Counting below zero stops at zero. An expired countdown is only drawn
/// until the time-up stops it (#2499).
/// </remarks>
public sealed class EscapeTimer
{
    /// <summary>The countdown a pending start installs.</summary>
    private int _pendingDuration;

    /// <summary>
    /// The centre delay under way (#2499): the initial delay, started as the start enters it, then the centre count's,
    /// started as the initial delay ends.
    /// </summary>
    private UpdateCountdown _delay = UpdateCountdown.Ended();

    /// <summary>Which step of the timer's sequence the timer's next update (<see cref="Process"/>) runs.</summary>
    public EscapeTimerPhase Phase { get; private set; }

    /// <summary>
    /// Whether the timer is running and shown: from the initial delay its start enters, through its expiry, until a clear,
    /// the time-up's stop or a new request.
    /// </summary>
    public bool IsActive => Phase is not (EscapeTimerPhase.Inactive or EscapeTimerPhase.Starting or EscapeTimerPhase.StoppedAtTimeUp);

    /// <summary>The countdown remaining, in centiseconds, 0 through <see cref="LongestTime"/>.</summary>
    private int RemainingCentiseconds { get; set; }

    /// <summary>The minutes, seconds and centiseconds the timer shows.</summary>
    public EscapeTimeReadout Readout => EscapeTimeReadout.Of(RemainingCentiseconds);

    /// <summary>
    /// The timer's signed place on the screen, from the screen's top-left corner, its fractions kept as it slides into its
    /// corner.
    /// </summary>
    private FixedVector<ScreenPixels> Position { get; set; }

    /// <summary>The whole-pixel screen place the timer's label and digits are drawn from.</summary>
    public Offset16<ScreenPixels> ScreenPlace => Position.Whole;

    /// <summary>Requests the one-minute Ceres countdown; the timer's next update (<see cref="Process"/>) starts it.</summary>
    public void RequestCeresStart() => RequestStart(CeresDuration);

    /// <summary>Requests the three-minute Mother Brain countdown; the timer's next update (<see cref="Process"/>) starts it.</summary>
    public void RequestMotherBrainStart() => RequestStart(MotherBrainDuration);

    /// <summary>Runs one timer update and reports whether the countdown ran out on it.</summary>
    /// <param name="frameCounter">The game's frame counter, which selects this update's step of the countdown cadence.</param>
    /// <param name="holdAtOneSecond">Whether the countdown holds at one second instead of expiring, as an assist can ask; it runs normally until then.</param>
    public bool Process(int frameCounter, bool holdAtOneSecond)
    {
        // An expired countdown is only drawn: it never counts, holds or expires again until an escape restarts it.
        bool expired = Phase switch
        {
            EscapeTimerPhase.Inactive or EscapeTimerPhase.StoppedAtTimeUp or EscapeTimerPhase.Expired => false,
            EscapeTimerPhase.Starting => Start(),
            EscapeTimerPhase.InitialDelay => ProcessInitialDelay(),
            EscapeTimerPhase.CountingAtCentre => ProcessCentreDelay(frameCounter),
            EscapeTimerPhase.MovingIntoPlace => ProcessMovement(frameCounter),
            EscapeTimerPhase.InPlace => CountDown(frameCounter),
            _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerPhase)} {Phase}."),
        };

        // Apply the assist's floor before publishing expiration to the gameplay loop.
        // Startup delays and timer movement still run; inactive and expired timers keep their countdown.
        if (holdAtOneSecond && IsActive && Phase != EscapeTimerPhase.Expired && RemainingCentiseconds < TimeoutFloor)
        {
            RemainingCentiseconds = TimeoutFloor;
            return false;
        }
        if (expired)
            Phase = EscapeTimerPhase.Expired;
        return expired;
    }

    /// <summary>The time-up black-out stops the timer without clearing its countdown or position.</summary>
    public void StopAtTimeUp() => Phase = EscapeTimerPhase.StoppedAtTimeUp;

    /// <summary>Sets the remaining countdown, as starting the timer installs its duration.</summary>
    /// <param name="centiseconds">The remaining countdown, 0 through <see cref="LongestTime"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The countdown is negative or longer than the timer shows.</exception>
    private void SetRemaining(int centiseconds) =>
        RemainingCentiseconds = Ensure.BetweenInclusive(0, LongestTime, centiseconds);

    /// <summary>Stops the timer, empties its countdown and returns it to the screen's centre.</summary>
    public void Clear()
    {
        Position = FixedVector<ScreenPixels>.FromWholePixels(Offset16<ScreenPixels>.Of(ClearedPixel, ClearedPixel));
        _pendingDuration = 0;
        RemainingCentiseconds = 0;
        Phase = EscapeTimerPhase.Inactive;
    }

    /// <summary>Requests a start that installs <paramref name="duration"/> centiseconds.</summary>
    private void RequestStart(int duration)
    {
        _pendingDuration = duration;
        Phase = EscapeTimerPhase.Starting;
    }

    /// <summary>Clears the timer, begins the requested countdown, and enters the initial delay.</summary>
    private bool Start()
    {
        int duration = _pendingDuration;
        Clear();
        SetRemaining(duration);
        Phase = EscapeTimerPhase.InitialDelay;
        _delay = UpdateCountdown.Lasting(InitialDelayFrames);
        return false;
    }

    /// <summary>Waits at the centre without counting down, then begins the centre count.</summary>
    private bool ProcessInitialDelay()
    {
        if (_delay.StepEnds())
        {
            Phase = EscapeTimerPhase.CountingAtCentre;
            _delay = UpdateCountdown.Lasting(CentreCountingFrames);
        }

        // The initial delay never counts down, even on the update that ends it.
        return false;
    }

    /// <summary>Counts down at the centre until the centre count's delay has passed.</summary>
    private bool ProcessCentreDelay(int frameCounter)
    {
        if (_delay.StepEnds())
            Phase = EscapeTimerPhase.MovingIntoPlace;

        return CountDown(frameCounter);
    }

    /// <summary>Moves toward the settled corner, clamping each axis there, while counting down.</summary>
    private bool ProcessMovement(int frameCounter)
    {
        int axesInPosition = 0;

        SignedSixteenSixteen settledX = SignedSixteenSixteen.FromWholePixels(SettledX);
        SignedSixteenSixteen nextX = Position.X + RightwardSpeed.ToSixteenSixteenDelta();
        if (!(nextX - settledX).IsNegative)
        {
            axesInPosition++;
            nextX = settledX;
        }

        SignedSixteenSixteen settledY = SignedSixteenSixteen.FromWholePixels(SettledY);
        SignedSixteenSixteen nextY = Position.Y - UpwardSpeed.ToSixteenSixteenDelta();
        if ((nextY - settledY).IsNegative)
        {
            axesInPosition++;
            nextY = settledY;
        }
        Position = new(nextX, nextY);

        if (axesInPosition == 2)
            Phase = EscapeTimerPhase.InPlace;

        return CountDown(frameCounter);
    }

    /// <summary>Removes this update's step of the cadence, stopping at zero, and reports whether zero is reached.</summary>
    private bool CountDown(int frameCounter)
    {
        RemainingCentiseconds = Math.Max(0, RemainingCentiseconds - EscapeTimerCadence.Centiseconds(frameCounter));
        return RemainingCentiseconds == 0;
    }
}
