namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The miscellaneous dust clouds and explosions (#1760): each holds where it was raised while its animation plays and
/// leaves once outside the camera. Their spawners start the animation they choose.
/// </summary>
internal sealed partial class DustExplosionProjectileBehavior : SingleMotionProjectileBehavior
{
    /// <summary>The one dust behaviour; it keeps no state.</summary>
    internal static DustExplosionProjectileBehavior Instance { get; } = new();

    /// <summary>Use <see cref="Instance"/>.</summary>
    private DustExplosionProjectileBehavior()
        : base(context => CullDustExplosionOutsideCamera(context.Projectile, context.Camera))
    {
    }

    /// <summary>Removes a dust cloud or explosion once it lies outside the view, the view's right and bottom edges already outside.</summary>
    internal static void CullDustExplosionOutsideCamera(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        if (!EnemyView.ContainsBeforeFarEdges(projectile.Position.Whole, camera))
            projectile.Delete();
    }
}
