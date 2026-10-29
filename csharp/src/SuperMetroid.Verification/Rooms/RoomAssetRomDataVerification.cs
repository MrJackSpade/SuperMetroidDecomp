using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Retail checks for compiled room tileset definitions, character streams and installed editable character atlases.</summary>
internal static class RoomAssetRomDataVerification
{
    /// <summary>
    /// Proves the named room-asset definitions remain inside the retail cartridge and that
    /// every graphics set selected by a retail room state resolves to three valid streams.
    /// </summary>
    internal static void VerifyRoomAssetRomData()
    {
        ValidateLibraryBackgroundClear(LibraryBackgroundStagingDefinitions.ClearFx, "clear FX");
        ValidateLibraryBackgroundClear(LibraryBackgroundStagingDefinitions.ClearBg2, "clear BG2");
        AssertEqual(
            RoomGraphicsLayoutTooling.CreBlockDefinitionsByteCount,
            RoomGraphicsLayoutTooling.CreBlockDefinitionCount *
                RoomMetatileFormat.BytesPerBlock,
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

        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);

        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer.")))
            .Distinct()
            .ToArray();
        var graphicsSets = new HashSet<byte>();
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
            {
                graphicsSets.Add(checked((byte)SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer, context).State.GraphicsSet));
            }
        }

        // Test the complete native table, including entries not reached by the room-state
        // contexts above. This is definition metadata, not an editable visual payload.
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            TilesetDefinition compiled = RoomTilesets.Get(graphicsSet);
            int pointerAddress = RoomTilesetTableConstants.PointerTableAddress +
                graphicsSet * sizeof(ushort);
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), pointerAddress);
            int definitionAddress = RoomTilesetTableConstants.DefinitionBank | pointer;
            AssertEqual(RomDataReader.ReadLongFixedBank(CartridgeImportSource.Require(bus),
                    definitionAddress + RoomTilesetTableConstants.BlockDefinitionsAddressOffset),
                compiled.BlockDefinitionsAddress,
                $"graphics set ${graphicsSet:X2} compiled block source");
            AssertEqual(RomDataReader.ReadLongFixedBank(CartridgeImportSource.Require(bus),
                    definitionAddress + RoomTilesetTableConstants.CharacterAddressOffset),
                compiled.CharacterAddress,
                $"graphics set ${graphicsSet:X2} compiled character source");
            AssertEqual(RomDataReader.ReadLongFixedBank(CartridgeImportSource.Require(bus),
                    definitionAddress + RoomTilesetTableConstants.PaletteAddressOffset),
                compiled.PaletteAddress,
                $"graphics set ${graphicsSet:X2} compiled palette source");
        }
        AssertThrows<InvalidDataException>(() => RoomTilesets.Get(RoomTilesetDefinitions.Count),
            "graphics set after the last retail entry fails rather than reading unrelated bank-$8F data");

        foreach (byte graphicsSet in graphicsSets)
        {
            int pointerAddress = RoomTilesetTableConstants.PointerTableAddress +
                graphicsSet * sizeof(ushort);
            ushort definitionPointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), pointerAddress);
            int definitionAddress = RoomTilesetTableConstants.DefinitionBank | definitionPointer;
            int blockAddress = RomDataReader.ReadLongFixedBank(
                CartridgeImportSource.Require(bus),
                definitionAddress + RoomTilesetTableConstants.BlockDefinitionsAddressOffset);
            int characterAddress = RomDataReader.ReadLongFixedBank(
                CartridgeImportSource.Require(bus),
                definitionAddress + RoomTilesetTableConstants.CharacterAddressOffset);
            int paletteAddress = RomDataReader.ReadLongFixedBank(
                CartridgeImportSource.Require(bus),
                definitionAddress + RoomTilesetTableConstants.PaletteAddressOffset);

            AssertTrue(RomDataReader.Decompress(bus, blockAddress).Length > 0,
                $"graphics set ${graphicsSet:X2} block stream is in the retail ROM");
            AssertTrue(RomDataReader.Decompress(bus, characterAddress).Length > 0,
                $"graphics set ${graphicsSet:X2} character stream is in the retail ROM");
            AssertTrue(
                RomDataReader.Decompress(bus, paletteAddress).Length >=
                    RoomStaticPaletteFormat.ByteCount,
                $"graphics set ${graphicsSet:X2} has the complete background palette range");
        }

        // Both ordinary and Ceres rooms must exercise the real loader with the whole
        // source table forbidden, including the overlapping Ceres character transfers.
        var installation = RepositoryInstallation.Installation;
        var characters = installation.LoadRoomCharacters();
        var palettes = installation.LoadRoomPalettes();
        var layouts = installation.LoadRoomVisualLayouts();
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            SelectedRoom header = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer);
            LoadedRoomAssets native = LoadFixtureRoomAssets(header, palettes, layouts);
            LoadedRoomAssets guarded = LoadFixtureRoomAssets(header, palettes, layouts);
            AssertEqual(native.Tileset, guarded.Tileset,
                $"room $8F:{roomPointer:X4} resolves compiled graphics metadata");
            AssertTrue(native.CreCharactersIn(characters).Span.SequenceEqual(guarded.CreCharactersIn(characters).Span) &&
                native.TilesetCharactersIn(characters).Span.SequenceEqual(guarded.TilesetCharactersIn(characters).Span) &&
                ReferenceEquals(native.TilesetColours, guarded.TilesetColours) &&
                // The room's blocks are its tileset's block pictures and the inputs each depends on (#1431, #1466).
                ReferenceEquals(native.BlockSheet, guarded.BlockSheet) && native.BlockSheet.Tileset == native.Tileset,
                $"room $8F:{roomPointer:X4} retains exact decompressed visual inputs");
            var nativeVram = new SnesVram();
            var guardedVram = new SnesVram();
            var nativeCgram = new GameplayColours(new SpriteColourSets());
            var guardedCgram = new GameplayColours(new SpriteColourSets());
            native.LoadGraphics(BgArtworkFixtures.For(nativeVram), nativeCgram.BackgroundSets);
            guarded.LoadGraphics(BgArtworkFixtures.For(guardedVram), guardedCgram.BackgroundSets);
            AssertTrue(nativeVram.ImageEquals(guardedVram) &&
                new CgramPalettes(nativeCgram.AsPaletteColors()).ImageEquals(new CgramPalettes(guardedCgram.AsPaletteColors())),
                $"room $8F:{roomPointer:X4} retains exact VRAM/CGRAM output");
        }

        Console.WriteLine(
            $"  Room assets: {graphicsSets.Count} retail graphics sets validated; all " +
            $"{RoomTilesetDefinitions.Count} definitions compiled with live table reads blocked.");
    }

    /// <summary>Exercises every distinct room-character PNG against its native 4-bpp source.</summary>
    internal static void VerifyRoomCharacterAtlases()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Room character atlases: private ROM absent; retail audit skipped.");
            return;
        }

        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomCharacterAtlasExtractor.Extract(bus);
        var sources = new Dictionary<string, int>
        {
            [RoomCharacterAtlasFormat.CreFileName] = (int)RoomTilesetSourceDefinitions.CreCharacters,
        };
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesets.Get(graphicsSet).CharacterAddress;
            sources[RoomCharacterAtlasFormat.SourceFileName(address)] = address;
        }
        int ghostSource = RoomBackgroundSourceDefinitions.TourianStatueGhostCharacters;
        sources[RoomCharacterAtlasFormat.SourceFileName(ghostSource)] = ghostSource;
        AssertEqual(sources.Count, files.Count, "one PNG per distinct retail room-character source");

        var compiledBySource = new Dictionary<int, RoomCharacterAtlas>();
        RoomCharacterAtlas? compiledCre = null;
        foreach ((string name, int sourceAddress) in sources)
        {
            byte[] native = sourceAddress == ghostSource
                ? RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), sourceAddress,
                    LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount)
                : RomDataReader.Decompress(bus, sourceAddress);
            byte[] png = files[name];
            RoomCharacterAtlas atlas = RoomCharacterAtlasPng.Read(new MemoryStream(png), native.Length);
            if (name == RoomCharacterAtlasFormat.CreFileName) compiledCre = atlas;
            else compiledBySource.Add(sourceAddress, atlas);
            AssertTrue(atlas.Characters.Span.SequenceEqual(native),
                $"room atlas {name} preserves every native character byte");
            var vram = new SnesVram();
            atlas.LoadTo(new VramRegion(vram, PpuMemoryLayout.Vram));
            AssertTrue(vram.Bytes[..native.Length].SequenceEqual(native),
                $"room atlas {name} uploads exactly the native character range");
        }
        var catalog = new RoomCharacterAtlases((compiledCre ?? throw new InvalidOperationException("Verification requires compiledCre.")), compiledBySource.ToDictionary(pair => SuperMetroid.Core.Rooms.RoomTilesetNative.CharacterSetAt(pair.Key), pair => pair.Value));
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            SelectedRoom room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer);
            LoadedRoomAssets native = LoadFixtureRoomAssets(room);
            LoadedRoomAssets installed = LoadFixtureRoomAssets(room);
            AssertTrue(installed.CreCharactersIn(catalog).Span.SequenceEqual(native.CreCharactersIn(FixtureRoomCharacters).Span) &&
                installed.TilesetCharactersIn(catalog).Span.SequenceEqual(native.TilesetCharactersIn(FixtureRoomCharacters).Span),
                $"room $8F:{roomPointer:X4} loads compiled characters with source reads forbidden");
            var nativeVram = new SnesVram();
            var installedVram = new SnesVram();
            var nativeCgram = new GameplayColours(new SpriteColourSets());
            var installedCgram = new GameplayColours(new SpriteColourSets());
            native.LoadGraphics(BgArtworkFixtures.For(nativeVram), nativeCgram.BackgroundSets);
            installed.LoadGraphics(BgArtworkFixtures.For(installedVram), installedCgram.BackgroundSets);
            AssertTrue(nativeVram.ImageEquals(installedVram) &&
                new CgramPalettes(nativeCgram.AsPaletteColors()).ImageEquals(new CgramPalettes(installedCgram.AsPaletteColors())),
                $"room $8F:{roomPointer:X4} retains exact compiled-art VRAM/CGRAM output");
        }

        SelectedRoom landing = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0x91f8);
        int landingSource = RoomTilesets.Get(landing.State.GraphicsSet).CharacterAddress;
        byte[] landingNative = RomDataReader.Decompress(bus, landingSource);
        int landingTiles = landingNative.Length / RoomCharacterAtlasFormat.BytesPerTile;
        int landingColumns = Math.Min(RoomCharacterAtlasFormat.TileColumns, landingTiles);
        int landingRows = (landingTiles + landingColumns - 1) / landingColumns;
        IndexedPngImage landingImage = IndexedPng.Read(
            new MemoryStream(files[RoomCharacterAtlasFormat.SourceFileName(landingSource)]),
            new(checked((ushort)(landingColumns * 8)), checked((ushort)(landingRows * 8))));
        byte[] painted = (byte[])landingImage.Pixels.Clone();
        painted[0] ^= 1;
        using var paintedPng = new MemoryStream();
        IndexedPng.Write(paintedPng, new(landingImage.Size.X, landingImage.Size.Y), painted,
            landingImage.Colours);
        compiledBySource[landingSource] = RoomCharacterAtlasPng.Read(
            new MemoryStream(paintedPng.ToArray()), landingNative.Length);
        var editedCatalog = new RoomCharacterAtlases(compiledCre, compiledBySource.ToDictionary(pair => SuperMetroid.Core.Rooms.RoomTilesetNative.CharacterSetAt(pair.Key), pair => pair.Value));
        LoadedRoomAssets editedRoom = LoadFixtureRoomAssets(landing);
        AssertTrue(!editedRoom.TilesetCharactersIn(editedCatalog).Span.SequenceEqual(landingNative),
            "edited installed room PNG changes real room-loader character bytes");

        // A painted pixel must change the runtime character transfer, while edits in
        // empty padding cells must fail rather than disappear silently.
        // Retail sources happen to end on complete 32-tile rows. A bounded 33-tile
        // prefix from a real source tests the partial-row policy independently.
        int partialSource = RoomTilesets.Get(0).CharacterAddress;
        byte[] partialNative = RomDataReader.Decompress(bus, partialSource)
            .AsSpan(0, 33 * RoomCharacterAtlasFormat.BytesPerTile).ToArray();
        int tileCount = partialNative.Length / RoomCharacterAtlasFormat.BytesPerTile;
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        byte[] partialPixels = SnesGraphics.DecodePlanarTiles(partialNative, 4,
            RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var partialPng = new MemoryStream();
        IndexedPng.Write(partialPng, size, partialPixels, SnesGraphics.DiagnosticPalette(16));
        IndexedPngImage decoded = IndexedPng.Read(new MemoryStream(partialPng.ToArray()),
            new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
        byte[] edited = (byte[])decoded.Pixels.Clone();
        edited[0] ^= 1;
        using var changedPng = new MemoryStream();
        IndexedPng.Write(changedPng, new(decoded.Size.X, decoded.Size.Y), edited, decoded.Colours);
        AssertTrue(!RoomCharacterAtlasPng.Read(new MemoryStream(changedPng.ToArray()), partialNative.Length)
                .Characters.Span.SequenceEqual(partialNative),
            "painting a room PNG changes the compiled runtime character bytes");
        int firstPaddingTile = tileCount;
        edited = (byte[])decoded.Pixels.Clone();
        edited[(firstPaddingTile / columns * 8) * decoded.Size.X + firstPaddingTile % columns * 8] = 1;
        using var invalidPng = new MemoryStream();
        IndexedPng.Write(invalidPng, new(decoded.Size.X, decoded.Size.Y), edited, decoded.Colours);
        AssertThrows<InvalidDataException>(() => RoomCharacterAtlasPng.Read(
                new MemoryStream(invalidPng.ToArray()), partialNative.Length),
            "room atlas rejects artwork in unused final-row cells");

        Console.WriteLine($"  Room character atlases: {files.Count} distinct CRE/graphics-set PNGs " +
            "roundtrip exactly; Ceres/Landing Site load without source reads, edits reach room loading, " +
            "and padding edits are rejected.");
    }

    /// <summary>Checks a library-background clear's WRAM source/destination identity, whole-word size and WRAM/VRAM bounds.</summary>
    private static void ValidateLibraryBackgroundClear(
        LibraryBackgroundClear transfer,
        string description)
    {
        AssertEqual(
            LibraryBackgroundListConstants.WorkRamBank | transfer.WorkRamDestination,
            transfer.WorkRamSourceAddress,
            $"{description} WRAM source corresponds to its fill destination");
        AssertTrue((transfer.ByteCount & 1) == 0,
            $"{description} transfer contains complete words");
        AssertTrue(transfer.WorkRamDestination + transfer.ByteCount <=
            LibraryBackgroundListConstants.BankByteCount,
            $"{description} transfer remains in bank $7E");
        AssertTrue(transfer.VramDestination.Word + transfer.ByteCount / 2 <= 0x8000,
            $"{description} transfer remains in 64-KiB VRAM");
    }

    /// <summary>Runs the compiled room tileset and graphics-stream checks.</summary>
    [VerificationCommand("--room-tileset-definitions")]
    internal static int RoomTilesetDefinitionsCommand()
    {
        Suite(nameof(VerifyRoomAssetRomData), () => VerifyRoomAssetRomData());
        return 0;
    }

    /// <summary>Runs character PNG roundtrips and the installed-artwork room-loading check.</summary>
    [VerificationCommand("--room-character-atlases")]
    internal static int RoomCharacterAtlasesCommand()
    {
        Suite(nameof(VerifyRoomCharacterAtlases), () => VerifyRoomCharacterAtlases());
        return 0;
    }
}
