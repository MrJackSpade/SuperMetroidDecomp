using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// How the room stops the eye door's shots and sweat drops: they pass only through air of every kind, extension blocks
/// whose links lead nowhere further and grapple blocks, and every other block stops them flush against its face, slopes
/// included. A first block outside the room's grid is a wall.
/// </summary>
internal sealed class EyeDoorEffectCollision : EnemyProjectileBlockCollision
{
    /// <summary>The collision the eye door's shots and sweat drops move with.</summary>
    internal static EyeDoorEffectCollision Rule { get; } = new();

    /// <summary>The eye door's collision has no state; <see cref="Rule"/> is its one instance.</summary>
    private EyeDoorEffectCollision()
    {
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">A later block of the scan lies past the room's last block.</exception>
    protected override EnemyProjectileBlockReaction BlockReaction(
        RoomLevelData level,
        EnemyProjectileScannedMove move,
        Position16<RoomBlocks> block,
        bool firstOfScan)
    {

        // A first block outside the room is a wall, so every later block steps on from a block of the grid; one that runs
        // on past the room's last block reads terrain the room does not hold.
        if (!firstOfScan && !level.Size.Contains(block))
            throw new InvalidDataException("Enemy projectile block scan ran past the end of the room's level data.");
        if (EnemyTerrain.ResolveEnemyCollisionBlock(level, block) is not { } resolved)
            return EnemyProjectileBlockReaction.StopsAtFace;

        return resolved.CollisionType is (
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.ShootableAir or
            RoomCollisionType.UnrevealedAir or
            RoomCollisionType.BombableAir or
            RoomCollisionType.HorizontalExtension or
            RoomCollisionType.VerticalExtension or
            RoomCollisionType.GrappleBlock)
            ? EnemyProjectileBlockReaction.Passes
            : EnemyProjectileBlockReaction.StopsAtFace;
    }
}
