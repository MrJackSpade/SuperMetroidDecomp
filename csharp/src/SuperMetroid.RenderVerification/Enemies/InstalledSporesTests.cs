using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Enemies;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Source-selected spores FX acceptance, not a gameplay or read-discovery probe.</summary>
internal static class InstalledSporesTests
{
    /// <summary>Expected layer depth of an OBJ pixel for sprite priorities 0-3.</summary>
    private static readonly int[] ObjPriorityLayers =
        [0, 1, 4, 7];

    /// <summary>Loads the installed spores presentation, checks owner state and pixels, then renders each frame on both D3D11 device kinds.</summary>
    internal static void Run(string installationRoot, string romPath, string nativeArtworkDirectory)
    {
        string installedMaps = Path.Combine(Path.GetFullPath(installationRoot), "game", "maps");
        // The animated tiles' page is import data no installation holds (#1636): the cartridge-derived extraction's is read.
        string maps = SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(
            SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.MapDirectoryName);
        ImportAreaMapPresentation stock = MapPresentationFiles.LoadForImport(installedMaps, maps);
        CheckArtwork(stock.RoomFxAnimatedTiles, maps, romPath, nativeArtworkDirectory);
        var packets = new List<DisplayPacket>();
        CheckOwners(stock, packets);
        CheckPixels(packets);
        foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
        {
            using var device = new D3D11RenderDevice(kind);
            using var renderer = new D3D11FrameRenderer(device);
            foreach (DisplayPacket packet in packets)
            {
                PixelComparison.Verify(packet, DisplayRendering.Render(packet),
                    renderer.RenderForReadback(packet), $"{kind}: spores case {packet.FrameNumber}");
            }
            Console.WriteLine($"{kind}: {packets.Count} spore/source-priority frames match the WARP reference exactly.");
        }
    }

    /// <summary>Checks the installed spores FX over freeze, scroll wrap, and animated transfers while guarding gameplay state.</summary>
    private static void CheckOwners(ImportAreaMapPresentation stock, List<DisplayPacket> packets)
    {
        var mechanics = RoomFxAnimatedTileLoops.All.Single(
            value => value.AnimatedObject == AnimatedTileObject.Spores);
        Require(mechanics.InstructionPointer == 0x82ed && mechanics.PictureByteCount == 48 &&
            AnimatedTileVramDestinations.WordOf(mechanics.AnimatedObject) == 0x4280 && mechanics.GotoInstructionPointer == 0x82f9 &&
            mechanics.Frames.Count == 3 && mechanics.Frames.All(frame => frame.Duration == 10),
            "Compiled spores controls differ from pinned $87:82ED-$8302.");
        int records = 0, comparisons = 0;
        foreach (var definition in CoreAccess.AllRoomFxRecords().Where(record => record.Type == RoomFxType.Spores))
        {
            records++;
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            byte[] initialRam = memory.WorkRam.ToArray();
            var baselineFrames = new ArtworkFrameSelections();
            RoomFxOverlayState baseline = RoomFxOverlayState.ForDefinition(new RoomFxSession(), definition, 0);
            Require(baseline.Type == RoomFxType.Spores && baseline.CaptureForDisplay() is not null,
                "The cartridge's spores FX has no display capture.");
            Require(baseline.Blend == RoomEffectBlendDefinitions.SporesBrightenBehind,
                "A compiled spores record describes another blend.");
            // Since #1348 the room's BG3 plane shows the effect's page directly instead of a copy written into VRAM.
            // The plane names the effect's page (#1466); the installed sheet pictures its words.
            Require(baseline.EffectPageType == RoomFxType.Spores && baseline.EffectPlane.EffectPage == RoomFxType.Spores,
                "The spores room does not show the spores BG3 page.");
            int tick = 0;
            for (int frame = 0; frame < 1050; frame++)
            {
                bool frozen = frame is >= 37 and < 45;
                var camera = new Position16<RoomPixels>(unchecked((ushort)(frame * 79)), unchecked((ushort)(65520 + frame * 3)));
                RoomFxOverlayRenderSnapshot before = (baseline.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires baseline.CaptureForDisplay()."));
                baseline.Step(baselineFrames, new RoomFxOverlayFrame { Camera = camera, TimeIsFrozen = frozen, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.None });
                var capture = (baseline.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires baseline.CaptureForDisplay()."));
                if (frozen) Require(capture == before, "Frozen spores changed scroll or phase.");
                else
                {
                    ushort accumulator = unchecked((ushort)(-tick * 64));
                    Require(capture.Scroll == new Position16<OverlayPixels>(camera.X,
                        unchecked((ushort)(camera.Y + (sbyte)(LittleEndianBytes.HighByte(accumulator))))), "Spores lost native signed 8.8 camera anchoring.");
                    int source = AnimatedTileImageSources.OfCursor(mechanics,
                        mechanics.FrameInstructionPointer(tick / 10 % 3));
                    Require(stock.RoomFxAnimatedTiles.TryResolve(source, 48, out var expected), "Missing stock spores frame.");
                    // Animated frames are selected, and the room's BG3 pictures show the selected frame (#1406).
                    ArtworkFrame cadence = AnimatedTileFrames.Frame(AnimatedTileObject.Spores, tick / 10 % 3);
                    Require(baselineFrames.TryShown(AnimatedTileFrames.Of(AnimatedTileObject.Spores), out ArtworkFrame stockShown) && stockShown == cadence,
                        "Stock spore cadence differs.");
                    tick++;
                }
                if (frame is < 50 or 510 or 511 or 512 or 1024 or 1049)
                {
                    var liquid = new SamusLiquidPhysicsState(new BreathBubbleAndLavaSprayWaits(), new SuperMetroid.Core.Samus.Drawing.SamusParticles());
                    liquid.ConfigureFor(baseline);
                    Require(liquid.FxType == RoomFxType.Spores && liquid.WaterSurfaceY is null &&
                        liquid.LavaAcidSurfaceY is null, "Spore artwork activated liquid physics.");
                }
                Require(baseline.SoundRequests.Count == 0 && baseline.SustainedEarthquake is null,
                    "Spore presentation produced audio or earthquake side effects.");
                if (frame is 0 or 10 or 20 or 30 or 44 or 511 or 1024)
                {
                    var gameplay = new GameplaySceneLayer(new(default, default,
                        ScreenLayers.Background), RoomBackgroundsFixtures.Blank() with
                        {
                            FxOverlay = FxOverlayFixtures.PageOf(baseline.EffectPlane, stock.RoomFxPages,
                                new byte[SnesVram.ByteCount], new RgbColor[SnesCgram.ColorCount]),
                        });
                    var layer = GameplayEffectCapture.CaptureEffectBlendingBehind(gameplay, (baseline.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires baseline.CaptureForDisplay().")));
                    Require(!layer.RevealBlocks && layer.Blend.Blended == (BlendedLayers.Background | BlendedLayers.Actors | BlendedLayers.Backdrop) &&
                        layer.Blend.Operation == BlendOperation.Add && !layer.Blend.Halved &&
                        layer.Window.IsClosed && layer.OverlayBehind is not null && !layer.BackgroundBehind,
                        "Spore capture does not brighten the background, the actors and the backdrop with the spores lying behind them.");
                    packets.Add(new(packets.Count + 1, new LayeredScene(
                        FramePaint.FromNativeGameplay(new RgbColor[SnesCgram.ColorCount], []), [layer], 15)));
                }
                comparisons++;
            }
            Require(memory.WorkRam.SequenceEqual(initialRam),
                "Spore presentation mutated active game RAM.");
        }
        Require(records > 0, "Source-selected spore records were not exercised.");
        Console.WriteLine($"{records} compiled spores records: {comparisons} control steps; signed wrap and freeze verified.");
    }

    /// <summary>Reports a spores verification failure with its specific source or rendering context.</summary>
    private static void Require(bool condition, string context)
    {
        if (!condition) throw new InvalidOperationException(context);
    }

    /// <summary>Compares installed animated-tile atlas segments with the retail ROM.</summary>
    private static void CheckArtwork(RoomFxAnimatedTileAtlas stock, string maps, string romPath,
        string nativeArtworkDirectory)
    {
        using var currentPng = File.OpenRead(Path.Combine(maps, RoomFxAnimatedTileAtlasFile.FileName));
        IndexedPngImage image = IndexedPng.Read(currentPng, new(RoomFxAnimatedTileAtlasFile.Width, 8));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, new(image.Size.X, image.Size.Y), 2);
        // Appending spores must not shift any earlier strip: every segment of the installed sheet,
        // frames and shared treadmill/statue strips alike, equals its native cartridge bytes.
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        int segmentOffset = 0;
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFile.Segments)
        {
            int segmentSource = RoomFxAnimatedTileSources.Of(segment).Value;
            for (int index = 0; index < segment.ByteCount; index++)
                Require(planar[segmentOffset + index] == SuperMetroid.AssetExtraction.Cartridge.SnesCartridgeImportExtensions.ReadCartridgeByte(rom, segmentSource + index),
                    $"Installed sheet segment ${segmentSource:X6} differs from its native bytes at +{index}.");
            segmentOffset += segment.ByteCount;
        }
        Require(segmentOffset == planar.Length, "Installed sheet length differs from its native segment list.");
        int sporesOffset = RoomFxAnimatedTileAtlasFormat.PreSporesTileCount * 16;
        var controls = RoomFxAnimatedTileLoops.All.Single(
            value => value.AnimatedObject == AnimatedTileObject.Spores);
        for (int frame = 0; frame < 3; frame++)
        {
            int source = AnimatedTileImageSources.OfCursor(controls, controls.FrameInstructionPointer(frame));
            Require(stock.TryResolve(source, 48, out var bytes), "Spore frame identity is absent.");
            byte[] pinnedArt = File.ReadAllBytes(Path.Combine(Path.GetFullPath(nativeArtworkDirectory),
                $"AnimatedTiles_Spores_{frame}.bin"));
            Require(bytes.Span.SequenceEqual(pinnedArt), "Extracted spores differ from pinned native artwork.");
            Require(bytes.Span.SequenceEqual(planar.AsSpan(sporesOffset + frame * 48, 48)),
                "Spore transfer does not resolve the appended PNG segment.");
        }
        Console.WriteLine("Pinned three-frame spores art and unchanged historical atlas prefix pass.");
    }

    /// <summary>Compares source-aware spores blending against an independent Mode 1 priority and BGR555 pixel oracle.</summary>
    private static void CheckPixels(List<DisplayPacket> packets)
    {
        int cases = 0;
        foreach (int palette in Enumerable.Range(0, 8))
        foreach (int objPriority in Enumerable.Range(0, 4))
        foreach (bool bg1High in new[] { false, true })
        foreach (bool bg2High in new[] { false, true })
        foreach (int enabled in Enumerable.Range(0, 4))
        {
            var vram = new SnesVram(); var colors = new RgbColor[SnesCgram.ColorCount];
            colors[0] = new RgbColor(16, 16, 16);
            colors[1] = new RgbColor(82, 0, 0); colors[2] = new RgbColor(0, 99, 0); colors[3] = new RgbColor(0, 0, 57);
            colors[128 + palette * 16 + 1] = new RgbColor(148, 16, 0);
            for (int y = 0; y < 8; y++)
            {
                vram.LoadBytes(y * 2, new byte[] { 255, 0 });
                vram.LoadBytes(32 + y * 2, new byte[] { 255, 0 });
                vram.LoadBytes(64 + y * 2, new byte[] { 0, 255 });
                vram.LoadBytes(GameplayVramLayout.HudCharacterWord * 2 + 48 + y * 2, new byte[] { 255, 255 });
            }
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)(2 | (bg2High ? 0x2000 : 0)), 2048).ToArray(),
                GameplayVramLayout.Bg2TilemapWord, 1);
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)3, 1024).ToArray(), 0x5c00, 1);
            var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
            oam[0] = 16; oam[1] = 64; oam[3] = (byte)(objPriority << 4 | palette << 1);
            var registers = new GameplaySceneView(new Position16<RoomPixels>(0, 0), new Position16<BackgroundPixels>(0, 0),
                ScreenLayers.Sprites | ((enabled & 1) != 0 ? ScreenLayers.Foreground : 0) |
                    ((enabled & 2) != 0 ? ScreenLayers.Background : 0));
            var layer = GameplayEffectCapture.CaptureEffectBlendingBehind(new(registers, RoomBackgroundsFixtures.Uniform(vram.Bytes, colors, new((ushort)(1 | (bg1High ? 0x2000 : 0)))) with
                {
                    FxOverlay = FxOverlayFixtures.PageFromVram(vram.Bytes, colors, 0x4000),
                }),
                new(RoomFxType.Spores, RoomEffectBlendDefinitions.SporesBrightenBehind, Position16<OverlayPixels>.Zero, null));
            var scene = new LayeredScene(FramePaint.FromOamGameplay(vram.Bytes, colors, oam, 1, 0), [layer], 15);
            RgbaPixel[] pixels = DisplayRendering.Render(scene);
            // Independent Mode-1 priority and integer BGR555 oracle. A color-only
            // post-process cannot distinguish excluded BG1/low OBJ from BG2 here.
            foreach (bool objectPixel in new[] { false, true })
            {
                int rank = -1; int red = 2, green = 2, blue = 2; bool blend = true;
                void Insert(int candidate, int r, int g, bool eligible)
                {
                    if (candidate < rank) return;
                    rank = candidate; red = r; green = g; blue = 0; blend = eligible;
                }
                if ((enabled & 2) != 0) Insert(bg2High ? 5 : 2, 0, 12, true);
                if ((enabled & 1) != 0) Insert(bg1High ? 6 : 3, 10, 0, false);
                if (objectPixel) Insert(ObjPriorityLayers[objPriority], 18, 2, palette >= 4);
                byte Expand(int value) => (byte)((value << 3) | (value >> 2));
                var expected = new RgbaPixel(Expand(red), Expand(green), Expand(blue + (blend ? 7 : 0)));
                int x = objectPixel ? 16 : 64;
                Require(pixels[64 * 256 + x] == expected,
                    $"Spore source exclusion differs: x{x}, expected {expected}, actual {pixels[64 * 256 + x]}; palette {palette}, OBJ {objPriority}, BG priorities {bg1High}/{bg2High}, planes {enabled}.");
            }
            Require(pixels[0] == new RgbaPixel(16, 16, 16), "Spores blended into the HUD.");
            var packet = new DisplayPacket(packets.Count + 1, scene);
            packets.Add(packet); cases++;
        }
        Console.WriteLine($"{cases} independent spore pixel cases cover BG1 exclusion, BG2/backdrop addition, all OBJ palettes/priorities and HUD preservation.");
    }
}
