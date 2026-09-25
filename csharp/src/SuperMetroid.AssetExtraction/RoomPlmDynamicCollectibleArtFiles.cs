using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs one indexed 64x8 PNG per dynamic permanent item plus editable tile
/// palette offsets. PNG tile order is the native eight-tile upload order:
/// frame zero uses tiles 0-3 and frame one uses tiles 4-7.
/// </summary>
public static class RoomPlmDynamicCollectibleArtFiles
{
    public const string PaletteFileName = "palettes.json";
    public const string ManifestFileName = "manifest.json";
    private const int FormatVersion = 1;
    private const int Width = 64;
    private const int Height = 8;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Stable PNG name for an item kind, independent of room placement.</summary>
    public static string FileName(InWorldCollectibleKind kind)
    {
        if (kind < InWorldCollectibleKind.Bombs ||
            kind > InWorldCollectibleKind.ReserveTank)
            throw new ArgumentOutOfRangeException(nameof(kind));
        return $"item-{(int)kind:D2}-{kind.ToString().ToLowerInvariant()}.png";
    }

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        var palettes = new List<PaletteEntry>();
        foreach (RoomPlmDynamicCollectibleGraphic graphic in
                 RoomPlmDynamicCollectibleGraphicsDefinitions.All)
        {
            for (int offset = 0; offset < graphic.Tiles.Length; offset++)
            {
                if (bus.ReadByte(0x890000 | (graphic.GraphicsPointer + offset)) !=
                    graphic.Tiles.Span[offset])
                    throw new InvalidDataException(
                        $"Native item {graphic.Kind} tile {offset} differs from the compiled definition.");
            }
            byte[] pixels = SnesGraphics.DecodePlanarTiles(graphic.Tiles.Span,
                4, Width / 8, out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels,
                SnesGraphics.DiagnosticPalette(16));
            byte[] encoded = png.ToArray();
            if (!ReadTiles(encoded, FileName(graphic.Kind)).AsSpan()
                    .SequenceEqual(graphic.Tiles.Span))
                throw new InvalidDataException(
                    $"Item {graphic.Kind} PNG roundtrip changed cartridge tiles.");
            string name = FileName(graphic.Kind);
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Sha(encoded));
            palettes.Add(new PaletteEntry(graphic.Kind.ToString(),
                graphic.PaletteOffsets.Span.ToArray()
                    .Select(offset => (int)offset).ToArray()));
        }
        byte[] paletteJson = JsonSerializer.SerializeToUtf8Bytes(
            new PaletteDocument(FormatVersion, palettes.ToArray()), JsonOptions);
        using (var output = new FileStream(Path.Combine(directory, PaletteFileName),
                   FileMode.CreateNew, FileAccess.Write))
            output.Write(paletteJson);
        hashes.Add(PaletteFileName, Sha(paletteJson));
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new ArtManifest(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions);
    }

    public static RoomPlmDynamicCollectibleArtCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        ArtManifest manifest = ReadJson<ArtManifest>(File.ReadAllBytes(manifestPath),
            manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256,
                SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.Hashes is null ||
            manifest.Hashes.Count !=
                RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount + 1)
            throw new InvalidDataException(
                $"Item artwork manifest {manifestPath} is incompatible or incomplete.");

        byte[] stockPaletteBytes = ReadStock(stockDirectory, manifest,
            PaletteFileName);
        PaletteDocument stockPalettes = ReadJson<PaletteDocument>(stockPaletteBytes,
            Path.Combine(stockDirectory, PaletteFileName));
        byte[][] stockOffsets = ParsePalettes(stockPalettes);
        for (int index = 0; index < stockOffsets.Length; index++)
        {
            InWorldCollectibleKind kind = (InWorldCollectibleKind)(
                RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind + index);
            if (!stockOffsets[index].AsSpan().SequenceEqual(
                    RoomPlmDynamicCollectibleGraphicsDefinitions.Get(kind)
                        .PaletteOffsets.Span))
                throw new InvalidDataException(
                    $"Stock item palette offsets for {kind} differ from compiled cartridge data.");
        }

        string? paletteOverride = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, PaletteFileName);
        byte[][] selectedOffsets = paletteOverride is not null &&
            File.Exists(paletteOverride)
            ? ParsePalettes(ReadJson<PaletteDocument>(
                File.ReadAllBytes(paletteOverride), paletteOverride))
            : stockOffsets;
        var entries = new List<RoomPlmDynamicCollectibleArtEntry>();
        foreach (RoomPlmDynamicCollectibleGraphic graphic in
                 RoomPlmDynamicCollectibleGraphicsDefinitions.All)
        {
            string name = FileName(graphic.Kind);
            byte[] stockTiles = ReadTiles(ReadStock(stockDirectory, manifest, name),
                Path.Combine(stockDirectory, name));
            if (!stockTiles.AsSpan().SequenceEqual(graphic.Tiles.Span))
                throw new InvalidDataException(
                    $"Stock item PNG {name} differs from compiled cartridge art.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            byte[] selectedTiles = overridePath is not null && File.Exists(overridePath)
                ? ReadTiles(File.ReadAllBytes(overridePath), overridePath)
                : stockTiles;
            int index = (int)graphic.Kind -
                RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind;
            entries.Add(new RoomPlmDynamicCollectibleArtEntry(graphic.Kind,
                selectedTiles, selectedOffsets[index]));
        }
        return new RoomPlmDynamicCollectibleArtCatalog(entries);
    }

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static byte[] ReadStock(string directory, ArtManifest manifest,
        string name)
    {
        string path = Path.Combine(directory, name);
        byte[] bytes = File.ReadAllBytes(path);
        if (!manifest.Hashes.TryGetValue(name, out string? expected) ||
            !string.Equals(expected, Sha(bytes), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Stock item artwork {path} failed its manifest hash.");
        return bytes;
    }

    private static byte[] ReadTiles(byte[] png, string path)
    {
        try
        {
            IndexedPngImage image = IndexedPng.Read(
                new MemoryStream(png, writable: false), Width, Height);
            return SnesPlanarTileEncoder.Encode(image.Pixels,
                image.Width, image.Height, 4);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid item PNG {path}: {error.Message}",
                error);
        }
    }

    private static byte[][] ParsePalettes(PaletteDocument document)
    {
        if (document.Version != FormatVersion || document.Entries is null ||
            document.Entries.Length !=
                RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount)
            throw new InvalidDataException(
                "Item palette document has an incompatible version or entry count.");
        var result = new byte[document.Entries.Length][];
        var seen = new bool[result.Length];
        foreach (PaletteEntry entry in document.Entries)
        {
            if (entry is null ||
                !Enum.TryParse(entry.Id, ignoreCase: false,
                    out InWorldCollectibleKind kind) ||
                !string.Equals(entry.Id, kind.ToString(), StringComparison.Ordinal))
                throw new InvalidDataException(
                    "Item palette document has an unknown item kind.");
            int index = (int)kind -
                RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind;
            if ((uint)index >= result.Length || seen[index] ||
                entry.Offsets is not { Length: 8 } ||
                entry.Offsets.Any(offset => offset is < 0 or > 7))
                throw new InvalidDataException(
                    $"Item palette entry {entry.Id} is duplicate or invalid.");
            result[index] = entry.Offsets.Select(offset => (byte)offset).ToArray();
            seen[index] = true;
        }
        if (seen.Any(present => !present))
            throw new InvalidDataException("Item palette document is incomplete.");
        return result;
    }

    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Item artwork JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid item artwork JSON {path}.", error);
        }
    }

    private static string Sha(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private sealed record ArtManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> Hashes);
    private sealed record PaletteDocument(int Version, PaletteEntry[] Entries);
    private sealed record PaletteEntry(string Id, int[] Offsets);
}
