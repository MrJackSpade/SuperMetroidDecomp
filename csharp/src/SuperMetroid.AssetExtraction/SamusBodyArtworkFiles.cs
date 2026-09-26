using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Samus's split body DMA into indexed PNG atlases and editable selector JSON.</summary>
/// <remarks>
/// Each definition occupies sixteen 4-bpp tiles (64 by 16 pixels) in its set's PNG.
/// Unused tiles are zero. Native transfer sizes and pose/frame choices live in JSON;
/// no game mechanics or animation delay bytes are represented here.
/// </remarks>
public static class SamusBodyArtworkFiles
{
    public const string ManifestFileName = "samus-body.json";
    private const int FormatVersion = 1;
    private const int TileWidth = 64;
    private const int DefinitionHeight = 16;
    private const int DefinitionEndExclusive = 0xD7D3;

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
        Directory.CreateDirectory(directory);
        ushort[] topPointers = ReadPointers(bus,
            SamusRenderingRomData.TileTransfers.TopDefinitionListPointers,
            SamusBodyArtworkCatalog.TopSetCount);
        ushort[] bottomPointers = ReadPointers(bus,
            SamusRenderingRomData.TileTransfers.BottomDefinitionListPointers,
            SamusBodyArtworkCatalog.BottomSetCount);
        ushort[] posePointers = ReadPointers(bus,
            SamusRenderingRomData.TileTransfers.AnimationDefinitionListPointers,
            SamusBodyArtworkCatalog.PoseCount);
        var frames = new SamusBodyFrameSelection[SamusBodyArtworkCatalog.FrameCount];
        for (int index = 0; index < frames.Length; index++)
        {
            int address = SamusBodyArtworkCatalog.FirstFrameAddress + index * 4;
            frames[index] = new SamusBodyFrameSelection(bus.ReadByte(address),
                bus.ReadByte(address + 1), bus.ReadByte(address + 2), bus.ReadByte(address + 3));
        }

        ushort[] allPointers = topPointers.Concat(bottomPointers).OrderBy(value => value).ToArray();
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        DefinitionEntry[][] top = ExtractHalf(bus, directory, topPointers, allPointers, true, hashes);
        DefinitionEntry[][] bottom = ExtractHalf(bus, directory, bottomPointers, allPointers, false, hashes);
        var manifest = new Manifest(FormatVersion, sourceCartridgeSha256,
            topPointers, bottomPointers, posePointers, frames, top, bottom, hashes);
        // Constructing the catalog catches missing/invalid references before publication.
        _ = BuildCatalog(directory, manifest, null);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
    }

    public static SamusBodyArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        Manifest stock = ReadManifest(Path.Combine(stockDirectory, ManifestFileName));
        ValidateManifest(stock);
        // Always verify installed stock first; a replacement never conceals corruption.
        _ = BuildCatalog(stockDirectory, stock, null);
        string? overrideManifest = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, ManifestFileName);
        Manifest selected = overrideManifest is not null && File.Exists(overrideManifest)
            ? ReadManifest(overrideManifest) : stock;
        ValidateManifest(selected);
        if (selected.Hashes.Count != stock.Hashes.Count ||
            stock.Hashes.Any(entry => !selected.Hashes.TryGetValue(entry.Key, out string? hash) ||
                !string.Equals(hash, entry.Value, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("Samus body override must retain stock PNG provenance hashes.");
        if (selected.Top.Length != stock.Top.Length || selected.Bottom.Length != stock.Bottom.Length ||
            !selected.TopPointers.AsSpan().SequenceEqual(stock.TopPointers) ||
            !selected.BottomPointers.AsSpan().SequenceEqual(stock.BottomPointers) ||
            selected.Top.Where((set, i) => set.Length != stock.Top[i].Length).Any() ||
            selected.Bottom.Where((set, i) => set.Length != stock.Bottom[i].Length).Any())
            throw new InvalidDataException("Samus body override changes native definition identities.");
        for (int set = 0; set < stock.Top.Length; set++)
        for (int position = 0; position < stock.Top[set].Length; position++)
            if (selected.Top[set][position].SourceAddress != stock.Top[set][position].SourceAddress)
                throw new InvalidDataException("Samus body override changes a native top source identity.");
        for (int set = 0; set < stock.Bottom.Length; set++)
        for (int position = 0; position < stock.Bottom[set].Length; position++)
            if (selected.Bottom[set][position].SourceAddress != stock.Bottom[set][position].SourceAddress)
                throw new InvalidDataException("Samus body override changes a native bottom source identity.");
        return BuildCatalog(stockDirectory, selected, overrideDirectory);
    }

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static DefinitionEntry[][] ExtractHalf(ISnesAddressSpace bus, string directory,
        ushort[] pointers, ushort[] sortedPointers, bool upperHalf,
        Dictionary<string, string> hashes)
    {
        var sets = new DefinitionEntry[pointers.Length][];
        for (int set = 0; set < pointers.Length; set++)
        {
            int start = pointers[set];
            int order = Array.IndexOf(sortedPointers, pointers[set]);
            int end = order + 1 == sortedPointers.Length ? DefinitionEndExclusive : sortedPointers[order + 1];
            if (start < 0x8000 || end <= start || (end - start) % 7 != 0)
                throw new InvalidDataException($"Samus body set {set:X2} has invalid native bounds.");
            int count = (end - start) / 7;
            byte[] planar = new byte[count * SamusBodyArtworkCatalog.BytesPerDefinitionSlot];
            sets[set] = new DefinitionEntry[count];
            for (int position = 0; position < count; position++)
            {
                int definitionAddress = 0x920000 | (start + position * 7);
                ushort sourceOffset = ReadWord(bus, definitionAddress);
                byte sourceBank = bus.ReadByte(definitionAddress + 2);
                ushort firstSize = ReadWord(bus, definitionAddress + 3);
                ushort secondSize = ReadWord(bus, definitionAddress + 5);
                int byteCount = firstSize + secondSize;
                if (sourceOffset < 0x8000 || firstSize == 0 || byteCount >
                    SamusBodyArtworkCatalog.BytesPerDefinitionSlot || byteCount % 32 != 0 ||
                    sourceOffset + byteCount > 0x10000)
                    throw new InvalidDataException($"Invalid Samus body DMA at ${definitionAddress:X6}.");
                int sourceAddress = sourceBank << 16 | sourceOffset;
                for (int i = 0; i < byteCount; i++)
                    planar[position * SamusBodyArtworkCatalog.BytesPerDefinitionSlot + i] =
                        bus.ReadByte(sourceAddress + i);
                sets[set][position] = new DefinitionEntry(sourceAddress, firstSize, secondSize);
            }
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, TileWidth / 8,
                out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
            string name = FileName(upperHalf, set);
            byte[] bytes = png.ToArray();
            File.WriteAllBytes(Path.Combine(directory, name), bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        return sets;
    }

    private static SamusBodyArtworkCatalog BuildCatalog(string stockDirectory, Manifest manifest,
        string? overrideDirectory)
    {
        SamusBodyTileDefinition[][] top = LoadHalf(stockDirectory, overrideDirectory,
            manifest, true);
        SamusBodyTileDefinition[][] bottom = LoadHalf(stockDirectory, overrideDirectory,
            manifest, false);
        return new SamusBodyArtworkCatalog(manifest.TopPointers, manifest.BottomPointers,
            manifest.PosePointers, manifest.Frames, top, bottom);
    }

    private static SamusBodyTileDefinition[][] LoadHalf(string stockDirectory,
        string? overrideDirectory, Manifest manifest, bool upperHalf)
    {
        DefinitionEntry[][] metadata = upperHalf ? manifest.Top : manifest.Bottom;
        var result = new SamusBodyTileDefinition[metadata.Length][];
        for (int set = 0; set < metadata.Length; set++)
        {
            string name = FileName(upperHalf, set);
            byte[] stockPng = File.ReadAllBytes(Path.Combine(stockDirectory, name));
            if (!manifest.Hashes.TryGetValue(name, out string? expectedHash) ||
                !string.Equals(expectedHash, Convert.ToHexString(SHA256.HashData(stockPng)),
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock Samus body PNG {name} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            byte[] selected = overridePath is not null && File.Exists(overridePath)
                ? File.ReadAllBytes(overridePath) : stockPng;
            int height = checked(metadata[set].Length * DefinitionHeight);
            IndexedPngImage image = IndexedPng.Read(new MemoryStream(selected, false), TileWidth, height);
            byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, TileWidth, height, 4);
            result[set] = new SamusBodyTileDefinition[metadata[set].Length];
            for (int position = 0; position < result[set].Length; position++)
            {
                DefinitionEntry entry = metadata[set][position];
                int byteCount = entry.FirstSize + entry.SecondSize;
                if (entry.FirstSize == 0 || byteCount > SamusBodyArtworkCatalog.BytesPerDefinitionSlot ||
                    byteCount % 32 != 0)
                    throw new InvalidDataException($"Samus body definition {name}/{position} has invalid sizes.");
                ReadOnlySpan<byte> slot = planar.AsSpan(
                    position * SamusBodyArtworkCatalog.BytesPerDefinitionSlot,
                    SamusBodyArtworkCatalog.BytesPerDefinitionSlot);
                if (slot[byteCount..].IndexOfAnyExcept((byte)0) >= 0)
                    throw new InvalidDataException($"Samus body PNG {name} paints unused tiles in definition {position}.");
                result[set][position] = new SamusBodyTileDefinition(
                    entry.SourceAddress, entry.FirstSize, entry.SecondSize,
                    slot[..byteCount].ToArray());
            }
        }
        return result;
    }

    private static void ValidateManifest(Manifest manifest)
    {
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) || manifest.TopPointers is null ||
            manifest.BottomPointers is null || manifest.PosePointers is null ||
            manifest.Frames is null || manifest.Top is null || manifest.Bottom is null ||
            manifest.Hashes is null || manifest.Top.Length != SamusBodyArtworkCatalog.TopSetCount ||
            manifest.Bottom.Length != SamusBodyArtworkCatalog.BottomSetCount ||
            manifest.Top.Any(set => set is null || set.Length == 0) ||
            manifest.Bottom.Any(set => set is null || set.Length == 0))
            throw new InvalidDataException("Samus body manifest does not match this installation.");
    }

    private static Manifest ReadManifest(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(path), JsonOptions) ??
                throw new InvalidDataException($"Samus body manifest {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid Samus body manifest {path}.", error);
        }
    }

    private static ushort[] ReadPointers(ISnesAddressSpace bus, int start, int count) =>
        Enumerable.Range(0, count).Select(i => ReadWord(bus, start + i * 2)).ToArray();

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static string FileName(bool upperHalf, int set) =>
        $"{(upperHalf ? "top" : "bottom")}-{set:X2}.png";

    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        ushort[] TopPointers, ushort[] BottomPointers, ushort[] PosePointers,
        SamusBodyFrameSelection[] Frames, DefinitionEntry[][] Top,
        DefinitionEntry[][] Bottom, Dictionary<string, string> Hashes);

    private sealed record DefinitionEntry(int SourceAddress, ushort FirstSize, ushort SecondSize);
}
