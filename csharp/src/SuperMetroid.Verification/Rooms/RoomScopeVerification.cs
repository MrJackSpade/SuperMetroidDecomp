using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Runtime;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Confirms #1329's room scope: a door load builds a new scope and carries only the named camera state and beam charge.</summary>
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
        source.Projectiles.Charge.FlareCounter = 30;
        const int heatPhase = 3;
        runtime.PaletteFxSession.SamusInHeatPaletteIndex = heatPhase;
        source.Enemies.Earthquake.EarthquakeType = 5;
        source.Enemies.RoomStatus.EnemyDoorTransitionActive = true;
        source.Enemies.Elevator.ElevatorDoorTransitionActive = true;
        runtime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyEscaped;
        RoomEnemyProjectileSlot sourceProjectile = source.Enemies.Projectiles.EnemyProjectiles[3];
        sourceProjectile.Position = FixedPosition<RoomPixels>.FromWords(0x0140, 0x1234, 0x0090, 0x5678);
        sourceProjectile.Velocity = new EightEightVector<RoomPixels>(0x0200, unchecked((short)0xFE00));
        SamusProjectileSlot sourceShot = source.Projectiles.SlotTable.Slots[2];
        sourceShot.Position = FixedPosition<RoomPixels>.FromWords(0x0120, 0x4321, 0x0080, 0x8765);
        sourceShot.AuxiliaryPhase = SpazerComboPhase.FlyingUpTowardsPoint;

        SelectedRoom parlor = runtime.RoomLoader.LoadSelectedRoom(RoomHeaderDefinitions.Parlor);
        var camera = new Position16<RoomPixels>(0, 0);
        var door = new DoorDefinition(
            Name: "room scope verification door",
            DestinationRoom: RoomHeaderDefinitions.Parlor,
            Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
            RoomObjectBlock: default,
            DestinationScreen: camera.ToRoomScreen(),
            SamusSpeed: DoorSamusSpeed.None,
            SetupCode: DoorSetupCode.None);
        runtime.RoomLoader.LoadRoom(door, parlor, camera, RoomViewportLoadMode.StreamThroughDoor);

        RoomScope destination = runtime.Room.Scope;
        VerificationAssertions.AssertTrue(!ReferenceEquals(source, destination) && !ReferenceEquals(previous, destination.Camera),
            "the door load builds a new scope and camera");
        VerificationAssertions.AssertTrue(ReferenceEquals(parlor, destination.Room) && ReferenceEquals(door, destination.Door),
            "the scope holds the destination room and door");
        VerificationAssertions.AssertTrue(ReferenceEquals(destination.Camera, runtime.Room.Camera) &&
            ReferenceEquals(destination.LevelData, runtime.Room.LevelData),
            "the room views read the adopted scope");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.RoomObjects, destination.RoomObjects) &&
            !destination.RoomObjects.SlotTable.Slots.Intersect(source.RoomObjects.SlotTable.Slots).Any(),
            "no PLM slot survives the load: the destination builds its own PLM system");
        VerificationAssertions.AssertTrue(ReferenceEquals(destination.RoomObjects, runtime.RoomObjects),
            "the runtime's PLM system is the loaded room's");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.Projectiles, destination.Projectiles) &&
            !ReferenceEquals(source.BombProjectiles, destination.BombProjectiles) &&
            !ReferenceEquals(source.BombProjectiles.PowerBombExplosion, destination.BombProjectiles.PowerBombExplosion),
            "the destination builds its own projectile and bomb systems");
        VerificationAssertions.AssertTrue(destination.Projectiles.Slots.All(slot => !slot.IsActive) &&
            destination.BombProjectiles.Slots.All(slot => !slot.IsActive) && destination.Projectiles.SlotTable.ProjectileCounter == 0 &&
            destination.BombProjectiles.BombCounter == 0,
            "the destination's projectile pools are empty");
        VerificationAssertions.AssertTrue(ReferenceEquals(source.Projectiles.Charge, destination.Projectiles.Charge) &&
            ReferenceEquals(runtime.BeamCharge, destination.Projectiles.Charge),
            "the held beam charge is the session's, shared by both rooms");
        VerificationAssertions.AssertEqual(30, destination.Projectiles.Charge.FlareCounter, "the flare counter survives the load");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.FxOverlay, destination.FxOverlay) &&
            !ReferenceEquals(source.PaletteFx, destination.PaletteFx) && !ReferenceEquals(source.CeresHaze, destination.CeresHaze) &&
            !ReferenceEquals(source.SandTiles, destination.SandTiles) && !ReferenceEquals(source.SpikeTiles, destination.SpikeTiles) &&
            !ReferenceEquals(source.TreadmillTiles, destination.TreadmillTiles) &&
            !ReferenceEquals(source.WreckedShipTreadmill, destination.WreckedShipTreadmill) &&
            !ReferenceEquals(source.TourianStatues, destination.TourianStatues),
            "the destination builds its own FX, animated tiles, haze and statue sequence");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.MaridiaElevatube, destination.MaridiaElevatube) &&
            !ReferenceEquals(source.CeresElevatorShaft, destination.CeresElevatorShaft) &&
            !destination.MaridiaElevatube.IsActive && !destination.CeresElevatorShaft.IsActive,
            "the destination builds its own room mains, inactive in a room that does not select them");
        VerificationAssertions.AssertTrue(ReferenceEquals(source.FxOverlay.Fireflea, destination.FxOverlay.Fireflea) &&
            ReferenceEquals(runtime.RoomFx.Fireflea, destination.FxOverlay.Fireflea),
            "the Fireflea state is the session's, shared by both rooms");
        VerificationAssertions.AssertEqual(heatPhase, runtime.PaletteFxSession.SamusInHeatPaletteIndex,
            "the palette-FX heat phase survives the load on the session");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.Enemies, destination.Enemies) &&
            !ReferenceEquals(source.Enemies.SlotTable, destination.Enemies.SlotTable),
            "the destination builds its own enemy system and slots");
        VerificationAssertions.AssertEqual(0, destination.Enemies.Earthquake.EarthquakeType, "no enemy earthquake survives the load");
        VerificationAssertions.AssertTrue(destination.Enemies.RoomStatus.EnemyDoorTransitionActive &&
            destination.Enemies.Elevator.ElevatorDoorTransitionActive,
            "the door-transition gates carry into the destination's enemies");
        VerificationAssertions.AssertEqual(CeresEscapeStatus.RidleyEscaped, destination.Enemies.Ceres.CeresStatus,
            "Ceres's escape progress survives the load on the enemy session");
        RoomEnemyProjectileSlot destinationProjectile = destination.Enemies.Projectiles.EnemyProjectiles[3];
        VerificationAssertions.AssertEqual(FixedPosition<RoomPixels>.FromWords(0x0140, 0, 0x0090, 0), destinationProjectile.Position,
            "an enemy projectile slot's whole coordinates survive the load; its fractions clear");
        VerificationAssertions.AssertEqual(sourceProjectile.Velocity, destinationProjectile.Velocity,
            "an enemy projectile slot's velocity survives the load");
        SamusProjectileSlot destinationShot = destination.Projectiles.SlotTable.Slots[2];
        VerificationAssertions.AssertEqual(FixedPosition<RoomPixels>.FromWords(0, 0x4321, 0, 0), destinationShot.Position,
            "a shot slot starts the next room with the horizontal sub-pixel position the outgoing room's left (RoomHandOver), and nothing else of its position");
        VerificationAssertions.AssertEqual(SpazerComboPhase.Circling, destinationShot.AuxiliaryPhase,
            "no Spazer phase is handed over: every Spazer special attack sets it before reading it");
        VerificationAssertions.AssertEqual(previous.Position.X.Fraction, destination.Camera.Position.X.Fraction, "X fraction word carried");
        VerificationAssertions.AssertEqual(previous.Position.Y.Fraction, destination.Camera.Position.Y.Fraction, "Y fraction word carried");
        VerificationAssertions.AssertEqual(previous.IdealPosition, destination.Camera.IdealPosition, "ideal position carried");
        VerificationAssertions.AssertEqual(previous.Speed, destination.Camera.Speed, "camera speed carried");
        VerificationAssertions.AssertEqual(previous.PreviousSamusPoint, destination.Camera.PreviousSamusPoint, "previous Samus point carried");
        Console.WriteLine("Room scope: a streamed door load builds a new scope, PLM system, projectile pools, FX and animated tiles, enemies, keeps the held beam charge, door gates and session FX and enemy state; its camera carries the fractions, ideal position, speed and previous Samus point.");
    }

    /// <summary>Runs the room-scope carry-over confirmation.</summary>
    [VerificationCommand("--room-scope")]
    internal static int RoomScopeCommand()
    {
        VerifyRoomScopeCarryOver();
        return 0;
    }
}
