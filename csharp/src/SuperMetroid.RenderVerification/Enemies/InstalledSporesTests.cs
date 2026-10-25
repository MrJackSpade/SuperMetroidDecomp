using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Diagnostics.SaveStates;
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

    /// <summary>Loads stock and edited spores presentation, checks owner state and pixels, then renders each frame on both D3D11 device kinds.</summary>
    internal static void Run(string installationRoot, string romPath, string nativeArtworkDirectory)
    {
        string maps = Path.Combine(Path.GetFullPath(installationRoot), "game", "maps");
        AreaMapPresentationCatalog stock = MapPresentationFiles.Load(maps, null);
        CheckArtwork(stock.RoomFxAnimatedTiles, maps, romPath, nativeArtworkDirectory);
        var packets = new List<DisplayPacket>();
        using var overridesScratch = new TestScratchDirectory("installed-spores");
        string overrides = overridesScratch.Root;
        try
        {
            Directory.CreateDirectory(overrides);
            WriteOverrides(maps, overrides);
            AreaMapPresentationCatalog edited = MapPresentationFiles.Load(maps, overrides);
            Require(stock.ContentIdentity != edited.ContentIdentity, "Spore overrides did not affect selected content identity.");
            CheckOwners(stock, edited, packets);
            CheckPixels(packets);
            foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
            {
                using var device = new D3D11RenderDevice(kind);
                using var renderer = new D3D11FrameRenderer(device);
                foreach (DisplayPacket packet in packets)
                {
                    PixelComparison.Verify(packet, DisplayRendering.Render(packet),
                        renderer.RenderForReadback(packet), $"{kind}: spores case {packet.Identity}");
                }
                Console.WriteLine($"{kind}: {packets.Count} spore/source-priority frames match the WARP reference exactly.");
            }
            CheckFileFailures(maps, overrides);
            foreach (string file in Directory.GetFiles(overrides)) File.Delete(file);
            Require(MapPresentationFiles.Load(maps, overrides).ContentIdentity == stock.ContentIdentity,
                "Removing the overrides did not restore stock content identity.");
        }
        finally
        {
            string owner = Path.GetFullPath(Path.Combine("csharp", "test-temp")) + Path.DirectorySeparatorChar;
            if (!overrides.StartsWith(owner, StringComparison.OrdinalIgnoreCase) ||
                !Path.GetFileName(overrides).StartsWith("installed-spores-", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing to remove a non-fixture directory.");
            if (Directory.Exists(overrides)) Directory.Delete(overrides, true);
        }
        Console.WriteLine("Spore FX: installed-only timing, freeze, scroll, editing, legacy artwork and native layer exclusions verified. No gameplay/read discovery or player data used.");
    }

    /// <summary>Compares stock and edited spores FX over freeze, scroll wrap, and animated transfers while guarding gameplay state.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    private static void CheckOwners(AreaMapPresentationCatalog stock, AreaMapPresentationCatalog edited,
        List<DisplayPacket> packets)
    {
        var mechanics = RoomFxAnimatedTileMechanicsDefinitions.All.Single(
            value => value.AnimatedObject == AnimatedTileObject.Spores);
        Require(mechanics.InstructionPointer == 0x82ed && mechanics.FrameByteCount == 48 &&
            AnimatedTileVramDestinations.WordOf(mechanics.AnimatedObject) == 0x4280 && mechanics.GotoInstructionPointer == 0x82f9 &&
            mechanics.Frames.Count == 3 && mechanics.Frames.All(frame => frame.Duration == 10),
            "Compiled spores controls differ from pinned $87:82ED-$8302.");
        int records = 0, comparisons = 0, alteredTransfers = 0;
        foreach (var definition in CoreAccess.AllRoomFxRecords().Where(record => record.Type == RoomFxType.Spores))
        {
            records++;
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            byte[] initialRam = memory.WorkRam.ToArray();
            var baselineFrames = new ArtworkFrameSelections(); var editedFrames = new ArtworkFrameSelections();
            var colors = new GameplayColours(new SpriteColourSets()); var editedColors = new GameplayColours(new SpriteColourSets());
            RoomLayer3FxState baseline = RoomLayer3FxState.ForDefinition(Create(stock), colors, definition, 0);
            RoomLayer3FxState replacement = RoomLayer3FxState.ForDefinition(Create(edited), editedColors, definition, 0);
            Require(baseline.Type == RoomFxType.Spores && baseline.CaptureForDisplay() is not null,
                "The cartridge's spores FX has no display capture.");
            Require(baseline.LayerBlendConfiguration == LayerBlendingConfiguration.Spores,
                "A compiled spores record selects incompatible blending.");
            // Since #1348 the room's BG3 plane shows the effect's page directly instead of a copy written into VRAM.
            Require(baseline.EffectPageType == RoomFxType.Spores && baseline.EffectPageWords()
                    .SelectMany(word => new[] { (byte)word, (byte)(word >> 8) })
                    .SequenceEqual(stock.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Spores).ToArray()),
                "The spores room does not show the installed spores BG3 page.");
            int tick = 0;
            for (int frame = 0; frame < 1050; frame++)
            {
                bool frozen = frame is >= 37 and < 45;
                var camera = new Position16<RoomPixels>(unchecked((ushort)(frame * 79)), unchecked((ushort)(65520 + frame * 3)));
                RoomLayer3FxRenderSnapshot before = (baseline.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires baseline.CaptureForDisplay()."));
                baseline.Step(baselineFrames, new RoomLayer3FxFrame { Camera = camera, TimeIsFrozen = frozen, MainGameLoopCarry = false });
                replacement.Step(editedFrames, new RoomLayer3FxFrame { Camera = camera, TimeIsFrozen = frozen, MainGameLoopCarry = false });
                var capture = (baseline.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires baseline.CaptureForDisplay()."));
                if (frozen) Require(capture == before, "Frozen spores changed scroll or phase.");
                else
                {
                    ushort accumulator = unchecked((ushort)(-tick * 64));
                    Require(capture.Scroll == new Position16<Bg3Pixels>(camera.X,
                        unchecked((ushort)(camera.Y + (sbyte)(SnesLittleEndian.HighByte(accumulator))))), "Spores lost native signed 8.8 camera anchoring.");
                    int source = AnimatedTileImageSources.OfCursor(mechanics,
                        mechanics.Frames[tick / 10 % 3].InstructionPointer);
                    Require(stock.RoomFxAnimatedTiles.TryResolve(source, 48, out var expected), "Missing stock spores frame.");
                    Require(edited.RoomFxAnimatedTiles.TryResolve(source, 48, out var altered), "Missing edited spores frame.");
                    // Animated frames are selected, and the room's BG3 pictures show the selected frame (#1406).
                    ArtworkFrame cadence = AnimatedTileFrames.Frame(AnimatedTileObject.Spores, tick / 10 % 3);
                    Require(baselineFrames.TryShown(AnimatedTileFrames.Of(AnimatedTileObject.Spores), out ArtworkFrame stockShown) && stockShown == cadence,
                        "Stock spore cadence differs.");
                    Require(editedFrames.TryShown(AnimatedTileFrames.Of(AnimatedTileObject.Spores), out ArtworkFrame editedShown) && editedShown == cadence,
                        "Edited spore cadence differs.");
                    if (!expected.Span.SequenceEqual(altered.Span)) alteredTransfers++;
                    tick++;
                }
                Require(capture == replacement.CaptureForDisplay(), "Presentation overrides changed scroll/capture mechanics.");
                if (frame is < 50 or 510 or 511 or 512 or 1024 or 1049)
                {
                    Require(Graph(baseline).SequenceEqual(Graph(replacement)), "Presentation edits changed serialized FX control state.");
                    var liquid = new SamusLiquidPhysicsState(); liquid.ConfigureFor(replacement);
                    Require(liquid.FxType == RoomFxType.Spores && liquid.FxYPosition == ushort.MaxValue &&
                        liquid.LavaAcidYPosition == ushort.MaxValue, "Spore artwork activated liquid physics.");
                }
                Require(baseline.SoundRequests.Count == 0 && replacement.SoundRequests.Count == 0 &&
                    baseline.EarthquakeRequest is null && replacement.EarthquakeRequest is null,
                    "Spore presentation produced audio or earthquake side effects.");
                if (frame is 0 or 10 or 20 or 30 or 44 or 511 or 1024)
                {
                    var gameplay = new OrdinaryGameplayRenderLayer(new(default, default, new Extent16<TilemapCells>(64, 32),
                        ScreenLayers.Bg2), RoomBackgroundsFixtures.Blank() with
                        {
                            Bg3 = Layer3CellsFixtures.FromWords(Layer3Plane.Compose(new ushort[Layer3Plane.HudRows * Layer3Plane.Columns],
                                replacement.EffectPageWords()).Words, new byte[SnesPpuLayout.VramByteCount], new RgbColor[SnesPpuLayout.CgramColorCount]),
                        });
                    var layer = GameplayEffectCapture.CaptureSpores(gameplay, (replacement.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires replacement.CaptureForDisplay().")));
                    Require(!layer.RevealBlocks && layer.Blend.Layers == (ColorBlend.Bg2 | ColorBlend.Sprites | ColorBlend.Backdrop) &&
                        layer.Lines.All(line => line.Left > line.Right) && layer.Subscreen is not null,
                        "Spore capture does not blend the background, the sprites and the backdrop with the spores.");
                    packets.Add(new(new(packets.Count + 1, 1, 0), new LayeredScene(
                        new(editedColors.Colors, []), [layer], 15)));
                }
                comparisons++;
            }
            Require(memory.WorkRam.SequenceEqual(initialRam),
                "Spore presentation mutated active game RAM.");
        }
        Require(records > 0 && alteredTransfers > 0, "Source-selected spore records or edits were not exercised.");
        Console.WriteLine($"{records} compiled spores records: {comparisons} stock/edited control comparisons; {alteredTransfers} changed frame transfers, signed wrap and freeze verified.");
    }

    /// <summary>Constructs a Layer 3 FX owner using the selected installed tile, map, and palette catalogs.</summary>
    private static RoomFxSession Create(AreaMapPresentationCatalog presentation) => new()
    {
        AnimatedTileArtwork = presentation.RoomFxAnimatedTiles,
        Layer3Tilemaps = presentation.RoomFxLayer3Tilemaps,
        PaletteBlendColors = presentation.RoomFxPaletteBlends,
    };

    /// <summary>Serializes FX control state through the production debugger graph codec for stock/edit comparison.</summary>
    private static byte[] Graph(object value)
    {
        using var bytes = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(bytes, value);
        return bytes.ToArray();
    }

    /// <summary>Reports a spores verification failure with its specific source or rendering context.</summary>
    private static void Require(bool condition, string context)
    {
        if (!condition) throw new InvalidOperationException(context);
    }

    /// <summary>Compares installed animated-tile atlas segments with the retail ROM and checks legacy atlas edits retain their positions.</summary>
    [SuperMetroid.Core.Hardware.CpuBusBoundary("exact bus test: retail ROM oracle for every installed animated-tile atlas segment")]
    private static void CheckArtwork(RoomFxAnimatedTileAtlas stock, string maps, string romPath,
        string nativeArtworkDirectory)
    {
        using var currentPng = File.OpenRead(Path.Combine(maps, RoomFxAnimatedTileAtlasFormat.FileName));
        IndexedPngImage image = IndexedPng.Read(currentPng, new(RoomFxAnimatedTileAtlasFormat.Width, 8));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, new(image.Size.X, image.Size.Y), 2);
        // Appending spores must not shift any earlier strip: every segment of the installed sheet,
        // frames and shared treadmill/statue strips alike, equals its native cartridge bytes.
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        int segmentOffset = 0;
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
        {
            int segmentSource = RoomFxAnimatedTileSources.Of(segment).Value;
            for (int index = 0; index < segment.ByteCount; index++)
                Require(planar[segmentOffset + index] == SuperMetroid.AssetExtraction.Cartridge.SnesCartridgeImportExtensions.ReadCartridgeByte(rom, segmentSource + index),
                    $"Installed sheet segment ${segmentSource:X6} differs from its native bytes at +{index}.");
            segmentOffset += segment.ByteCount;
        }
        Require(segmentOffset == planar.Length, "Installed sheet length differs from its native segment list.");
        int sporesOffset = RoomFxAnimatedTileAtlasFormat.PreSporesWidth * 2;
        var controls = RoomFxAnimatedTileMechanicsDefinitions.All.Single(
            value => value.AnimatedObject == AnimatedTileObject.Spores);
        for (int frame = 0; frame < 3; frame++)
        {
            int source = AnimatedTileImageSources.OfCursor(controls, controls.Frames[frame].InstructionPointer);
            Require(stock.TryResolve(source, 48, out var bytes), "Spore frame identity is absent.");
            byte[] pinnedArt = File.ReadAllBytes(Path.Combine(Path.GetFullPath(nativeArtworkDirectory),
                $"AnimatedTiles_Spores_{frame}.bin"));
            Require(bytes.Span.SequenceEqual(pinnedArt), "Extracted spores differ from pinned native artwork.");
            Require(bytes.Span.SequenceEqual(planar.AsSpan(sporesOffset + frame * 48, 48)),
                "Spore transfer does not resolve the appended PNG segment.");
        }
        foreach (int width in new[] { RoomFxAnimatedTileAtlasFormat.LegacyWidth,
            RoomFxAnimatedTileAtlasFormat.PreStatueWidth, RoomFxAnimatedTileAtlasFormat.PreSporesWidth })
        {
            var pixels = new byte[width * 8];
            for (int y = 0; y < 8; y++)
                image.Pixels.AsSpan(y * image.Size.X, width).CopyTo(pixels.AsSpan(y * width));
            pixels[0] = (byte)((pixels[0] + 1) % 4);
            using var legacyPng = new MemoryStream();
            IndexedPng.Write(legacyPng, new(checked((ushort)width), 8), pixels, image.Colours);
            legacyPng.Position = 0;
            RoomFxAnimatedTileAtlas legacy = RoomFxAnimatedTileAtlasPng.Read(legacyPng, stock);
            byte[] expected = (byte[])planar.Clone();
            SnesPlanarTileEncoder.Encode(pixels, new(checked((ushort)width), 8), 2).CopyTo(expected, 0);
            int offset = 0;
            foreach (var segment in RoomFxAnimatedTileAtlasFormat.Segments)
            {
                if (segment is RoomFxAtlasSegment.Image held)
                {
                    Require(legacy.TryResolve(held.Picture, segment.ByteCount, out var actual), "Legacy frame vanished.");
                    Require(actual.Span.SequenceEqual(expected.AsSpan(offset, segment.ByteCount)),
                        "Legacy sheet edit/tail inheritance changed a transfer.");
                }
                offset += segment.ByteCount;
            }
            // Statue frames overlap the middle strip and must retain their original
            // aliases even though spores now occupy the final nine characters.
            foreach (var statue in TourianStatueAnimatedTileMechanicsDefinitions.All)
            foreach (var picture in TourianStatueTileStripDefinitions.Pictures)
            {
                int source = AnimatedTileImageSources.TourianStatue(statue.AnimatedObject, picture);
                int statueOffset = RoomFxAnimatedTileAtlasFormat.PreStatueTileCount * 16 +
                    source - AnimatedTileImageSources.TourianStatueStripStart;
                Require(legacy.TryResolve(source, statue.FrameByteCount, out var actual) &&
                    actual.Span.SequenceEqual(expected.AsSpan(statueOffset, statue.FrameByteCount)),
                    "Appending spores shifted an existing statue alias.");
            }
        }
        Console.WriteLine("Pinned three-frame spores art and unchanged historical atlas prefix pass; all three older PNG geometries retain edits and stock tails.");
    }

    /// <summary>Writes distinct spores tile, map, and blend-color overrides to exercise the installed presentation boundary.</summary>
    private static void WriteOverrides(string maps, string overrides)
    {
        using var input = File.OpenRead(Path.Combine(maps, RoomFxAnimatedTileAtlasFormat.FileName));
        IndexedPngImage image = IndexedPng.Read(input, new(RoomFxAnimatedTileAtlasFormat.Width, 8));
        for (int y = 0; y < 8; y++)
        for (int x = RoomFxAnimatedTileAtlasFormat.PreSporesWidth; x < image.Size.X; x++)
            image.Pixels[y * image.Size.X + x] = (byte)((x + y) % 3 + 1);
        using (var output = File.Create(Path.Combine(overrides, RoomFxAnimatedTileAtlasFormat.FileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);

        JsonNode tilemaps = RequiredJson.Parse(File.ReadAllBytes(Path.Combine(maps, RoomFxLayer3TilemapFormat.FileName)));
        foreach (JsonNode? cell in tilemaps.Required("pages").Required("Spores").AsArray().RequiredElements())
        {
            cell["tileColumn"] = 16; cell["tileRow"] = 2;
            cell["palette"] = 6; cell["flipX"] = true;
        }
        File.WriteAllText(Path.Combine(overrides, RoomFxLayer3TilemapFormat.FileName), tilemaps.ToJsonString());
        JsonNode palettes = RequiredJson.Parse(File.ReadAllBytes(Path.Combine(maps, RoomFxPaletteBlendDefinitions.FileName)));
        foreach (JsonNode? blend in palettes.Required("blends").AsObject().Select(pair => pair.Value))
        foreach (JsonNode? color in (blend ?? throw new InvalidOperationException("Verification requires blend.")).AsArray().RequiredElements())
        {
            color["red"] = 3; color["green"] = 6; color["blue"] = 9;
        }
        File.WriteAllText(Path.Combine(overrides, RoomFxPaletteBlendDefinitions.FileName), palettes.ToJsonString());
    }

    /// <summary>Checks malformed and missing stock or override assets report the selected path, even with a matching manifest hash.</summary>
    private static void CheckFileFailures(string maps, string overrides)
    {
        string manifestPath = Path.Combine(maps, AreaMapCatalogFormat.ManifestFile);
        byte[] manifestBytes = File.ReadAllBytes(manifestPath);
        int failures = 0;
        foreach (string file in new[] { RoomFxAnimatedTileAtlasFormat.FileName,
            RoomFxLayer3TilemapFormat.FileName, RoomFxPaletteBlendDefinitions.FileName })
        {
            string path = Path.Combine(overrides, file);
            byte[] original = File.ReadAllBytes(path);
            File.WriteAllBytes(path, [0, 1, 2]);
            try
            {
                Reject(path);
            }
            finally { File.WriteAllBytes(path, original); }
            string baselinePath = Path.Combine(maps, file);
            byte[] stockBytes = File.ReadAllBytes(baselinePath);
            try
            {
                File.Delete(baselinePath); Reject(baselinePath);
                File.WriteAllBytes(baselinePath, [0, 1, 2]); Reject(baselinePath);
                JsonNode manifest = RequiredJson.Parse(manifestBytes);
                manifest.Required("sha256")[file] = Convert.ToHexString(SHA256.HashData(new byte[] { 0, 1, 2 }));
                File.WriteAllText(manifestPath, manifest.ToJsonString());
                Reject(baselinePath); // A valid edit must not conceal hash-valid malformed stock.
            }
            finally
            {
                File.WriteAllBytes(baselinePath, stockBytes);
                File.WriteAllBytes(manifestPath, manifestBytes);
            }
        }
        Console.WriteLine($"{failures} missing/corrupt/hash-valid-malformed/override failures report the exact selected filename and retain bad data.");

        void Reject(string expectedPath)
        {
            bool rejected = false;
            try { _ = MapPresentationFiles.Load(maps, overrides); }
            catch (Exception error) when (error is IOException or InvalidDataException)
            {
                rejected = true;
                Require(error.Message.Contains(Path.GetFullPath(expectedPath), StringComparison.OrdinalIgnoreCase),
                    $"Bad presentation diagnostic omits selected path {expectedPath}: {error}");
            }
            Require(rejected, "Malformed/missing spore presentation was silently ignored.");
            failures++;
        }
    }

    /// <summary>Compares source-aware spores blending against an independent Mode 1 priority and BGR555 pixel oracle.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    private static void CheckPixels(List<DisplayPacket> packets)
    {
        int cases = 0;
        foreach (int palette in Enumerable.Range(0, 8))
        foreach (int objPriority in Enumerable.Range(0, 4))
        foreach (bool bg1High in new[] { false, true })
        foreach (bool bg2High in new[] { false, true })
        foreach (int enabled in Enumerable.Range(0, 4))
        {
            var vram = new SnesVram(); var colors = new GameplayColours(new SpriteColourSets());
            colors.AsPaletteColors().SetColor(0, new RgbColor(2, 2, 2));
            colors.AsPaletteColors().SetColor(1, new RgbColor(10, 0, 0)); colors.AsPaletteColors().SetColor(2, new RgbColor(0, 12, 0)); colors.AsPaletteColors().SetColor(3, new RgbColor(0, 0, 7));
            colors.AsPaletteColors().SetColor(128 + palette * 16 + 1, new RgbColor(18, 2, 0));
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
            var registers = new OrdinaryGameplayView(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32),
                ScreenLayers.Sprites | ((enabled & 1) != 0 ? ScreenLayers.Bg1 : 0) |
                    ((enabled & 2) != 0 ? ScreenLayers.Bg2 : 0));
            var layer = GameplayEffectCapture.CaptureSpores(new(registers, RoomBackgroundsFixtures.Uniform(vram.Bytes, colors.Colors, new((ushort)(1 | (bg1High ? 0x2000 : 0)))) with
                {
                    Bg3 = Layer3CellsFixtures.FromVram(vram.Bytes, colors.Colors, 0x4000),
                }),
                new(RoomFxType.Spores, LayerBlendingConfiguration.Spores, Position16<Bg3Pixels>.Zero, new Position16<RoomPixels>(0, ushort.MaxValue)));
            var scene = new LayeredScene(FramePaint.FromOam(vram.Bytes, colors.Colors, oam, 1, 0), [layer], 15);
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
            var packet = new DisplayPacket(new(packets.Count + 1, 1, 0), scene);
            packets.Add(packet); cases++;
        }
        Console.WriteLine($"{cases} independent spore pixel cases cover BG1 exclusion, BG2/backdrop addition, all OBJ palettes/priorities and HUD preservation.");
    }
}
