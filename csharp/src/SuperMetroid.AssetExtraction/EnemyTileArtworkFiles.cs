using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports every retail room-enemy graphics sheet; user PNG overrides live outside stock content.</summary>
public static class EnemyTileArtworkFiles
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);

        var entries = new Dictionary<ushort, EnemyTileFileEntry>();
        foreach (ushort graphicsSetPointer in RoomStateDefinitions.All
                     .Select(state => state.EnemyTilesetPointer).Distinct().Order())
        {
            int cursor = RoomEnemyRomLayout.TilesetBank | graphicsSetPointer;
            for (int slot = 0; slot <= 4; slot++, cursor += 4)
            {
                ushort definitionPointer = RomDataReader.ReadWordFixedBank(bus, cursor);
                if (definitionPointer == 0xffff) break;
                if (slot == 4)
                    throw new InvalidDataException($"Enemy graphics set ${graphicsSetPointer:X4} exceeds four entries.");
                if (entries.ContainsKey(definitionPointer)) continue;

                RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, definitionPointer);
                int byteCount = definition.TileDataSize & 0x7fff;
                int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(byteCount);
                byte[] planar = RomDataReader.ReadFixedBank(bus, definition.TileDataAddress, byteCount);
                int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
                byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, columns,
                    out int width, out int height);
                using var png = new MemoryStream();
                IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
                byte[] encoded = png.ToArray();
                RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
                    new MemoryStream(encoded, writable: false), byteCount);
                if (!roundtrip.Transfer.Span.SequenceEqual(planar))
                    throw new InvalidDataException($"Enemy ${definitionPointer:X4} tile PNG changed native pixels.");

                File.WriteAllBytes(Path.Combine(directory, EnemyTileArtworkFormat.FileName(definitionPointer)), encoded);
                byte[] nativeColors = RomDataReader.ReadFixedBank(bus,
                    (definition.Bank << 16) | definition.PalettePointer,
                    EnemyPaletteSheet.ColorCount * sizeof(ushort));
                var colors = new PaletteRgb5[EnemyPaletteSheet.ColorCount];
                for (int color = 0; color < colors.Length; color++)
                {
                    ushort word = BinaryPrimitives.ReadUInt16LittleEndian(nativeColors.AsSpan(color * 2));
                    if ((word & 0x8000) != 0)
                        throw new InvalidDataException($"Enemy ${definitionPointer:X4} palette has an unrepresentable high bit.");
                    colors[color] = new PaletteRgb5
                    {
                        Red = word & 31,
                        Green = word >> 5 & 31,
                        Blue = word >> 10 & 31,
                    };
                }
                byte[] paletteJson = EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument
                {
                    Version = 1,
                    Colors = colors,
                });
                var nativeCgram = new SnesCgram();
                var compiledCgram = new SnesCgram();
                nativeCgram.LoadBytes(nativeColors);
                EnemyPaletteSheet.Load(new MemoryStream(paletteJson, writable: false))
                    .LoadTo(compiledCgram, 0);
                if (!nativeCgram.Colors.SequenceEqual(compiledCgram.Colors))
                    throw new InvalidDataException($"Enemy ${definitionPointer:X4} palette JSON changed native colors.");
                File.WriteAllBytes(Path.Combine(directory,
                    EnemyTileArtworkFormat.PaletteFileName(definitionPointer)), paletteJson);
                entries.Add(definitionPointer, new EnemyTileFileEntry(
                    byteCount, Convert.ToHexString(SHA256.HashData(encoded)),
                    Convert.ToHexString(SHA256.HashData(paletteJson))));
            }
        }
        if (entries.Count != EnemyTileArtworkFormat.RetailDefinitionCount)
            throw new InvalidDataException($"Expected {EnemyTileArtworkFormat.RetailDefinitionCount} retail enemy sheets; found {entries.Count}.");
        ValidateDefinitionIds(entries.Keys);
        byte[] firstMelt = ExtractCrocomireMelt(bus,
            CrocomireMeltingTransferDefinitions.Passes[0],
            CrocomireMeltingArtworkFormat.FirstByteCount);
        byte[] secondMelt = ExtractCrocomireMelt(bus,
            CrocomireMeltingTransferDefinitions.Passes[1],
            CrocomireMeltingArtworkFormat.SecondByteCount);
        byte[] firstMeltTilemap = ExtractCrocomireMeltTilemap(bus,
            CrocomireMeltingArtworkAddresses.FirstTilemap);
        byte[] secondMeltTilemap = ExtractCrocomireMeltTilemap(bus,
            CrocomireMeltingArtworkAddresses.SecondTilemap);
        File.WriteAllBytes(Path.Combine(directory, CrocomireMeltingArtworkFormat.FirstFileName), firstMelt);
        File.WriteAllBytes(Path.Combine(directory, CrocomireMeltingArtworkFormat.SecondFileName), secondMelt);
        File.WriteAllBytes(Path.Combine(directory,
            CrocomireMeltingArtworkFormat.FirstTilemapFileName), firstMeltTilemap);
        File.WriteAllBytes(Path.Combine(directory,
            CrocomireMeltingArtworkFormat.SecondTilemapFileName), secondMeltTilemap);
        byte[] spritemapJson = EnemySpritemapFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, EnemySpritemapDefinitions.FileName),
            spritemapJson);
        byte[] extendedJson = EnemyExtendedFrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, EnemyExtendedFrameDefinitions.FileName),
            extendedJson);
        byte[] upperKraid = ExtractKraidTilemap(bus, KraidBackgroundRomData.UpperTilemap);
        byte[] lowerKraid = ExtractKraidTilemap(bus, KraidBackgroundRomData.LowerTilemap);
        File.WriteAllBytes(Path.Combine(directory, KraidBackgroundArtworkFormat.UpperFileName),
            upperKraid);
        File.WriteAllBytes(Path.Combine(directory, KraidBackgroundArtworkFormat.LowerFileName),
            lowerKraid);
        ushort[] headPointers = KraidHeadInstructionDefinitions.All.ToArray()
            .Where(frame => frame.Kind == KraidHeadInstructionKind.Frame)
            .Select(frame => frame.Tilemap).Distinct().Order().ToArray();
        var kraidHeadHashes = new Dictionary<ushort, string>();
        foreach (ushort pointer in headPointers)
        {
            byte[] native = RomDataReader.ReadFixedBank(bus,
                KraidBackgroundRomData.NativeBank | pointer,
                KraidBackgroundRomData.HeadTilemapWords * sizeof(ushort));
            byte[] json = KraidHeadTilemapAtlas.Encode(native);
            File.WriteAllBytes(Path.Combine(directory,
                KraidBackgroundArtworkFormat.HeadFileName(pointer)), json);
            kraidHeadHashes.Add(pointer, Convert.ToHexString(SHA256.HashData(json)));
        }
        byte[] kraidRoomBackground = ExtractKraidRoomBackground(bus);
        File.WriteAllBytes(Path.Combine(directory,
            KraidBackgroundArtworkFormat.RoomBackgroundFileName), kraidRoomBackground);
        byte[] kraidColors = ExtractKraidColors(bus);
        File.WriteAllBytes(Path.Combine(directory, KraidColorFormat.FileName), kraidColors);
        var manifest = new EnemyTileManifest(EnemyTileArtworkFormat.Version,
            sourceCartridgeSha256, entries,
            Convert.ToHexString(SHA256.HashData(firstMelt)),
            Convert.ToHexString(SHA256.HashData(secondMelt)),
            Convert.ToHexString(SHA256.HashData(firstMeltTilemap)),
            Convert.ToHexString(SHA256.HashData(secondMeltTilemap)),
            Convert.ToHexString(SHA256.HashData(spritemapJson)),
            Convert.ToHexString(SHA256.HashData(extendedJson)),
            Convert.ToHexString(SHA256.HashData(upperKraid)),
            Convert.ToHexString(SHA256.HashData(lowerKraid)),
            kraidHeadHashes,
            Convert.ToHexString(SHA256.HashData(kraidRoomBackground)),
            Convert.ToHexString(SHA256.HashData(kraidColors)));
        File.WriteAllBytes(Path.Combine(directory, EnemyTileArtworkFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
    }

    /// <summary>Checks stock hashes, then compiles selected PNGs without reading a cartridge.</summary>
    public static EnemyTileArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, EnemyTileArtworkFormat.ManifestFileName);
        EnemyTileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<EnemyTileManifest>(File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Enemy tile manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid enemy tile manifest {manifestPath}.", error);
        }
        if (manifest.Version != EnemyTileArtworkFormat.Version ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null ||
            manifest.Entries.Count != EnemyTileArtworkFormat.RetailDefinitionCount ||
            string.IsNullOrWhiteSpace(manifest.CrocomireFirstSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireSecondSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireFirstTilemapSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireSecondTilemapSha256) ||
            string.IsNullOrWhiteSpace(manifest.EnemyCompositionsSha256) ||
            string.IsNullOrWhiteSpace(manifest.EnemyExtendedCompositionsSha256) ||
            string.IsNullOrWhiteSpace(manifest.KraidUpperSha256) ||
            string.IsNullOrWhiteSpace(manifest.KraidLowerSha256) ||
            manifest.KraidHeadsSha256 is null ||
            string.IsNullOrWhiteSpace(manifest.KraidRoomBackgroundSha256) ||
            string.IsNullOrWhiteSpace(manifest.KraidColorsSha256))
            throw new InvalidDataException($"Enemy tile manifest {manifestPath} does not describe this installation.");
        ValidateDefinitionIds(manifest.Entries.Keys);
        ushort[] expectedHeadPointers = KraidHeadInstructionDefinitions.All.ToArray()
            .Where(frame => frame.Kind == KraidHeadInstructionKind.Frame)
            .Select(frame => frame.Tilemap).Distinct().Order().ToArray();
        if (!manifest.KraidHeadsSha256.Keys.Order().SequenceEqual(expectedHeadPointers))
            throw new InvalidDataException("Enemy tile manifest omits or substitutes a Kraid head frame.");

        var sheets = new Dictionary<ushort, RoomCharacterAtlas>();
        var palettes = new Dictionary<ushort, EnemyPaletteSheet>();
        foreach ((ushort definitionPointer, EnemyTileFileEntry entry) in manifest.Entries)
        {
            RoomCharacterAtlasFormat.ValidateTileCount(entry.NativeByteCount);
            string fileName = EnemyTileArtworkFormat.FileName(definitionPointer);
            string stockPath = Path.Combine(stockDirectory, fileName);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock enemy tile PNG {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, fileName);
            string selectedPath = overridePath is not null && File.Exists(overridePath) ? overridePath : stockPath;
            try
            {
                byte[] selected = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
                sheets.Add(definitionPointer, RoomCharacterAtlas.Load(
                    new MemoryStream(selected, writable: false), entry.NativeByteCount));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid enemy tile PNG {selectedPath}: {error.Message}", error);
            }
            string paletteName = EnemyTileArtworkFormat.PaletteFileName(definitionPointer);
            string stockPalettePath = Path.Combine(stockDirectory, paletteName);
            byte[] stockPalette = File.ReadAllBytes(stockPalettePath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockPalette)), entry.PaletteSha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock enemy palette {stockPalettePath} failed its manifest hash.");
            string? paletteOverridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, paletteName);
            string selectedPalettePath = paletteOverridePath is not null && File.Exists(paletteOverridePath)
                ? paletteOverridePath : stockPalettePath;
            try
            {
                byte[] selected = selectedPalettePath == stockPalettePath
                    ? stockPalette : File.ReadAllBytes(selectedPalettePath);
                palettes.Add(definitionPointer, EnemyPaletteSheet.Load(
                    new MemoryStream(selected, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid enemy palette {selectedPalettePath}: {error.Message}", error);
            }
        }
        byte[] first = ReadCrocomireAsset(CrocomireMeltingArtworkFormat.FirstFileName,
            manifest.CrocomireFirstSha256);
        byte[] second = ReadCrocomireAsset(CrocomireMeltingArtworkFormat.SecondFileName,
            manifest.CrocomireSecondSha256);
        byte[] firstTilemap = ReadCrocomireAsset(
            CrocomireMeltingArtworkFormat.FirstTilemapFileName,
            manifest.CrocomireFirstTilemapSha256);
        byte[] secondTilemap = ReadCrocomireAsset(
            CrocomireMeltingArtworkFormat.SecondTilemapFileName,
            manifest.CrocomireSecondTilemapSha256);
        CrocomireMeltingArtwork crocomire;
        try
        {
            crocomire = CrocomireMeltingArtwork.Load(
                new MemoryStream(first, writable: false),
                new MemoryStream(second, writable: false),
                new MemoryStream(firstTilemap, writable: false),
                new MemoryStream(secondTilemap, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire melt artwork in {overrideDirectory ?? stockDirectory}: {error.Message}", error);
        }
        byte[] compositionJson = ReadStockOrOverride(
            EnemySpritemapDefinitions.FileName, manifest.EnemyCompositionsSha256);
        EnemySpritemapCatalog spritemaps;
        try
        {
            // Older overrides cannot know the later Skultera, Waver, and
            // Skree/Metaree/Zoa and Pipe Bug identities.
            // Merge only their validated frames onto verified current stock content,
            // preserving existing user edits through an extraction upgrade.
            string stockCompositionPath = Path.Combine(
                stockDirectory, EnemySpritemapDefinitions.FileName);
            byte[] stockComposition = File.ReadAllBytes(stockCompositionPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockComposition)),
                    manifest.EnemyCompositionsSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Stock enemy compositions {stockCompositionPath} failed its manifest hash.");
            EnemySpritemapCatalog stockSpritemaps = EnemySpritemapCatalog.Load(
                new MemoryStream(stockComposition, writable: false));
            spritemaps = EnemySpritemapCatalog.Load(
                new MemoryStream(compositionJson, writable: false), stockSpritemaps);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid enemy compositions in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        byte[] extendedJson = ReadStockOrOverride(
            EnemyExtendedFrameDefinitions.FileName,
            manifest.EnemyExtendedCompositionsSha256);
        EnemyExtendedFrameCatalog extendedFrames;
        try
        {
            // V1 has walking frames and v2 adds wall frames. Overlay either
            // validated legacy file onto complete, hash-checked v3 stock so
            // newly added Ninja art cannot discard the user's existing edits.
            string stockExtendedPath = Path.Combine(stockDirectory,
                EnemyExtendedFrameDefinitions.FileName);
            byte[] stockExtendedJson = File.ReadAllBytes(stockExtendedPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockExtendedJson)),
                    manifest.EnemyExtendedCompositionsSha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Stock extended enemy compositions {stockExtendedPath} failed its manifest hash.");
            EnemyExtendedFrameCatalog stockExtended = EnemyExtendedFrameCatalog.Load(
                new MemoryStream(stockExtendedJson, writable: false));
            extendedFrames = EnemyExtendedFrameCatalog.Load(
                new MemoryStream(extendedJson, writable: false), stockExtended);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid extended enemy compositions in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        RoomBackgroundTilemapAtlas upperKraid = LoadKraidTilemap(
            KraidBackgroundArtworkFormat.UpperFileName, manifest.KraidUpperSha256);
        RoomBackgroundTilemapAtlas lowerKraid = LoadKraidTilemap(
            KraidBackgroundArtworkFormat.LowerFileName, manifest.KraidLowerSha256);
        var kraidHeads = new Dictionary<ushort, KraidHeadTilemapAtlas>();
        foreach ((ushort pointer, string sha256) in manifest.KraidHeadsSha256)
        {
            string fileName = KraidBackgroundArtworkFormat.HeadFileName(pointer);
            byte[] selected = ReadStockOrOverride(fileName, sha256);
            try
            {
                kraidHeads.Add(pointer, KraidHeadTilemapAtlas.Load(
                    new MemoryStream(selected, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Kraid head tilemap {fileName}: {error.Message}", error);
            }
        }
        string roomBackgroundName = KraidBackgroundArtworkFormat.RoomBackgroundFileName;
        byte[] roomBackgroundPng = ReadStockOrOverride(
            roomBackgroundName, manifest.KraidRoomBackgroundSha256);
        RoomCharacterAtlas roomBackground;
        try
        {
            roomBackground = RoomCharacterAtlas.Load(
                new MemoryStream(roomBackgroundPng, writable: false),
                KraidBackgroundRomData.RoomBackgroundTileBytes);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid room-background PNG {roomBackgroundName}: {error.Message}", error);
        }
        KraidColorCatalog kraidColors;
        try
        {
            byte[] selected = ReadStockOrOverride(KraidColorFormat.FileName,
                manifest.KraidColorsSha256);
            kraidColors = KraidColorCatalog.Load(new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid color file {KraidColorFormat.FileName}: {error.Message}", error);
        }
        return new EnemyTileArtworkCatalog(sheets, palettes, crocomire,
            spritemaps, extendedFrames, new KraidBackgroundArtwork(upperKraid, lowerKraid,
                kraidHeads, roomBackground), kraidColors);

        RoomBackgroundTilemapAtlas LoadKraidTilemap(string fileName, string expectedSha256)
        {
            byte[] selected = ReadStockOrOverride(fileName, expectedSha256);
            try
            {
                return RoomBackgroundTilemapAtlas.Load(
                    new MemoryStream(selected, writable: false),
                    KraidBackgroundRomData.DecompressedTilemapBytes);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Kraid BG2 tilemap {fileName}: {error.Message}", error);
            }
        }

        byte[] ReadCrocomireAsset(string fileName, string expectedSha256)
            => ReadStockOrOverride(fileName, expectedSha256);

        byte[] ReadStockOrOverride(string fileName, string expectedSha256)
        {
            string stockPath = Path.Combine(stockDirectory, fileName);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), expectedSha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock enemy asset {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, fileName);
            return overridePath is not null && File.Exists(overridePath)
                ? File.ReadAllBytes(overridePath) : stock;
        }
    }

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static void ValidateDefinitionIds(IEnumerable<ushort> pointers)
    {
        string joined = string.Join(",", pointers.Order().Select(pointer => $"{pointer:X4}"));
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(joined)));
        if (!string.Equals(hash, EnemyTileArtworkFormat.RetailDefinitionIdsSha256,
                StringComparison.Ordinal))
            throw new InvalidDataException("Enemy tile manifest omits or substitutes a retail graphics definition.");
    }

    private static byte[] ExtractCrocomireMelt(ISnesAddressSpace bus,
        CrocomireMeltingPass pass, int encodedByteCount)
    {
        var planar = new byte[encodedByteCount];
        foreach (CrocomireMeltingCopy copy in pass.Copies.Span)
        {
            int destination = copy.DestinationWord - 0x4000;
            for (int index = 0; index < (pass.WordsToCopy + 1) * 2; index++)
            {
                planar[destination + index] = bus.ReadByte(
                    (pass.SourceBank << 16) | unchecked((ushort)(copy.SourceWord + index)));
            }
        }
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(encodedByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount),
            out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] encoded = png.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(encoded, writable: false), encodedByteCount);
        if (!roundtrip.Transfer.Span.SequenceEqual(planar))
            throw new InvalidDataException(
                $"Crocomire melt pass ${pass.HeaderOffset:X4} PNG changed native graphics bytes.");
        return encoded;
    }

    private static byte[] ExtractKraidTilemap(ISnesAddressSpace bus, int sourceAddress)
    {
        byte[] native = RomDataReader.Decompress(bus, sourceAddress,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        return RoomBackgroundTilemapExtractor.Encode(native);
    }

    private static byte[] ExtractKraidRoomBackground(ISnesAddressSpace bus)
    {
        byte[] planar = RomDataReader.ReadFixedBank(bus,
            KraidBackgroundRomData.RoomBackgroundTileAddress,
            KraidBackgroundRomData.RoomBackgroundTileBytes);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            KraidBackgroundRomData.RoomBackgroundTileBytes /
                RoomCharacterAtlasFormat.BytesPerTile,
            out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] encoded = png.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(encoded, writable: false),
            KraidBackgroundRomData.RoomBackgroundTileBytes);
        if (!roundtrip.Transfer.Span.SequenceEqual(planar))
            throw new InvalidDataException("Kraid room-background PNG changed native tile bytes.");
        return encoded;
    }

    private static byte[] ExtractKraidColors(ISnesAddressSpace bus) =>
        KraidColorCatalog.Write(new KraidColorDocument
        {
            Version = KraidColorFormat.Version,
            RoomBackdrop = ReadKraidColors(bus, KraidPaletteSource.RoomBackdrop),
            InitialTarget = ReadKraidColors(bus, KraidPaletteSource.InitialTarget),
            Health = ReadKraidColors(bus, KraidPaletteSource.Health),
            Secondary = ReadKraidColors(bus, KraidPaletteSource.Secondary),
            DeathArm = ReadKraidColors(bus, KraidPaletteSource.DeathArm),
        });

    private static PaletteRgb5[] ReadKraidColors(ISnesAddressSpace bus,
        KraidPaletteSource source)
    {
        int count = KraidPaletteRomData.ColorCount(source);
        byte[] native = RomDataReader.ReadFixedBank(bus,
            KraidPaletteRomData.SourceAddress(source), count * sizeof(ushort));
        var colors = new PaletteRgb5[count];
        for (int index = 0; index < count; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * 2));
            if ((word & 0x8000) != 0)
                throw new InvalidDataException(
                    $"Kraid {source} color {index} has an unrepresentable high bit.");
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }

    private static byte[] ExtractCrocomireMeltTilemap(ISnesAddressSpace bus,
        int sourceAddress)
    {
        var cells = new CrocomireMeltingTilemapCell[
            CrocomireMeltingArtworkFormat.TilemapCellCount];
        for (int index = 0; index < cells.Length; index++)
        {
            int address = sourceAddress + index * 2;
            ushort raw = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
            var word = new SnesBgTilemapWord(raw);
            cells[index] = new CrocomireMeltingTilemapCell
            {
                TileIndex = word.CharacterIndex,
                Palette = word.PaletteIndex,
                Priority = word.HasPriority,
                FlipX = word.FlipHorizontally,
                FlipY = word.FlipVertically,
            };
        }
        int terminator = sourceAddress + cells.Length * 2;
        if ((ushort)(bus.ReadByte(terminator) | bus.ReadByte(terminator + 1) << 8) != 0xffff)
            throw new InvalidDataException(
                $"Crocomire melt tilemap ${sourceAddress:X6} lacks its native terminator.");
        using var json = new MemoryStream();
        CrocomireMeltingArtwork.WriteTilemap(json, new CrocomireMeltingTilemapDocument
        {
            Version = CrocomireMeltingArtworkFormat.TilemapVersion,
            Width = CrocomireMeltingArtworkFormat.TilemapWidth,
            Height = CrocomireMeltingArtworkFormat.TilemapHeight,
            Cells = cells,
        });
        return json.ToArray();
    }

    private sealed record EnemyTileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<ushort, EnemyTileFileEntry> Entries,
        string CrocomireFirstSha256, string CrocomireSecondSha256,
        string CrocomireFirstTilemapSha256, string CrocomireSecondTilemapSha256,
        string EnemyCompositionsSha256, string EnemyExtendedCompositionsSha256,
        string KraidUpperSha256, string KraidLowerSha256,
        Dictionary<ushort, string> KraidHeadsSha256,
        string KraidRoomBackgroundSha256,
        string KraidColorsSha256);

    private sealed record EnemyTileFileEntry(int NativeByteCount, string Sha256, string PaletteSha256);
}
