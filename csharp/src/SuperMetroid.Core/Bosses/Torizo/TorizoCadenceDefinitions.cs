namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The counter phases of the Torizos' hurt flash (#1606) and attack choice (#1619).</summary>
internal static class TorizoCadenceDefinitions
{
    /// <summary>
    /// The phase of a Torizo attack choice's count on which it takes the first of its two attacks (#1619): for half
    /// its counts, then not for the other half. The count is the frame counter offset by Samus's X in two-pixel steps
    /// (<see cref="TorizoChoices.InFirstAttackPhase"/>), so the choice alternates with the alternation's half-period and Samus moving
    /// shifts it.
    /// </summary>
    internal static readonly UpdateAlternation FirstAttackPhase = new(8);

    /// <summary>
    /// The phase of a hurt Torizo's flash timer on which its body shows white: on its odd counts, so the white flash
    /// alternates every update with its health or death colours.
    /// </summary>
    internal static readonly UpdateAlternation HurtWhiteShown = new(1);
}
