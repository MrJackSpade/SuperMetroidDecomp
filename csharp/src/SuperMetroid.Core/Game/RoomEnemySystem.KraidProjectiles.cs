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
        rock.XVelocity = ReadWord(_bus!, 0xa7bc65 + (random & 0x000e));
        rock.YVelocity = unchecked((ushort)-0x0400);
        rock.GraphicsIndex = 0x0600;
        return true;
    }

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
        int randomOffset = random & 0x003f;
        if ((random & 2) == 0)
            randomOffset = unchecked((short)~randomOffset);
        rock.XPosition = unchecked((ushort)(body.XPosition + randomOffset));
        rock.YPosition = 432;
        rock.XSubposition = 0;
        rock.YSubposition = 0;
        rock.XVelocity = unchecked((ushort)(random & 0x03f0));
        rock.YVelocity = unchecked((ushort)-0x0500);
        rock.GraphicsIndex = 0x0600;
        state.SpawnedRiseRockCount++;
        LastKraidSoundEffect = new KraidSoundRequest(SoundEffectLibrary.Library3, 0x001e);
    }

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

    private static void RunKraidRockPreInstruction(
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

    private static void RunKraidCeilingRockPreInstruction(
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

    private ushort ReadKraidRandomNumber() =>
        RequireRandomNumber();
}
