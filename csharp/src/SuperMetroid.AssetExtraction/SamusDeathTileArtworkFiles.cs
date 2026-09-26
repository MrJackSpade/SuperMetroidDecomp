using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the five native death graphics transfers into one indexed PNG.</summary>
public static class SamusDeathTileArtworkFiles
{
    private const int FormatVersion = 1;
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
        var planar = new byte[SamusDeathTileAtlasFormat.TotalByteCount];
        ReadOnlySpan<SamusDeathTileSegment> segments = SamusSpecialSequenceRomData.Death.TileSegments;
        if (segments.Length != SamusDeathTileAtlasFormat.SegmentCount)
            throw new InvalidDataException("Samus death atlas does not cover every native tile transfer.");
        for (int segment = 0; segment < segments.Length; segment++)
        for (int offset = 0; offset < SamusSpecialSequenceRomData.Death.TileSegmentByteCount; offset++)
            planar[segment * SamusSpecialSequenceRomData.Death.TileSegmentByteCount + offset] =
                bus.ReadByte(segments[segment].SourceAddress + offset);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,
            SamusDeathTileAtlasFormat.BitsPerPixel, SamusDeathTileAtlasFormat.Width / 8,
            out int width, out int height);
        using var output = new MemoryStream();
        IndexedPng.Write(output, width, height, pixels,
            SnesGraphics.DiagnosticPalette(1 << SamusDeathTileAtlasFormat.BitsPerPixel));
        byte[] png = output.ToArray();
        File.WriteAllBytes(Path.Combine(directory, SamusDeathTileAtlasFormat.ArtworkFileName), png);
        File.WriteAllBytes(Path.Combine(directory, SamusDeathTileAtlasFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new ArtworkManifest(FormatVersion,
                sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(png))), JsonOptions));
        _ = Load(directory, null);
    }

    public static SamusDeathTileAtlas Load(string stockDirectory, string? overrideDirectory)
    {
        ArtworkManifest manifest = ReadManifest(Path.Combine(stockDirectory,
            SamusDeathTileAtlasFormat.ManifestFileName));
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Samus death-tile manifest does not match the pinned cartridge.");
        byte[] stock = File.ReadAllBytes(Path.Combine(stockDirectory,
            SamusDeathTileAtlasFormat.ArtworkFileName));
        if (!string.Equals(manifest.ArtworkSha256, Convert.ToHexString(SHA256.HashData(stock)),
            StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Stock Samus death-tile PNG failed its manifest hash.");
        _ = SamusDeathTileAtlas.Load(new MemoryStream(stock, writable: false));
        string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory,
            SamusDeathTileAtlasFormat.ArtworkFileName);
        byte[] selected = overridePath is not null && File.Exists(overridePath)
            ? File.ReadAllBytes(overridePath) : stock;
        return SamusDeathTileAtlas.Load(new MemoryStream(selected, writable: false));
    }

    private static ArtworkManifest ReadManifest(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<ArtworkManifest>(File.ReadAllBytes(path), JsonOptions) ??
                throw new InvalidDataException($"Samus death-tile manifest {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid Samus death-tile manifest {path}.", error);
        }
    }

    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256, string ArtworkSha256);
}
