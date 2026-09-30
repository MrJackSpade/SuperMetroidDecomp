using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
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
                    bus.ReadCartridgeByte(segments[segment].SourceAddress + offset);
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
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(stockDirectory, SamusDeathTileAtlasFormat.ManifestFileName));
        ArtworkManifest manifest = manifestFile.Json<ArtworkManifest>(JsonOptions);
        return manifestFile.WithContext(() => LoadArtwork());

        SamusDeathTileAtlas LoadArtwork()
        {
            if (manifest.Version != FormatVersion ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Samus death-tile manifest does not match the pinned cartridge.");
            SamusArtworkFile stock = SamusArtworkFile.Stock(Path.Combine(stockDirectory,
                SamusDeathTileAtlasFormat.ArtworkFileName), manifest.ArtworkSha256);
            _ = stock.Compile(SamusDeathTileAtlas.Load);
            return stock.Select(overrideDirectory).Compile(SamusDeathTileAtlas.Load);
        }
    }

    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256, string ArtworkSha256);
}
