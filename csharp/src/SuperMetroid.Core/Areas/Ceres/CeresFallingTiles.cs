using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The ceiling tiles that fall in Ceres station's escape, light and dark: their spawn into the room's enemy projectiles,
/// and their fall under gravity until each strikes the room, where it raises dust and sounds its fall.
/// </summary>
internal sealed partial class CeresFallingTiles
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
    internal SingleMotionProjectileBehavior Behavior => field ??= new(Drop);

    /// <summary>Moves a falling tile (<see cref="Behavior"/>).</summary>
    private void Drop(EnemyProjectileMotionContext context) => Drop(context.Projectile, context.Level);

    /// <summary>
    /// Spawns the falling tile the falling-debris room main requests: the tile starts falling at
    /// <paramref name="xPosition"/>, light or dark as the room's current random number chooses. The room main supplies
    /// the tile's animation, which shows its look, and the graphics it draws in. A spawn that finds every place held
    /// spawns nothing.
    /// </summary>
    internal void Spawn(int xPosition, EnemyProjectileAnimation animation, SpriteGraphics graphics)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.CeresFallingDebris, Behavior, graphics, animation);
        if (projectile is null)
            return;

        projectile.Position = projectile.Position.WithWhole(new(xPosition, CeresFallingDebrisDefinitions.StartingY));
        projectile.Velocity = new(SignedEightEight.Zero, CeresFallingDebrisDefinitions.StartingFallSpeed);
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
