using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.RenderVerification.Device;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Compares ordinary gameplay rendering across native geometry, layer, and HDMA settings.</summary>
internal static class OrdinarySmokeTests
{
    /// <summary>Checks GPU pixels against software output and bounds steady-state CPU submission allocations.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        HardwareWindowTests.Run(device, renderer);
        var random = new Random(32115);
        var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount]; random.NextBytes(oam);
        Bgr555[] colors = Enumerable.Range(0, 256).Select(_ => Bgr555.FromWord((ushort)random.Next(32768))).ToArray();
        var memory = PpuMemorySnapshot.FromOam(vram, colors, oam, 128, 3);
        // Even HDMA cases show BG2 from a room plane; case one from the ocean sky's rows, whose last chunks show
        // nothing (#1347); case three from the VRAM tilemap.
        var rooms = RoomBackgroundsFixtures.Seeded(random, new(511, 255), new(65535, 511), door: true, overlay: true);
        var sky = rooms with { Bg2 = null, Sky = RoomSkyTilemapFixtures.Numbered().Tilemap(ScrollingSkyChunkTable.Ocean) };
        ushort[] x = Enumerable.Range(0, 192).Select(i => unchecked((ushort)(65535 - i * 17))).ToArray();
        ushort[] y = Enumerable.Range(0, 192).Select(i => unchecked((ushort)(511 + i * 3))).ToArray();
        int count = 0;
        foreach (var geometry in new[] { new OrdinarySmokeRunCase(64,32), new OrdinarySmokeRunCase(32,64), new OrdinarySmokeRunCase(64,64) })
        for (int mask = 0; mask < 8; mask++)
        for (int hdma = 0; hdma < 4; hdma++)
        {
            var flags = (mask & 1) != 0 ? SnesMainScreenLayers.Bg1 : 0;
            if ((mask & 2) != 0) flags |= SnesMainScreenLayers.Bg2;
            if ((mask & 4) != 0) flags |= SnesMainScreenLayers.Obj;
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>((ushort)geometry.First, (ushort)geometry.Second), 0x4800, 0, 0x6000, 0x4000, flags);
            var layer = new OrdinaryGameplayRenderLayer(registers, hdma % 2 == 0 ? rooms : hdma == 1 ? sky : rooms with { Bg2 = null }, new Bg2HdmaScrollTables(
                (hdma & 1) != 0 ? x : default, (hdma & 2) != 0 ? y : default));
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, geometry {geometry}, layers {flags}, HDMA {hdma}");
            if (count == 96)
            {
                for (int warmup = 0; warmup < 8; warmup++) renderer.Render(frame);
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int sample = 0; sample < 32; sample++) renderer.Render(frame);
                long allocated = (GC.GetAllocatedBytesForCurrentThread() - before) / 32;
                Console.WriteLine($"{device.Kind}: ordinary CPU submission allocates {allocated} bytes/frame (capture/readback excluded).");
                if (allocated > 4096) throw new InvalidOperationException($"Ordinary submission allocates {allocated} bytes/frame; upload scratch must be reused.");
                GpuTimingTests.Run(device, renderer, frame);
            }
        }
        // Planes drawn from block pictures (#1392), some rows recoloured by the scene, with every layer mask.
        var pictured = RoomBackgroundsFixtures.Seeded(random, new(511, 255), new(65535, 511), door: true, overlay: true, pictureColours: colors);
        for (int mask = 0; mask < 8; mask++)
        for (int hdma = 0; hdma < 4; hdma += 3)
        {
            var flags = (mask & 1) != 0 ? SnesMainScreenLayers.Bg1 : 0;
            if ((mask & 2) != 0) flags |= SnesMainScreenLayers.Bg2;
            if ((mask & 4) != 0) flags |= SnesMainScreenLayers.Obj;
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>(64, 32), 0x4800, 0, 0x6000, 0x4000, flags);
            var layer = new OrdinaryGameplayRenderLayer(registers, pictured, new Bg2HdmaScrollTables(
                (hdma & 1) != 0 ? x : default, (hdma & 2) != 0 ? y : default));
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, block pictures, layers {flags}, HDMA {hdma}");
        }
        // A library background drawn from its image and rebound cells' pictures (#1392), through each map size BG2 takes.
        var background = pictured with { Bg2 = null, Bg2Image = RoomBackgroundsFixtures.SeededBackground(random, colors) };
        foreach (var geometry in new[] { new OrdinarySmokeRunCase(64, 32), new OrdinarySmokeRunCase(32, 64), new OrdinarySmokeRunCase(32, 32) })
        for (int hdma = 0; hdma < 4; hdma += 3)
        {
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>((ushort)geometry.First, (ushort)geometry.Second), 0x4800, 0, 0x6000, 0x4000,
                SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
            var layer = new OrdinaryGameplayRenderLayer(registers, background, new Bg2HdmaScrollTables(
                (hdma & 1) != 0 ? x : default, (hdma & 2) != 0 ? y : default));
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, background image, geometry {geometry}, HDMA {hdma}");
        }
        // The scrolling sky drawn from its image (#1392), some rows showing no page, with and without HDMA scrolls.
        var skyImage = sky with { SkyImage = RoomBackgroundsFixtures.SeededSky(random, colors) };
        for (int hdma = 0; hdma < 4; hdma += 3)
        {
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>(32, 64), 0x4800, 0, 0x6000, 0x4000, SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
            var layer = new OrdinaryGameplayRenderLayer(registers, skyImage, new Bg2HdmaScrollTables(
                (hdma & 1) != 0 ? x : default, (hdma & 2) != 0 ? y : default));
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, sky image, HDMA {hdma}");
        }
        // A boss body's BG2 tilemap (#1352) at the 64x64 map its registers select, with and without HDMA scrolls.
        var boss = rooms with
        {
            Bg2 = null,
            BossBg2 = new BossBg2Tilemap([.. Enumerable.Range(0, 64 * 64).Select(_ => (ushort)random.Next(65536))], new(64, 64)),
        };
        for (int hdma = 0; hdma < 4; hdma++)
        {
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>(64, 64), 0x4800, 0, 0x6000, 0x4000, SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
            var layer = new OrdinaryGameplayRenderLayer(registers, boss, new Bg2HdmaScrollTables(
                (hdma & 1) != 0 ? x : default, (hdma & 2) != 0 ? y : default));
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, boss BG2 tilemap, HDMA {hdma}");
        }
        // The HUD drawn from two-bit pictures (#1392), colour zero opaque in its row's colour, some words from their characters.
        var hudPictured = pictured with { Bg3 = RoomBackgroundsFixtures.SeededBg3Pictures(random, colors, pictured.Bg3) };
        for (int hdma = 0; hdma < 4; hdma += 3)
        {
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>(64, 32), 0x4800, 0, 0x6000, 0x4000, SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
            var layer = new OrdinaryGameplayRenderLayer(registers, hudPictured, new Bg2HdmaScrollTables(
                (hdma & 1) != 0 ? x : default, (hdma & 2) != 0 ? y : default));
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, BG3 pictures, HDMA {hdma}");
        }
        // Words written over BG2 drawn from their pictures (#1392), some still from their characters, over each BG2 map size.
        var typed = pictured with { Bg2Overlay = RoomBackgroundsFixtures.SeededPicturedOverlay(random, colors) };
        foreach (var geometry in new[] { new OrdinarySmokeRunCase(64, 32), new OrdinarySmokeRunCase(32, 64) })
        {
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>((ushort)geometry.First, (ushort)geometry.Second), 0x4800, 0, 0x6000, 0x4000,
                SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
            var layer = new OrdinaryGameplayRenderLayer(registers, typed, default);
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, BG2 overlay pictures, geometry {geometry}");
        }
        // Words written over BG1 drawn from their pictures (#1392), as the Ceres escape's warning is, some still from their
        // characters.
        var warned = pictured with { Bg1Overlay = RoomBackgroundsFixtures.SeededPicturedOverlay(random, colors) };
        {
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>(64, 32), 0x4800, 0, 0x6000, 0x4000, SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
            var layer = new OrdinaryGameplayRenderLayer(registers, warned, default);
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, BG1 overlay pictures");
        }
        // The boss body drawn from its pictures (#1392), some words still from their characters.
        BossBg2Tilemap bossTilemap = boss.BossBg2 ?? throw new InvalidOperationException("The boss case has no body.");
        var bodyPictured = boss with { BossBg2 = bossTilemap.WithPictures(RoomBackgroundsFixtures.SeededBodyPictures(random, colors, bossTilemap.Words.Length)) };
        for (int hdma = 0; hdma < 4; hdma += 3)
        {
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                new Extent16<TilemapCells>(64, 64), 0x4800, 0, 0x6000, 0x4000, SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
            var layer = new OrdinaryGameplayRenderLayer(registers, bodyPictured, new Bg2HdmaScrollTables(
                (hdma & 1) != 0 ? x : default, (hdma & 2) != 0 ? y : default));
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, boss body pictures, HDMA {hdma}");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact ordinary gameplay comparisons passed.");
    }
}
