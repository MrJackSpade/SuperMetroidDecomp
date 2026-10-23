using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpriteCasesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks imported map sprites and editable overrides in file-select and pause presentation.</summary>
internal static class MapSpritesVerification
{
    /// <summary>Compares native sprite bytes and clipping, then checks composition and PNG edits through rendered maps.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the native map OBJ characters")]
    internal static void VerifyMapSprites(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var native = new OamBuffer();
        var installed = new OamBuffer();
        var authored = ScreenSpritesFixtures.Blank();
        Suite(nameof(VerifyMapSpriteNativeCompositions), () => VerifyMapSpriteNativeCompositions(bus, original.Sprites));
        // Independent coordinate oracle: the native carry/sign test parks wrapped
        // Y at $180/$E0, rather than letting it reappear on the opposite screen edge.
        for (int origin = 0; origin < 256; origin++)
        for (int offset = 0; offset < 256; offset++)
        {
            installed.BeginFrame();
            installed.AddOnScreenSpritePart(SnesSpritemapXWord.Create(-1, true), new SnesSpritemapYByte((byte)offset),
                SnesObjAttributeWord.Create(3, 2, 1, SnesTileFlipFlags.Horizontal), new Position16<OamPixels>(0, (ushort)origin));
            int sum = origin + offset;
            int signedY = origin + unchecked((sbyte)offset);
            bool hidden = signedY is < -32 or >= 224;
            AssertEqual(hidden ? (byte)128 : (byte)255, installed.LowTable[0], "typed sprite X clipping");
            AssertEqual(hidden ? (byte)224 : (byte)sum, installed.LowTable[1], "typed sprite Y clipping");
            AssertEqual((byte)3, (byte)(installed.HighTable[0] & 3), "typed sprite preserves high X and large size");
        }
        var vram = new SnesVram(); original.Sprites.LoadArtworkTo(new VramStore(vram).All.Slice(MenuVramLayout.MapSpriteByteAddress / sizeof(ushort), MapSpriteFormat.ByteCount / sizeof(ushort)));
        AssertTrue(vram.Bytes.Slice(MenuVramLayout.MapSpriteByteAddress, MapSpriteFormat.ByteCount).SequenceEqual(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount)), "map OBJ PNG round-trips native characters exactly");
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original, original));

        Directory.CreateDirectory(overrides);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        byte[] json = File.ReadAllBytes(Path.Combine(stock, MapSpriteFormat.JsonFile));
        var document = RequiredJson.Deserialize<MapSpriteDocument>(json, options);
        string path = Path.Combine(overrides, MapSpriteFormat.JsonFile);
        var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames);
        frames["Marker.Boss"] = [new SpriteVisualPart { OffsetX = 12, OffsetY = -4, TileColumn = 2, TileRow = 3,
            Size = 16, Priority = 3, Palette = 5, FlipX = true, FlipY = true }];
        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(document with { Frames = frames }, options));
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity, "composition changes catalog identity");
        Suite(nameof(VerifyRenderedEdit), () => VerifyRenderedEdit(edited));
        for (int palette = 0; palette < 8; palette++)
        {
            authored.BeginFrame(); MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(edited.Sprites), MapSpriteId.MarkerBoss, authored, new(100, 100), (ushort)(palette << 9)); authored.FinishFrame();
            AssertEqual((byte)112, authored.LowTable[0], "authored horizontal offset reaches OAM");
            AssertEqual((byte)96, authored.LowTable[1], "authored vertical offset reaches OAM");
            var attributes = new SnesObjAttributeWord(SnesLittleEndian.Word(authored.LowTable[2], authored.LowTable[3]));
            AssertEqual(50, attributes.TileNumber, "authored tile region reaches OAM");
            AssertEqual(5, attributes.PaletteIndex, "explicit palette overrides caller blink palette");
            AssertEqual(3, attributes.Priority, "authored priority reaches OAM");
            AssertTrue(attributes.FlipHorizontally && attributes.FlipVertically && (authored.HighTable[0] & 2) != 0,
                "authored flips and large size reach OAM");
        }
        frames["Marker.Boss"] = [];
        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(document with { Frames = frames }, options));
        authored.BeginFrame(); MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(AreaMapPresentationCatalog.Load(stock, overrides).Sprites), MapSpriteId.MarkerBoss, authored, new(0, 0), 0); authored.FinishFrame();
        AssertEqual(0, authored.LastFinalizedSpriteCount, "authored empty visual frame is valid");
        frames.Remove("Marker.Boss");
        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(document with { Frames = frames }, options));
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "missing semantic sprite frame rejected");
        foreach (var invalid in new[] { document.Frames["Marker.Boss"][0] with { OffsetX = 256 },
            document.Frames["Marker.Boss"][0] with { Size = 16, TileColumn = 15 },
            document.Frames["Marker.Boss"][0] with { Palette = 8 } })
        {
            frames["Marker.Boss"] = [invalid];
            File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(document with { Frames = frames }, options));
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "invalid sprite visual fields rejected");
        }
        File.WriteAllBytes(path, json);
        string pngPath = Path.Combine(overrides, MapSpriteFormat.PngFile);
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(stock, MapSpriteFormat.PngFile)))
            image = IndexedPng.Read(input, new(MapSpriteFormat.Width, MapSpriteFormat.Height));
        for (int pixel = 0; pixel < image.Pixels.Length; pixel++) image.Pixels[pixel] = (byte)((image.Pixels[pixel] + 1) % 16);
        using (var output = File.Create(pngPath)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        var pngEdit = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(pngEdit.ContentIdentity != original.ContentIdentity, "PNG alone changes selected content identity");
        Suite(nameof(VerifyRenderedEdit), () => VerifyRenderedEdit(pngEdit));
        File.WriteAllText(pngPath, "invalid PNG");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt sprite PNG override rejected");
        File.WriteAllBytes(pngPath, File.ReadAllBytes(Path.Combine(stock, MapSpriteFormat.PngFile)));
        Console.WriteLine("Map sprites: 26 native compositions, clipping/capacity/palette parity, guarded file-select lifecycle and authored visual fields verified.");

        void VerifyRenderedEdit(AreaMapPresentationCatalog replacement)
        {
            var guard = NewMapSpriteGuard(bus);
            var system = new Bank80SystemState(); system.SetAreaMapAcquired(AreaId.WreckedShip);
            system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
            var marker = new FileSelectStationMarker(AreaId.WreckedShip, 0,
                original.SaveMarkers);
            var room = new FileSelectRoomMapGraphics(system, AreaId.WreckedShip, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            var nativeRoom = new FileSelectRoomMapGraphics(system,
                AreaId.WreckedShip, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            var before = nativeRoom.Render(new(0, 0), marker);
            AssertTrue(before.SequenceEqual(room.Render(new(0, 0), marker)), "stock map sprite pixels match with source reads blocked");
            room.BindFixtureArtwork(replacement);
            AssertTrue(!before.SequenceEqual(room.Render(new(0, 0), marker)), "independent sprite resource edit changes file-select pixels");
            using var capture = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(capture, room); capture.Position = 0;
            room = DebuggerObjectGraphSerializer.Deserialize<FileSelectRoomMapGraphics>(capture);
            room.BindFixtureArtwork(original);
            AssertTrue(before.SequenceEqual(room.Render(new(0, 0), marker)), "restored sprite resources bind current content");
            var pauseNative = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = AreaId.WreckedShip, RoomMapPosition = new(19, 20), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = AreaId.WreckedShip, RoomMapPosition = new(19, 20), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
            var pauseBefore = pauseNative.Render();
            AssertTrue(pauseBefore.SequenceEqual(pause.Render()), "pause map sprite pixels match with source reads blocked");
            pause.BindFixtureArtwork(replacement);
            AssertTrue(!pauseBefore.SequenceEqual(pause.Render()), "independent sprite resource edit changes pause pixels");
            pause.BindFixtureArtwork(original);
            AssertTrue(pauseBefore.SequenceEqual(pause.Render()), "pause sprite rebind restores stock pixels");
        }
    }

    /// <summary>
    /// Import-only spritemap oracle shared by map and pause-UI parity checks. Gameplay
    /// OAM cannot read immutable cartridge sprites after installation.
    /// </summary>
    internal static void DrawImportedSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        int address, Position16<OamPixels> origin, ushort paletteBits,
        bool originIsOnScreen = true) => ImportedSpritemapOracle.DrawGeneric(
            CartridgeImportSource.Require(bus), oam, address, origin,
            paletteBits, originIsOnScreen);

    /// <summary>Forbids runtime reads of installed map characters and every native map sprite pointer and body.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle reading the native map spritemap pointers to forbid every migrated sprite source")]
    private static MutableCartridgeReadGuard NewMapSpriteGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var forbidden = new HashSet<int>();
        Add(MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount);
        Add(FileSelectMapRomData.LabelSpritemapBase, 2);
        foreach (var frame in MapSpriteDefinitions.Frames)
        {
            int entry = FileSelectMapRomData.SpritemapPointerTableAddress + (int)frame.Id * 2;
            int pointer = FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(cartridge, entry);
            Add(entry, 2); Add(pointer, 2 + 5 * RomDataReader.ReadWordFixedBank(cartridge, pointer));
        }
        void Add(int start, int count) { for (int index = 0; index < count; index++) forbidden.Add(start + index); }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains, address => $"Installed map read migrated sprite data at {address:X6}.");
        return guard;
    }
}
