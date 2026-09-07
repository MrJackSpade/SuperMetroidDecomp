using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGrappleDemoTrajectory()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var scene = AttractDemoScene.Read(bus, 0, 4)!;
        AssertEqual(AttractDemoRomData.InputObjects.GrappleBeam, scene.InputObject,
            "retail basic grapple demo identity");
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeAttractDemo(scene);
        var samus = runtime.Samus!;
        var level = runtime.LevelData!;
        var expectedColumns = new SortedSet<int>();
        for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
                if (level.GetCollisionBlock(x, y).CollisionType == RoomCollisionType.GrappleBlock)
                    expectedColumns.Add(x);
        AssertTrue(expectedColumns.Count > 0, "demo room supplies grapple ceiling terrain");
        var connectedColumns = new List<int>();
        var input = new AttractDemoInput(bus, scene);
        int scriptedShots = 0;
        GrapplePhase previousPhase = GrapplePhase.Inactive;
        for (int frame = 0; frame < scene.Duration; frame++)
        {
            input.Step(bus, SuperMetroidGameState.PlayingDemo, samus.ReadMovementType(bus));
            if ((input.Script.NewlyPressed & (ushort)SnesButton.X) != 0) scriptedShots++;
            runtime.StepFrame(0, advanceGameTime: false);
            if (frame == 164)
                AssertEqual(SamusPoseIds.NormalJumpAimDiagonalUpRightPose, samus.Pose,
                    "held shoulder aim survives independent grapple-release movement ownership");
            if (frame == 168)
            {
                AssertEqual(GrapplePhase.Firing, samus.Grapple.Phase, "second scripted grapple fires");
                AssertEqual((byte)SamusProjectileDirection.UpRight, samus.Grapple.FireDirection,
                    "second grapple aims up at the next ceiling anchor, not horizontally");
            }
            if (samus.Grapple.Phase == GrapplePhase.ConnectedSwinging &&
                previousPhase != GrapplePhase.ConnectedSwinging)
                connectedColumns.Add(samus.Grapple.AnchorX >> 4);
            previousPhase = samus.Grapple.Phase;
        }
        AssertEqual(6, scriptedShots, "unmodified basic demo authors six grapple shots");
        AssertEqual(scriptedShots, connectedColumns.Count,
            "every scripted grapple shot connects during the complete demo");
        AssertTrue(connectedColumns.All(expectedColumns.Contains) &&
            connectedColumns.Zip(connectedColumns.Skip(1), (a, b) => b > a).All(increases => increases),
            "demo attachments advance across the real grapple ceiling in order");
        AssertTrue(samus.XPosition > (expectedColumns.Max + 1) * 16,
            "demo clears the last ceiling anchor and reaches the far landing");
        AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase,
            "demo releases its final grapple before finishing");
        Console.WriteLine("  Grapple demo: post-release aim, second-shot direction and all six ordered retail attachments agree.");
    }
}
