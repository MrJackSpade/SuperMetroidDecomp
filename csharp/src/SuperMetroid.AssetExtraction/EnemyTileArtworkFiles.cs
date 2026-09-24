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
        var manifest = new EnemyTileManifest(EnemyTileArtworkFormat.Version,
            sourceCartridgeSha256, entries,
            Convert.ToHexString(SHA256.HashData(firstMelt)),
            Convert.ToHexString(SHA256.HashData(secondMelt)),
            Convert.ToHexString(SHA256.HashData(firstMeltTilemap)),
            Convert.ToHexString(SHA256.HashData(secondMeltTilemap)));
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
            string.IsNullOrWhiteSpace(manifest.CrocomireSecondTilemapSha256))
            throw new InvalidDataException($"Enemy tile manifest {manifestPath} does not describe this installation.");
        ValidateDefinitionIds(manifest.Entries.Keys);

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
        return new EnemyTileArtworkCatalog(sheets, palettes, crocomire);

        byte[] ReadCrocomireAsset(string fileName, string expectedSha256)
        {
            string stockPath = Path.Combine(stockDirectory, fileName);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), expectedSha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock Crocomire melt asset {stockPath} failed its manifest hash.");
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
        string CrocomireFirstTilemapSha256, string CrocomireSecondTilemapSha256);

    private sealed record EnemyTileFileEntry(int NativeByteCount, string Sha256, string PaletteSha256);
}
