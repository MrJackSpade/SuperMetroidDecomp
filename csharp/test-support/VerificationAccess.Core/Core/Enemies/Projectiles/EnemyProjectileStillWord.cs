namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A pre-instruction routine that stands for holding still: an empty routine, or the word zero of a freed slot.</summary>
internal sealed class EnemyProjectileStillWord : EnemyProjectileMotionWord
{
    /// <summary>The one routine kind that stands for holding still.</summary>
    internal static EnemyProjectileStillWord Instance { get; } = new();

    /// <summary>Only <see cref="Instance"/> exists.</summary>
    private EnemyProjectileStillWord()
    {
    }

    /// <inheritdoc/>
    internal override EnemyProjectileMotion? MotionOf(EnemyProjectileBehavior? behavior) => null;

    /// <summary>Names holding still.</summary>
    public override string ToString() => "holding still";
}
