using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ceres Ridley's Mode 7 body as the room's published map and his latched transform (#1357).</summary>
internal static class CeresRidleyBodyPublicationVerification
{
    /// <summary>
    /// Ridley's getaway at two rotations publishes his simulation transform and the body frames his transfers drew
    /// into the room's Mode 7 map, which the Mode 7 capture draws; the arena publishes its blocks' pictures, which the import
    /// composed over what the arena loads (#1791).
    /// </summary>
    internal static void VerifyCeresRidleyBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(runtime.Display.Latched is OrdinaryRoomFrameLatch, "the arena shows its ordinary layers, its blocks' pictures among them, before the getaway");

        AssertTrue(runtime.Enemies.CeresRidley() is not null, "The Ceres Ridley room loads Ridley.");
        // The station's escape flies the getaway (#2228). Its setup places the floor's scroll (-128, 32), its centre
        // (64, 64), the starting table position and animation frames ($A6:A2E3); the check turns it from one step.
        CeresEscape escape = runtime.Enemies.CeresEscape;
        escape.BeginGetaway();
        escape.Floor.Angle = RotationAngleConstants.FromRaw(1);
        RotationAngle? previousAngle = null;
        foreach ((int nmiFrame, CeresFloorActorPicture wings) in new[] { new VerifyCeresRidleyBodyPublicationCase(0, CeresFloorActorPicture.WingsLowered), new VerifyCeresRidleyBodyPublicationCase(8, CeresFloorActorPicture.WingsRaised) })
        {
            escape.RunGetawayRoomMain(runtime.Player.Samus, runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ridley's arena shows his floor."), nmiFrame);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            AssertTrue(previousAngle is not { } previous || escape.Floor.Angle != previous, "the getaway turns Ridley between the two frames");
            previousAngle = escape.Floor.Angle;
            PerspectiveFloorFrameLatch getaway = runtime.Display.LatchedPerspectiveFloor;
            AssertEqual(escape.ShownFloor
                ?? throw new InvalidOperationException("Ridley flies his getaway."), getaway.Floor.Transform, $"angle {escape.Floor.Angle} publishes Ridley's floor");
            AssertTrue(getaway.BackgroundBand is not null, "the getaway shows the room's background below its floor");
            AffineFloor map = getaway.FloorMap;
            RoomFloorSheet sheet = (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("The arena shows a floor.")).Sheet;
            AssertTrue(map.DrawnOver.Contains(sheet.Picture(wings)),
                $"angle {escape.Floor.Angle} publishes his {wings} body frame");
            var mode7 = (TurningRoomLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
            AssertTrue(mode7.Floor.Equals(map), $"angle {escape.Floor.Angle} draws his published body");
            VerifyChamberWalls(mode7, runtime.Display.LatchedRoomFrame.Scrolls.ForegroundPosition);
        }
        Console.WriteLine("  Ceres Ridley body: two getaway rotations publish his transform and body frames on the Mode 7 layer.");
    }

    /// <summary>
    /// The getaway shows the chamber's two walls behind its turning floor (#1545), each picture's origin on its wall's room
    /// point, on a plane scrolled with the camera the actors are drawn against.
    /// </summary>
    private static void VerifyChamberWalls(TurningRoomLayer mode7, Position16<RoomPixels> camera)
    {
        TurningRoomScenery walls = mode7.Scenery ?? throw new InvalidOperationException("The getaway shows the chamber's walls.");
        Position16<RoomPixels>[] points = [CeresGetawayChamberWallDefinitions.LeftWall, CeresGetawayChamberWallDefinitions.RightWall];
        AssertEqual(points.Length, walls.Plane.Pictures.Length, "the getaway shows the chamber's two walls");
        for (int wall = 0; wall < points.Length; wall++)
        {
            PlacedPlanePicture placed = walls.Plane.Pictures[wall];
            AssertEqual(new Offset16<PlanePixels>(points[wall].X - placed.Picture.Origin.X, points[wall].Y - placed.Picture.Origin.Y),
                placed.Corner, $"wall {wall}'s picture places its origin on the wall's room point");
        }
        AssertEqual(camera.AsPlaneOffset(), walls.Scroll, "the walls scroll with the actors' camera");
    }

    /// <summary>Runs the Ceres Ridley body publication check.</summary>
    [VerificationCommand("--ceres-ridley-body")]
    internal static int CeresRidleyBodyCommand()
    {
        Suite(nameof(VerifyCeresRidleyBodyPublication), () => VerifyCeresRidleyBodyPublication());
        return 0;
    }
}
