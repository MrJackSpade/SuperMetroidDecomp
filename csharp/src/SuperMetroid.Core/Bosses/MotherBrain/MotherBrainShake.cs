namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// A shake of Mother Brain's brain or of the Baby Metroid struck by her onion ring (#2469): the updates it has left,
/// counted down to none, and the offset it shows, which its cadence steps through the shared shake's four offsets as the
/// updates count down (<see cref="MotherBrainShakeDefinitions.Offsets"/>).
/// </summary>
/// <param name="cadence">The shake's cadence: which of the four offsets each count of updates left shows.</param>
internal sealed class MotherBrainShake(UpdateCounterField cadence)
{
    /// <summary>The updates the shake has left, zero or more; none once it has stopped.</summary>
    public int UpdatesLeft { get; private set; }

    /// <summary>Whether the shake has updates left.</summary>
    internal bool IsShaking => UpdatesLeft != 0;

    /// <summary>Whether the shake still had updates left when it last counted one off.</summary>
    internal bool ShookAtLastStep { get; private set; }

    /// <summary>The offset the shake shows with the updates it has left.</summary>
    internal Offset16<RoomPixels> Offset => MotherBrainShakeDefinitions.Offsets[cadence.Of(UpdatesLeft)];

    /// <summary>Starts the shake over with <paramref name="updates"/> updates left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below zero.</exception>
    internal void ShakeFor(int updates) => UpdatesLeft = Ensure.AtLeastZero(updates);

    /// <summary>Stops the shake: it has no update left.</summary>
    internal void Stop() => UpdatesLeft = 0;

    /// <summary>Counts one of the shake's updates off, while it has any left.</summary>
    internal void Step()
    {
        ShookAtLastStep = IsShaking;
        if (ShookAtLastStep)
            UpdatesLeft--;
    }
}
