using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile $8F8F fired by Crocomire's open-mouth volley.</summary>
public sealed partial class RoomEnemySystem
{
    private const int CrocomireProjectileGradientTable = 0x869059;
    private const int CrocomireProjectileSineTable = 0xa0b443;

    // These three tables are indexed by the physical bank-$86 projectile slot. Crocomire
    // deliberately clears the pool and then allocates eight actors from slot 17 downward,
    // so entries 10..17 produce the wall's authored fan rather than eight identical shards.
    private static readonly ushort[] CrocomireSpikeAccelerationDelta =
    [
        0x0000, 0x0000, 0x0ff0, 0x0ee0, 0x0cc0, 0x0aa0,
        0x0880, 0x0660, 0x0440, 0x0220, 0x0ff0, 0x0ee0,
        0x0cc0, 0x0aa0, 0x0880, 0x0660, 0x0440, 0x0220,
    ];

    private static readonly ushort[] CrocomireSpikeMaximumAcceleration =
    [
        0x0000, 0x0000, 0xff00, 0xee00, 0xcc00, 0xaa00,
        0x8800, 0x6600, 0x4400, 0x2200, 0xff00, 0xee00,
        0xcc00, 0xaa00, 0x8800, 0x6600, 0x4400, 0x2200,
    ];

    private static readonly byte[] CrocomireSpikeMaximumVelocity =
    [
        0, 0, 4, 4, 3, 3, 2, 2, 1, 1, 6, 5, 4, 3, 2, 2, 1, 1,
    ];

    /// <summary>Ports <c>EprojInit_CrocomireProjectile</c> at $86:9023.</summary>
    private void SpawnCrocomireProjectile(RoomEnemySlot body, ushort spawnParameter)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireProjectile,
            graphicsIndex: EnemyPaletteBits.Palette5);
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
        _enemyProjectiles[0].GraphicsIndex = EnemyPaletteBits.Palette5;
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
        projectile.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CrocomiresProjectile_Fired;
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

    /// <summary>Ports <c>Crocomire_Func_52</c> and initializer $86:9286.</summary>
    private void SpawnNextCrocomireBridgeFragment(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        ushort cursor = death.BridgeFragmentCursor;
        if (unchecked((short)(cursor - 22)) >= 0)
            return;

        // The second SpawnEprojWithGfx argument is the same unaligned 0,2,...,20 cursor.
        // Its copied graphics word is immaterial: the definition initializer immediately
        // replaces it with $0400. Preserve the observable X table and cursor progression.
        death.BridgeFragmentCursor = unchecked((ushort)(cursor + 2));
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
            graphicsIndex: 0);
        projectile.XPosition = ReadWord(_bus!, CrocomireBridgeFragmentGraphicsTable + cursor);
        projectile.YPosition = 187;
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.XVelocity = 0;
        projectile.YVelocity = unchecked((ushort)((_nextRandom!() & 0x003f) + 64));
        projectile.GraphicsIndex = 0x0400;
    }

    /// <summary>Ports the eight descending allocations at $A4:99B6 and initializer $86:90CF.</summary>
    private void SpawnCrocomireSpikeWallPieces()
    {
        CrocomireEnemyState state = _crocomire ??
            throw new InvalidOperationException("Crocomire spike wall spawned without an owner.");
        ushort graphicsIndex = unchecked((ushort)(
            state.Body.VramTilesIndex | state.Body.PaletteIndex));

        // The initializer indexes Y by native projectile index minus $14. Clearing the pool
        // immediately before this call guarantees slots 17..10 and Y=$38,$48,...,$A8.
        for (int count = 0; count < 8; count++)
        {
            RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
            if (projectile is null)
                return;

            InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.CrocomireSpikeWallPieces,
                graphicsIndex);
            int yTableIndex = projectile.SlotIndex - 10;
            if ((uint)yTableIndex >= 8)
            {
                throw new InvalidDataException(
                    $"Crocomire spike wall occupied unexpected projectile slot {projectile.SlotIndex}.");
            }

            projectile.XPosition = 528;
            projectile.YPosition = unchecked((ushort)(0x0038 + yTableIndex * 0x0010));
            projectile.XVelocity = 0;
            projectile.YVelocity = 0xfffb;
            projectile.Variable0 = 0;
            projectile.Variable1 = 0x8800;
            projectile.XSubposition = 0;
            projectile.YSubposition = 0;
        }
    }

    /// <summary>Ports the byte-precise slot-dependent shard motion at $86:9115.</summary>
    private void RunCrocomireSpikeWallPiece(RoomEnemyProjectileSlot projectile)
    {
        int slot = projectile.SlotIndex;
        ushort maximumAcceleration = CrocomireSpikeMaximumAcceleration[slot];
        ushort acceleration = projectile.Variable0;
        if (acceleration != maximumAcceleration)
        {
            acceleration = unchecked((ushort)(
                acceleration + CrocomireSpikeAccelerationDelta[slot]));
            if (acceleration >= maximumAcceleration)
                acceleration = maximumAcceleration;
        }
        projectile.Variable0 = acceleration;

        // The 65816 switches to 8-bit A here. The high acceleration byte is added to the
        // fractional velocity byte; carry advances the signed integral velocity byte, which
        // is then capped by this physical slot's maximum.
        int velocityFractionSum = (projectile.XVelocity & 0x00ff) + (acceleration >> 8);
        byte velocityFraction = unchecked((byte)velocityFractionSum);
        byte velocityWhole = unchecked((byte)(
            (projectile.XVelocity >> 8) + (velocityFractionSum >> 8)));
        byte maximumVelocity = CrocomireSpikeMaximumVelocity[slot];
        if (unchecked((sbyte)(velocityWhole - maximumVelocity)) >= 0)
            velocityWhole = maximumVelocity;
        projectile.XVelocity = unchecked((ushort)((velocityWhole << 8) | velocityFraction));

        // Move X with the same split-byte math. Only the high subposition byte participates;
        // its low byte remains untouched, exactly like EnemyProjectile_XSubPositions+1,X.
        int subpositionSum = (projectile.XSubposition >> 8) + velocityFraction;
        projectile.XSubposition = unchecked((ushort)(
            (projectile.XSubposition & 0x00ff) | ((subpositionSum & 0xff) << 8)));
        int wholeDelta = unchecked((sbyte)velocityWhole) + (subpositionSum >> 8);
        projectile.XPosition = unchecked((ushort)(projectile.XPosition + wholeDelta));

        // Y velocity and position are ordinary signed 16.16 pairs, with $3000 acceleration.
        uint velocity = unchecked(((uint)projectile.YVelocity << 16) | projectile.Variable1);
        velocity = unchecked(velocity + 0x00003000u);
        projectile.YVelocity = unchecked((ushort)(velocity >> 16));
        projectile.Variable1 = unchecked((ushort)velocity);
        uint position = unchecked(((uint)projectile.YPosition << 16) | projectile.YSubposition);
        position = unchecked(position + velocity);
        projectile.YPosition = unchecked((ushort)(position >> 16));
        projectile.YSubposition = unchecked((ushort)position);

        if (projectile.YPosition < 0x00a8)
            return;

        ushort impactX = projectile.XPosition;
        ushort impactY = projectile.YPosition;
        int nativeIndex = projectile.SlotIndex * 2;
        projectile.Clear();
        if ((nativeIndex & 2) == 0)
            LastCrocomireSoundEffect = 0x0029;
        SpawnRoomGraphicsDustExplosion(impactX, impactY, animationIndex: 0x0015);
        LastCrocomireSoundEffect = 0x0025;
    }

    /// <summary>Ports bridge-fragment vertical collision and gravity at $86:92BA.</summary>
    private static void RunCrocomireBridgeFragment(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
            return;
        }

        projectile.YVelocity = unchecked((ushort)(
            (projectile.YVelocity + 24) & 0x3fff));
    }
}
