using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Installs the projectile presentation documents with their versioned provenance manifest, which the player edits in place,
/// and extracts the beam, trail and grapple characters (#1636) and the beam colours the projectile and frozen-enemy sheets are
/// pre-coloured in (#1637) into the component's import data, which only the import reads.
/// </summary>
public static class ProjectilePresentationFiles
{
    /// <summary>Projectile provenance manifest filename.</summary>
    public const string ManifestFileName = "projectile-manifest.json";
    /// <summary>Current projectile manifest schema version covering all projectile, beam, flare, trail, and Grapple visual assets.</summary>
    public const int Version = 18;
    /// <summary>Strict manifest JSON encoding with required fields and no unknown properties.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>
    /// Writes into the installer's unpublished staging directory after ROM validation: the documents into
    /// <paramref name="directory"/>, the character pages and the beam colours into its import-data directory.
    /// </summary>
    public static void Extract(ISnesAddressSpace validatedBus, string directory)
    {
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        byte[] bytes = ProjectileSpriteExtractor.Extract(validatedBus);
        byte[] frameBindings = ProjectileFrameBindingExtractor.Extract(validatedBus);
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(importData);
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
        File.WriteAllBytes(Path.Combine(importData, GrappleTileDefinitions.FileName), grappleTiles);
        File.WriteAllBytes(Path.Combine(directory, ChargeFlareSpriteExtractor.FileName), flareCompositions);
        File.WriteAllBytes(Path.Combine(directory, ChargeFlarePlacementExtractor.FileName), flarePlacement);
        File.WriteAllBytes(Path.Combine(importData, ProjectileTrailAtlasDefinitions.FileName), trailTiles);
        File.WriteAllBytes(Path.Combine(directory, ProjectileTrailVisualDefinitions.FileName), trails);
        File.WriteAllBytes(Path.Combine(importData, BeamPaletteFile.FileName), palettes);
        File.WriteAllBytes(Path.Combine(directory, HyperBeamFxColorFormat.FileName), hyperBeamFxColors);
        foreach (var file in beams) File.WriteAllBytes(Path.Combine(importData, file.Key), file.Value);
        File.WriteAllText(Path.Combine(directory, ManifestFileName), JsonSerializer.Serialize(
            new Manifest(Version, SupportedCartridge.Sha256), Options));
    }

    /// <summary>
    /// Checks the manifest's provenance and loads the installed projectile documents, as the player may have edited them.
    /// No character page (#1636) or beam colour (#1637) is read. An invalid file fails loudly with its path.
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
        ProjectileFile composition = Read(ProjectileSpriteDefinitions.FileName);
        ProjectileFile frameBindings = Read(ProjectileFrameBindingFormat.FileName);
        ProjectileFile hyperBeamFxColors = Read(HyperBeamFxColorFormat.FileName);
        ProjectileFile trails = Read(ProjectileTrailVisualDefinitions.FileName);
        ProjectileFile flarePlacement = Read(ChargeFlarePlacementExtractor.FileName);
        ProjectileFile flareCompositions = Read(ChargeFlareSpriteExtractor.FileName);
        ProjectileFile grappleSprites = Read(GrappleSpriteDefinitions.FileName);
        ProjectileFile grappleFlare = Read(GrappleFlarePlacementDefinitions.FileName);
        ProjectileFile grappleSwing = Read(GrappleSwingFrameExtractor.FileName);
        return new(composition.Compile(InstalledProjectileSprites.Load),
            Identity(new ProjectilePresentationSourceFiles { Composition = composition, HyperBeamFxColors = hyperBeamFxColors, Trails = trails, FlarePlacement = flarePlacement, FlareCompositions = flareCompositions, GrappleSprites = grappleSprites, GrappleFlare = grappleFlare, GrappleSwing = grappleSwing, FrameBindings = frameBindings }),
            hyperBeamFxColors.Compile(stream => HyperBeamFxColorFile.Read(stream).ToColours()),
            trails.Compile(InstalledProjectileTrails.Load),
            flarePlacement.Compile(ChargeFlarePlacementFile.Load),
            flareCompositions.Compile(ChargeFlareSprites.Load),
            new InstalledGrapplePresentation(grappleSprites.Compile(InstalledGrappleSprites.Load),
                grappleFlare.Compile(ChargeFlarePlacementFile.LoadGrapple), grappleSwing.Compile(GrappleSwingFrameFile.Load)),
            frameBindings.Compile(ProjectileFrameBindingFile.Load));

        ProjectileFile Read(string name)
        {
            string path = Path.Combine(directory, name);
            return new(path, File.ReadAllBytes(path));
        }
    }

    /// <summary>
    /// Loads the projectile presentation as the import composes from it (#1636): the installed documents in
    /// <paramref name="directory"/> and the beam, trail and grapple characters and the beam colours in its import data
    /// <paramref name="importData"/>.
    /// An invalid file fails loudly with its path.
    /// </summary>
    public static ImportProjectileArtwork LoadForImport(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        InstalledProjectilePresentation presentation = Load(directory);
        ProjectileFile Read(string name)
        {
            string path = Path.Combine(importData, name);
            return new(path, File.ReadAllBytes(path));
        }
        InstalledBeamPalettes beamPalettes = Read(BeamPaletteFile.FileName).Compile(BeamPaletteFile.Read);
        return new ImportProjectileArtwork(presentation,
            BeamTileCatalog.FromAtlases(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount)
                .Select(index => Read(BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(index)))
                    .Compile(stream => BeamTileAtlas.Load(stream, BeamTileAtlasDefinitions.SelectionAt(index)))).ToArray(),
                beamPalettes, presentation.HyperBeamFxColors),
            beamPalettes,
            Read(ProjectileTrailAtlasDefinitions.FileName).Compile(ProjectileTrailAtlas.Load),
            Read(GrappleTileDefinitions.FileName).Compile(GrappleTileAtlas.Load));
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
    /// <summary>
    /// Hashes fixed-size component digests in a fixed order for the installed documents: the installation holds no
    /// character page (#1636), so the identity covers every installed file.
    /// </summary>
    private static string Identity(ProjectilePresentationSourceFiles files)
    {
        // Fixed-size component hashes in a fixed order prevent ambiguous concatenation.
        return Hash(System.Text.Encoding.ASCII.GetBytes(Hash(files.Composition.Bytes) + Hash(files.HyperBeamFxColors.Bytes) + Hash(files.Trails.Bytes) + Hash(files.FlarePlacement.Bytes) + Hash(files.FlareCompositions.Bytes) + Hash(files.GrappleSprites.Bytes) + Hash(files.GrappleFlare.Bytes) + Hash(files.GrappleSwing.Bytes) + Hash(files.FrameBindings.Bytes)));
    }
    /// <summary>Cartridge provenance and expected hashes for every extracted projectile presentation asset.</summary>
    private sealed record Manifest(int Version, string RomSha256);

    /// <summary>The installed projectile presentation documents whose contents form the presentation identity (#633, #1636).</summary>
    [ArchitectureReview("context-bag", "The projectile presentation's source files read for one extraction; every member is file content, not a service.")]
    private sealed record ProjectilePresentationSourceFiles
    {
        /// <summary>Enemy projectile OAM compositions keyed by native frame.</summary>
        public required ProjectileFile Composition { get; init; }

        /// <summary>Hyper Beam effect palette colors independent of shot mechanics.</summary>
        public required ProjectileFile HyperBeamFxColors { get; init; }

        /// <summary>Trail frame compositions and visual selections.</summary>
        public required ProjectileFile Trails { get; init; }

        /// <summary>Charge-flare placement data relative to Samus's pose.</summary>
        public required ProjectileFile FlarePlacement { get; init; }

        /// <summary>Charge-flare OAM compositions.</summary>
        public required ProjectileFile FlareCompositions { get; init; }

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
