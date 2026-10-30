using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks D3D11 Mode 7 rendering against the WARP reference over native PPU geometry.</summary>
internal static class Mode7SmokeTests
{
    /// <summary>Compares scale, offset, overflow, ordered-layer, HUD, and floor-band cases with the WARP reference's exact pixels.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32113);
        var vram = new byte[SnesVram.ByteCount];
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        random.NextBytes(vram); random.NextBytes(oam);
        RgbColor[] palette = Enumerable.Range(0, 256).Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
        var memory = FramePaint.FromOam(vram, palette, oam, 128, 3);
        // The HUD is its own layer (#1466), drawn over the lines the floor leaves to it.
        PicturedHud hudPictures = PicturedHudFixtures.FromVram(vram, palette, 0x4000);
        var hudLayer = new HudLayer(hudPictures, TransparentColorZero: false, Priority: null);
        AffineFloor map = AffineFloorFixtures.Floor(vram, palette);
        // Below the floor band, the room's layer-2 plane drawn from its block pictures (#1403).
        RoomBackgroundLayer floorRoom = RoomBackgroundsFixtures.Seeded(random, default, new Position16<TilePlanePixels>(17, 65535), pictureColours: palette).Bg2
            ?? throw new InvalidOperationException("A seeded room with a BG2 scroll has a layer-2 plane.");
        int count = 0;
        foreach (short scale in new short[] { 0, 1, 255, 256, 512, -256, short.MinValue, short.MaxValue })
        foreach (short offset in new short[] { -1024, -1, 0, 1023, short.MinValue, short.MaxValue })
        foreach (FloorOverflowMode overflow in Enum.GetValues<FloorOverflowMode>())
        {
            var registers = new AffineTransform(scale, 113, -113, scale, new Offset16<FloorPixels>(128, 112), new Offset16<FloorPixels>(offset, offset), overflow);
            var frame = new DisplayPacket(new(++count, 1, 0),
                new FloorScene(memory, registers, DisplayBrightness.Combine(13, 11), floor: map));
            Check(frame);
            Check(new DisplayPacket(new(++count, 1, 0), new LayeredScene(memory,
                new DisplayLayer[] { new SpritesLayer(), new TurningFloorLayer(registers, map),
                    new ScreenTintLayer(3, 7, 11) }, 15)));
            Check(new DisplayPacket(new(++count, 1, 0), new LayeredScene(memory,
                new DisplayLayer[] { new TurningFloorLayer(registers, map, AddsForeground: true) }, 15)));
        }
        Check(new DisplayPacket(new(++count, 1, 0), new FloorScene(memory, null, 15)));
        foreach (int hud in new[] { 0, 8, 32, 208 })
        foreach (int? floorStart in new int?[] { null, 208, 223 })
        foreach (short scale in new short[] { 256, -113, 511, 0 })
        {
            var band = floorStart is { } start
                ? new TurningRoomLowerBand(start, new Position16<BackgroundPixels>(17, 65535), new Extent16<TilemapCells>(64, 32), floorRoom)
                : (TurningRoomLowerBand?)null;
            var layer = new TurningRoomLayer(
                new(scale, 71, -71, scale, new(128, 112), new(-17, 211), FloorOverflowMode.Wrap),
                map, hud, band);
            Check(new DisplayPacket(new(++count, 1, 0),
                new LayeredScene(memory, hud == 0 ? new DisplayLayer[] { layer } : new DisplayLayer[] { layer, hudLayer }, 13)));
        }
        // A Ceres room's floor with its floor animation frames drawn over it (#1467), and the fixture's floor, with outside
        // pixels transparent, as Ceres rooms show them, or repeating.
        FloorMemory floorMemory = AffineFloorConstants.Split(vram);
        var ceres = new RoomFloor(AffineFloorConstants.CeresRoom(floorMemory.Map, floorMemory.Pixels, palette));
        ceres.Draw(CeresFloorAnimation.PictureOf(CeresFloorAnimationFrame.ElevatorDark));
        ceres.Draw(CeresFloorAnimation.PictureOf(CeresFloorAnimationFrame.WingFrame1));
        ceres.Draw(CeresFloorAnimation.PictureOf(CeresFloorAnimationFrame.BabyFrame2));
        foreach (AffineFloor shown in new[] { map, ceres.Publish() })
        foreach (int hud in new[] { 0, 32 })
        foreach (short scale in new short[] { 256, -113, 511 })
        foreach (bool fill in new[] { false, true })
        {
            var layer = new TurningRoomLayer(
                new(scale, 71, -71, scale, new(128, 112), new(-17, 211), fill ? FloorOverflowMode.Wrap : FloorOverflowMode.Transparent),
                shown, hud, null);
            Check(new DisplayPacket(new(++count, 1, 0),
                new LayeredScene(memory, hud == 0 ? new DisplayLayer[] { layer } : new DisplayLayer[] { layer, hudLayer }, 13)));
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact Mode 7 comparisons passed.");

        void Check(DisplayPacket frame) => PixelComparison.Verify(frame,
            DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
            $"{device.Kind}: Mode 7 sample {count}");
    }
}
