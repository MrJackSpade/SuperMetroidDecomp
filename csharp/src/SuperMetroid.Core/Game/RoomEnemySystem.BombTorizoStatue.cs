using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Allocates the room-graphics enemy projectile requested by PLM instruction
    /// <c>$84:D357</c>, then runs initializer <c>$86:A764</c> against the publishing PLM's
    /// block coordinates.
    /// </summary>
    public RoomEnemyProjectileSlot? SpawnBombTorizoStatueBreakingProjectile(
        BombTorizoStatueProjectileRequest request)
    {
        EnsureLoaded();
        if (request.DefinitionPointer !=
            BombTorizoStatueFragmentDefinitions.ProjectileDefinition)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Bomb Torizo statue projectile must use definition $A993, not " +
                $"${request.DefinitionPointer:X4}.");
        }
        BombTorizoStatueFragmentDefinition definition =
            BombTorizoStatueFragmentDefinitions.ForParameter(request.Parameter);

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return null; // SpawnEprojInner silently drops work when all eighteen slots fill.

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoStatueBreaking,
            graphicsIndex: 0);

        projectile.InstructionPointer = definition.InstructionList;
        projectile.XPosition = unchecked((ushort)(
            request.PlmBlockX * 16 + definition.XOffset));
        projectile.YPosition = unchecked((ushort)(
            request.PlmBlockY * 16 + definition.YOffset));
        projectile.YVelocity = definition.YVelocity;
        projectile.Variable1 = definition.Acceleration;
        return projectile;
    }

    /// <summary>
    /// Ports pre-instruction <c>$86:A8EF</c>, including its authentic unindexed read from
    /// physical slot zero's variable-F word when applying acceleration.
    /// </summary>
    private void RunBombTorizoStatueBreakingPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        bool collided = MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.YVelocity) < 0 || !collided)
        {
            // The initializer writes $0010 to every fragment's own F word, but A8EF's ROM
            // instruction omits X and reads `$1B23` (physical slot zero) literally. The
            // retail quirk normally leaves these first eight actors at constant speed;
            // retaining it matters if another projectile already occupies slot zero.
            projectile.YVelocity = unchecked((ushort)(
                projectile.YVelocity + _enemyProjectiles[0].Variable1));
            if ((projectile.YVelocity & 0xf000) == 0x1000)
                projectile.YVelocity = 0x1000;
            return;
        }

        projectile.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_BombTorizoStatueFragment_Stopped;
    }
}
