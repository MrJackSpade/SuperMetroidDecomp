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

    /// <summary>Exports the five $0400-byte death-explosion character transfers as a 64-by-160 indexed PNG and validates the written stock installation.</summary>
    /// <param name="bus">Cartridge source for bank-$9B pages $8400, $8800, $8C00, $9000, and $8000 in native queue order.</param>
    /// <param name="directory">Artwork directory, created if absent; existing PNG and companion manifest are overwritten.</param>
    /// <param name="sourceCartridgeSha256">Caller-supplied provenance recorded verbatim; post-write loading requires the supported cartridge hash.</param>
    /// <remarks>Pixel indices represent four-bit OBJ pens, not runtime colors. Writes are separate, with no rollback if later file/stock validation fails; animation timing and OAM placement are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Transfer coverage, provenance, or generated indexed artwork is incompatible.</exception>
    /// <exception cref="IOException">Filesystem output or post-write input fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        var planar = new byte[SamusDeathTileAtlasFormat.TotalByteCount];
        var segments = SamusSpecialSequenceRomData.Death.TileSegments;
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

    /// <summary>Checks stock death-explosion provenance, hash, and PNG admission before selecting an optional replacement atlas.</summary>
    /// <param name="stockDirectory">Artwork directory containing the required death-explosion PNG and version-one manifest.</param>
    /// <param name="overrideDirectory">Optional artwork directory; null or an absent replacement PNG selects validated stock.</param>
    /// <returns>A ROM-independent atlas for all five fixed transfers, retaining selected character pixels without changing destinations, palettes, or sequence timing.</returns>
    /// <remarks>Overrides need no manifest but must decode as the same 64-by-160 four-bit character sheet. Required stock is validated even when replaced.</remarks>
    /// <exception cref="InvalidDataException">Manifest provenance/version, stock hash, or selected PNG dimensions/indices are invalid; admission errors include the source filename.</exception>
    /// <exception cref="IOException">A required stock or selected override file cannot be read.</exception>
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
