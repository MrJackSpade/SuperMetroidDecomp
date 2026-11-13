using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.IntegrationVerification.Gameplay;

/// <summary>Constructs a bounded source-room boundary; subsequent frames use the frontend door owner.</summary>
internal static class MaridiaPipeIncomingDoorSeed
{
    /// <summary>Places Samus above the authored Plasma Spark door block and lets gravity drive the source-room approach before the frontend owns the transition.</summary>
    public static void Apply(SuperMetroidRuntime runtime, DoorDefinition door)
    {
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.PlasmaSpark);
        var level = runtime.Room.LoadedLevelData;
        RoomCollisionBlock? entry = null;
        for (int index = 0; index < level.Size.Area && entry is null; index++)
        {
            var block = level.GetCollisionBlock(level.Size.CellAtRowMajorIndex(index));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            if (level.ResolveDoorCollision(block.Behavior, runtime.Player.IntroducedSamus.Pose.ForRoomCollision(), false).Pointer != door.Pointer) continue;
            entry = block;
        }
        if (entry is not { } entryBlock) throw new InvalidDataException("Plasma Spark has no matching northern elevatube door block.");
        ushort x = MaridiaPipeFixtureDefinitions.TubeCenterX;
        ushort y = checked((ushort)(entryBlock.At.Y * 16 - 96));
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.PlasmaSpark,
            new Position16<RoomPixels>((ushort)(LittleEndianBytes.HighByteInPlace(x)), (ushort)(LittleEndianBytes.HighByteInPlace(y))));
        runtime.Player.IntroducedSamus.Kinematics.Position = runtime.Player.IntroducedSamus.Kinematics.Position.WithX(new FixedCoordinate(x, 0));
        runtime.Player.IntroducedSamus.Kinematics.Position = runtime.Player.IntroducedSamus.Kinematics.Position.WithY(new FixedCoordinate(y, 0));
        Console.WriteLine($"incoming seed block={entry} Samus={x}/{y} door={door.Pointer:X4}");
    }
}
