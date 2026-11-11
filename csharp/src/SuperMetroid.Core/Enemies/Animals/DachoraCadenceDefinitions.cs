namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The counter phase on which the Dachora's speed echoes flicker (#1606).</summary>
internal static class DachoraCadenceDefinitions
{
    /// <summary>
    /// The phase of the frame counter of 0 through 255 on which a Dachora echo that shows on odd updates is shown: set
    /// on odd updates. An echo that shows on even updates is shown while it is clear.
    /// </summary>
    internal static readonly UpdateCounterField EchoOddUpdate = new(0, 1);
}
