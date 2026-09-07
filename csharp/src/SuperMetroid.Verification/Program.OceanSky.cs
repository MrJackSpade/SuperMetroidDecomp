using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyOceanSky()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.WestOcean, cameraX: 128, cameraY: 768);
        runtime.VramWrites.DrainTo(runtime.Vram, bus);
        var scene = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        var ordinary = (OrdinaryGameplayRenderLayer)scene.Layers[0];
        Console.WriteLine($"Ocean room: main={runtime.ActiveRoom!.State.MainCodePointer:X4}, BG2={ordinary.Registers.Bg2WidthTiles}x{ordinary.Registers.Bg2HeightTiles}, sky={runtime.ScrollingSky is not null}");
        Directory.CreateDirectory("csharp/test-temp/issue-349-ocean");
        PngWriter.WriteRgba("csharp/test-temp/issue-349-ocean/current.png", 256, 224,
            SoftwareLayeredSnapshotRenderer.Render(scene));
        AssertEqual(32, ordinary.Registers.Bg2WidthTiles, "Ocean setup $88:A800 writes BG2SC=$4A: 32 tiles wide");
        AssertEqual(64, ordinary.Registers.Bg2HeightTiles, "Ocean setup uses two vertically stacked BG2 pages");
        AssertTrue(runtime.ScrollingSky is not null, "Ocean main installs scrolling-sky row streaming");
        var writes = new VramWriteQueue();
        // Travel down in one-tile increments. After the first screen, every visible row
        // has passed through the same circular queue used by the room's real main routine.
        for (ushort cameraY = 0; cameraY <= 768; cameraY += 8)
        {
            runtime.ScrollingSky!.ProcessFrame(cameraY, false, writes, runtime.ActiveRoom.State.MainCodePointer);
            writes.DrainTo(runtime.Vram, bus);
            if (cameraY < 256) continue;
            for (int line = 32; line < 224; line += 8)
            {
                int worldY = cameraY + line;
                int pointerAddress = 0x88ADA6 + (worldY >> 8) * 2;
                int pointer = bus.ReadByte(pointerAddress) | bus.ReadByte(pointerAddress + 1) << 8;
                for (int column = 0; column < 32; column++)
                {
                    int source = 0x8A0000 | ((pointer + (worldY & 0xF8) * 8 + column * 2) & 0xFFFF);
                    ushort expected = (ushort)(bus.ReadByte(source) | bus.ReadByte(source + 1) << 8);
                    int destination = (0x4800 + ((worldY & 0x1F8) >> 3) * 32 + column) * 2;
                    ushort actual = (ushort)(runtime.Vram.Bytes[destination] | runtime.Vram.Bytes[destination + 1] << 8);
                    AssertEqual(expected, actual, $"ocean visible map row {worldY:X4}, column {column} follows ROM ocean chunk");
                }
            }
        }
        scene = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        ordinary = (OrdinaryGameplayRenderLayer)scene.Layers[0];
        var bgOnly = new OrdinaryGameplayRenderLayer(ordinary.Registers with { MainScreenLayers = SnesMainScreenLayers.Bg2 },
            ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
        var background = SoftwareLayeredSnapshotRenderer.Render(new(scene.Memory, new RenderLayer[] { bgOnly }, scene.ObjectSelection, scene.Brightness));
        // Force the native BG2SC=$4A layout independently of the capture's selected dimensions.
        var nativeLayout = new OrdinaryGameplayRenderLayer(bgOnly.Registers with { Bg2WidthTiles = 32, Bg2HeightTiles = 64 },
            ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
        var expectedPixels = SoftwareLayeredSnapshotRenderer.Render(new(scene.Memory, new RenderLayer[] { nativeLayout }, scene.ObjectSelection, scene.Brightness));
        AssertTrue(background.SequenceEqual(expectedPixels), "ocean background has no horizontal insertion of the vertical page");
        var wrongLayout = new OrdinaryGameplayRenderLayer(bgOnly.Registers with { Bg2WidthTiles = 64, Bg2HeightTiles = 32 },
            ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
        var wrongPixels = SoftwareLayeredSnapshotRenderer.Render(new(scene.Memory, new RenderLayer[] { wrongLayout }, scene.ObjectSelection, scene.Brightness));
        AssertTrue(!background.SequenceEqual(wrongPixels), "fixture exposes the old horizontal-page interpretation visibly");
        PngWriter.WriteRgba("csharp/test-temp/issue-349-ocean/wrong-layout.png", 256, 224, wrongPixels);
        PngWriter.WriteRgba("csharp/test-temp/issue-349-ocean/background.png", 256, 224, background);
        PngWriter.WriteRgba("csharp/test-temp/issue-349-ocean/after.png", 256, 224, SoftwareLayeredSnapshotRenderer.Render(scene));
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = (ushort)(runtime.Camera!.XPosition + 128);
        samus.YPosition = (ushort)(runtime.Camera.YPosition + 128);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.StepFrame(0);
        ushort upper = unchecked((ushort)((runtime.Camera.YPosition & 0x7F8) - 16));
        int sourcePointer = 0x88ADA6 + (upper >> 8) * 2;
        int sourceWord = bus.ReadByte(sourcePointer) | bus.ReadByte(sourcePointer + 1) << 8;
        int expectedSource = 0x8A0000 | ((sourceWord + (upper & 255) * 8) & 65535);
        AssertTrue(runtime.VramWrites.Entries.Any(entry => entry.SourceAddress == expectedSource && entry.SizeInBytes == 64),
            "ordinary runtime frame queues the ocean table, not the land table");
        Console.WriteLine("  Ocean sky: visible rows across 65 camera positions match ocean ROM chunks and native vertical-page rendering.");
    }
}
