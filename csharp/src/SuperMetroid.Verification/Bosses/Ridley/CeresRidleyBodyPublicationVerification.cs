using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ceres Ridley's Mode 7 body as the room's published map and his latched transform (#1357).</summary>
internal static class CeresRidleyBodyPublicationVerification
{
    /// <summary>
    /// Ridley's getaway at two rotations publishes his simulation transform and the body frames his transfers drew
    /// into the room's Mode 7 map, which the Mode 7 capture draws; the arena's backgrounds draw from the object
    /// character page as room state, not a latched register.
    /// </summary>
    internal static void VerifyCeresRidleyBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(RoomBlockArtwork.Sprites, runtime.Display.Latched.Rooms?.BlockArtwork,
            "the arena's backgrounds draw from the object character page");

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
        foreach ((int nmiFrame, CeresFloorAnimationFrame wings) in new[] { new VerifyCeresRidleyBodyPublicationCase(0, CeresFloorAnimationFrame.WingFrame1), new VerifyCeresRidleyBodyPublicationCase(8, CeresFloorAnimationFrame.WingFrame0) })
        {
            runtime.Enemies.CeresRidleyBehavior.Getaway.TickCeresRidleyGetawayFloor(ridley, null, nmiFrame);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            AssertTrue(previousAngle is not { } previous || ridley.FloorAngle != previous, "the getaway turns Ridley between the two frames");
            previousAngle = ridley.FloorAngle;
            PerspectiveFloor floor = runtime.Display.Latched.PerspectiveFloor
                ?? throw new InvalidOperationException("Ridley's getaway publishes his floor.");
            AssertEqual(CeresPerspectiveFloors.RidleyGetaway(CeresRidleyGetaway.FloorTransform(ridley)
                ?? throw new InvalidOperationException("Ridley flies his getaway.")), floor, $"angle {ridley.FloorAngle} publishes Ridley's floor");
            AffineFloor map = runtime.Display.Latched.Rooms?.Floor
                ?? throw new InvalidOperationException("The arena publishes its Mode 7 map.");
            foreach (CeresFloorTileRun transfer in CeresFloorAnimation.Get(wings))
                AssertTrue(map.Cells.Slice(transfer.FloorCell, transfer.TileNumbers.Count).Select(cell => (byte)cell.Tile).SequenceEqual(transfer.TileNumbers),
                    $"angle {ridley.FloorAngle} publishes his {wings} body frame");
            var mode7 = (TurningRoomLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
            AssertTrue(mode7.Map.Cells.SequenceEqual(map.Cells), $"angle {ridley.FloorAngle} draws his published body");
        }
        Console.WriteLine("  Ceres Ridley body: two getaway rotations publish his transform and body frames on the Mode 7 layer.");
    }

    /// <summary>Runs the Ceres Ridley body publication check.</summary>
    [VerificationCommand("--ceres-ridley-body")]
    internal static int CeresRidleyBodyCommand()
    {
        Suite(nameof(VerifyCeresRidleyBodyPublication), () => VerifyCeresRidleyBodyPublication());
        return 0;
    }
}
