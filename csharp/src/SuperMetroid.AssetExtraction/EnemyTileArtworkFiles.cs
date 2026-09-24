using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
                entries.Add(definitionPointer, new EnemyTileFileEntry(
                    byteCount, Convert.ToHexString(SHA256.HashData(encoded))));
            }
        }
        if (entries.Count != EnemyTileArtworkFormat.RetailDefinitionCount)
            throw new InvalidDataException($"Expected {EnemyTileArtworkFormat.RetailDefinitionCount} retail enemy sheets; found {entries.Count}.");
        ValidateDefinitionIds(entries.Keys);
        var manifest = new EnemyTileManifest(EnemyTileArtworkFormat.Version,
            sourceCartridgeSha256, entries);
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
            manifest.Entries.Count != EnemyTileArtworkFormat.RetailDefinitionCount)
            throw new InvalidDataException($"Enemy tile manifest {manifestPath} does not describe this installation.");
        ValidateDefinitionIds(manifest.Entries.Keys);

        var sheets = new Dictionary<ushort, RoomCharacterAtlas>();
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
        }
        return new EnemyTileArtworkCatalog(sheets);
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

    private sealed record EnemyTileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<ushort, EnemyTileFileEntry> Entries);

    private sealed record EnemyTileFileEntry(int NativeByteCount, string Sha256);
}
