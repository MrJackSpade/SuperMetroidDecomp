namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// How a block an enemy projectile's axis move scans reacts to the move (<see cref="EnemyProjectileBlockCollision"/>): it
/// lets the move pass, stops it against its face, or stops it and aligns the projectile at a place of its own on the axis.
/// </summary>
internal abstract partial record EnemyProjectileBlockReaction
{
    /// <summary>Only the nested cases derive.</summary>
    private EnemyProjectileBlockReaction()
    {
    }

    /// <summary>The block lets the move pass.</summary>
    internal static EnemyProjectileBlockReaction Passes { get; } = new Passing();

    /// <summary>The block stops the move against its face.</summary>
    internal static EnemyProjectileBlockReaction StopsAtFace { get; } = new AtFace();

    /// <summary>The block stops the move and places the projectile at <paramref name="Position"/> on the move's axis.</summary>
    /// <param name="Position">Where the block places the projectile on the move's axis.</param>
    internal sealed partial record StopsAligned(int Position) : EnemyProjectileBlockReaction;

    /// <summary>The block lets the move pass.</summary>
    private sealed partial record Passing : EnemyProjectileBlockReaction;

    /// <summary>The block stops the move against its face.</summary>
    private sealed partial record AtFace : EnemyProjectileBlockReaction;
}
