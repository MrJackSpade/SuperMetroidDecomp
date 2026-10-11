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
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(body.Position.X.Whole + 16)), unchecked((ushort)(body.Position.Y.Whole - 96))));
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(KraidRockLaunchDefinitions.FromRandom(random)));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-0x0400)));
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
        // $86:9D17 tests bit zero: clear complements the offset to the left.
        int randomOffset = random & 0x003f;
        if ((random & 1) == 0)
            randomOffset = unchecked((short)~randomOffset);
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(body.Position.X.Whole + randomOffset)), 432));
        rock.Velocity = new(new SnesSignedEightEight(unchecked((ushort)(random & 0x03f0))), new SnesSignedEightEight(unchecked((ushort)-0x0500)));
        rock.GraphicsIndex = 0x0600;
        state.SpawnedRiseRockCount++;
        LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e));
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
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(xPosition, 312));
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(0));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((ReadKraidRandomNumber() & 0x003f) + 0x0040))));
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

        short horizontal = unchecked((short)(rock.Velocity.X.RawValue + 8));
        if (horizontal >= 0x0100)
            horizontal = -0x0100;
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)horizontal)));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(rock.Velocity.Y.RawValue + 0x0040))));
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
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((rock.Velocity.Y.RawValue + 0x0018) & 0x3fff))));
    }

    private ushort ReadKraidRandomNumber() =>
        RequireRandomNumber();
}
