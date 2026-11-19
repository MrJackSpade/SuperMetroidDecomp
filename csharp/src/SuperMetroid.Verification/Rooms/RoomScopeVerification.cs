using SuperMetroid.Core;
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
using SuperMetroid.Core.Fx;

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
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        RoomScope source = runtime.Room.Scope ?? throw new InvalidOperationException("The debug load adopted no scope.");
        // The Landing Site's main composes the land sky, and its setup the sky's drifting bands (#2485).
        VerificationAssertions.AssertTrue(source.Sky?.State is { Sky: SuperMetroid.Core.Rooms.Scrolling.ScrollingSky.Land, HasBands: true },
            "the Landing Site composes the land sky with the bands its setup spawns");
        ScrollBoundaryCamera previous = source.Camera;
        previous.Position = previous.Position.WithFractionsOf(FixedPosition<RoomPixels>.FromStoredParts(0, 0x1234, 0, 0x5678));
        PrivateState.Set(previous, nameof(ScrollBoundaryCamera.IdealPosition), new Position16<RoomPixels>(0x0123, 0x0456));
        previous.Speed = FixedVector<RoomPixels>.FromStoredParts(2, 0x8000, 0xFFFF, 0x4000);
        previous.FinishSamusScrolling(FixedPosition<RoomPixels>.FromStoredParts(0x0300, 0x1111, 0x0400, 0x2222));
        source.Projectiles.Charge.FlareCounter = 30;
        var heatPhase = SuperMetroid.Core.Fx.ColourEffects.HeatPhase.InOrder[3];
        runtime.ColourEffectSession.PublishedHeatPhase = heatPhase;
        source.Enemies.Earthquake.Start(NativeEarthquakeTypes.Shake(5), 4);
        runtime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyEscaped;
        EnemyProjectile sourceProjectile = source.Enemies.Projectiles.AllocateEnemyProjectile() ??
            throw new InvalidOperationException("The Landing Site holds every enemy-projectile place.");
        sourceProjectile.Position = FixedPosition<RoomPixels>.FromStoredParts(0x0140, 0x1234, 0x0090, 0x5678);
        sourceProjectile.Velocity = new(SignedEightEight.FromPixels(2), SignedEightEight.FromPixels(-2));
        SamusProjectile sourceShot = source.Projectiles.Pool.Shots[2];
        sourceShot.Damage = 1;
        sourceShot.Position = FixedPosition<RoomPixels>.FromStoredParts(0x0120, 0x4321, 0x0080, 0x8765);

        SelectedRoom parlor = runtime.RoomLoader.LoadSelectedRoom(RoomDefinitions.Parlor);
        var camera = new Position16<RoomPixels>(0, 0);
        var door = new DoorDefinition(
            Name: "room scope verification door",
            DestinationRoom: RoomDefinitions.Parlor,
            Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
            RoomObjectBlock: default,
            DestinationScreen: camera.ToRoomScreen(),
            SamusSpeed: DoorSamusSpeed.None,
            Setup: DoorSetupDefinitions.None);
        runtime.RoomLoader.LoadRoom(door, parlor, camera, RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn);

        RoomScope destination = runtime.Room.Scope;
        VerificationAssertions.AssertTrue(!ReferenceEquals(source, destination) && !ReferenceEquals(previous, destination.Camera),
            "the door load builds a new scope and camera");
        VerificationAssertions.AssertTrue(ReferenceEquals(parlor, destination.Room) && ReferenceEquals(door, destination.Door),
            "the scope holds the destination room and door");
        VerificationAssertions.AssertTrue(ReferenceEquals(destination.Camera, runtime.Room.Camera) &&
            ReferenceEquals(destination.LevelData, runtime.Room.LevelData),
            "the room views read the adopted scope");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.RoomObjects, destination.RoomObjects) &&
            !destination.RoomObjects.Places.InUpdateOrder.Intersect(source.RoomObjects.Places.InUpdateOrder).Any(),
            "no PLM slot survives the load: the destination builds its own PLM system");
        VerificationAssertions.AssertTrue(ReferenceEquals(destination.RoomObjects, runtime.RoomObjects),
            "the runtime's PLM system is the loaded room's");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.Projectiles, destination.Projectiles) &&
            !ReferenceEquals(source.BombProjectiles, destination.BombProjectiles) &&
            !ReferenceEquals(source.BombProjectiles.PowerBombExplosion, destination.BombProjectiles.PowerBombExplosion),
            "the destination builds its own projectile and bomb systems");
        VerificationAssertions.AssertTrue(destination.Projectiles.Shots.All(slot => !slot.IsActive) &&
            destination.BombProjectiles.Bombs.All(slot => !slot.IsActive) && destination.Projectiles.Pool.ShotCount.Count == 0 &&
            destination.BombProjectiles.BombCounter == 0,
            "the destination's projectile pools are empty");
        VerificationAssertions.AssertTrue(ReferenceEquals(source.Projectiles.Charge, destination.Projectiles.Charge) &&
            ReferenceEquals(runtime.BeamCharge, destination.Projectiles.Charge),
            "the held beam charge is the session's, shared by both rooms");
        VerificationAssertions.AssertEqual(30, destination.Projectiles.Charge.FlareCounter, "the flare counter survives the load");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.FxOverlay, destination.FxOverlay) &&
            !ReferenceEquals(source.ColourEffects, destination.ColourEffects) &&
            !ReferenceEquals(source.BlockAnimations, destination.BlockAnimations),
            "the destination builds its own FX and animated tiles");
        VerificationAssertions.AssertTrue(destination.CeresHaze is null && destination.TourianStatues is null &&
            destination.Main is null && destination.MaridiaElevatube is null && destination.CeresElevatorShaft is null,
            "a room whose state runs no main and whose setup spawns nothing composes no main, haze or statue sequence (#2287)");
        VerificationAssertions.AssertEqual(heatPhase, runtime.ColourEffectSession.PublishedHeatPhase,
            "the palette-FX heat phase survives the load on the session");
        VerificationAssertions.AssertTrue(!ReferenceEquals(source.Enemies, destination.Enemies) &&
            !ReferenceEquals(source.Enemies.Places, destination.Enemies.Places),
            "the destination builds its own enemy system and slots");
        VerificationAssertions.AssertTrue(destination.Enemies.Earthquake.Shake == RoomEarthquakeDefinitions.UnshakenRoomShake && !destination.Enemies.Earthquake.IsShaking,
            "no enemy earthquake survives the load");
        VerificationAssertions.AssertEqual(CeresEscapeStatus.RidleyEscaped, destination.Enemies.Ceres.CeresStatus,
            "Ceres's escape progress survives the load on the enemy session");
        SuperMetroid.Core.Compatibility.EnemyProjectileHandOver carried = runtime.Room.HandOver.EnemyProjectile(sourceProjectile.Place);
        VerificationAssertions.AssertEqual(new Position16<RoomPixels>(0x0140, 0x0090), carried.Position,
            "an enemy projectile slot's whole coordinates survive the load; its fractions clear");
        VerificationAssertions.AssertEqual(sourceProjectile.Velocity, carried.Velocity,
            "an enemy projectile slot's velocity survives the load");
        VerificationAssertions.AssertEqual(0x4321, destination.Projectiles.Pool.LeftBeamStart.Kept(2),
            "a shot place keeps for its next projectile the horizontal sub-pixel position of the outgoing room's shot in it (RoomHandOver, LeftBeamStartFromLastShot)");
        VerificationAssertions.AssertEqual(FixedPosition<RoomPixels>.Zero, destination.Projectiles.Pool.Shots[2].Position,
            "nothing else of the outgoing shot reaches the next room: the place reads as at rest at the origin");
        VerificationAssertions.AssertEqual(previous.Position.X.Fraction, destination.Camera.Position.X.Fraction, "X fraction word carried");
        VerificationAssertions.AssertEqual(previous.Position.Y.Fraction, destination.Camera.Position.Y.Fraction, "Y fraction word carried");
        VerificationAssertions.AssertEqual(previous.IdealPosition, destination.Camera.IdealPosition, "ideal position carried");
        VerificationAssertions.AssertEqual(previous.Speed, destination.Camera.Speed, "camera speed carried");
        VerificationAssertions.AssertEqual(previous.PreviousSamusPoint, destination.Camera.PreviousSamusPoint, "previous Samus point carried");
        Console.WriteLine("Room scope: a streamed door load builds a new scope, PLM system, projectile pools, FX and animated tiles, enemies, keeps the held beam charge, the palette-FX heat phase and session enemy state; its camera carries the fractions, ideal position, speed and previous Samus point.");
    }

    /// <summary>Runs the room-scope carry-over confirmation.</summary>
    [VerificationCommand("--room-scope")]
    internal static int RoomScopeCommand()
    {
        VerifyRoomScopeCarryOver();
        return 0;
    }
}
