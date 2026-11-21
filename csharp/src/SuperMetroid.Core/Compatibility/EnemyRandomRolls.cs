namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The parts of the shared random number enemies' choices read where the game read neither the number's ordinary roll,
/// its second roll nor its range (#2563): a corpse's dust spread, a falling spark's launch, a Norfair lava jumper's
/// launch, a Polyp's rock, a scattered projectile's height side and a Norfair Rio's dive.
/// </summary>
/// <remarks>
/// Read at each choice's draw, through the definition or behaviour that names the choice. Evidence: each choice's
/// original masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays
/// compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which part of the shared random number each of these enemies' random choices reads, exactly as the game read it")]
internal static class EnemyRandomRolls
{
    /// <summary>
    /// The roll of two that moves a Tourian or Sidehopper corpse's dust cloud a fine step to the right: the number's second
    /// place.
    /// </summary>
    internal static RandomSampleField CorpseDustFineSpread { get; } = new(1, 1);

    /// <summary>
    /// The roll of four that moves a Tourian or Sidehopper corpse's dust cloud zero to three coarse steps to the right: the
    /// number's fourth and fifth places.
    /// </summary>
    internal static RandomSampleField CorpseDustCoarseSpread { get; } = new(3, 2);

    /// <summary>
    /// The roll of eight a falling spark's launch is chosen by: the number's third through fifth places.
    /// </summary>
    internal static RandomSampleField FallingSparkLaunch { get; } = new(2, 3);

    /// <summary>
    /// The roll of four a Norfair lava jumper's launch is chosen by: the number's tenth and eleventh places.
    /// </summary>
    internal static RandomSampleField NorfairLavaJumpLaunch { get; } = new(9, 2);

    /// <summary>
    /// The roll of sixteen that places a Polyp rock's rise, which a horizontal velocity's draw reads of its own number: the
    /// number's second through fifth places.
    /// </summary>
    internal static RandomSampleField PolypLaunchPlace { get; } = new(1, 4);

    /// <summary>
    /// The roll of eight a Polyp launch's cooldown is chosen by: the number's second through fourth places.
    /// </summary>
    internal static RandomSampleField PolypCooldown { get; } = new(1, 3);

    /// <summary>
    /// The roll of two that turns a scattered projectile's Y offset upward: the number's fifteenth place, beside the
    /// sixteenth that turns its X offset.
    /// </summary>
    internal static RandomSampleField ScatterYSide { get; } = new(14, 1);

    /// <summary>
    /// The roll of two that sends a diving Norfair Rio down at its slower launch velocity: the number's third place.
    /// </summary>
    internal static RandomSampleField NorfairRioSlowLaunch { get; } = new(2, 1);
}
