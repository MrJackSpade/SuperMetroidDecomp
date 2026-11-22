namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The parts of the shared random number Mother Brain's choices read where the game read neither the number's ordinary
/// roll, its second roll nor its range (#2563): a falling glass shard's sparkle and her forward inch's wait.
/// </summary>
/// <remarks>
/// Read at each choice's draw, through the definition or behaviour that names the choice. Evidence: each choice's
/// original masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays
/// compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which part of the shared random number each of Mother Brain's random choices reads, exactly as the game read it")]
internal static partial class MotherBrainRandomRolls
{
    /// <summary>
    /// The chance a falling glass shard sparkles each update, one in four: the number's sixth and eleventh places both
    /// clear.
    /// </summary>
    internal static RandomClearBitsChance ShardSparkles { get; } = RandomClearBitsChance.WhenAllClear(new RandomSampleField(5, 1), new RandomSampleField(10, 1));

    /// <summary>
    /// The roll of two that makes phase three's forward inch wait longer: the number's second place.
    /// </summary>
    internal static RandomSampleField ForwardInchWait { get; } = new(1, 1);
}
