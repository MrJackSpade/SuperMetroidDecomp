namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's native view of enemy-projectile pre-instructions: the bank-$86 routine words that stand for each motion
/// (#1843). Several empty routines stand for holding still, and the routines of every kind that moves one way stand for
/// <see cref="SingleMotion.Moving"/>, so checks decode a native word and compare motions: a projectile's through the
/// behaviour bound to it (#2571), a cue's by the motion it names.
/// </summary>
internal static class EnemyProjectileMotionAccess
{
    /// <summary>What the bank-$86 routine <paramref name="word"/> stands for: a motion of the projectile's behaviour, or holding still.</summary>
    internal static EnemyProjectileMotionWord FromWord(ushort word) => EnemyProjectileMotionWordsConstants.FromWord(word);

    /// <summary>Whether the bank-$86 routine <paramref name="word"/> stands for <paramref name="motion"/> of its family.</summary>
    /// <typeparam name="TMotion">The family's motions.</typeparam>
    internal static bool StandsFor<TMotion>(ushort word, TMotion motion) where TMotion : struct, Enum =>
        FromWord(word) is EnemyProjectileFamilyMotionWord<TMotion> family && EqualityComparer<TMotion>.Default.Equals(family.Motion, motion);

    /// <summary>Whether the bank-$86 routine <paramref name="word"/> stands for holding still.</summary>
    internal static bool StandsForHoldingStill(ushort word) => FromWord(word) is EnemyProjectileStillWord;

    /// <summary>The bank-$86 routine that flies a Space Pirate's or Mother Brain's laser <paramref name="direction"/> (#2278).</summary>
    internal static ushort PirateLaserFlight(HorizontalDirection direction) => EnemyProjectileMotionWords.PirateLaserFlights[direction];

    /// <summary>The bank-$86 routine that flies a Ninja Space Pirate's claw thrown <paramref name="direction"/> (#2278).</summary>
    internal static ushort PirateClawThrow(HorizontalDirection direction) => EnemyProjectileMotionWords.PirateClawThrows[direction];

    /// <summary>The bank-$86 routine that launches Ki-Hunter acid spat <paramref name="direction"/> (#2338).</summary>
    internal static ushort KiHunterAcidLaunch(HorizontalDirection direction) => EnemyProjectileMotionWordsConstants2.KiHunterAcidLaunches[direction];

    /// <summary>Every bank-$86 routine word that stands for a motion or for holding still, with the word zero of a freed slot.</summary>
    internal static IReadOnlyCollection<ushort> RoutineWords => EnemyProjectileMotionWords.Motions.Keys;
}
