namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The parts of the shared random number Crocomire's acid smoke and dust read for their side (#2563): neither the number's
/// ordinary roll, its second roll nor its range, but its second place, beside the offset the ordinary roll draws.
/// </summary>
/// <remarks>
/// Read at each choice's draw, through the definition or behaviour that names the choice. Evidence: each choice's
/// original masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays
/// compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which part of the shared random number Crocomire's acid smoke reads for its side, exactly as the game read it")]
internal static partial class CrocomireRandomRolls
{
    /// <summary>
    /// The roll of two the acid smoke's random number makes for the smoke's side: the number's second place, which the
    /// offset's ordinary roll also reads.
    /// </summary>
    internal static RandomSampleField AcidSmokeSide { get; } = new(1, 1);

    /// <summary>The first sample of the half of the random range that raises a step toward the spike wall's dust on his left (#2182).</summary>
    private const int SpikeWallDustLeftFrom = 2048;

    /// <summary>The first sample of the half of the random range that mirrors a foot dust puff to his other side (#2182).</summary>
    private const int FootDustMirroredFrom = 4096;

    /// <summary>
    /// Whether a step toward the spike wall raises its dust on his left (#2563): one random number in two, the half of the
    /// random range from 2048 on (<see cref="BossRandomRolls.InHalfFrom"/>).
    /// </summary>
    /// <param name="sample">The step's random number, 0 through 65535.</param>
    internal static bool SpikeWallDustOnLeft(int sample) => BossRandomRolls.InHalfFrom(sample, SpikeWallDustLeftFrom);

    /// <summary>
    /// Whether a random foot dust puff is mirrored to his other side (#2563): one random number in two, the half of the
    /// random range from 4096 on (<see cref="BossRandomRolls.InHalfFrom"/>).
    /// </summary>
    /// <param name="sample">The puff's random number, 0 through 65535.</param>
    internal static bool FootDustMirrored(int sample) => BossRandomRolls.InHalfFrom(sample, FootDustMirroredFrom);
}
