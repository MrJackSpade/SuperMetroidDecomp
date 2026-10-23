using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

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
        AssertEqual(RoomBackgroundCharacters.ObjectCharacterPage, runtime.Display.Latched.Rooms?.Characters,
            "the arena's backgrounds draw from the object character page");

        RidleyEnemyState ridley = runtime.Enemies.RidleyEncounter.CeresRidley
            ?? throw new InvalidOperationException("The Ceres Ridley room loads Ridley.");
        ridley.Mode7Active = true;
        ridley.Mode7Angle = SnesAngle.FromRaw(1);
        ridley.Mode7Offset = new Offset16<Mode7Pixels>(-128, 32);
        ridley.Mode7Center = new Position16<Mode7Pixels>(64, 64);
        // The getaway setup's starting table position and animation frames ($A6:A2E3).
        ridley.Mode7TableIndex = 0;
        ridley.Mode7BabyFrame = 0;
        ridley.Mode7WingFrame = 0;
        SnesAngle? previousAngle = null;
        foreach ((int nmiFrame, CeresMode7TransferList wings) in new[] { new VerifyCeresRidleyBodyPublicationCase(0, CeresMode7TransferList.WingFrame1), new VerifyCeresRidleyBodyPublicationCase(8, CeresMode7TransferList.WingFrame0) })
        {
            runtime.Enemies.CeresRidleyBehavior.Getaway.TickCeresRidleyMode7Getaway(ridley, null, nmiFrame);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            AssertTrue(previousAngle is not { } previous || ridley.Mode7Angle != previous, "the getaway turns Ridley between the two frames");
            previousAngle = ridley.Mode7Angle;
            CeresRidleyMode7Snapshot transform = runtime.Display.Latched.Bosses.CeresRidleyMode7
                ?? throw new InvalidOperationException("Ridley's getaway publishes his Mode 7 transform.");
            AssertEqual(CeresRidleyMode7Snapshot.Capture(ridley), transform, $"angle {ridley.Mode7Angle} publishes Ridley's transform");
            AffineFloor map = runtime.Display.Latched.Rooms?.Mode7
                ?? throw new InvalidOperationException("The arena publishes its Mode 7 map.");
            foreach (CeresMode7Transfer transfer in CeresMode7TransferDefinitions.Get(wings))
                AssertTrue(map.Cells.Slice(transfer.DestinationWord, transfer.TileNumbers.Count).Select(cell => (byte)cell.Tile).SequenceEqual(transfer.TileNumbers),
                    $"angle {ridley.Mode7Angle} publishes his {wings} body frame");
            var mode7 = (Mode7GameplayRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
            AssertTrue(mode7.Map.Cells.SequenceEqual(map.Cells), $"angle {ridley.Mode7Angle} draws his published body");
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
