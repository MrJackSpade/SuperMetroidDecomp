namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's view of the motion an enemy projectile moves by (#2571): whether it holds still or moves by a given
/// motion object of its behaviour, the motion a decoded native word stands for through that behaviour, and the motion a
/// snapshot import starts.
/// </summary>
internal static class EnemyProjectileMotionValueAccess
{
    /// <summary>The motion views of a projectile.</summary>
    extension(EnemyProjectile self)
    {
        /// <summary>Whether the projectile holds still, moving by no motion of its own.</summary>
        internal bool HoldsStill => self.Motion is null;

        /// <summary>Whether the projectile moves by <paramref name="motion"/>, one of its behaviour's motion objects.</summary>
        /// <param name="motion">One of the behaviour's motions.</param>
        internal bool IsMovingBy(EnemyProjectileMotion motion) => ReferenceEquals(self.Motion, motion);

        /// <summary>
        /// The motion the bank-$86 routine <paramref name="word"/> stands for, of the behaviour bound to the projectile, or
        /// null for holding still.
        /// </summary>
        /// <param name="word">A native pre-instruction routine.</param>
        /// <exception cref="InvalidOperationException">The projectile's behaviour does not own the motion the word stands for.</exception>
        internal EnemyProjectileMotion? MotionOfWord(ushort word) => EnemyProjectileMotionAccess.FromWord(word).MotionOf(self.Behavior);

        /// <summary>
        /// Starts the motion the bank-$86 routine <paramref name="word"/> stands for on the projectile through its behaviour,
        /// or holds it still.
        /// </summary>
        /// <param name="word">A native pre-instruction routine.</param>
        /// <exception cref="InvalidOperationException">The projectile's behaviour does not own the motion the word stands for.</exception>
        internal void StartMotionFromWord(ushort word)
        {
            if (self.MotionOfWord(word) is { } motion)
                self.StartMotion(motion);
            else
                self.HoldStill();
        }
    }
}
