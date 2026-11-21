namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The counter phase on which the Dachora's speed echoes flicker (#1606).</summary>
internal static class DachoraCadenceDefinitions
{
    /// <summary>
    /// The phase of the frame counter on which a Dachora echo that shows on odd updates is shown:
    /// on odd updates. An echo that shows on even updates is shown on the others.
    /// </summary>
    internal static readonly UpdateAlternation EchoOddUpdate = new(1);
}
