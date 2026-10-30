using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Rooms;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks that both Nuclear Waffles visibly move after normal entry into Pillar Room.</summary>
internal static class PillarFirstEntryVerification
{
    /// <summary>Stages the neighboring doorway and lets the normal coroutine load population, FX, graphics, and PLMs.</summary>
    internal static void VerifyPillarFirstEntry()
    {
        RoomHeaderDefinition source = RoomHeaderDefinitions.FastPillarsSetup, destination = RoomHeaderDefinitions.Pillar;

        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(source);
        var level = runtime.Room.LoadedLevelData;
        int doorX = -1, doorY = -1;
        int behavior = 0;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(block.Behavior.Value, SamusPoseId.FacingRightNormalPose.ForRoomCollision(), false);
            if (door.Door?.DestinationRoom != destination) continue;
            doorX = x; doorY = y; behavior = block.Behavior.Value;
            break;
        }
        AssertTrue(doorX >= 0, "Pillar Room incoming retail door exists");
        runtime.LoadCartridgeRoomForDebug(source,
            new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.EquippedItems = samus.CollectedItems = (SamusEquipmentFlags.ScrewAttack |
            SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots |
            SamusEquipmentFlags.SpaceJump);
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.Room.LoadedLevelData.ResolveDoorCollision(behavior, samus.Pose.ForRoomCollision(), true);
        var transition = new DoorTransitionState();
        var audio = new AudioQueues();
        transition.Begin(runtime);
        for (int frame = 0; transition.IsActive && frame < 400; frame++)
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Compatibility.InstantRoomLoad.Instance);
        AssertTrue(!transition.IsActive, "Pillar Room transition completes");
        AssertEqual(destination, runtime.Room.LoadedRoom.Definition, "Pillar Room entered through retail door");
        AssertEqual(2, runtime.Enemies.NuclearWaffleStates.Count(state => state is not null),
            "both Puromi actors survive normal destination initialization");

        var visibleFrames = new int[2];
        var visiblePositions = new HashSet<MotherBrainRinkaDoorSpawnExpected>[] { new(), new() };
        for (int frame = 0; frame < 600; frame++)
        {
            ControllerButton input = ControllerButton.Right;
            if (frame % 20 is >= 3 and < 18) input |= ControllerButton.A;
            runtime.StepFrame(input);
            var heads = runtime.Enemies.SlotTable.Slots.Take(2).ToArray();
            var states = runtime.Enemies.NuclearWaffleStates.Where(state => state is not null).ToArray();
            if (frame % 8 != 0) continue;
            var snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            var pixels = DisplayRendering.Render(snapshot);
            // Remove all OBJ only from an immutable comparison frame. Keep the real
            // terrain, acid, color math and priority ladder: emitted OAM hidden behind
            // opaque scenery must not count as visible. Samus stays well above these
            // head-local windows, and the two owners are separated by 320 pixels.
            var memory = snapshot.Paint;
            var withoutObjects = new LayeredScene(
                FramePaint.FromNative(memory.Colors, []),
                snapshot.Layers, snapshot.Brightness);
            var background = DisplayRendering.Render(withoutObjects);
            for (int actor = 0; actor < 2; actor++)
            {
                int x = heads[actor].Position.X.Whole - runtime.Room.LoadedCamera.PixelPosition.X;
                int y = heads[actor].Position.Y.Whole - runtime.Room.LoadedCamera.PixelPosition.Y;
                if (x < 8 || x >= 248 || y < 40 || y >= 216) continue;
                if (Math.Abs(samus.Position.X.Whole - heads[actor].Position.X.Whole) < 32 &&
                    Math.Abs(samus.Position.Y.Whole - heads[actor].Position.Y.Whole) < 40) continue;
                int changed = 0;
                for (int dy = -8; dy < 8; dy++)
                for (int dx = -8; dx < 8; dx++)
                {
                    int pixel = (y + dy) * 256 + x + dx;
                    if (pixels[pixel] != background[pixel]) changed++;
                }
                if (changed < 8) continue;
                visibleFrames[actor]++;
                visiblePositions[actor].Add(new MotherBrainRinkaDoorSpawnExpected(heads[actor].Position.X.Whole, heads[actor].Position.Y.Whole));
            }

        }
        for (int actor = 0; actor < 2; actor++)
        {
            Console.WriteLine($"Puromi {actor}: {visibleFrames[actor]} visible sampled frames, {visiblePositions[actor].Count} visible head positions.");
            AssertTrue(visibleFrames[actor] >= 2 && visiblePositions[actor].Count >= 2,
                $"Puromi {actor} visibly moves in the composited room, not merely OAM");
        }
        Console.WriteLine($"Pillar entry complete. Both actors visibly move after the retail door handoff; this is not a full pre-room playthrough replay.");
    }

    /// <summary>Runs the Pillar Room first-entry visibility check.</summary>
    [VerificationCommand("--pillar-first-entry")]
    internal static int PillarFirstEntryCommand()
    {
        Suite(nameof(VerifyPillarFirstEntry), () => VerifyPillarFirstEntry());
        return 0;
    }
}
