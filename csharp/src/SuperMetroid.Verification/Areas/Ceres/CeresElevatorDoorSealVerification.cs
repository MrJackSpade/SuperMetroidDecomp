using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World.RoomStates;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// Confirms #2494: once Ceres Ridley is dead, the elevator room's setup seals the room's door, as the original's setup
/// (<c>$8F:C96E</c>) does by spawning the room object (<c>$84:BA48</c>) whose setup gives the door's block at (15, 38) and
/// the three below it the solid block type, keeping each block's picture and behaviour.
/// </summary>
internal static class CeresElevatorDoorSealVerification
{
    /// <summary>The elevator room's door blocks, which lead back to the Falling Tile room, top first.</summary>
    private static readonly Position16<RoomBlocks>[] DoorBlocks = [new(15, 38), new(15, 39), new(15, 40), new(15, 41)];

    /// <summary>
    /// Loads the elevator room through the Falling Tile room's door before and after Ceres Ridley is dead: before, the door's
    /// four blocks are door blocks and Samus walking right into them starts the door back; after, each is a solid block
    /// showing the same picture with the same behaviour, and Samus walking right stops against them.
    /// </summary>
    internal static void VerifyCeresElevatorDoorSealsDuringEscape()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();

        RoomLevelData beforeEscape = LoadElevatorRoomThroughItsDoor(runtime);
        AssertTrue(ReferenceEquals(CeresRoomStateDefinitions.CeresElevatorDefault, runtime.Room.LoadedRoom.State),
            "before Ceres Ridley is dead the elevator room loads its default state");
        RoomCollisionBlock[] door = [.. DoorBlocks.Select(beforeEscape.GetCollisionBlock)];
        RoomBlockCell[] shownDoor = [.. DoorBlocks.Select(block => beforeEscape.ForegroundPlane.Cells[block])];
        AssertTrue(!runtime.RoomObjects.Places.InUpdateOrder.Any(place => place.Active),
            "before the escape the elevator room holds no room object");
        foreach (RoomCollisionBlock block in door)
            AssertEqual(RoomCollisionType.DoorBlock, block.Terrain.Collision, $"before the escape block {block.At} is a door block");
        AssertTrue(ReferenceEquals(CeresDoorDefinitions.CeresElevatorToFallingTile, WalkRightIntoTheDoor(runtime, beforeEscape, out _)),
            "before the escape Samus walking right into the door starts the door back to the Falling Tile room");

        runtime.System.RecordBossesDefeated(AreaId.Ceres, DefeatedBosses.AreaBoss);
        RoomLevelData duringEscape = LoadElevatorRoomThroughItsDoor(runtime);
        AssertTrue(ReferenceEquals(CeresRoomStateDefinitions.CeresElevatorWhenAreaBossDead, runtime.Room.LoadedRoom.State),
            "once Ceres Ridley is dead the elevator room loads its escape state");
        foreach (RoomCollisionBlock before in door)
        {
            RoomCollisionBlock sealedBlock = duringEscape.GetCollisionBlock(before.At);
            AssertEqual(before.Terrain.WithCollision(RoomCollisionType.SolidBlock), sealedBlock.Terrain,
                $"during the escape block {before.At} is a solid block showing the door's picture");
            AssertEqual(before.Behavior, sealedBlock.Behavior, $"during the escape block {before.At} keeps its behaviour");
        }
        AssertTrue(shownDoor.SequenceEqual(DoorBlocks.Select(block => duringEscape.ForegroundPlane.Cells[block])),
            "during the escape the sealed door shows what the door showed");
        RoomObjectPlace[] held = [.. runtime.RoomObjects.Places.InUpdateOrder.Where(place => place.Active)];
        AssertTrue(held is [{ Behaviour: CeresElevatorDoorSealObject }] && held[0].Block == DoorBlocks[0],
            "the seal holds the room's one held place, at the door's top block, until the room objects first update");
        runtime.RoomObjects.Step(duringEscape, new RoomObjectFrameInputs
        {
            Scrolls = RoomObjectFixtures.UnreadScrolls(), EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0,
            CollectedItems = default, PowerBombExplosionActive = false,
        });
        AssertTrue(!runtime.RoomObjects.Places.InUpdateOrder.Any(place => place.Active), "the seal leaves the room on its first update");
        AssertTrue(WalkRightIntoTheDoor(runtime, duringEscape, out int rightmostPixel) is null,
            "during the escape Samus walking right into the sealed door starts no door");
        AssertEqual(DoorBlocks[0].X * 16 - 1, rightmostPixel,
            "during the escape the sealed door's blocks stop Samus flush against them, her body ending on the column left of the door");
        Console.WriteLine("Ceres elevator door: the escape state seals the door's four blocks, keeping their pictures, and Samus cannot pass.");
    }

    /// <summary>Loads the elevator room through the Falling Tile room's door into it, in the state the session selects.</summary>
    private static RoomLevelData LoadElevatorRoomThroughItsDoor(SuperMetroidRuntime runtime)
    {
        DoorDefinition door = CeresDoorDefinitions.FallingTileToCeresElevator;
        SelectedRoom elevator = runtime.RoomLoader.LoadSelectedRoom(door.DestinationRoom);
        runtime.RoomLoader.LoadRoom(door, elevator, door.DestinationScreen.ToRoomPixels(),
            RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn);
        return runtime.Room.LoadedLevelData;
    }

    /// <summary>
    /// Stands Samus in the doorway left of the door's blocks and walks her right a pixel at a time against the level's
    /// blocks alone, returning the door her collision starts, or null when none, and the rightmost pixel column her body covers
    /// where she ends.
    /// </summary>
    private static DoorDefinition? WalkRightIntoTheDoor(SuperMetroidRuntime runtime, RoomLevelData level, out int rightmostPixel)
    {
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Kinematics.Position = samus.Kinematics.Position.WithWhole(new(224, 640));
        samus.Kinematics.Radius = new(5, 16);
        for (int pixel = 0; pixel < 32 && runtime.RoomObjects.DoorContacts.EnteredDoor is null; pixel++)
        {
            SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = runtime.RoomObjects }, samus.Kinematics,
                samus, samus.Pose, new HorizontalBlockMove { Displacement = SignedSixteenSixteen.FromWholePixels(1) });
        }
        rightmostPixel = samus.Kinematics.Position.X.Whole + samus.Kinematics.Radius.X - 1;
        return runtime.RoomObjects.DoorContacts.EnteredDoor;
    }
}
