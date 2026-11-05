using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Installs the projectile presentation files with their versioned provenance manifest; the player edits them in place.</summary>
public static class ProjectilePresentationFiles
{
    /// <summary>Projectile provenance manifest filename.</summary>
    public const string ManifestFileName = "projectile-manifest.json";
    /// <summary>Current projectile manifest schema version covering all projectile, beam, flare, trail, and Grapple visual assets.</summary>
    public const int Version = 16;
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
        File.WriteAllBytes(Path.Combine(directory, GrappleSwingFrameExtractor.FileName), grappleSwing);
        File.WriteAllBytes(Path.Combine(directory, GrappleFlarePlacementDefinitions.FileName), grappleFlare);
        File.WriteAllBytes(Path.Combine(directory, GrappleSpriteDefinitions.FileName), grappleSprites);
        File.WriteAllBytes(Path.Combine(directory, GrappleTileDefinitions.FileName), grappleTiles);
        File.WriteAllBytes(Path.Combine(directory, ChargeFlareSpriteExtractor.FileName), flareCompositions);
        File.WriteAllBytes(Path.Combine(directory, ChargeFlarePlacementExtractor.FileName), flarePlacement);
        File.WriteAllBytes(Path.Combine(directory, ProjectileTrailAtlasDefinitions.FileName), trailTiles);
        File.WriteAllBytes(Path.Combine(directory, ProjectileTrailVisualDefinitions.FileName), trails);
        File.WriteAllBytes(Path.Combine(directory, BeamPaletteFile.FileName), palettes);
        File.WriteAllBytes(Path.Combine(directory, HyperBeamFxColorFormat.FileName), hyperBeamFxColors);
        foreach (var file in beams) File.WriteAllBytes(Path.Combine(directory, file.Key), file.Value);
        File.WriteAllText(Path.Combine(directory, ManifestFileName), JsonSerializer.Serialize(
            new Manifest(Version, SupportedCartridge.Sha256, Hash(bytes),
                beams.ToDictionary(pair => pair.Key, pair => Hash(pair.Value)), Hash(palettes), Hash(hyperBeamFxColors), Hash(trails), Hash(trailTiles), Hash(flarePlacement), Hash(flareCompositions), Hash(grappleTiles), Hash(grappleSprites), Hash(grappleFlare), Hash(grappleSwing), Hash(frameBindings)), Options));
    }

    /// <summary>
    /// Checks the manifest's provenance and loads the installed projectile presentation, as the player may have edited it.
    /// An invalid file fails loudly with its path.
    /// </summary>
    public static InstalledProjectilePresentation Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        Manifest manifest = JsonAssetDocument.Read<Manifest>(manifestStream, Options,
            $"projectile manifest {manifestPath}");
        if (manifest.Version != Version || manifest.RomSha256 != SupportedCartridge.Sha256)
            throw new InvalidDataException($"Projectile manifest {manifestPath} does not match the supported cartridge/revision.");
        ProjectileFile Read(string name)
        {
            string path = Path.Combine(directory, name);
            return new(path, File.ReadAllBytes(path));
        }
        ProjectileFile composition = Read(ProjectileSpriteDefinitions.FileName);
        ProjectileFile frameBindings = Read(ProjectileFrameBindingFormat.FileName);
        var beams = new Dictionary<string, ProjectileFile>();
        for (int i = 0; i < BeamTileAtlasDefinitions.ArtworkCount; i++)
        {
            string name = BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(i));
            beams.Add(name, Read(name));
        }
        ProjectileFile palettes = Read(BeamPaletteFile.FileName);
        ProjectileFile hyperBeamFxColors = Read(HyperBeamFxColorFormat.FileName);
        ProjectileFile trails = Read(ProjectileTrailVisualDefinitions.FileName);
        ProjectileFile trailTiles = Read(ProjectileTrailAtlasDefinitions.FileName);
        ProjectileFile flarePlacement = Read(ChargeFlarePlacementExtractor.FileName);
        ProjectileFile flareCompositions = Read(ChargeFlareSpriteExtractor.FileName);
        ProjectileFile grappleTiles = Read(GrappleTileDefinitions.FileName);
        ProjectileFile grappleSprites = Read(GrappleSpriteDefinitions.FileName);
        ProjectileFile grappleFlare = Read(GrappleFlarePlacementDefinitions.FileName);
        ProjectileFile grappleSwing = Read(GrappleSwingFrameExtractor.FileName);
        return new(composition.Compile(InstalledProjectileSprites.Load),
            Identity(new ProjectilePresentationSourceFiles { Composition = composition, Beams = beams, Palettes = palettes, HyperBeamFxColors = hyperBeamFxColors, Trails = trails, TrailTiles = trailTiles, FlarePlacement = flarePlacement, FlareCompositions = flareCompositions, GrappleTiles = grappleTiles, GrappleSprites = grappleSprites, GrappleFlare = grappleFlare, GrappleSwing = grappleSwing, FrameBindings = frameBindings }),
            BeamTileCatalog.FromAtlases(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount)
                .Select(index => beams[BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(index))].Compile(stream => BeamTileAtlas.Load(stream, BeamTileAtlasDefinitions.SelectionAt(index)))).ToArray(),
                palettes.Compile(BeamPaletteFile.Read), hyperBeamFxColors.Compile(stream => HyperBeamFxColorFile.Read(stream).ToColours())),
            trails.Compile(InstalledProjectileTrails.Load),
            trailTiles.Compile(ProjectileTrailAtlas.Load),
            flarePlacement.Compile(ChargeFlarePlacementFile.Load),
            flareCompositions.Compile(ChargeFlareSprites.Load),
            grappleTiles.Compile(stream => GrappleTileAtlas.Load(stream, grappleSprites.Compile(InstalledGrappleSprites.Load),
                grappleFlare.Compile(ChargeFlarePlacementFile.LoadGrapple), grappleSwing.Compile(GrappleSwingFrameFile.Load))),
            frameBindings.Compile(ProjectileFrameBindingFile.Load));
    }

    /// <summary>Retains a file's identity through codec admission without hiding the original failure.</summary>
    private sealed record ProjectileFile(string Path, byte[] Bytes)
    {
        /// <summary>Decodes the installed bytes while adding their file path to malformed-asset diagnostics.</summary>
        internal T Compile<T>(Func<Stream, T> compile)
        {
            using var stream = new MemoryStream(Bytes, writable: false);
            try { return compile(stream); }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid projectile asset {Path}: {error.Message}", error);
            }
        }

    }

    /// <summary>Encodes a projectile asset's SHA-256 digest as uppercase hexadecimal.</summary>
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    /// <summary>Hashes fixed-size component digests in presentation selection order for the installed assets.</summary>
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
    [ArchitectureReview("context-bag", "The projectile presentation's source files read for one extraction; every member is file content, not a service.")]
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
