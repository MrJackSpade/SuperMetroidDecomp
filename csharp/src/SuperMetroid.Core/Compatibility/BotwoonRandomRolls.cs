namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The parts of the shared random number Botwoon's walks read to choose a path and the hole it leads to (#2563): neither the
/// number's ordinary roll, its second roll nor its range, but a roll of four from its fourth and fifth places.
/// </summary>
/// <remarks>
/// Read at each choice's draw, through the definition or behaviour that names the choice. Evidence: each choice's
/// original masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays
/// compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which part of the shared random number Botwoon's walk reads to choose its destination, exactly as the game read it")]
internal static class BotwoonRandomRolls
{
    /// <summary>
    /// The roll of four a walk's random draw makes to choose where the walk leads: the draw's fourth and fifth places, so
    /// the ordinary roll's lowest three places do not move it.
    /// </summary>
    internal static RandomSampleField WalkDestination { get; } = new(3, 2);

    /// <summary>
    /// The roll of eight that decides whether Botwoon, leaving a hole at full speed, follows an authored path: the number's
    /// second through fourth places.
    /// </summary>
    internal static RandomSampleField PathChoice { get; } = new(1, 3);
}
