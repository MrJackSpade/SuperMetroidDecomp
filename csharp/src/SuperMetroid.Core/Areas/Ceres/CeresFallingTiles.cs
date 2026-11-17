using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The ceiling tiles that fall in Ceres station's escape, light and dark: their spawn into the room's enemy projectiles,
/// and their fall under gravity until each strikes the room, where it raises dust and sounds its fall.
/// </summary>
[RoomLifetime]
internal sealed class CeresFallingTiles
{
    /// <summary>The room's enemy projectiles, which hold the tiles and the dust they raise.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The enemy sound queue a tile's fall sounds in.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the tiles to the loaded room, its enemy projectiles and the enemy sound queue.</summary>
    internal CeresFallingTiles(EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _projectiles = projectiles;
        _audio = audio;
    }

    /// <summary>
    /// The Ceres falling tiles, light and dark (#1760): each falls under gravity until it strikes the room, where it
    /// raises dust and sounds its fall.
    /// </summary>
    internal SingleMotionProjectileBehavior Behavior => field ??= new(
        context => Drop(context.Projectile, context.Level));

    /// <summary>
    /// Spawns the light or dark falling tile the falling-debris room main requests at <paramref name="xPosition"/>. The X
    /// coordinate and light or dark variant come directly from the room's current random number; this setup owns
    /// only projectile fields. The room main supplies the tile's animation and the graphics it draws in. A spawn that
    /// finds every place held spawns nothing.
    /// </summary>
    internal void Spawn(int xPosition, bool dark, EnemyProjectileAnimation animation, SpriteGraphics graphics)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RoomEnemyProjectileKind kind = dark
            ? RoomEnemyProjectileKind.CeresFallingDebrisDark
            : RoomEnemyProjectileKind.CeresFallingDebrisLight;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, Behavior, graphics);
        projectile.Position = projectile.Position.WithWhole(new(xPosition, CeresFallingDebrisDefinitions.StartingY));
        projectile.Velocity = new(SignedEightEight.Zero, CeresFallingDebrisDefinitions.StartingFallSpeed);
        projectile.StartAnimation(animation);
    }

    /// <summary>Drops a tile, accelerating, and raises dust and sounds its fall where it strikes the room.</summary>
    private void Drop(EnemyProjectile projectile, RoomLevelData level)
    {
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + CeresFallingDebrisDefinitions.FallAcceleration);
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            Position16<RoomPixels> impact = projectile.Position.Whole;
            projectile.Delete();
            _projectiles.SpawnDustOrExplosion(impact, DustExplosionAnimation.SmallDustCloud);
            _audio.QueueEnemySound(EnemySound.CeresTileFall, priority: SoundRequestPriority.Normal);
        }
    }
}
