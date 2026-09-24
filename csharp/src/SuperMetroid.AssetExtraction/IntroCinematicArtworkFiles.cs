using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs opening-scene indexed PNGs separately from persistent player overrides.</summary>
public static class IntroCinematicArtworkFiles
{
    private const int FormatVersion = 2;

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);

        var hashes = new Dictionary<string, string>();
        WriteSheet(IntroCinematicArtworkFormat.BackgroundFileName,
            RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.BackgroundCharacters,
                maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundByteCount),
            IntroCinematicArtworkFormat.BackgroundByteCount);
        WriteSheet(IntroCinematicArtworkFormat.IntroObjectFileName,
            RomDataReader.ReadFixedBank(bus, IntroCinematicRomData.Assets.IntroObjectCharacters,
                IntroCinematicArtworkFormat.IntroObjectByteCount),
            IntroCinematicArtworkFormat.IntroObjectByteCount);
        WriteSheet(IntroCinematicArtworkFormat.CinematicObjectFileName,
            RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.ObjectCharacters,
                maximumOutputBytes: IntroCinematicArtworkFormat.CinematicObjectByteCount),
            IntroCinematicArtworkFormat.CinematicObjectByteCount);

        var manifest = new IntroCinematicArtworkManifest(FormatVersion, sourceCartridgeSha256, hashes);
        using var stream = new FileStream(Path.Combine(directory, IntroCinematicArtworkFormat.ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(stream, manifest, JsonOptions);
        return;

        void WriteSheet(string name, byte[] planar, int expectedByteCount)
        {
            if (planar.Length != expectedByteCount)
                throw new InvalidDataException(
                    $"Intro sheet {name} has {planar.Length} tile bytes, expected {expectedByteCount}.");
            byte[] indexes = SnesGraphics.DecodePlanarTiles(planar, 4,
                IntroCinematicArtworkFormat.TileColumns, out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, indexes, SnesGraphics.DiagnosticPalette(16));
            byte[] encoded = png.ToArray();
            RoomCharacterAtlas roundTrip = RoomCharacterAtlas.Load(
                new MemoryStream(encoded, writable: false), expectedByteCount);
            if (!roundTrip.Transfer.Span.SequenceEqual(planar))
                throw new InvalidDataException($"Intro PNG {name} did not round-trip its cartridge tiles.");
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }
    }

    /// <summary>Checks every stock hash before selecting independently editable PNGs.</summary>
    public static IntroCinematicArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, IntroCinematicArtworkFormat.ManifestFileName);
        IntroCinematicArtworkManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<IntroCinematicArtworkManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Intro artwork manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid intro artwork manifest {manifestPath}.", error);
        }
        string[] names =
        [
            IntroCinematicArtworkFormat.BackgroundFileName,
            IntroCinematicArtworkFormat.IntroObjectFileName,
            IntroCinematicArtworkFormat.CinematicObjectFileName,
        ];
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.StockSha256 is null || manifest.StockSha256.Count != names.Length ||
            names.Any(name => !manifest.StockSha256.ContainsKey(name)))
            throw new InvalidDataException($"Intro artwork manifest {manifestPath} does not describe this installation.");

        return new IntroCinematicArtworkCatalog(
            LoadSheet(names[0], IntroCinematicArtworkFormat.BackgroundByteCount),
            LoadSheet(names[1], IntroCinematicArtworkFormat.IntroObjectByteCount),
            LoadSheet(names[2], IntroCinematicArtworkFormat.CinematicObjectByteCount));

        RoomCharacterAtlas LoadSheet(string name, int nativeByteCount)
        {
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), manifest.StockSha256[name],
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock intro PNG {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            byte[] selected = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
            try
            {
                return RoomCharacterAtlas.Load(new MemoryStream(selected, writable: false), nativeByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid intro PNG {selectedPath}: {error.Message}", error);
            }
        }
    }

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record IntroCinematicArtworkManifest(
        int Version, string SourceCartridgeSha256, Dictionary<string, string> StockSha256);
}
