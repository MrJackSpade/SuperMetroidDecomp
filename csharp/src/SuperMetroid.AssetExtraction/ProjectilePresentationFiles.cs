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
    /// <summary>Strict manifest JSON encoding with required fields and no unknown properties.</summary>
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
        /// <summary>Reads one named stock asset after verifying its recorded content hash.</summary>
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
        /// <summary>Selects a persistent user replacement after all stock assets pass validation.</summary>
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
            Identity(new ProjectilePresentationSourceFiles { Composition = stock, Beams = stockBeams, Palettes = stockPalettes, HyperBeamFxColors = stockHyperBeamFxColors, Trails = stockTrails, TrailTiles = stockTrailTiles, FlarePlacement = stockFlarePlacement, FlareCompositions = stockFlareCompositions, GrappleTiles = stockGrappleTiles, GrappleSprites = stockGrappleSprites, GrappleFlare = stockGrappleFlare, GrappleSwing = stockGrappleSwing, FrameBindings = stockFrameBindings }), Identity(new ProjectilePresentationSourceFiles { Composition = selected, Beams = selectedBeams, Palettes = selectedPalettes, HyperBeamFxColors = selectedHyperBeamFxColors, Trails = selectedTrails, TrailTiles = selectedTrailTiles, FlarePlacement = selectedFlarePlacement, FlareCompositions = selectedFlareCompositions, GrappleTiles = selectedGrappleTiles, GrappleSprites = selectedGrappleSprites, GrappleFlare = selectedGrappleFlare, GrappleSwing = selectedGrappleSwing, FrameBindings = selectedFrameBindings }),
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

    /// <summary>Rejects changed or missing stock content before admitting its bytes to a projectile codec.</summary>
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
        /// <summary>Decodes the selected bytes while adding their file path to malformed-asset diagnostics.</summary>
        internal T Compile<T>(Func<Stream, T> compile)
        {
            using var stream = new MemoryStream(Bytes, writable: false);
            try { return compile(stream); }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid projectile asset {Path}: {error.Message}", error);
            }
        }

        /// <summary>Uses a same-named override if present, otherwise retains the validated stock file.</summary>
        internal ProjectileFile Select(string? directory)
        {
            string? replacement = directory is null ? null : System.IO.Path.Combine(directory, System.IO.Path.GetFileName(Path));
            return replacement is not null && File.Exists(replacement)
                ? new(replacement, File.ReadAllBytes(replacement)) : this;
        }
    }

    /// <summary>Encodes a projectile asset's SHA-256 digest as uppercase hexadecimal.</summary>
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    /// <summary>Hashes fixed-size component digests in presentation selection order for stock or selected assets.</summary>
    private static string Identity(ProjectilePresentationSourceFiles files)
    {
        // Fixed-size component hashes in fixed selection order prevent ambiguous concatenation.
        string hashes = Hash(files.Composition.Bytes);
        for (int i = 0; i < BeamTileAtlasDefinitions.ArtworkCount; i++)
            hashes += Hash(files.Beams[BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(i))].Bytes);
        return Hash(System.Text.Encoding.ASCII.GetBytes(hashes + Hash(files.Palettes.Bytes) + Hash(files.HyperBeamFxColors.Bytes) + Hash(files.Trails.Bytes) + Hash(files.TrailTiles.Bytes) + Hash(files.FlarePlacement.Bytes) + Hash(files.FlareCompositions.Bytes) + Hash(files.GrappleTiles.Bytes) + Hash(files.GrappleSprites.Bytes) + Hash(files.GrappleFlare.Bytes) + Hash(files.GrappleSwing.Bytes) + Hash(files.FrameBindings.Bytes)));
    }
    /// <summary>Cartridge provenance and expected hashes for every extracted projectile presentation asset.</summary>
    private sealed record Manifest(int Version, string RomSha256, string ContentSha256, Dictionary<string, string> BeamHashes, string PaletteSha256, string HyperBeamFxColorsSha256, string TrailSha256, string TrailTilesSha256, string FlarePlacementSha256, string FlareCompositionsSha256, string GrappleTilesSha256, string GrappleSpritesSha256, string GrappleFlareSha256, string GrappleSwingSha256, string FrameBindingsSha256);

    /// <summary>The extracted projectile presentation files whose contents form the presentation identity (#633).</summary>
    private sealed record ProjectilePresentationSourceFiles
    {
        /// <summary>Enemy projectile OAM compositions keyed by native frame.</summary>
        public required ProjectileFile Composition { get; init; }

        /// <summary>Beam tile atlases keyed by their selection-specific PNG filenames.</summary>
        public required Dictionary<string, ProjectileFile> Beams { get; init; }

        /// <summary>Beam palette rows for ordinary shot variants.</summary>
        public required ProjectileFile Palettes { get; init; }

        /// <summary>Hyper Beam effect palette colors independent of shot mechanics.</summary>
        public required ProjectileFile HyperBeamFxColors { get; init; }

        /// <summary>Trail frame compositions and visual selections.</summary>
        public required ProjectileFile Trails { get; init; }

        /// <summary>Pixel atlas used by projectile trail compositions.</summary>
        public required ProjectileFile TrailTiles { get; init; }

        /// <summary>Charge-flare placement data relative to Samus's pose.</summary>
        public required ProjectileFile FlarePlacement { get; init; }

        /// <summary>Charge-flare OAM compositions.</summary>
        public required ProjectileFile FlareCompositions { get; init; }

        /// <summary>Grapple beam tile atlas used by its sprite compositions.</summary>
        public required ProjectileFile GrappleTiles { get; init; }

        /// <summary>Grapple beam and flare OAM sprite definitions.</summary>
        public required ProjectileFile GrappleSprites { get; init; }

        /// <summary>Grapple flare placement independent of charge-flare placement.</summary>
        public required ProjectileFile GrappleFlare { get; init; }

        /// <summary>Grapple swinging frame selection and placement.</summary>
        public required ProjectileFile GrappleSwing { get; init; }

        /// <summary>Native projectile frame identities mapped to editable displayed frames.</summary>
        public required ProjectileFile FrameBindings { get; init; }
    }
}

/// <summary>Loaded content and separate original/selected byte identities for diagnostics.</summary>
public sealed record InstalledProjectilePresentation(ProjectileSpriteCatalog Catalog, string StockSha256, string SelectedSha256, BeamTileCatalog BeamTiles, ProjectileTrailCatalog Trails, ChargeFlarePlacementCatalog FlarePlacement, ChargeFlareSpriteCatalog FlareCompositions, GrappleTileAtlas GrappleTiles, ProjectileFrameBindingCatalog FrameBindings);
