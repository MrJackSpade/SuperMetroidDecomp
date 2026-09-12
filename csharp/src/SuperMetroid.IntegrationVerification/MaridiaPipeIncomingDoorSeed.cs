using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Constructs a bounded source-room boundary; subsequent frames use the frontend door owner.</summary>
internal static class MaridiaPipeIncomingDoorSeed
{
    public static void Apply(SuperMetroidRuntime runtime, ISnesAddressSpace bus, CartridgeDoorHeader door)
    {
        runtime.LoadCartridgeRoomForDebug(MaridiaPipeFixtureDefinitions.PlasmaSparkRoom);
        var level = runtime.LevelData!;
        int entry = -1;
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            var block = level.GetCollisionBlockByIndex(index);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            if (level.ResolveDoorCollision(bus, block.Behavior, runtime.Samus!.Pose, false).Pointer != door.Pointer) continue;
            entry = index;
            break;
        }
        if (entry < 0) throw new InvalidDataException("Plasma Spark has no matching northern elevatube door block.");
        ushort x = checked((ushort)((entry % level.WidthInBlocks) * 16 + 8));
        ushort y = checked((ushort)((entry / level.WidthInBlocks) * 16 - 16));
        runtime.LoadCartridgeRoomForDebug(MaridiaPipeFixtureDefinitions.PlasmaSparkRoom,
            (ushort)(x & 0xff00), (ushort)(y & 0xff00));
        runtime.Samus!.Kinematics.SetXFixed((uint)x << 16);
        runtime.Samus.Kinematics.SetYFixed((uint)y << 16);
        runtime.LevelData!.ResolveDoorCollision(bus,
            runtime.LevelData.GetCollisionBlockByIndex(entry).Behavior, runtime.Samus.Pose, true);
        Console.WriteLine($"incoming seed block={entry} Samus={x}/{y} door={door.Pointer:X4}");
    }
}

/// <summary>Cartridge identities used by the bounded #391 diagnostic.</summary>
internal static class MaridiaPipeFixtureDefinitions
{
    /// <summary>$8F:D340, RoomHeader_PlasmaSpark: the authored source of door $83:A5AC.</summary>
    public const ushort PlasmaSparkRoom = 0xd340;

    /// <summary>$8F:D408, RoomHeader_Toilet: the tall Maridia elevatube.</summary>
    public const ushort TubeRoom = 0xd408;
}
