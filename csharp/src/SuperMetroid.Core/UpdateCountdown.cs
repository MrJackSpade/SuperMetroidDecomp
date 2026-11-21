namespace SuperMetroid.Core;

/// <summary>
/// A wait counted in updates at its true length (#2541, #2561): the updates it has left, the one it ends on included. The
/// owner that waits counts it once on each of its updates and reads from it whether it is over, its age or the updates it
/// has left. A wait of <c>n</c> updates is over on its <c>n</c>th count; a wait that is over stays over until it is
/// restarted. Every wait of the game is one of these or an animation segment, apart from the boss fights' own
/// (<see cref="Bosses.BossCountdown"/>, <see cref="Bosses.HeldBossCountdown"/>) and the timers whose wrap past zero the
/// game shows (<see cref="Compatibility.WrappingCountdown"/>).
/// </summary>
/// <remarks>
/// Three ways of counting cover what owners do once the wait is over: <see cref="Count"/> for an owner that never counts
/// an over wait (counting one is a defect and throws), <see cref="Step"/> for an owner that asks every update whether the
/// wait is over, and <see cref="StepEnds"/> for an owner that acts only on the update the wait ends.
/// </remarks>
public sealed partial class UpdateCountdown
{
    /// <summary>A wait lasting <paramref name="updates"/> updates with all of them left.</summary>
    private UpdateCountdown(int updates)
    {
        Length = updates;
        UpdatesLeft = updates;
    }

    /// <summary>The updates the wait lasts, as it was last started: none for a wait made over.</summary>
    [IntegerQuantity("updates the wait lasts")]
    public int Length { get; internal set; }

    /// <summary>The updates the wait has left, the one it ends on included: none once it is over.</summary>
    [IntegerQuantity("updates the wait has left")]
    public int UpdatesLeft { get; internal set; }

    /// <summary>The updates the wait has counted so far, from none at its start to <see cref="Length"/> once it is over.</summary>
    public int Age => Length - UpdatesLeft;

    /// <summary>Whether the wait has counted none of its updates yet.</summary>
    public bool IsUncounted => UpdatesLeft == Length;

    /// <summary>Whether the wait has counted its first update and no other.</summary>
    public bool HasCountedOnlyItsFirst => Length != 0 && UpdatesLeft == Length - 1;

    /// <summary>Whether the wait has counted every one of its updates.</summary>
    public bool IsOver => UpdatesLeft == 0;

    /// <summary>Whether the wait still has updates left.</summary>
    public bool IsRunning => UpdatesLeft != 0;

    /// <summary>A wait of <paramref name="updates"/> updates that starts now, its first count on its first update.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal static UpdateCountdown Lasting(int updates) => new(Ensure.BetweenInclusive(1, int.MaxValue, updates));

    /// <summary>A wait of a single update that starts now: it is over on its first count.</summary>
    internal static UpdateCountdown LastingOneUpdate() => Lasting(1);

    /// <summary>A wait that is already over: one not yet started, or one whose owner has nothing to wait for.</summary>
    internal static UpdateCountdown Ended() => new(0);

    /// <summary>
    /// A wait of <paramref name="updates"/> updates that starts now, or one already over when there are none: a wait whose
    /// length is chosen by the update that starts it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is negative.</exception>
    internal static UpdateCountdown For(int updates) => new(Ensure.BetweenInclusive(0, int.MaxValue, updates));

    /// <summary>Starts the wait again, lasting <paramref name="updates"/> updates; a wait restarted for none is over at once.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is negative.</exception>
    internal void Restart(int updates)
    {
        Length = Ensure.BetweenInclusive(0, int.MaxValue, updates);
        UpdatesLeft = updates;
    }

    /// <summary>
    /// Changes how long the wait lasts to <paramref name="updates"/>, keeping the updates it has counted: a wait that has
    /// already counted as many is over.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is negative.</exception>
    internal void LastFor(int updates)
    {
        int age = Age;
        Length = Ensure.BetweenInclusive(0, int.MaxValue, updates);
        UpdatesLeft = Math.Max(0, updates - age);
    }

    /// <summary>Gives the wait <paramref name="updates"/> more updates than it has left, lengthening it by as many.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is negative.</exception>
    internal void Lengthen(int updates)
    {
        Ensure.BetweenInclusive(0, int.MaxValue - Length, updates);
        Length += updates;
        UpdatesLeft += updates;
    }

    /// <summary>Ends the wait now, with the updates it had left uncounted.</summary>
    internal void End()
    {
        Length -= UpdatesLeft;
        UpdatesLeft = 0;
    }

    /// <summary>Counts one update of a wait its owner never counts once it is over.</summary>
    /// <returns>Whether this count ended the wait.</returns>
    /// <exception cref="InvalidOperationException">The wait is already over.</exception>
    internal bool Count()
    {
        if (IsOver)
            throw new InvalidOperationException($"A wait of {Length} updates is counted after it is over.");
        UpdatesLeft--;
        return IsOver;
    }

    /// <summary>Counts one update of a running wait; a wait that is over stays over.</summary>
    /// <returns>Whether the wait is over after this update: on the update it ends and every one after.</returns>
    internal bool Step()
    {
        if (IsRunning)
            UpdatesLeft--;
        return IsOver;
    }

    /// <summary>Counts one update of a running wait; a wait that is over stays over.</summary>
    /// <returns>Whether this update ended the wait: never once it was already over.</returns>
    internal bool StepEnds() => IsRunning && Count();

    /// <inheritdoc />
    public override string ToString() => $"{UpdatesLeft} of {Length} updates left";
}
