namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared small-explosion pose timing in $86:E138 and $86:ED69. Their six OAM
/// compositions at $8D:B023-B082 and BDFF-BE5E are identical; the enemy-death
/// program inserts sound and pickup behavior without changing these phases.
/// </summary>
internal static class SmallExplosionAnimationDefinitions
{
    /// <summary>$86:E138/ED69, Common_SmallExplosion_0/EnemyDeathExplosion_0: one centered 8x8 ignition tile $5F.</summary>
    private const ushort IgnitionDuration = 4;
    /// <summary>$86:E13C/ED6D, Common_SmallExplosion_1/EnemyDeathExplosion_1: four 8x8 tiles $8A form the initial 16x16 expansion.</summary>
    private const ushort InitialExpansionDuration = 6;
    /// <summary>$86:E140-E148/ED71-ED7B, Common_SmallExplosion_2..4: three 32x32 burst poses use tile groups $90/$92/$94.</summary>
    private const ushort ExpandedBurstDuration = 5;
    /// <summary>$86:E14C/ED7F, Common_SmallExplosion_5/EnemyDeathExplosion_5: tile group $96 holds the final fading pose before termination.</summary>
    private const ushort FinalFadeDuration = 6;

    /// <summary>Returns the authored update hold for one of the six shared small-explosion poses.</summary>
    /// <param name="frame">Zero-based pose index from ignition (0) through the final fade (5).</param>
    /// <returns>The pose hold in updates: 4 for ignition, 6 for initial expansion, 5 for each burst pose, or 6 for the final fade.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the six-pose sequence.</exception>
    internal static ushort Duration(int frame) => frame switch
    {
        0 => IgnitionDuration,
        1 => InitialExpansionDuration,
        >= 2 and <= 4 => ExpandedBurstDuration,
        5 => FinalFadeDuration,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
