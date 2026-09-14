using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // Stage only the neighboring doorway. The destination is loaded exclusively by
    // the normal door coroutine, including population, FX, graphics and PLM ownership.
    private static void VerifyPillarFirstEntry()
    {
        const ushort source = 0xb3a5, destination = 0xb457;
        const string output = "csharp/test-temp/issue-619-pillar";
        Directory.CreateDirectory(output);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(source);
        var level = runtime.LevelData!;
        int doorX = -1, doorY = -1;
        byte behavior = 0;
        for (int y = 0; y < level.HeightInBlocks && doorX < 0; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, 1, false);
            if (door.DestinationRoomPointer != destination) continue;
            doorX = x; doorY = y; behavior = block.Behavior;
            break;
        }
        AssertTrue(doorX >= 0, "Pillar Room incoming retail door exists");
        runtime.LoadCartridgeRoomForDebug(source,
            (ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256));
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = samus.CollectedItems = (ushort)(SamusEquipmentFlags.ScrewAttack |
            SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots |
            SamusEquipmentFlags.SpaceJump);
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = (ushort)(doorX * 16 + 8);
        samus.YPosition = (ushort)(doorY * 16 + 8);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.RunNmi(0, true);
        runtime.LevelData!.ResolveDoorCollision(bus, behavior, samus.Pose, true);
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        for (int frame = 0; transition.IsActive && frame < 400; frame++)
            transition.Step(runtime, audio, 0);
        AssertTrue(!transition.IsActive, "Pillar Room transition completes");
        AssertEqual(destination, runtime.ActiveRoom!.Pointer, "Pillar Room entered through retail door");
        AssertEqual(2, runtime.Enemies.NuclearWaffleStates.Count(state => state is not null),
            "both Puromi actors survive normal destination initialization");

        using var trace = new StreamWriter(Path.Combine(output, "trace.csv"));
        trace.WriteLine("frame,samusX,samusY,cameraX,cameraY,head0X,head0Y,angle0,head1X,head1Y,angle1,pose");
        var visibleFrames = new int[2];
        var visiblePositions = new HashSet<(ushort X, ushort Y)>[] { new(), new() };
        for (int frame = 0; frame < 600; frame++)
        {
            ushort input = (ushort)SnesButton.Right;
            if (frame % 20 is >= 3 and < 18) input |= (ushort)SnesButton.A;
            runtime.StepFrame(input);
            var heads = runtime.Enemies.Slots.Take(2).ToArray();
            var states = runtime.Enemies.NuclearWaffleStates.Where(state => state is not null).ToArray();
            trace.WriteLine($"{frame},{samus.XPosition},{samus.YPosition},{runtime.Camera!.XPosition},{runtime.Camera.YPosition},{heads[0].XPosition},{heads[0].YPosition},{states[0]!.CurrentAngle},{heads[1].XPosition},{heads[1].YPosition},{states[1]!.CurrentAngle},{samus.Pose:X2}");
            if (frame % 8 != 0) continue;
            var snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
            var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
            // Remove all OBJ only from an immutable comparison frame. Keep the real
            // terrain, acid, color math and priority ladder: emitted OAM hidden behind
            // opaque scenery must not count as visible. Samus stays well above these
            // head-local windows, and the two owners are separated by 320 pixels.
            var memory = snapshot.Memory;
            var withoutObjects = new LayeredRenderSnapshot(
                new PpuMemorySnapshot(memory.Vram, memory.Cgram, memory.Oam, modeledSpriteCount: 0),
                snapshot.Layers, snapshot.ObjectSelection, snapshot.Brightness);
            var background = SoftwareLayeredSnapshotRenderer.Render(withoutObjects);
            for (int actor = 0; actor < 2; actor++)
            {
                int x = heads[actor].XPosition - runtime.Camera.XPosition;
                int y = heads[actor].YPosition - runtime.Camera.YPosition;
                if (x < 8 || x >= 248 || y < 40 || y >= 216) continue;
                if (Math.Abs((int)samus.XPosition - heads[actor].XPosition) < 32 &&
                    Math.Abs((int)samus.YPosition - heads[actor].YPosition) < 40) continue;
                int changed = 0;
                for (int dy = -8; dy < 8; dy++)
                for (int dx = -8; dx < 8; dx++)
                {
                    int pixel = (y + dy) * 256 + x + dx;
                    if (pixels[pixel] != background[pixel]) changed++;
                }
                if (changed < 8) continue;
                visibleFrames[actor]++;
                visiblePositions[actor].Add((heads[actor].XPosition, heads[actor].YPosition));
            }
            PngWriter.WriteRgba(Path.Combine(output, $"frame-{frame:D3}.png"), 256, 224, pixels);
        }
        for (int actor = 0; actor < 2; actor++)
        {
            Console.WriteLine($"Puromi {actor}: {visibleFrames[actor]} visible sampled frames, {visiblePositions[actor].Count} visible head positions.");
            AssertTrue(visibleFrames[actor] >= 2 && visiblePositions[actor].Count >= 2,
                $"Puromi {actor} visibly moves in the composited room, not merely OAM");
        }
        Console.WriteLine($"Pillar entry capture complete: {output}. Both actors visibly move after the retail door handoff; this is not a full pre-room playthrough replay.");
    }
}
