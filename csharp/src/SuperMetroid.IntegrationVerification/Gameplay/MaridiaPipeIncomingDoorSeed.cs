using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Hardware;
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
        int entry = -1;
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            var block = level.GetCollisionBlockByIndex(index);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            if (level.ResolveDoorCollision(block.Bts.Value, runtime.Player.IntroducedSamus.Pose.ForRoomCollision(), false).Pointer != door.Pointer) continue;
            entry = index;
            break;
        }
        if (entry < 0) throw new InvalidDataException("Plasma Spark has no matching northern elevatube door block.");
        ushort x = MaridiaPipeFixtureDefinitions.TubeCenterX;
        ushort y = checked((ushort)((entry / level.Size.X) * 16 - 96));
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.PlasmaSpark,
            new Position16<RoomPixels>((ushort)(SnesLittleEndian.HighByteInPlace(x)), (ushort)(SnesLittleEndian.HighByteInPlace(y))));
        runtime.Player.IntroducedSamus.Kinematics.Position = runtime.Player.IntroducedSamus.Kinematics.Position.WithX(new SnesFixedPosition(x, 0));
        runtime.Player.IntroducedSamus.Kinematics.Position = runtime.Player.IntroducedSamus.Kinematics.Position.WithY(new SnesFixedPosition(y, 0));
        Console.WriteLine($"incoming seed block={entry} Samus={x}/{y} door={door.Pointer:X4}");
    }
}
