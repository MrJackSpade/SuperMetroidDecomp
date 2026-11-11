namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The counter phase of the Torizos' hurt flash (#1606).</summary>
internal static class TorizoCadenceDefinitions
{
    /// <summary>
    /// The phase of a hurt Torizo's flash timer on which its body shows white: set on its odd counts, so the white flash
    /// alternates every update with its health or death colours.
    /// </summary>
    internal static readonly UpdateCounterField HurtWhiteShown = new(0, 1);
}
