using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports Samus's death-explosion characters into one indexed PNG.</summary>
public static class SamusDeathTileArtworkFiles
{
    /// <summary>Current provenance manifest revision for the death-explosion characters.</summary>
    private const int FormatVersion = 2;
    /// <summary>Reads strict camel-case manifest JSON and rejects unknown fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Exports the five 32-character death-explosion pages as a 64-by-160 indexed PNG.</summary>
    /// <param name="bus">Cartridge source for the bank-$9B explosion characters, written in the atlas's page order.</param>
    /// <param name="directory">Artwork directory, created if absent; an existing companion manifest is overwritten.</param>
    /// <param name="importData">The import-data directory receiving the page (#1636), which only the import reads.</param>
    /// <param name="sourceCartridgeSha256">Caller-supplied provenance recorded verbatim.</param>
    /// <remarks>Pixel indices represent four-bit OBJ pens, not runtime colors. Writes are separate, with no rollback if a later write fails; animation timing and OAM placement are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is null, empty, or whitespace.</exception>
    /// <exception cref="IOException">Filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string importData, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(importData);
        var planar = new byte[SamusDeathTileAtlasFormat.TotalByteCount];
        for (int index = 0; index < planar.Length; index++)
            planar[index] = bus.ReadCartridgeByte(SamusSourceDefinitions.DeathExplosionCharacters.Value +
                SamusDeathTileAtlasFormat.ExplosionByte(index));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,
            SamusDeathTileAtlasFormat.BitsPerPixel, SamusDeathTileAtlasFormat.Width / 8,
            out Extent16<ImagePixels> size);
        using var output = new MemoryStream();
        IndexedPng.Write(output, size, pixels,
            SnesGraphics.DiagnosticPalette(1 << SamusDeathTileAtlasFormat.BitsPerPixel));
        byte[] png = output.ToArray();
        File.WriteAllBytes(Path.Combine(importData, SamusDeathTileAtlasFormat.ArtworkFileName), png);
        File.WriteAllBytes(Path.Combine(directory, SamusDeathTileAtlasFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new ArtworkManifest(FormatVersion,
                sourceCartridgeSha256), JsonOptions));
    }

    /// <summary>Checks the death-explosion manifest's provenance and loads the import's atlas (#1636).</summary>
    /// <param name="directory">Artwork directory containing the death-explosion manifest.</param>
    /// <param name="importData">The component's import-data directory, holding the death-explosion page.</param>
    /// <returns>A ROM-independent atlas of the explosion characters, retaining the installed character pixels without changing palettes or sequence timing.</returns>
    /// <remarks>The PNG must decode as the 64-by-160 four-bit character sheet.</remarks>
    /// <exception cref="InvalidDataException">Manifest provenance/version or PNG dimensions/indices are invalid; admission errors include the source filename.</exception>
    /// <exception cref="IOException">A required installed file cannot be read.</exception>
    public static SamusDeathTileAtlas LoadForImport(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(directory, SamusDeathTileAtlasFormat.ManifestFileName));
        ArtworkManifest manifest = manifestFile.Json<ArtworkManifest>(JsonOptions);
        return manifestFile.WithContext(() => LoadArtwork());

        SamusDeathTileAtlas LoadArtwork()
        {
            if (manifest.Version != FormatVersion ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Samus death-tile manifest does not match the pinned cartridge.");
            return SamusArtworkFile.Read(Path.Combine(importData, SamusDeathTileAtlasFormat.ArtworkFileName))
                .Compile(SamusDeathTileAtlas.Load);
        }
    }

    /// <summary>Binds the extracted PNG hash to its source cartridge and manifest revision.</summary>
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256);
}
