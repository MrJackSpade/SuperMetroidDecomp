using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's four bank-$86 rock definitions. These are physical shared-pool actors: they use
/// the cartridge definitions and instruction lists, collide with room blocks, can damage
/// Samus according to native property bits, and may fail to spawn when all eighteen slots
/// are occupied.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Attempts to launch Kraid's spit rock from the body using the next room random value.</summary>
    /// <param name="body">Body slot supplying the projectile's launch position.</param>
    /// <returns><see langword="true"/> if a shared projectile slot was available and initialized.</returns>
    private bool SpawnKraidSpitRock(RoomEnemySlot body)
    {
        RoomEnemyProjectileSlot? rock = AllocateEnemyProjectile();
        if (rock is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidSpitRock,
            graphicsIndex: 0x0600);
        ushort random = ReadKraidRandomNumber();
        rock.XPosition = unchecked((ushort)(body.XPosition + 16));
        rock.YPosition = unchecked((ushort)(body.YPosition - 96));
        rock.XSubposition = 0;
        rock.YSubposition = 0;
        rock.XVelocity = KraidRockLaunchDefinitions.FromRandom(random);
        rock.YVelocity = unchecked((ushort)-0x0400);
        rock.GraphicsIndex = 0x0600;
        return true;
    }

    /// <summary>Records a rising-rock request and emits a randomized left or right rock when a slot is free.</summary>
    /// <param name="body">Body slot supplying the origin for the rock's randomized horizontal position.</param>
    /// <param name="state">Kraid state whose request and successful-spawn counts are updated.</param>
    private void RequestKraidRisingRock(RoomEnemySlot body, KraidEnemyState state)
    {
        state.RiseRockSpawnRequestCount++;
        ushort random = ReadKraidRandomNumber();
        RoomEnemyProjectileKind kind = (random & 0x0010) != 0
            ? RoomEnemyProjectileKind.KraidRisingRockRight
            : RoomEnemyProjectileKind.KraidRisingRockLeft;
        RoomEnemyProjectileSlot? rock = AllocateEnemyProjectile();
        if (rock is null)
            return;

        InitializeEnemyProjectileFromDefinition(rock, kind, graphicsIndex: 0x0600);
        // $86:9D17 tests bit zero: clear complements the offset to the left.
        int randomOffset = random & 0x003f;
        if ((random & 1) == 0)
            randomOffset = unchecked((short)~randomOffset);
        rock.XPosition = unchecked((ushort)(body.XPosition + randomOffset));
        rock.YPosition = 432;
        rock.XSubposition = 0;
        rock.YSubposition = 0;
        rock.XVelocity = unchecked((ushort)(random & 0x03f0));
        rock.YVelocity = unchecked((ushort)-0x0500);
        rock.GraphicsIndex = 0x0600;
        state.SpawnedRiseRockCount++;
        LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e));
    }

    /// <summary>Attempts to create a ceiling rock at the given horizontal position with randomized downward speed.</summary>
    /// <param name="xPosition">World X coordinate assigned to the new rock.</param>
    /// <returns><see langword="true"/> if a shared projectile slot was available and initialized.</returns>
    private bool SpawnKraidCeilingRock(ushort xPosition)
    {
        RoomEnemyProjectileSlot? rock = AllocateEnemyProjectile();
        if (rock is null)
            return false;
        InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidCeilingRock,
            graphicsIndex: 0x0600);
        rock.XPosition = xPosition;
        rock.YPosition = 312;
        rock.XSubposition = 0;
        rock.YSubposition = 0;
        rock.XVelocity = 0;
        rock.YVelocity = unchecked((ushort)((ReadKraidRandomNumber() & 0x003f) + 0x0040));
        rock.GraphicsIndex = 0x0600;
        return true;
    }

    /// <summary>Moves a bouncing rock, removes it on block collision, and advances its horizontal and vertical speeds.</summary>
    /// <param name="rock">Projectile slot being advanced.</param>
    /// <param name="level">Room geometry used to detect block collisions during movement.</param>
    private void RunKraidRockPreInstruction(
        RoomEnemyProjectileSlot rock,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(rock, level, horizontal: true) ||
            MoveProjectileAxis(rock, level, horizontal: false))
        {
            rock.Clear();
            return;
        }

        short horizontal = unchecked((short)(rock.XVelocity + 8));
        if (horizontal >= 0x0100)
            horizontal = -0x0100;
        rock.XVelocity = unchecked((ushort)horizontal);
        rock.YVelocity = unchecked((ushort)(rock.YVelocity + 0x0040));
    }

    /// <summary>Advances a ceiling rock's fall, removing it when vertical movement collides with room geometry.</summary>
    /// <param name="rock">Projectile slot being advanced.</param>
    /// <param name="level">Room geometry used to detect vertical block collisions.</param>
    private void RunKraidCeilingRockPreInstruction(
        RoomEnemyProjectileSlot rock,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(rock, level, horizontal: false))
        {
            rock.Clear();
            return;
        }
        rock.YVelocity = unchecked((ushort)((rock.YVelocity + 0x0018) & 0x3fff));
    }

    /// <summary>Reads the random value required by Kraid's projectile selection and launch rules.</summary>
    /// <returns>The next room random number supplied by the active simulation context.</returns>
    private ushort ReadKraidRandomNumber() =>
        RequireRandomNumber();
}
