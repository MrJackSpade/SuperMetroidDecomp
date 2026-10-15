using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The shared delete reaction, <c>InstList_EnemyProjectile_Delete</c> ($86:84FC): the shot or touch list of every projectile
/// without a typed reaction of its own. Its single command deletes the projectile on the pass after the reaction starts,
/// as the native list does.
/// </summary>
internal sealed class EnemyProjectileDeleteAnimation : EnemyProjectileAnimation
{
    /// <summary>The one shared reaction; it keeps no state.</summary>
    internal static EnemyProjectileDeleteAnimation Instance { get; } = new();

    /// <summary>Use <see cref="Instance"/>.</summary>
    private EnemyProjectileDeleteAnimation()
    {
    }

    /// <inheritdoc/>
    private protected override bool TryShowNextFrame(
        RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus, out int duration, out EnemyProjectileFramePresentation presentation)
    {
        projectile.Clear();
        duration = 0;
        presentation = default;
        return false;
    }
}
