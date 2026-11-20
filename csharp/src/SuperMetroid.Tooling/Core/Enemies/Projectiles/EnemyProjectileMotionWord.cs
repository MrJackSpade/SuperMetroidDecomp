namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a bank-$86 enemy-projectile pre-instruction routine stands for (#2571): one motion of the behaviour bound to the
/// projectile, which the word resolves through that behaviour, or holding still.
/// </summary>
internal abstract class EnemyProjectileMotionWord
{
    /// <summary>Only the codec's word kinds derive from it.</summary>
    private protected EnemyProjectileMotionWord()
    {
    }

    /// <summary>The motion the word stands for, of <paramref name="behavior"/>, or null for holding still.</summary>
    /// <param name="behavior">The behaviour bound to the projectile.</param>
    /// <exception cref="InvalidOperationException"><paramref name="behavior"/> does not own the motion the word stands for.</exception>
    internal abstract EnemyProjectileMotion? MotionOf(EnemyProjectileBehavior? behavior);

    /// <summary>
    /// The name of every motion of the family the word's motion belongs to, as <see cref="object.ToString"/> names a word
    /// that stands for it, so a check can confirm each motion stands for a routine; none for a word of no enum family.
    /// </summary>
    internal virtual IEnumerable<string> FamilyMotionNames => [];
}
