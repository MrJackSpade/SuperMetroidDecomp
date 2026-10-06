using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Versioned stock composition provenance, separate from persistent user overrides.</summary>
public static class ProjectilePresentationFiles
{
    public const string ManifestFileName = "projectile-manifest.json";
    public const int Version = 15;
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
            _ = file.Compile(BeamTileAtlas.Load);
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
        _ = stockGrappleFlare.Compile(ChargeFlarePlacementCatalog.Load);
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
                .Select(index => selectedBeams[BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(index))].Compile(BeamTileAtlas.Load)).ToArray(),
                selectedPalettes.Compile(BeamPaletteCatalog.Load), selectedHyperBeamFxColors.Compile(HyperBeamFxColorCatalog.Load)),
            selectedTrails.Compile(stream => ProjectileTrailCatalog.Load(stream, selectedTrailTiles.Compile(ProjectileTrailAtlas.Load))),
            selectedFlarePlacement.Compile(ChargeFlarePlacementCatalog.Load),
            selectedFlareCompositions.Compile(ChargeFlareSpriteCatalog.Load),
            selectedGrappleTiles.Compile(stream => GrappleTileAtlas.Load(stream, selectedGrappleSprites.Compile(GrappleSpriteCatalog.Load),
                selectedGrappleFlare.Compile(ChargeFlarePlacementCatalog.Load), selectedGrappleSwing.Compile(GrappleSwingFrameCatalog.Load))),
            selectedFrameBindings.Compile(ProjectileFrameBindingCatalog.Load));
    }

    private static ProjectileFile ReadStock(string directory, string name, string? expectedHash)
    {
        string path = Path.Combine(directory, name);
        byte[] bytes = File.ReadAllBytes(path);
        if (!string.Equals(Hash(bytes), expectedHash, StringComparison.Ordinal))
            throw new InvalidDataException($"Projectile stock hash mismatch: {path}.");
        return new(path, bytes);
    }

    /// <summary>Retains a file's identity through codec admission without hiding the original failure.</summary>
    private sealed record ProjectileFile(string Path, byte[] Bytes)
    {
        internal T Compile<T>(Func<Stream, T> compile)
        {
            using var stream = new MemoryStream(Bytes, writable: false);
            try { return compile(stream); }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid projectile asset {Path}: {error.Message}", error);
            }
        }

        internal ProjectileFile Select(string? directory)
        {
            string? replacement = directory is null ? null : System.IO.Path.Combine(directory, System.IO.Path.GetFileName(Path));
            return replacement is not null && File.Exists(replacement)
                ? new(replacement, File.ReadAllBytes(replacement)) : this;
        }
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static string Identity(ProjectileFile composition, Dictionary<string, ProjectileFile> beams, ProjectileFile palettes, ProjectileFile hyperBeamFxColors, ProjectileFile trails, ProjectileFile trailTiles, ProjectileFile flarePlacement, ProjectileFile flareCompositions, ProjectileFile grappleTiles, ProjectileFile grappleSprites, ProjectileFile grappleFlare, ProjectileFile grappleSwing, ProjectileFile frameBindings)
    {
        // Fixed-size component hashes in fixed selection order prevent ambiguous concatenation.
        string hashes = Hash(composition.Bytes);
        for (int i = 0; i < BeamTileAtlasDefinitions.ArtworkCount; i++)
            hashes += Hash(beams[BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(i))].Bytes);
        return Hash(System.Text.Encoding.ASCII.GetBytes(hashes + Hash(palettes.Bytes) + Hash(hyperBeamFxColors.Bytes) + Hash(trails.Bytes) + Hash(trailTiles.Bytes) + Hash(flarePlacement.Bytes) + Hash(flareCompositions.Bytes) + Hash(grappleTiles.Bytes) + Hash(grappleSprites.Bytes) + Hash(grappleFlare.Bytes) + Hash(grappleSwing.Bytes) + Hash(frameBindings.Bytes)));
    }
    private sealed record Manifest(int Version, string RomSha256, string ContentSha256, Dictionary<string, string> BeamHashes, string PaletteSha256, string HyperBeamFxColorsSha256, string TrailSha256, string TrailTilesSha256, string FlarePlacementSha256, string FlareCompositionsSha256, string GrappleTilesSha256, string GrappleSpritesSha256, string GrappleFlareSha256, string GrappleSwingSha256, string FrameBindingsSha256);
}

/// <summary>Loaded content and separate original/selected byte identities for diagnostics.</summary>
public sealed record InstalledProjectilePresentation(ProjectileSpriteCatalog Catalog, string StockSha256, string SelectedSha256, BeamTileCatalog BeamTiles, ProjectileTrailCatalog Trails, ChargeFlarePlacementCatalog FlarePlacement, ChargeFlareSpriteCatalog FlareCompositions, GrappleTileAtlas GrappleTiles, ProjectileFrameBindingCatalog FrameBindings);
