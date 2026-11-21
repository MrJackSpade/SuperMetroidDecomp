namespace SuperMetroid.Core.Fx;

/// <summary>
/// The darkness the Firefleas killed in a Fireflea room bring (#2367): one step deeper for each Fireflea killed, from none
/// through five, the deepest, which every later death keeps (<see cref="FirefleaFxDefinitions.DarknessLevels"/>, #2573).
/// From the fifth death the room stops flashing.
/// </summary>
public readonly record struct FirefleaDarkness
{
    /// <summary>The darkness of <paramref name="firefleasKilled"/> Firefleas killed.</summary>
    /// <param name="firefleasKilled">The Firefleas killed in the room, none through five.</param>
    /// <exception cref="ArgumentOutOfRangeException">The darkness has no step for that many deaths.</exception>
    internal FirefleaDarkness(int firefleasKilled) =>
        FirefleasKilled = Ensure.BetweenInclusive(0, FirefleaFxDefinitions.DarknessLevels.Length - 1, firefleasKilled);

    /// <summary>The Firefleas killed in the room whose darkness shows, none through five.</summary>
    internal int FirefleasKilled { get; }

    /// <summary>The fade levels of grey the darkness subtracts from the room.</summary>
    public int Level => FirefleaFxDefinitions.DarknessLevels[FirefleasKilled];

    /// <summary>
    /// The darkness one more Fireflea death brings: a step deeper, or, past the fifth death, which has the deepest step,
    /// the same darkness.
    /// </summary>
    internal FirefleaDarkness Deeper => FirefleasKilled + 1 < FirefleaFxDefinitions.DarknessLevels.Length ? new(FirefleasKilled + 1) : this;

    /// <summary>Whether the room has stopped flashing and holds its flash's brightest look.</summary>
    internal bool HoldsFlash => FirefleasKilled >= FirefleaFxDefinitions.KilledToHoldFlash;
}
