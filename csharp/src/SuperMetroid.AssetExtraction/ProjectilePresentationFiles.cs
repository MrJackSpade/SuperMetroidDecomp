using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Versioned stock composition provenance, separate from persistent user overrides.</summary>
public static class ProjectilePresentationFiles
{
    /// <summary>Stock projectile provenance manifest filename, separate from persistent override content.</summary>
    public const string ManifestFileName = "projectile-manifest.json";
    /// <summary>Current projectile manifest schema version covering all projectile, beam, flare, trail, and Grapple visual assets.</summary>
    public const int Version = 15;
    /// <summary>Strict camel-case JSON settings for stock projectile provenance.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Writes into the installer's unpublished staging directory after ROM validation.</summary>
    public static void Extract(ISnesAddressSpace validatedBus, string directory)
    {
        byte[] bytes = ProjectileSpriteExtractor.Extract(validatedBus);
        byte[] frameBindings = ProjectileFrameBindingExtractor.Extract(validatedBus);
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, ProjectileSpriteDefinitions.FileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, ProjectileFrameBindingFormat.FileName), frameBindings);
        var beams = BeamTileExtractor.Extract(validatedBus);
        byte[] palettes = BeamPaletteExtractor.Extract(validatedBus);
        byte[] hyperBeamFxColors = HyperBeamFxColorExtractor.Extract(validatedBus);
        byte[] trails = ProjectileTrailExtractor.Extract(validatedBus);
        byte[] trailTiles = ProjectileTrailAtlasExtractor.Extract(validatedBus);
        byte[] flarePlacement = ChargeFlarePlacementExtractor.Extract(validatedBus);
        byte[] flareCompositions = ChargeFlareSpriteExtractor.Extract(validatedBus);
        byte[] grappleTiles = GrappleTileExtractor.Extract(validatedBus);
        byte[] grappleSprites = GrappleSpriteExtractor.Extract(validatedBus);
        byte[] grappleFlare = GrappleFlarePlacementExtractor.Extract(validatedBus);
        byte[] grappleSwing = GrappleSwingFrameExtractor.Extract(validatedBus);
        File.WriteAllBytes(Path.Combine(directory, GrappleSwingFrameDefinitions.FileName), grappleSwing);
        File.WriteAllBytes(Path.Combine(directory, GrappleFlarePlacementDefinitions.FileName), grappleFlare);
        File.WriteAllBytes(Path.Combine(directory, GrappleSpriteDefinitions.FileName), grappleSprites);
        File.WriteAllBytes(Path.Combine(directory, GrappleTileDefinitions.FileName), grappleTiles);
        File.WriteAllBytes(Path.Combine(directory, ChargeFlareSpriteDefinitions.FileName), flareCompositions);
        File.WriteAllBytes(Path.Combine(directory, ChargeFlarePlacementDefinitions.FileName), flarePlacement);
        File.WriteAllBytes(Path.Combine(directory, ProjectileTrailAtlasDefinitions.FileName), trailTiles);
        File.WriteAllBytes(Path.Combine(directory, ProjectileTrailVisualDefinitions.FileName), trails);
        File.WriteAllBytes(Path.Combine(directory, BeamPaletteDefinitions.FileName), palettes);
        File.WriteAllBytes(Path.Combine(directory, HyperBeamFxColorFormat.FileName), hyperBeamFxColors);
        foreach (var file in beams) File.WriteAllBytes(Path.Combine(directory, file.Key), file.Value);
        File.WriteAllText(Path.Combine(directory, ManifestFileName), JsonSerializer.Serialize(
            new Manifest(Version, SupportedCartridge.Sha256, Hash(bytes),
                beams.ToDictionary(pair => pair.Key, pair => Hash(pair.Value)), Hash(palettes), Hash(hyperBeamFxColors), Hash(trails), Hash(trailTiles), Hash(flarePlacement), Hash(flareCompositions), Hash(grappleTiles), Hash(grappleSprites), Hash(grappleFlare), Hash(grappleSwing), Hash(frameBindings)), Options));
        _ = Load(directory, null);
    }

    /// <summary>Validates stock even when overridden. Invalid overrides never fall back to stock.</summary>
    public static InstalledProjectilePresentation Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        Manifest manifest = JsonAssetDocument.Read<Manifest>(manifestStream, Options,
            $"projectile manifest {manifestPath}");
        if (manifest.Version != Version || manifest.RomSha256 != SupportedCartridge.Sha256)
            throw new InvalidDataException($"Projectile manifest {manifestPath} does not match the supported cartridge/revision.");
        ProjectileFile Stock(string name, string? expectedHash) => ReadStock(stockDirectory, name, expectedHash);
        ProjectileFile stock = Stock(ProjectileSpriteDefinitions.FileName, manifest.ContentSha256);
        _ = stock.Compile(ProjectileSpriteCatalog.Load);
        ProjectileFile stockFrameBindings = Stock(ProjectileFrameBindingFormat.FileName, manifest.FrameBindingsSha256);
        _ = stockFrameBindings.Compile(ProjectileFrameBindingCatalog.Load);
        if (manifest.BeamHashes is null || manifest.BeamHashes.Count != BeamTileAtlasDefinitions.ArtworkCount)
            throw new InvalidDataException($"Projectile manifest {manifestPath} must identify every beam PNG.");
        var stockBeams = new Dictionary<string, ProjectileFile>();
        for (int i = 0; i < BeamTileAtlasDefinitions.ArtworkCount; i++)
        {
            string name = BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(i));
            if (!manifest.BeamHashes.TryGetValue(name, out string? expected))
                throw new InvalidDataException($"Projectile manifest {manifestPath} is missing beam {name}.");
            ProjectileFile file = Stock(name, expected);
            _ = file.Compile(stream => BeamTileAtlas.Load(stream, BeamTileAtlasDefinitions.SelectionAt(i)));
            stockBeams.Add(name, file);
        }
        ProjectileFile stockPalettes = Stock(BeamPaletteDefinitions.FileName, manifest.PaletteSha256);
        _ = stockPalettes.Compile(BeamPaletteCatalog.Load);
        ProjectileFile stockHyperBeamFxColors = Stock(HyperBeamFxColorFormat.FileName, manifest.HyperBeamFxColorsSha256);
        _ = stockHyperBeamFxColors.Compile(HyperBeamFxColorCatalog.Load);
        ProjectileFile stockTrails = Stock(ProjectileTrailVisualDefinitions.FileName, manifest.TrailSha256);
        _ = stockTrails.Compile(stream => ProjectileTrailCatalog.Load(stream));
        ProjectileFile stockTrailTiles = Stock(ProjectileTrailAtlasDefinitions.FileName, manifest.TrailTilesSha256);
        _ = stockTrailTiles.Compile(ProjectileTrailAtlas.Load);
        ProjectileFile stockFlarePlacement = Stock(ChargeFlarePlacementDefinitions.FileName, manifest.FlarePlacementSha256);
        _ = stockFlarePlacement.Compile(ChargeFlarePlacementCatalog.Load);
        ProjectileFile stockFlareCompositions = Stock(ChargeFlareSpriteDefinitions.FileName, manifest.FlareCompositionsSha256);
        _ = stockFlareCompositions.Compile(ChargeFlareSpriteCatalog.Load);
        ProjectileFile stockGrappleTiles = Stock(GrappleTileDefinitions.FileName, manifest.GrappleTilesSha256);
        _ = stockGrappleTiles.Compile(stream => GrappleTileAtlas.Load(stream));
        ProjectileFile stockGrappleSprites = Stock(GrappleSpriteDefinitions.FileName, manifest.GrappleSpritesSha256);
        _ = stockGrappleSprites.Compile(GrappleSpriteCatalog.Load);
        ProjectileFile stockGrappleFlare = Stock(GrappleFlarePlacementDefinitions.FileName, manifest.GrappleFlareSha256);
        _ = stockGrappleFlare.Compile(ChargeFlarePlacementCatalog.LoadGrapple);
        ProjectileFile stockGrappleSwing = Stock(GrappleSwingFrameDefinitions.FileName, manifest.GrappleSwingSha256);
        _ = stockGrappleSwing.Compile(GrappleSwingFrameCatalog.Load);
        // Finish stock validation before opening any optional replacement.
        ProjectileFile Select(ProjectileFile baseline) => baseline.Select(overrideDirectory);
        ProjectileFile selected = Select(stock);
        ProjectileFile selectedFrameBindings = Select(stockFrameBindings);
        var selectedBeams = stockBeams.ToDictionary(pair => pair.Key, pair => Select(pair.Value));
        ProjectileFile selectedPalettes = Select(stockPalettes);
        ProjectileFile selectedHyperBeamFxColors = Select(stockHyperBeamFxColors);
        ProjectileFile selectedTrails = Select(stockTrails);
        ProjectileFile selectedTrailTiles = Select(stockTrailTiles);
        ProjectileFile selectedFlarePlacement = Select(stockFlarePlacement);
        ProjectileFile selectedFlareCompositions = Select(stockFlareCompositions);
        ProjectileFile selectedGrappleTiles = Select(stockGrappleTiles);
        ProjectileFile selectedGrappleSprites = Select(stockGrappleSprites);
        ProjectileFile selectedGrappleFlare = Select(stockGrappleFlare);
        ProjectileFile selectedGrappleSwing = Select(stockGrappleSwing);
        return new(selected.Compile(ProjectileSpriteCatalog.Load),
            Identity(stock, stockBeams, stockPalettes, stockHyperBeamFxColors, stockTrails, stockTrailTiles, stockFlarePlacement, stockFlareCompositions, stockGrappleTiles, stockGrappleSprites, stockGrappleFlare, stockGrappleSwing, stockFrameBindings), Identity(selected, selectedBeams, selectedPalettes, selectedHyperBeamFxColors, selectedTrails, selectedTrailTiles, selectedFlarePlacement, selectedFlareCompositions, selectedGrappleTiles, selectedGrappleSprites, selectedGrappleFlare, selectedGrappleSwing, selectedFrameBindings),
            BeamTileCatalog.FromAtlases(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount)
                .Select(index => selectedBeams[BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(index))].Compile(stream => BeamTileAtlas.Load(stream, BeamTileAtlasDefinitions.SelectionAt(index)))).ToArray(),
                selectedPalettes.Compile(BeamPaletteCatalog.Load), selectedHyperBeamFxColors.Compile(HyperBeamFxColorCatalog.Load)),
            selectedTrails.Compile(stream => ProjectileTrailCatalog.Load(stream, selectedTrailTiles.Compile(ProjectileTrailAtlas.Load))),
            selectedFlarePlacement.Compile(ChargeFlarePlacementCatalog.Load),
            selectedFlareCompositions.Compile(ChargeFlareSpriteCatalog.Load),
            selectedGrappleTiles.Compile(stream => GrappleTileAtlas.Load(stream, selectedGrappleSprites.Compile(GrappleSpriteCatalog.Load),
                selectedGrappleFlare.Compile(ChargeFlarePlacementCatalog.LoadGrapple), selectedGrappleSwing.Compile(GrappleSwingFrameCatalog.Load))),
            selectedFrameBindings.Compile(ProjectileFrameBindingCatalog.Load));
    }

    /// <summary>Reads a stock projectile resource and verifies its manifest hash.</summary>
    /// <param name="directory">Directory containing the stock resource.</param>
    /// <param name="name">Manifest filename of the resource.</param>
    /// <param name="expectedHash">SHA-256 required by the manifest.</param>
    /// <returns>The file path and validated bytes.</returns>
    private static ProjectileFile ReadStock(string directory, string name, string? expectedHash)
    {
        string path = Path.Combine(directory, name);
        byte[] bytes = File.ReadAllBytes(path);
        if (!string.Equals(Hash(bytes), expectedHash, StringComparison.Ordinal))
            throw new InvalidDataException($"Projectile stock hash mismatch: {path}.");
        return new(path, bytes);
    }

    /// <summary>Retains a file's identity through codec admission without hiding the original failure.</summary>
    /// <summary>Retains the source identity and bytes of a selected projectile resource.</summary>
    /// <param name="Path">Path of the stock file or selected replacement.</param>
    /// <param name="Bytes">Bytes admitted to a resource codec.</param>
    private sealed record ProjectileFile(string Path, byte[] Bytes)
    {
        /// <summary>Compiles the retained bytes and adds the source path to invalid-data errors.</summary>
        /// <typeparam name="T">Type produced by the resource decoder.</typeparam>
        /// <param name="compile">Decoder that consumes a stream.</param>
        /// <returns>The decoded projectile resource.</returns>
        internal T Compile<T>(Func<Stream, T> compile)
        {
            using var stream = new MemoryStream(Bytes, writable: false);
            try { return compile(stream); }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid projectile asset {Path}: {error.Message}", error);
            }
        }

        /// <summary>Selects a same-named replacement when present, otherwise retaining the stock file.</summary>
        /// <param name="directory">Optional user override directory.</param>
        /// <returns>The replacement file when it exists, or this stock file.</returns>
        internal ProjectileFile Select(string? directory)
        {
            string? replacement = directory is null ? null : System.IO.Path.Combine(directory, System.IO.Path.GetFileName(Path));
            return replacement is not null && File.Exists(replacement)
                ? new(replacement, File.ReadAllBytes(replacement)) : this;
        }
    }

    /// <summary>Computes the uppercase SHA-256 identifier used for projectile resources.</summary>
    /// <param name="bytes">Resource bytes to hash.</param>
    /// <returns>The hexadecimal SHA-256 value.</returns>
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    /// <summary>Builds a stable identity from every selected projectile presentation component.</summary>
    /// <param name="composition">Projectile composition resource.</param>
    /// <param name="beams">Beam atlas resources keyed by their installed filenames.</param>
    /// <param name="palettes">Beam palette resource.</param>
    /// <param name="hyperBeamFxColors">Hyper-beam palette-effect color resource.</param>
    /// <param name="trails">Projectile trail visual definitions.</param>
    /// <param name="trailTiles">Projectile trail tile atlas.</param>
    /// <param name="flarePlacement">Charge-flare placement definitions.</param>
    /// <param name="flareCompositions">Charge-flare sprite compositions.</param>
    /// <param name="grappleTiles">Grapple tile resource.</param>
    /// <param name="grappleSprites">Grapple sprite compositions.</param>
    /// <param name="grappleFlare">Grapple flare placement definitions.</param>
    /// <param name="grappleSwing">Grapple swing frame definitions.</param>
    /// <param name="frameBindings">Projectile animation-frame bindings.</param>
    /// <returns>A SHA-256 identity over the ordered component hashes.</returns>
    private static string Identity(ProjectileFile composition, Dictionary<string, ProjectileFile> beams, ProjectileFile palettes, ProjectileFile hyperBeamFxColors, ProjectileFile trails, ProjectileFile trailTiles, ProjectileFile flarePlacement, ProjectileFile flareCompositions, ProjectileFile grappleTiles, ProjectileFile grappleSprites, ProjectileFile grappleFlare, ProjectileFile grappleSwing, ProjectileFile frameBindings)
    {
        // Fixed-size component hashes in fixed selection order prevent ambiguous concatenation.
        string hashes = Hash(composition.Bytes);
        for (int i = 0; i < BeamTileAtlasDefinitions.ArtworkCount; i++)
            hashes += Hash(beams[BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(i))].Bytes);
        return Hash(System.Text.Encoding.ASCII.GetBytes(hashes + Hash(palettes.Bytes) + Hash(hyperBeamFxColors.Bytes) + Hash(trails.Bytes) + Hash(trailTiles.Bytes) + Hash(flarePlacement.Bytes) + Hash(flareCompositions.Bytes) + Hash(grappleTiles.Bytes) + Hash(grappleSprites.Bytes) + Hash(grappleFlare.Bytes) + Hash(grappleSwing.Bytes) + Hash(frameBindings.Bytes)));
    }
    /// <summary>Stock cartridge provenance and hashes for every projectile presentation resource.</summary>
    /// <param name="Version">Projectile manifest schema version.</param>
    /// <param name="RomSha256">Supported source cartridge SHA-256.</param>
    /// <param name="ContentSha256">Hash of the projectile composition document.</param>
    /// <param name="BeamHashes">Beam atlas hashes keyed by filename.</param>
    /// <param name="PaletteSha256">Beam palette document hash.</param>
    /// <param name="HyperBeamFxColorsSha256">Hyper-beam color document hash.</param>
    /// <param name="TrailSha256">Projectile trail definition hash.</param>
    /// <param name="TrailTilesSha256">Projectile trail tile atlas hash.</param>
    /// <param name="FlarePlacementSha256">Charge-flare placement document hash.</param>
    /// <param name="FlareCompositionsSha256">Charge-flare composition document hash.</param>
    /// <param name="GrappleTilesSha256">Grapple tile resource hash.</param>
    /// <param name="GrappleSpritesSha256">Grapple sprite document hash.</param>
    /// <param name="GrappleFlareSha256">Grapple flare placement hash.</param>
    /// <param name="GrappleSwingSha256">Grapple swing frame document hash.</param>
    /// <param name="FrameBindingsSha256">Projectile frame-binding document hash.</param>
    private sealed record Manifest(int Version, string RomSha256, string ContentSha256, Dictionary<string, string> BeamHashes, string PaletteSha256, string HyperBeamFxColorsSha256, string TrailSha256, string TrailTilesSha256, string FlarePlacementSha256, string FlareCompositionsSha256, string GrappleTilesSha256, string GrappleSpritesSha256, string GrappleFlareSha256, string GrappleSwingSha256, string FrameBindingsSha256);
}

/// <summary>Loaded content and separate original/selected byte identities for diagnostics.</summary>
/// <param name="Catalog">Compiled projectile sprite compositions.</param>
/// <param name="StockSha256">Identity of the unmodified stock resource set.</param>
/// <param name="SelectedSha256">Identity of the resources selected after overrides.</param>
/// <param name="BeamTiles">Compiled beam atlases and palettes.</param>
/// <param name="Trails">Compiled projectile trail definitions and tiles.</param>
/// <param name="FlarePlacement">Compiled charge-flare placement entries.</param>
/// <param name="FlareCompositions">Compiled charge-flare sprite compositions.</param>
/// <param name="GrappleTiles">Compiled Grapple tile atlas and presentation data.</param>
/// <param name="FrameBindings">Compiled projectile frame bindings.</param>
public sealed record InstalledProjectilePresentation(ProjectileSpriteCatalog Catalog, string StockSha256, string SelectedSha256, BeamTileCatalog BeamTiles, ProjectileTrailCatalog Trails, ChargeFlarePlacementCatalog FlarePlacement, ChargeFlareSpriteCatalog FlareCompositions, GrappleTileAtlas GrappleTiles, ProjectileFrameBindingCatalog FrameBindings);
