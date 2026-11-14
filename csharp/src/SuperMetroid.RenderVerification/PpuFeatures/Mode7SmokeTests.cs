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
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks D3D11 turning-floor rendering against the WARP reference.</summary>
internal static class Mode7SmokeTests
{
    /// <summary>Compares angle, scale, offset, overflow, ordered-layer, HUD, and floor-band cases with the WARP reference's exact pixels.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32113);
        var vram = new byte[SnesVram.ByteCount];
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        random.NextBytes(vram); random.NextBytes(oam);
        RgbColor[] palette = Enumerable.Range(0, 256).Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
        // A turning floor is a frontend screen's; a turning room drawn with the room's lower band is a gameplay frame's (#1512).
        var memory = FramePaint.FromOam(vram, palette, oam, 128, 3);
        var gameplayMemory = FramePaint.FromOamGameplay(vram, palette, oam, 128, 3);
        // The HUD is its own layer (#1466), drawn over the lines the floor leaves to it.
        PicturedHud hudPictures = PicturedHudFixtures.FromVram(vram, palette, 0x4000);
        var hudLayer = new HudLayer(hudPictures, DrawsOpaque: true);
        AffineFloor map = AffineFloorFixtures.Floor(vram, palette);
        // Below the floor band, the room's layer-2 plane drawn from its block pictures (#1403).
        RoomBackgroundLayer floorRoom = RoomBackgroundsFixtures.Seeded(random, default, new Position16<PlanePixels>(17, 65535), pictureColours: palette).Background
            ?? throw new InvalidOperationException("A seeded room with a BG2 scroll has a layer-2 plane.");
        int count = 0;
        foreach (double angle in new[] { 0, 0.4, Math.PI / 2, -2.5 })
        foreach (double scale in new[] { 0, 0.25, 1, 2, 127.5 })
        foreach (short offset in new short[] { -1024, -1, 0, 1023, short.MinValue, short.MaxValue })
        foreach (FloorOverflowMode overflow in Enum.GetValues<FloorOverflowMode>())
        {
            var registers = new FloorView(angle, scale, new Offset16<FloorPixels>(128, 112), new Offset16<FloorPixels>(offset, offset), overflow);
            var frame = new DisplayPacket(0,
                new FloorScene(memory, registers, DisplayBrightness.Combine(13, 11), floor: map));
            Check(frame);
            Check(new DisplayPacket(0, new LayeredScene(new FramePaint(memory.Backdrop),
                new DisplayLayer[] { new SpritesLayer(memory.SpriteDraws), new TurningFloorLayer(registers, map),
                    new ScreenTintLayer(new FadeLevelColour(3, 7, 11).ToColour()) }, 15)));
            Check(new DisplayPacket(0, new LayeredScene(memory,
                new DisplayLayer[] { new TurningFloorLayer(registers, map, AddsForeground: true) }, 15)));
        }
        Check(new DisplayPacket(0, new FloorScene(memory, null, 15)));
        foreach (int hud in new[] { 0, 8, 32, 208 })
        foreach (int? floorStart in new int?[] { null, 208, 223 })
        foreach (double scale in new[] { 1, 0.5, 2, 0 })
        {
            var band = floorStart is { } start
                ? new TurningRoomLowerBand(start, new Position16<BackgroundPixels>(17, 65535), floorRoom)
                : (TurningRoomLowerBand?)null;
            var layer = new TurningRoomLayer(
                new FloorView(0.27, scale, new(128, 112), new(-17, 211), FloorOverflowMode.Wrap),
                map, hud, band);
            Check(new DisplayPacket(0,
                new LayeredScene(gameplayMemory, hud == 0 ? new DisplayLayer[] { layer } : new DisplayLayer[] { layer, hudLayer }, 13)));
        }
        // A Ceres room's floor with its floor animation frames drawn over it (#1467), and the fixture's floor, with outside
        // pixels transparent, as Ceres rooms show them, or repeating.
        FloorMemory floorMemory = AffineFloorConstants.Split(vram);
        var ceres = new RoomFloor(AffineFloorConstants.CeresRoom(floorMemory.Map, floorMemory.Pixels, palette));
        ceres.Show(CeresFloorAnimationFrame.ElevatorDark);
        ceres.Show(CeresFloorAnimationFrame.WingsLowered);
        ceres.Show(CeresFloorAnimationFrame.BabyHorizontal);
        foreach (AffineFloor shown in new[] { map, ceres.Publish() })
        foreach (int hud in new[] { 0, 32 })
        foreach (double scale in new[] { 1, 0.5, 2 })
        foreach (bool fill in new[] { false, true })
        {
            var layer = new TurningRoomLayer(
                new FloorView(-0.27, scale, new(128, 112), new(-17, 211), fill ? FloorOverflowMode.Wrap : FloorOverflowMode.Transparent),
                shown, hud, null);
            Check(new DisplayPacket(0,
                new LayeredScene(gameplayMemory, hud == 0 ? new DisplayLayer[] { layer } : new DisplayLayer[] { layer, hudLayer }, 13)));
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact Mode 7 comparisons passed.");

        void Check(DisplayPacket frame)
        {
            count++;
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: Mode 7 sample {count}");
        }
    }
}
