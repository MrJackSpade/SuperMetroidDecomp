namespace SuperMetroid.Core.Fx;

/// <summary>
/// The darkness the Firefleas killed in a Fireflea room bring (#2367): one step deeper for each Fireflea killed, from none
/// through five, and the faint darkness a sixth death shows (<see cref="FirefleaFxDefinitions.DarknessLevels"/>). From the
/// fifth death the room stops flashing.
/// </summary>
public readonly record struct FirefleaDarkness
{
    /// <summary>The darkness of <paramref name="firefleasKilled"/> Firefleas killed.</summary>
    /// <param name="firefleasKilled">The Firefleas killed in the room, none through six.</param>
    /// <exception cref="ArgumentOutOfRangeException">The darkness has no step for that many deaths.</exception>
    public FirefleaDarkness(int firefleasKilled) =>
        FirefleasKilled = Ensure.BetweenInclusive(0, FirefleaFxDefinitions.DarknessLevels.Length - 1, firefleasKilled);

    /// <summary>The Firefleas killed in the room, none through six.</summary>
    public int FirefleasKilled { get; }

    /// <summary>The fade levels of grey the darkness subtracts from the room.</summary>
    public int Level => FirefleaFxDefinitions.DarknessLevels[FirefleasKilled];

    /// <summary>Whether the room has stopped flashing and holds its flash's brightest look.</summary>
    internal bool HoldsFlash => FirefleasKilled >= FirefleaFxDefinitions.KilledToHoldFlash;
}
