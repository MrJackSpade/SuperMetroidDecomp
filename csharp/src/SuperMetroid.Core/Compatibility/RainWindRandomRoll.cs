namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The part of the shared random number a rainy room's load reads to choose its wind (#2563): neither the number's
/// ordinary roll, its second roll nor its range, but a roll of four from its third and fourth places.
/// </summary>
/// <remarks>
/// Read at each choice's draw, through the definition or behaviour that names the choice. Evidence: each choice's
/// original masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays
/// compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which part of the shared random number a rainy room reads to choose its wind, exactly as the game read it")]
internal static class RainWindRandomRoll
{
    /// <summary>
    /// The roll of four a rainy room's wind is chosen by: the number's third and fourth places.
    /// </summary>
    internal static RandomSampleField Wind { get; } = new(2, 2);
}
