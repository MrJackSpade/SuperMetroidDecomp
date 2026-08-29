using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile $8F8F fired by Crocomire's open-mouth volley.</summary>
public sealed partial class RoomEnemySystem
{
    private const int CrocomireProjectileGradientTable = 0x869059;
    private const int CrocomireProjectileSineTable = 0xa0b443;

    /// <summary>Ports <c>EprojInit_CrocomireProjectile</c> at $86:9023.</summary>
    private void SpawnCrocomireProjectile(RoomEnemySlot body, ushort spawnParameter)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireProjectile,
            graphicsIndex: 0x0a00);
        projectile.XVelocity = 0xfe00;
        projectile.YVelocity = 1;
        projectile.XPosition = unchecked((ushort)(body.XPosition - 32));
        projectile.YPosition = unchecked((ushort)(body.YPosition - 16));
        projectile.GeneralTimer = 0;
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.DirectionParameter = spawnParameter;
    }

    /// <summary>Ports the one-frame vector setup at $86:906B.</summary>
    private void StartCrocomireProjectileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        // The native setup performs one horizontal collision move and deliberately ignores
        // carry. It also writes the graphics word of physical projectile slot zero rather
        // than the current slot; retain that shipped indexing bug for debugger parity.
        MoveProjectileAxis(projectile, level, horizontal: true);
        _enemyProjectiles[0].GraphicsIndex = 0x0a00;
        projectile.GeneralTimer = unchecked((ushort)(
            projectile.GeneralTimer + projectile.XVelocity));

        // Spawn parameters are 2,4,...,18. The last index reads one word beyond the declared
        // nine-word gradient table on the cartridge. Reading the ROM address directly keeps
        // that documented OOB behavior instead of clamping it to a friendly host array.
        short gradient = unchecked((short)ReadWord(
            _bus!,
            CrocomireProjectileGradientTable + (projectile.DirectionParameter >> 1) * 2));
        byte angle = CalculateCartridgeAngle(-64, gradient);
        projectile.XVelocity = ReadCrocomireProjectileVelocity(
            unchecked((byte)(angle + 64)));
        projectile.YVelocity = ReadCrocomireProjectileVelocity(angle);
        projectile.PreInstruction = 0x90b3;
    }

    /// <summary>Ports <c>sub_8690B3</c>: delete on the first horizontal/vertical wall hit.</summary>
    private static void RunCrocomireProjectileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true) ||
            MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
        }
    }

    private ushort ReadCrocomireProjectileVelocity(byte angle)
    {
        short sample = unchecked((short)ReadWord(
            _bus!,
            CrocomireProjectileSineTable + angle * 2));
        return unchecked((ushort)(sample * 4));
    }
}
