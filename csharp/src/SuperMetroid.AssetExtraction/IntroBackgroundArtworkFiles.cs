using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs stock intro artwork separately from persistent player PNG overrides.</summary>
public static class IntroBackgroundArtworkFiles
{
    public const string ManifestFileName = "intro-background.json";
    private const int FormatVersion = 1;

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);

        byte[] planar = RomDataReader.Decompress(bus,
            IntroCinematicRomData.Assets.BackgroundCharacters,
            maximumOutputBytes: IntroBackgroundAtlasFormat.NativeByteCount);
        if (planar.Length != IntroBackgroundAtlasFormat.NativeByteCount)
            throw new InvalidDataException(
                $"Intro background has {planar.Length} tile bytes, expected {IntroBackgroundAtlasFormat.NativeByteCount}.");
        byte[] indexes = SnesGraphics.DecodePlanarTiles(planar, 4,
            IntroBackgroundAtlasFormat.TileColumns, out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, indexes, SnesGraphics.DiagnosticPalette(16));
        byte[] encoded = png.ToArray();
        IntroBackgroundAtlas roundTrip = IntroBackgroundAtlas.Load(new MemoryStream(encoded, writable: false));
        if (!roundTrip.Transfer.Span.SequenceEqual(planar))
            throw new InvalidDataException("Intro background PNG did not round-trip its cartridge tiles.");

        using (var output = new FileStream(Path.Combine(directory, IntroBackgroundAtlasFormat.FileName),
                   FileMode.CreateNew, FileAccess.Write))
            output.Write(encoded);
        var manifest = new IntroBackgroundManifest(
            FormatVersion, sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(encoded)));
        using var stream = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(stream, manifest, JsonOptions);
    }

    /// <summary>Validates stock provenance before selecting an independently editable PNG.</summary>
    public static IntroBackgroundAtlas Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        IntroBackgroundManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<IntroBackgroundManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Intro background manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid intro background manifest {manifestPath}.", error);
        }
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Intro background manifest {manifestPath} does not describe this installation.");

        string stockPath = Path.Combine(stockDirectory, IntroBackgroundAtlasFormat.FileName);
        byte[] stock = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), manifest.StockSha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock intro background PNG {stockPath} failed its manifest hash.");
        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, IntroBackgroundAtlasFormat.FileName);
        string selectedPath = overridePath is not null && File.Exists(overridePath)
            ? overridePath : stockPath;
        byte[] selected = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
        try
        {
            return IntroBackgroundAtlas.Load(new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid intro background PNG {selectedPath}: {error.Message}", error);
        }
    }

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record IntroBackgroundManifest(
        int Version, string SourceCartridgeSha256, string StockSha256);
}
