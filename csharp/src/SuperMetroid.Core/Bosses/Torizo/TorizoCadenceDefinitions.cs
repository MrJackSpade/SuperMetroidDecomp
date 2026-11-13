namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The counter phases of the Torizos' hurt flash (#1606) and attack choice (#1619).</summary>
internal static class TorizoCadenceDefinitions
{
    /// <summary>
    /// The phase of a Torizo attack choice's count on which it takes the first of its two attacks (#1619): set for eight
    /// counts, then clear for eight. The count is the frame counter offset by Samus's X in two-pixel steps
    /// (<see cref="TorizoChoices.InFirstAttackPhase"/>), so the choice alternates every eight updates and Samus moving
    /// shifts it.
    /// </summary>
    internal static readonly UpdateCounterField FirstAttackPhase = new(3, 1);

    /// <summary>
    /// The phase of a hurt Torizo's flash timer on which its body shows white: set on its odd counts, so the white flash
    /// alternates every update with its health or death colours.
    /// </summary>
    internal static readonly UpdateCounterField HurtWhiteShown = new(0, 1);
}
