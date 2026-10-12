using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using static SuiteTimingVerification;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using static RetailRuntimeFixtureVerification;

internal static class OceanSkyVerification
{
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ocean Sky")]
    internal static void VerifyOceanSky()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.WestOcean, camera: new Position16<RoomPixels>(128, 768));
        // Capture consumes the accepted NMI's register snapshot, not the room's
        // mutable scroll state. Draining only VRAM left the BG2 scanline window at
        // its zero-initialized 0..0 range, so both page layouts rendered no sky.
        runtime.RunNmi(0, mainLoopRequestedNmi: true);
        var scene = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        var ordinary = (OrdinaryGameplayRenderLayer)scene.Layers[0];
        Console.WriteLine($"Ocean room: main={runtime.ActiveRoom!.State.MainCodePointer:X4}, BG2={ordinary.Registers.Bg2MapSize.X}x{ordinary.Registers.Bg2MapSize.Y}, sky={runtime.ScrollingSky is not null}");

        AssertEqual(32, ordinary.Registers.Bg2MapSize.X, "Ocean setup $88:A800 writes BG2SC=$4A: 32 tiles wide");
        AssertEqual(64, ordinary.Registers.Bg2MapSize.Y, "Ocean setup uses two vertically stacked BG2 pages");
        AssertTrue(runtime.ScrollingSky is not null, "Ocean main installs scrolling-sky row streaming");
        var writes = new VramWriteQueue();
        // Travel down in one-tile increments. After the first screen, every visible row
        // has passed through the same circular queue used by the room's real main routine.
        for (ushort cameraY = 0; cameraY <= 768; cameraY += 8)
        {
            runtime.ScrollingSky!.ProcessFrame(cameraY, false, writes, runtime.ActiveRoom.State.MainCallback);
            writes.DrainTo(runtime.Vram, ReferenceMutableMemory.From(bus), runtime);
            if (cameraY < 256) continue;
            for (int line = 32; line < 224; line += 8)
            {
                int worldY = cameraY + line;
                int pointerAddress = 0x88ADA6 + RoomScreenOf.Pixel(worldY) * 2;
                int pointer = bus.ReadCartridgeWord(pointerAddress);
                for (int column = 0; column < 32; column++)
                {
                    int source = 0x8A0000 | (SnesAddressMath.OffsetInBank(pointer + (worldY & 0xF8) * 8 + column * 2));
                    ushort expected = bus.ReadCartridgeWord(source);
                    int destination = (0x4800 + ((worldY & 0x1F8) >> 3) * 32 + column) * 2;
                    ushort actual = (ushort)(SnesLittleEndian.Word(runtime.Vram.Bytes[destination], runtime.Vram.Bytes[destination + 1]));
                    AssertEqual(expected, actual, $"ocean visible map row {worldY:X4}, column {column} follows ROM ocean chunk");
                }
            }
        }
        scene = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        ordinary = (OrdinaryGameplayRenderLayer)scene.Layers[0];
        var bgOnly = new OrdinaryGameplayRenderLayer(ordinary.Registers with { MainScreenLayers = SnesMainScreenLayers.Bg2 },
            ordinary.Bg2LineScrolls);
        var background = SoftwareLayeredSnapshotRenderer.Render(new(scene.Memory, new RenderLayer[] { bgOnly }, scene.ObjectSelection, scene.Brightness));
        // Force the native BG2SC=$4A layout independently of the capture's selected dimensions.
        var nativeLayout = new OrdinaryGameplayRenderLayer(bgOnly.Registers with { Bg2MapSize = new Extent16<TilemapCells>(32, 64) },
            ordinary.Bg2LineScrolls);
        var expectedPixels = SoftwareLayeredSnapshotRenderer.Render(new(scene.Memory, new RenderLayer[] { nativeLayout }, scene.ObjectSelection, scene.Brightness));
        AssertTrue(background.SequenceEqual(expectedPixels), "ocean background has no horizontal insertion of the vertical page");
        var wrongLayout = new OrdinaryGameplayRenderLayer(bgOnly.Registers with { Bg2MapSize = new Extent16<TilemapCells>(64, 32) },
            ordinary.Bg2LineScrolls);
        var wrongPixels = SoftwareLayeredSnapshotRenderer.Render(new(scene.Memory, new RenderLayer[] { wrongLayout }, scene.ObjectSelection, scene.Brightness));
        AssertEqual(32, bgOnly.Registers.Bg2FirstScanline, "accepted ocean NMI exposes BG2 below the HUD");
        AssertEqual(224, bgOnly.Registers.Bg2EndScanline, "accepted ocean NMI exposes the full gameplay height");
        AssertTrue(!background.SequenceEqual(wrongPixels), "fixture exposes the old horizontal-page interpretation visibly");

        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Camera!.PixelPosition.X + 128), (ushort)(runtime.Camera.PixelPosition.Y + 128)));
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.StepFrame(0);
        ushort upper = unchecked((ushort)((runtime.Camera.PixelPosition.Y & 0x7F8) - 16));
        int sourcePointer = 0x88ADA6 + (SnesLittleEndian.HighByte(upper)) * 2;
        int sourceWord = bus.ReadCartridgeWord(sourcePointer);
        int expectedSource = 0x8A0000 | (SnesAddressMath.OffsetInBank(sourceWord + (SnesLittleEndian.LowByte(upper)) * 8));
        AssertTrue(runtime.VramWrites.Entries.Any(entry => entry.SourceAddress == expectedSource && entry.SizeInBytes == 64),
            "ordinary runtime frame queues the ocean table, not the land table");
        Console.WriteLine("  Ocean sky: visible rows across 65 camera positions match ocean ROM chunks and native vertical-page rendering.");
    }

    [VerificationCommand("--ocean-sky")]
    internal static int OceanSkyCommand()
    {
        Suite(nameof(VerifyOceanSky), () => VerifyOceanSky());
        return 0;
    }
}
