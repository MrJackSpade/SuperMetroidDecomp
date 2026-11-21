namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// A shake of Mother Brain's brain or of the Baby Metroid struck by her onion ring (#2469): the updates it has left,
/// counted down to none, and the offset it shows, which its cadence steps through the shared shake's four offsets as the
/// updates count down (<see cref="MotherBrainShakeDefinitions.Offsets"/>).
/// </summary>
/// <param name="cadence">The shake's cadence: which of the four offsets each count of updates left shows.</param>
internal sealed partial class MotherBrainShake(UpdateCycle cadence)
{
    /// <summary>The shake's updates, over once it has stopped.</summary>
    private readonly UpdateCountdown left = UpdateCountdown.Ended();

    /// <summary>The updates the shake has left, zero or more; none once it has stopped.</summary>
    public int UpdatesLeft => left.UpdatesLeft;

    /// <summary>Whether the shake has updates left.</summary>
    internal bool IsShaking => left.IsRunning;

    /// <summary>The offset the shake shows with the updates it has left.</summary>
    internal Offset16<RoomPixels> Offset => OffsetOn(cadence, UpdatesLeft);

    /// <summary>
    /// The offset a shake stepped by <paramref name="cadence"/> shows when its counter reads <paramref name="counter"/>
    /// (#2556): the shakes that run on a frame counter rather than on updates left.
    /// </summary>
    internal static Offset16<RoomPixels> OffsetOn(UpdateCycle cadence, int counter) =>
        MotherBrainShakeDefinitions.Offsets[cadence.StepAt(counter)];

    /// <summary>Starts the shake over with <paramref name="updates"/> updates left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below zero.</exception>
    internal void ShakeFor(int updates) => left.Restart(updates);

    /// <summary>Stops the shake: it has no update left.</summary>
    internal void Stop() => left.End();

    /// <summary>Counts one of the shake's updates off, while it has any left.</summary>
    /// <returns>Whether the shake still had an update left to count off.</returns>
    internal bool Step()
    {
        bool shook = IsShaking;
        left.Step();
        return shook;
    }
}
