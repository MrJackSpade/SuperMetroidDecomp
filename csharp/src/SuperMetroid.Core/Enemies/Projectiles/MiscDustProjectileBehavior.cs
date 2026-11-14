namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The miscellaneous dust clouds and explosions (#1760): each holds where it was raised while its animation plays and
/// leaves once outside the camera. Their spawners start the animation they choose.
/// </summary>
internal sealed class MiscDustProjectileBehavior : EnemyProjectileBehavior
{
    /// <summary>The one dust behaviour; it keeps no state.</summary>
    internal static MiscDustProjectileBehavior Instance { get; } = new();

    /// <summary>Use <see cref="Instance"/>.</summary>
    private MiscDustProjectileBehavior()
    {
    }

    /// <inheritdoc/>
    private protected override void Move(EnemyProjectileFrameBehaviorContext context)
    {
        if (context.Projectile.FrameBehavior == EnemyProjectileFrameBehavior.DustCameraCull)
            CullMiscDustOutsideCamera(context.Projectile, context.Camera);
        else
            base.Move(context);
    }

    /// <summary>Runs the strict 256x256 camera cull for dust/explosion actors.</summary>
    internal static void CullMiscDustOutsideCamera(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        // The cull compares the position against camera + 256 and culls when not below it, so
        // a projectile exactly 256 past the camera is already off-screen.
        if (CoordinateAxis.Difference(projectile.Position.X.Whole, camera.X) < 0 ||
            CoordinateAxis.Difference(projectile.Position.X.Whole, (camera.X + 256)) >= 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, camera.Y) < 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, (camera.Y + 256)) >= 0)
        {
            projectile.Delete();
        }
    }
}
