using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Confirms #1329's room scope: a door load builds a new scope and carries only the named camera state.</summary>
internal static class RoomScopeVerification
{
    /// <summary>
    /// Loads Parlor through a streamed door from a Landing Site camera with seeded fractions, ideal position, speed and
    /// previous Samus point: the new scope's camera inherits exactly those, and the room views show the new scope.
    /// </summary>
    internal static void VerifyRoomScopeCarryOver()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        RoomScope source = runtime.Room.Scope ?? throw new InvalidOperationException("The debug load adopted no scope.");
        ScrollBoundaryCamera previous = source.Camera;
        previous.Position = previous.Position.WithFractionsOf(FixedPosition<RoomPixels>.FromWords(0, 0x1234, 0, 0x5678));
        previous.IdealPosition = new Position16<RoomPixels>(0x0123, 0x0456);
        previous.Speed = FixedVector<RoomPixels>.FromWords(2, 0x8000, 0xFFFF, 0x4000);
        previous.FinishSamusScrolling(FixedPosition<RoomPixels>.FromWords(0x0300, 0x1111, 0x0400, 0x2222));

        CartridgeRoomHeader parlor = runtime.RoomLoader.LoadCartridgeRoomHeader(RoomHeaderDefinitions.Parlor);
        var camera = new Position16<RoomPixels>(0, 0);
        var door = new CartridgeDoorHeader(
            Name: "room scope verification door",
            DestinationRoom: RoomHeaderDefinitions.Parlor,
            Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
            PlmBlock: default,
            DestinationScreen: camera.ToRoomScreen(),
            SamusDistance: 0,
            SetupCodePointer: DoorSetupCode.None);
        runtime.RoomLoader.LoadCartridgeRoom(door, parlor, camera, RoomViewportLoadMode.StreamThroughDoor);

        RoomScope destination = runtime.Room.Scope ?? throw new InvalidOperationException("The door load adopted no scope.");
        VerificationAssert.AssertTrue(!ReferenceEquals(source, destination) && !ReferenceEquals(previous, destination.Camera),
            "the door load builds a new scope and camera");
        VerificationAssert.AssertTrue(ReferenceEquals(parlor, destination.Room) && ReferenceEquals(door, destination.Door),
            "the scope holds the destination room and door");
        VerificationAssert.AssertTrue(ReferenceEquals(destination.Camera, runtime.Room.Camera) &&
            ReferenceEquals(destination.LevelData, runtime.Room.LevelData) &&
            ReferenceEquals(destination.BackgroundStreamer, runtime.Room.BackgroundStreamer),
            "the room views read the adopted scope");
        VerificationAssert.AssertEqual(previous.Position.X.Fraction, destination.Camera.Position.X.Fraction, "X fraction word carried");
        VerificationAssert.AssertEqual(previous.Position.Y.Fraction, destination.Camera.Position.Y.Fraction, "Y fraction word carried");
        VerificationAssert.AssertEqual(previous.IdealPosition, destination.Camera.IdealPosition, "ideal position carried");
        VerificationAssert.AssertEqual(previous.Speed, destination.Camera.Speed, "camera speed carried");
        VerificationAssert.AssertEqual(previous.PreviousSamusPoint, destination.Camera.PreviousSamusPoint, "previous Samus point carried");
        Console.WriteLine("Room scope: a streamed door load builds a new scope whose camera carries the fractions, ideal position, speed and previous Samus point.");
    }

    /// <summary>Runs the room-scope carry-over confirmation.</summary>
    [VerificationCommand("--room-scope")]
    internal static int RoomScopeCommand()
    {
        VerifyRoomScopeCarryOver();
        return 0;
    }
}
