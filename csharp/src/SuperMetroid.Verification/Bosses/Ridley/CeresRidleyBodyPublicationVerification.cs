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
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(runtime.Display.Latched.Rooms?.Foreground.Pictures is not null, "the arena publishes its blocks' pictures");

        RidleyEnemyState ridley = runtime.Enemies.RidleyEncounter.CeresRidley
            ?? throw new InvalidOperationException("The Ceres Ridley room loads Ridley.");
        ridley.GetawayActive = true;
        ridley.FloorAngle = RotationAngle.FromRaw(1);
        ridley.FloorOffset = new Offset16<FloorPixels>(-128, 32);
        ridley.FloorCenter = new Position16<FloorPixels>(64, 64);
        // The getaway setup's starting table position and animation frames ($A6:A2E3).
        ridley.GetawayFrameIndex = 0;
        ridley.GetawayBabyFrame = 0;
        ridley.GetawayWingFrame = 0;
        RotationAngle? previousAngle = null;
        foreach ((int nmiFrame, CeresFloorAnimationFrame wings) in new[] { new VerifyCeresRidleyBodyPublicationCase(0, CeresFloorAnimationFrame.WingsLowered), new VerifyCeresRidleyBodyPublicationCase(8, CeresFloorAnimationFrame.WingsRaised) })
        {
            runtime.Enemies.CeresRidleyBehavior.Getaway.TickCeresRidleyGetawayFloor(ridley, null, nmiFrame);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            AssertTrue(previousAngle is not { } previous || ridley.FloorAngle != previous, "the getaway turns Ridley between the two frames");
            previousAngle = ridley.FloorAngle;
            PerspectiveFloor floor = runtime.Display.Latched.PerspectiveFloor
                ?? throw new InvalidOperationException("Ridley's getaway publishes his floor.");
            AssertEqual(CeresRidleyGetaway.FloorTransform(ridley)
                ?? throw new InvalidOperationException("Ridley flies his getaway."), floor.Transform, $"angle {ridley.FloorAngle} publishes Ridley's floor");
            AssertEqual(BelowPerspectiveFloor.RoomBackground, floor.Below, "the getaway shows the room's background below its floor");
            AffineFloor map = runtime.Display.Latched.Rooms?.Floor
                ?? throw new InvalidOperationException("The arena publishes its floor.");
            RoomFloorSheet sheet = (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("The arena shows a floor.")).Sheet;
            AssertTrue(map.DrawnOver.Contains(sheet.Picture(wings)),
                $"angle {ridley.FloorAngle} publishes his {wings} body frame");
            var mode7 = (TurningRoomLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
            AssertTrue(mode7.Map.Equals(map), $"angle {ridley.FloorAngle} draws his published body");
            VerifyChamberWalls(mode7, runtime.Display.Latched.Scrolls.ForegroundPosition);
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
