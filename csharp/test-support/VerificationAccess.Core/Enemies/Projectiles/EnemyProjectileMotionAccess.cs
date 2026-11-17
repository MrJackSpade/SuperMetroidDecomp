namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's native view of enemy-projectile pre-instructions: the bank-$86 routine words that stand for each motion
/// (#1843). Several empty routines stand for holding still, and the routines of every kind that moves one way stand for
/// <see cref="SingleMotion.Moving"/>, so checks decode a native word and compare motions.
/// </summary>
internal static class EnemyProjectileMotionAccess
{
    /// <summary>The motion the bank-$86 routine <paramref name="word"/> stands for, or null for holding still.</summary>
    internal static Enum? FromWord(ushort word) => EnemyProjectileMotionWordsConstants.FromWord(word);

    /// <summary>The bank-$86 routine that flies a Space Pirate's or Mother Brain's laser <paramref name="direction"/> (#2278).</summary>
    internal static ushort PirateLaserFlight(HorizontalDirection direction) => EnemyProjectileMotionWords.PirateLaserFlights[direction];

    /// <summary>The bank-$86 routine that flies a Ninja Space Pirate's claw thrown <paramref name="direction"/> (#2278).</summary>
    internal static ushort PirateClawThrow(HorizontalDirection direction) => EnemyProjectileMotionWords.PirateClawThrows[direction];

    /// <summary>The bank-$86 routine that launches Ki-Hunter acid spat <paramref name="direction"/> (#2338).</summary>
    internal static ushort KiHunterAcidLaunch(HorizontalDirection direction) => EnemyProjectileMotionWordsConstants2.KiHunterAcidLaunches[direction];

    /// <summary>Every bank-$86 routine word that stands for a motion or for holding still, with the word zero of a freed slot.</summary>
    internal static IReadOnlyCollection<ushort> RoutineWords => EnemyProjectileMotionWords.Motions.Keys;
}
