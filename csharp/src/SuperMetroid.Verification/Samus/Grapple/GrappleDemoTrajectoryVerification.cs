using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Runtime.Attract.AttractInputFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Runtime.Attract;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks the retail grapple attract demo's scripted shots, ceiling
/// connections, aim after release, and final landing.</summary>
internal static class GrappleDemoTrajectoryVerification
{
    /// <summary>Replays the basic grapple demo through production gameplay and
    /// checks six ordered attachments to the room's real ceiling anchors.</summary>
    internal static void VerifyGrappleDemoTrajectory()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var scene = SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(bus, 0, 4);
        AssertEqual(DemoInputObjectDefinitions.AttractGrappleBeam, (scene ?? throw new InvalidOperationException("Verification requires scene.")).InputObject,
            "retail basic grapple demo identity");
        var runtime = CreateRetailRuntimeFixture();
        runtime.AttractDemo.InitializeAttractDemo(scene);
        var samus = runtime.Player.IntroducedSamus;
        var level = runtime.Room.LoadedLevelData;
        var expectedColumns = new SortedSet<int>();
        for (int y = 0; y < level.Size.Y; y++)
            for (int x = 0; x < level.Size.X; x++)
                if (level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y))).CollisionType == RoomCollisionType.GrappleBlock)
                    expectedColumns.Add(x);
        AssertTrue(expectedColumns.Count > 0, "demo room supplies grapple ceiling terrain");
        var connectedColumns = new List<int>();
        var input = new ReferenceAttractInput(bus, scene);
        int scriptedShots = 0;
        GrapplePhase previousPhase = GrapplePhase.Inactive;
        for (int frame = 0; frame < scene.Duration; frame++)
        {
            input.Step(SuperMetroidGameState.PlayingDemo, samus.PoseMetadata.ReadMovementType());
            if ((input.Script.NewlyPressed & (ushort)SnesButton.X) != 0) scriptedShots++;
            runtime.StepFrame(0, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false } });
            if (frame == 164)
                AssertEqual(SamusPoseId.NormalJumpAimDiagonalUpRightPose, samus.Pose,
                    "held shoulder aim survives independent grapple-release movement ownership");
            if (frame == 168)
            {
                AssertEqual(GrapplePhase.Firing, samus.Grapple.Phase, "second scripted grapple fires");
                AssertEqual(SamusProjectileDirection.UpRight, samus.Grapple.FireDirection,
                    "second grapple aims up at the next ceiling anchor, not horizontally");
            }
            if (samus.Grapple.Phase == GrapplePhase.ConnectedSwinging &&
                previousPhase != GrapplePhase.ConnectedSwinging)
                connectedColumns.Add(samus.Grapple.Anchor.X >> 4);
            previousPhase = samus.Grapple.Phase;
        }
        AssertEqual(6, scriptedShots, "unmodified basic demo authors six grapple shots");
        AssertEqual(scriptedShots, connectedColumns.Count,
            "every scripted grapple shot connects during the complete demo");
        AssertTrue(connectedColumns.All(expectedColumns.Contains) &&
            connectedColumns.Zip(connectedColumns.Skip(1), (a, b) => b > a).All(increases => increases),
            "demo attachments advance across the real grapple ceiling in order");
        AssertTrue(samus.Position.X.Whole > (expectedColumns.Max + 1) * 16,
            "demo clears the last ceiling anchor and reaches the far landing");
        AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase,
            "demo releases its final grapple before finishing");
        Console.WriteLine("  Grapple demo: post-release aim, second-shot direction and all six ordered retail attachments agree.");
    }
}
