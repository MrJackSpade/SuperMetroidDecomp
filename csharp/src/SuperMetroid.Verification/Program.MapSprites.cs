using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// Verifies map-sprite clipping, stock cartridge artwork, installed file-select reads,
    /// and changes to authored sprite JSON and PNG resources.
    /// </summary>
    /// <param name="bus">Cartridge address space used by the native comparison paths.</param>
    /// <param name="stock">Directory containing the stock map-sprite resources.</param>
    /// <param name="overrides">Directory where modified resource documents and images are written.</param>
    /// <param name="original">Stock presentation catalog used as the baseline for resource edits.</param>
    private static void VerifyMapSprites(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var native = new OamBuffer();
        var installed = new OamBuffer();
        Suite(nameof(VerifyMapSpriteNativeCompositions), () => VerifyMapSpriteNativeCompositions(bus, original.Sprites));
        // Independent coordinate oracle: the native carry/sign test parks wrapped
        // Y at $180/$E0, rather than letting it reappear on the opposite screen edge.
        for (int origin = 0; origin < 256; origin++)
        for (int offset = 0; offset < 256; offset++)
        {
            installed.BeginFrame();
            installed.AddOnScreenSpritePart(SnesSpritemapXWord.Create(-1, true), (byte)offset,
                SnesObjAttributeWord.Create(3, 2, 1, SnesTileFlipFlags.Horizontal), 0, (ushort)origin);
            int sum = origin + offset;
            int signedY = origin + unchecked((sbyte)offset);
            bool hidden = signedY < -32 || signedY >= 224;
            AssertEqual(hidden ? (byte)128 : (byte)255, installed.LowTable[0], "typed sprite X clipping");
            AssertEqual(hidden ? (byte)224 : (byte)sum, installed.LowTable[1], "typed sprite Y clipping");
            AssertEqual((byte)3, (byte)(installed.HighTable[0] & 3), "typed sprite preserves high X and large size");
        }
        var vram = new SnesVram(); original.Sprites.LoadArtworkTo(vram, MapSpriteFormat.FileSelectDestination);
        AssertTrue(vram.Bytes.Slice(MapSpriteFormat.FileSelectDestination, MapSpriteFormat.ByteCount).SequenceEqual(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount)), "map OBJ PNG round-trips native characters exactly");
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(bus, new MapSpriteReadGuard(bus), original, original));

        Directory.CreateDirectory(overrides);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        byte[] json = File.ReadAllBytes(Path.Combine(stock, MapSpriteFormat.JsonFile));
        var document = JsonSerializer.Deserialize<MapSpriteDocument>(json, options)!;
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
            installed.BeginFrame(); edited.Sprites.Draw(9, installed, 100, 100, (ushort)(palette << 9));
            AssertEqual((byte)112, installed.LowTable[0], "authored horizontal offset reaches OAM");
            AssertEqual((byte)96, installed.LowTable[1], "authored vertical offset reaches OAM");
            var attributes = new SnesObjAttributeWord((ushort)(installed.LowTable[2] | installed.LowTable[3] << 8));
            AssertEqual(50, (int)attributes.TileNumber, "authored tile region reaches OAM");
            AssertEqual(5, (int)attributes.PaletteIndex, "explicit palette overrides caller blink palette");
            AssertEqual(3, (int)attributes.Priority, "authored priority reaches OAM");
            AssertTrue(attributes.FlipHorizontally && attributes.FlipVertically && (installed.HighTable[0] & 2) != 0,
                "authored flips and large size reach OAM");
        }
        frames["Marker.Boss"] = [];
        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(document with { Frames = frames }, options));
        installed.BeginFrame(); AreaMapPresentationCatalog.Load(stock, overrides).Sprites.Draw(9, installed, 0, 0, 0);
        AssertEqual(0, installed.NextByteOffset, "authored empty visual frame is valid");
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
            image = IndexedPng.Read(input, MapSpriteFormat.Width, MapSpriteFormat.Height);
        for (int pixel = 0; pixel < image.Pixels.Length; pixel++) image.Pixels[pixel] = (byte)((image.Pixels[pixel] + 1) % 16);
        using (var output = File.Create(pngPath)) IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        var pngEdit = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(pngEdit.ContentIdentity != original.ContentIdentity, "PNG alone changes selected content identity");
        Suite(nameof(VerifyRenderedEdit), () => VerifyRenderedEdit(pngEdit));
        File.WriteAllText(pngPath, "invalid PNG");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt sprite PNG override rejected");
        File.WriteAllBytes(pngPath, File.ReadAllBytes(Path.Combine(stock, MapSpriteFormat.PngFile)));
        Console.WriteLine("Map sprites: 26 native compositions, clipping/capacity/palette parity, guarded file-select lifecycle and authored visual fields verified.");

        void VerifyRenderedEdit(AreaMapPresentationCatalog replacement)
        {
            var guard = new MapSpriteReadGuard(bus);
            var system = new Bank80SystemState(); system.SetAreaMapAcquired(AreaId.WreckedShip);
            system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
            var marker = new FileSelectStationMarker(bus, AreaId.WreckedShip, 0,
                original.SaveMarkers);
            var room = new FileSelectRoomMapGraphics(guard, system, AreaId.WreckedShip, mapPresentation: original);
            var nativeRoom = new FileSelectRoomMapGraphics(bus, system,
                AreaId.WreckedShip, mapPresentation: original);
            var before = nativeRoom.Render(0, 0, marker);
            AssertTrue(before.AsSpan().SequenceEqual(room.Render(0, 0, marker)), "stock map sprite pixels match with source reads blocked");
            room.BindMapPresentation(replacement);
            AssertTrue(!before.AsSpan().SequenceEqual(room.Render(0, 0, marker)), "independent sprite resource edit changes file-select pixels");
            using var capture = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(capture, room); capture.Position = 0;
            room = DebuggerObjectGraphSerializer.Deserialize<FileSelectRoomMapGraphics>(capture);
            room.BindMapPresentation(original);
            AssertTrue(before.AsSpan().SequenceEqual(room.Render(0, 0, marker)), "restored sprite resources bind current content");
            var pauseNative = new PauseMenuState(bus, new SamusState(), system,
                AreaId.WreckedShip, 19, 20, mapPresentation: original);
            var pause = new PauseMenuState(guard, new SamusState(), system, AreaId.WreckedShip, 19, 20, mapPresentation: original);
            var pauseBefore = pauseNative.Render();
            AssertTrue(pauseBefore.AsSpan().SequenceEqual(pause.Render()), "pause map sprite pixels match with source reads blocked");
            pause.BindMapPresentation(replacement);
            AssertTrue(!pauseBefore.AsSpan().SequenceEqual(pause.Render()), "independent sprite resource edit changes pause pixels");
            pause.BindMapPresentation(original);
            AssertTrue(pauseBefore.AsSpan().SequenceEqual(pause.Render()), "pause sprite rebind restores stock pixels");
        }
    }

    /// <summary>
    /// Import-only spritemap oracle shared by map and pause-UI parity checks. Gameplay
    /// OAM cannot read immutable cartridge sprites after installation.
    /// </summary>
    private static void DrawImportedSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        int address, ushort originX, ushort originY, ushort paletteBits,
        bool originIsOnScreen = true) => ImportedSpritemapOracle.DrawGeneric(
            CartridgeImportSource.Require(bus), oam, address, originX, originY,
            paletteBits, originIsOnScreen);

    /// <summary>
    /// Wraps the cartridge address space and rejects reads from sprite data that should be
    /// supplied by installed map-presentation resources, while forwarding other memory access.
    /// </summary>
    private sealed class MapSpriteReadGuard : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Address space that receives reads outside the guarded sprite ranges and all writes.</summary>
        private readonly ISnesAddressSpace source;

        /// <summary>Cartridge byte addresses belonging to imported map-sprite assets and native spritemaps.</summary>
        private readonly HashSet<int> forbidden = new();

        /// <summary>Builds the guarded ranges for map-sprite assets and the native frame spritemaps.</summary>
        /// <param name="source">Cartridge address space used to locate native frame data and forward allowed access.</param>
        public MapSpriteReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            Add(MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount);
            Add(FileSelectMapRomData.LabelSpritemapBase, 2);
            foreach (var frame in MapSpriteDefinitions.Frames)
            {
                int entry = MenuPpuState.SpritemapPointerTableAddress + frame.NativeId * 2;
                int pointer = FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), entry);
                Add(entry, 2); Add(pointer, 2 + 5 * RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), pointer));
            }
            void Add(int start, int count) { for (int index = 0; index < count; index++) forbidden.Add(start + index); }
        }
        /// <summary>Rejects a read when its address belongs to sprite data migrated to installed resources.</summary>
        /// <param name="address">Cartridge byte address about to be read.</param>
        /// <exception cref="InvalidOperationException">The address is in a guarded sprite range.</exception>
        private void RejectSpriteSource(int address)
        {
            if (forbidden.Contains(address))
                throw new InvalidOperationException($"Installed map read migrated sprite data at {address:X6}.");
        }
        /// <summary>Checks the address against guarded sprite ranges, then reads from the wrapped address space.</summary>
        /// <param name="address">Address requested by the emulated system.</param>
        /// <returns>The wrapped address space's byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to migrated sprite data.</exception>
        public byte ReadByte(int address)
        {
            RejectSpriteSource(address);
            return source.ReadByte(address);
        }
        /// <summary>Checks migrated sprite ranges before forwarding an import-source cartridge read.</summary>
        /// <param name="address">Cartridge byte address requested by the importer.</param>
        /// <returns>The import source's byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to migrated sprite data.</exception>
        public byte ReadCartridgeByte(int address)
        {
            RejectSpriteSource(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }
        /// <summary>Forwards a WRAM read to the wrapped mutable-memory provider.</summary>
        /// <param name="address">WRAM address to read.</param>
        /// <returns>The byte stored at that WRAM address.</returns>
        /// <exception cref="InvalidOperationException">The wrapped source does not expose mutable memory.</exception>
        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Map-sprite guard source does not expose WRAM.")).ReadWorkRamByte(address);
        /// <summary>Forwards an SRAM read to the wrapped mutable-memory provider.</summary>
        /// <param name="address">SRAM address to read.</param>
        /// <returns>The byte stored at that SRAM address.</returns>
        /// <exception cref="InvalidOperationException">The wrapped source does not expose mutable memory.</exception>
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Map-sprite guard source does not expose SRAM.")).ReadSaveRamByte(address);
        /// <summary>Forwards a byte write unchanged to the wrapped address space.</summary>
        /// <param name="address">Address that receives the write.</param>
        /// <param name="value">Byte written at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
