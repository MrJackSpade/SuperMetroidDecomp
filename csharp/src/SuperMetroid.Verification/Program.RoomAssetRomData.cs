using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Proves the named room-asset definitions remain inside the retail cartridge and that
    /// every graphics set selected by a retail room state resolves to three valid streams.
    /// </summary>
    static void VerifyRoomAssetRomData()
    {
        ValidateTilemapTransfer(RoomAssetRomData.LibraryBackground.ClearFx, "clear FX");
        ValidateTilemapTransfer(RoomAssetRomData.LibraryBackground.ClearBg2, "clear BG2");
        AssertEqual(
            RoomAssetRomData.GraphicsLayout.CreBlockDefinitionsByteCount,
            RoomAssetRomData.GraphicsLayout.CreBlockDefinitionCount *
                RoomAssetRomData.GraphicsLayout.BytesPerBlockDefinition,
            "CRE block-table geometry");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        if (!File.Exists(romPath) || !File.Exists(symbolPath))
        {
            Console.WriteLine(
                "  Room assets: memory-layout checks pass; retail range audit skipped " +
                "(private inputs absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RoomAssetRomData.BoundedCompressedAsset[] boundedAssets =
        [
            RoomAssetRomData.LandingSite.CreBlockDefinitions,
            RoomAssetRomData.LandingSite.AreaBlockDefinitions,
            RoomAssetRomData.LandingSite.LevelData,
            RoomAssetRomData.LandingSite.CreCharacters,
            RoomAssetRomData.LandingSite.AreaCharacters,
        ];
        foreach (RoomAssetRomData.BoundedCompressedAsset asset in boundedAssets)
        {
            byte[] decompressed = DecompressBoundedRoomAsset(bus, asset);
            AssertTrue(decompressed.Length > 0,
                $"bounded room asset ${asset.Address:X6} decompresses to nonempty data");
        }

        byte[] creBlocks = DecompressBoundedRoomAsset(
            bus,
            RoomAssetRomData.LandingSite.CreBlockDefinitions);
        AssertEqual(
            RoomAssetRomData.GraphicsLayout.CreBlockDefinitionsByteCount,
            creBlocks.Length,
            "retail CRE block-definition byte count");

        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .ToArray();
        var graphicsSets = new HashSet<byte>();
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
            {
                graphicsSets.Add(CartridgeRoomHeader.Load(bus, roomPointer, context).State.GraphicsSet);
            }
        }

        // Test the complete native table, including entries not reached by the room-state
        // contexts above. This is definition metadata, not an editable visual payload.
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            TilesetDefinition compiled = RoomTilesetDefinitions.Get(graphicsSet);
            int pointerAddress = RoomAssetRomData.Tilesets.PointerTableAddress +
                graphicsSet * sizeof(ushort);
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, pointerAddress);
            int definitionAddress = RoomAssetRomData.Tilesets.DefinitionBank | pointer;
            AssertEqual(pointer, compiled.Pointer,
                $"graphics set ${graphicsSet:X2} compiled definition pointer");
            AssertEqual(RomDataReader.ReadLongFixedBank(bus,
                    definitionAddress + RoomAssetRomData.Tilesets.BlockDefinitionsAddressOffset),
                compiled.BlockDefinitionsAddress,
                $"graphics set ${graphicsSet:X2} compiled block source");
            AssertEqual(RomDataReader.ReadLongFixedBank(bus,
                    definitionAddress + RoomAssetRomData.Tilesets.CharacterAddressOffset),
                compiled.CharacterAddress,
                $"graphics set ${graphicsSet:X2} compiled character source");
            AssertEqual(RomDataReader.ReadLongFixedBank(bus,
                    definitionAddress + RoomAssetRomData.Tilesets.PaletteAddressOffset),
                compiled.PaletteAddress,
                $"graphics set ${graphicsSet:X2} compiled palette source");
        }
        AssertThrows<InvalidDataException>(() => RoomTilesetDefinitions.Get(RoomTilesetDefinitions.Count),
            "graphics set after the last retail entry fails rather than reading unrelated bank-$8F data");

        foreach (byte graphicsSet in graphicsSets)
        {
            int pointerAddress = RoomAssetRomData.Tilesets.PointerTableAddress +
                graphicsSet * sizeof(ushort);
            ushort definitionPointer = RomDataReader.ReadWordFixedBank(bus, pointerAddress);
            int definitionAddress = RoomAssetRomData.Tilesets.DefinitionBank | definitionPointer;
            int blockAddress = RomDataReader.ReadLongFixedBank(
                bus,
                definitionAddress + RoomAssetRomData.Tilesets.BlockDefinitionsAddressOffset);
            int characterAddress = RomDataReader.ReadLongFixedBank(
                bus,
                definitionAddress + RoomAssetRomData.Tilesets.CharacterAddressOffset);
            int paletteAddress = RomDataReader.ReadLongFixedBank(
                bus,
                definitionAddress + RoomAssetRomData.Tilesets.PaletteAddressOffset);

            AssertTrue(RomDataReader.Decompress(bus, blockAddress).Length > 0,
                $"graphics set ${graphicsSet:X2} block stream is in the retail ROM");
            AssertTrue(RomDataReader.Decompress(bus, characterAddress).Length > 0,
                $"graphics set ${graphicsSet:X2} character stream is in the retail ROM");
            AssertTrue(
                RomDataReader.Decompress(bus, paletteAddress).Length >=
                    RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount,
                $"graphics set ${graphicsSet:X2} has the complete background palette range");
        }

        // Both ordinary and Ceres rooms must exercise the real loader with the whole
        // source table forbidden, including the overlapping Ceres character transfers.
        var guardedBus = new TilesetDefinitionReadGuard(bus);
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            CartridgeRoomHeader header = CartridgeRoomHeader.Load(bus, roomPointer);
            CartridgeRoomAssets native = CartridgeRoomAssets.Load(bus, header);
            CartridgeRoomAssets guarded = CartridgeRoomAssets.Load(guardedBus, header);
            AssertEqual(native.Tileset, guarded.Tileset,
                $"room $8F:{roomPointer:X4} resolves compiled graphics metadata");
            AssertTrue(native.CreCharacters.AsSpan().SequenceEqual(guarded.CreCharacters) &&
                native.RoomCharacters.AsSpan().SequenceEqual(guarded.RoomCharacters) &&
                native.PaletteBytes.AsSpan().SequenceEqual(guarded.PaletteBytes) &&
                native.LevelData.BlockDefinitions.Span.SequenceEqual(guarded.LevelData.BlockDefinitions.Span),
                $"room $8F:{roomPointer:X4} retains exact decompressed visual inputs");
            var nativeVram = new SnesVram();
            var guardedVram = new SnesVram();
            var nativeCgram = new SnesCgram();
            var guardedCgram = new SnesCgram();
            native.LoadGraphics(nativeVram, nativeCgram);
            guarded.LoadGraphics(guardedVram, guardedCgram);
            AssertTrue(nativeVram.Bytes.SequenceEqual(guardedVram.Bytes) &&
                nativeCgram.Colors.SequenceEqual(guardedCgram.Colors),
                $"room $8F:{roomPointer:X4} retains exact VRAM/CGRAM output");
        }

        Console.WriteLine(
            $"  Room assets: {boundedAssets.Length} bounded streams and " +
            $"{graphicsSets.Count} retail graphics sets validated; all " +
            $"{RoomTilesetDefinitions.Count} definitions compiled with live table reads blocked.");
    }

    /// <summary>Exercises every distinct room-character PNG against its native 4-bpp source.</summary>
    static void VerifyRoomCharacterAtlases()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Room character atlases: private ROM absent; retail audit skipped.");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomCharacterAtlasExtractor.Extract(bus);
        var sources = new Dictionary<string, int>
        {
            [RoomCharacterAtlasFormat.CreFileName] = RoomAssetRomData.Tilesets.CreCharactersAddress,
        };
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesetDefinitions.Get(graphicsSet).CharacterAddress;
            sources[RoomCharacterAtlasFormat.SourceFileName(address)] = address;
        }
        int ghostSource = RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress;
        sources[RoomCharacterAtlasFormat.SourceFileName(ghostSource)] = ghostSource;
        AssertEqual(sources.Count, files.Count, "one PNG per distinct retail room-character source");

        var compiledBySource = new Dictionary<int, RoomCharacterAtlas>();
        RoomCharacterAtlas? compiledCre = null;
        foreach ((string name, int sourceAddress) in sources)
        {
            byte[] native = sourceAddress == ghostSource
                ? RomDataReader.ReadFixedBank(bus, sourceAddress,
                    RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount)
                : RomDataReader.Decompress(bus, sourceAddress);
            byte[] png = files[name];
            RoomCharacterAtlas atlas = RoomCharacterAtlas.Load(new MemoryStream(png), native.Length);
            if (name == RoomCharacterAtlasFormat.CreFileName) compiledCre = atlas;
            else compiledBySource.Add(sourceAddress, atlas);
            AssertTrue(atlas.Transfer.Span.SequenceEqual(native),
                $"room atlas {name} preserves every native character byte");
            var vram = new SnesVram();
            atlas.LoadTo(vram, 0);
            AssertTrue(vram.Bytes[..native.Length].SequenceEqual(native),
                $"room atlas {name} uploads exactly the native character range");
        }
        var catalog = new RoomCharacterAtlasCatalog(compiledCre!, compiledBySource);
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, roomPointer);
            CartridgeRoomAssets native = CartridgeRoomAssets.Load(bus, room);
            int areaSource = native.Tileset.CharacterAddress;
            var guardedBus = new RoomCharacterReadGuard(bus, areaSource);
            CartridgeRoomAssets installed = CartridgeRoomAssets.Load(guardedBus, room, catalog);
            AssertTrue(installed.CreCharacters.AsSpan().SequenceEqual(native.CreCharacters) &&
                installed.RoomCharacters.AsSpan().SequenceEqual(native.RoomCharacters),
                $"room $8F:{roomPointer:X4} loads compiled characters with source reads forbidden");
            var nativeVram = new SnesVram();
            var installedVram = new SnesVram();
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            native.LoadGraphics(nativeVram, nativeCgram);
            installed.LoadGraphics(installedVram, installedCgram);
            AssertTrue(nativeVram.Bytes.SequenceEqual(installedVram.Bytes) &&
                nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"room $8F:{roomPointer:X4} retains exact compiled-art VRAM/CGRAM output");
        }

        CartridgeRoomHeader landing = CartridgeRoomHeader.Load(bus, 0x91f8);
        int landingSource = RoomTilesetDefinitions.Get(landing.State.GraphicsSet).CharacterAddress;
        byte[] landingNative = RomDataReader.Decompress(bus, landingSource);
        int landingTiles = landingNative.Length / RoomCharacterAtlasFormat.BytesPerTile;
        int landingColumns = Math.Min(RoomCharacterAtlasFormat.TileColumns, landingTiles);
        int landingRows = (landingTiles + landingColumns - 1) / landingColumns;
        IndexedPngImage landingImage = IndexedPng.Read(
            new MemoryStream(files[RoomCharacterAtlasFormat.SourceFileName(landingSource)]),
            landingColumns * 8, landingRows * 8);
        byte[] painted = (byte[])landingImage.Pixels.Clone();
        painted[0] ^= 1;
        using var paintedPng = new MemoryStream();
        IndexedPng.Write(paintedPng, landingImage.Width, landingImage.Height, painted,
            landingImage.Palette);
        compiledBySource[landingSource] = RoomCharacterAtlas.Load(
            new MemoryStream(paintedPng.ToArray()), landingNative.Length);
        var editedCatalog = new RoomCharacterAtlasCatalog(compiledCre!, compiledBySource);
        CartridgeRoomAssets editedRoom = CartridgeRoomAssets.Load(
            new RoomCharacterReadGuard(bus, landingSource), landing, editedCatalog);
        AssertTrue(!editedRoom.RoomCharacters.AsSpan().SequenceEqual(landingNative),
            "edited installed room PNG changes real room-loader character bytes");

        // A painted pixel must change the runtime character transfer, while edits in
        // empty padding cells must fail rather than disappear silently.
        // Retail sources happen to end on complete 32-tile rows. A bounded 33-tile
        // prefix from a real source tests the partial-row policy independently.
        int partialSource = RoomTilesetDefinitions.Get(0).CharacterAddress;
        byte[] partialNative = RomDataReader.Decompress(bus, partialSource)
            .AsSpan(0, 33 * RoomCharacterAtlasFormat.BytesPerTile).ToArray();
        int tileCount = partialNative.Length / RoomCharacterAtlasFormat.BytesPerTile;
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        byte[] partialPixels = SnesGraphics.DecodePlanarTiles(partialNative, 4,
            RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
        using var partialPng = new MemoryStream();
        IndexedPng.Write(partialPng, width, height, partialPixels, SnesGraphics.DiagnosticPalette(16));
        IndexedPngImage decoded = IndexedPng.Read(new MemoryStream(partialPng.ToArray()),
            columns * 8, rows * 8);
        byte[] edited = (byte[])decoded.Pixels.Clone();
        edited[0] ^= 1;
        using var changedPng = new MemoryStream();
        IndexedPng.Write(changedPng, decoded.Width, decoded.Height, edited, decoded.Palette);
        AssertTrue(!RoomCharacterAtlas.Load(new MemoryStream(changedPng.ToArray()), partialNative.Length)
                .Transfer.Span.SequenceEqual(partialNative),
            "painting a room PNG changes the compiled runtime character bytes");
        int firstPaddingTile = tileCount;
        edited = (byte[])decoded.Pixels.Clone();
        edited[(firstPaddingTile / columns * 8) * decoded.Width + firstPaddingTile % columns * 8] = 1;
        using var invalidPng = new MemoryStream();
        IndexedPng.Write(invalidPng, decoded.Width, decoded.Height, edited, decoded.Palette);
        AssertThrows<InvalidDataException>(() => RoomCharacterAtlas.Load(
                new MemoryStream(invalidPng.ToArray()), partialNative.Length),
            "room atlas rejects artwork in unused final-row cells");

        Console.WriteLine($"  Room character atlases: {files.Count} distinct CRE/graphics-set PNGs " +
            "roundtrip exactly; Ceres/Landing Site load without source reads, edits reach room loading, " +
            "and padding edits are rejected.");
    }

    private sealed class RoomCharacterReadGuard(ISnesAddressSpace source, int areaCharacters)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address == RoomAssetRomData.Tilesets.CreCharactersAddress ||
                address == areaCharacters)
                throw new InvalidOperationException(
                    $"Installed room loader reread character source ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class TilesetDefinitionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            int first = RoomAssetRomData.Tilesets.DefinitionBank | RoomTilesetDefinitions.Get(0).Pointer;
            int end = RoomAssetRomData.Tilesets.PointerTableAddress +
                RoomTilesetDefinitions.Count * sizeof(ushort);
            if (address >= first && address < end)
                throw new InvalidOperationException(
                    $"Room loader reread compiled tileset definition data at ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private static byte[] DecompressBoundedRoomAsset(
        SuperMetroidAddressSpace bus,
        RoomAssetRomData.BoundedCompressedAsset asset)
    {
        AssertTrue(asset.Address >= 0x808000 && asset.Address <= 0xffffff,
            $"room asset ${asset.Address:X6} begins in mapped cartridge space");
        AssertTrue(asset.StoredByteCount > 0,
            $"room asset ${asset.Address:X6} has a positive stored extent");
        AssertTrue(asset.Address + asset.StoredByteCount - 1 <= 0xffffff,
            $"room asset ${asset.Address:X6} ends in 24-bit cartridge space");

        var stored = new byte[asset.StoredByteCount];
        for (int index = 0; index < stored.Length; index++)
            stored[index] = bus.ReadByte(asset.Address + index);
        return SmCompression.Decompress(stored);
    }

    private static void ValidateTilemapTransfer(
        RoomAssetRomData.TilemapTransfer transfer,
        string description)
    {
        AssertEqual(
            RoomAssetRomData.LibraryBackground.WorkRamBank | transfer.WorkRamDestination,
            transfer.WorkRamSourceAddress,
            $"{description} WRAM source corresponds to its fill destination");
        AssertTrue((transfer.ByteCount & 1) == 0,
            $"{description} transfer contains complete words");
        AssertTrue(transfer.WorkRamDestination + transfer.ByteCount <=
            RoomAssetRomData.LibraryBackground.BankByteCount,
            $"{description} transfer remains in bank $7E");
        AssertTrue(transfer.VramDestinationWord + transfer.ByteCount / 2 <= 0x8000,
            $"{description} transfer remains in 64-KiB VRAM");
    }
}
