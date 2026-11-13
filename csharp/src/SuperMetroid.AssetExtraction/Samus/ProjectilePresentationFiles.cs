using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Installs the projectile presentation documents, which the player edits in place, with no manifest (#1693): each document
/// names its own format and the installation receipt the cartridge. It extracts the beam, trail and grapple characters (#1636), the beam colours the projectile and frozen-enemy sheets are
/// pre-coloured in (#1637), the Hyper Beam's colour cycle Samus's sheets are pre-coloured in (#1693) and the projectile,
/// flare, trail and grapple compositions, whose parts are the original's packed sprite placements (#1638), into the
/// component's import data, which only the import reads.
/// </summary>
public static class ProjectilePresentationFiles
{
    /// <summary>
    /// Writes into the installer's unpublished staging directory after ROM validation: the documents into
    /// <paramref name="directory"/>, the character pages, the beam colours, the Hyper Beam's colour cycle and the compositions
    /// into its import-data directory.
    /// </summary>
    public static void Extract(ISnesAddressSpace validatedBus, string directory)
    {
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        byte[] bytes = ProjectileSpriteExtractor.Extract(validatedBus);
        byte[] frameBindings = ProjectileFrameBindingExtractor.Extract(validatedBus);
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(importData);
        File.WriteAllBytes(Path.Combine(importData, ProjectileSpriteDefinitions.FileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, ProjectileFrameBindingFormat.FileName), frameBindings);
        var beams = BeamTileExtractor.Extract(validatedBus);
        byte[] palettes = BeamPaletteExtractor.Extract(validatedBus);
        byte[] hyperBeamFxColors = HyperBeamFxColorExtractor.Extract(validatedBus);
        byte[] trails = ProjectileTrailExtractor.Extract(validatedBus);
        byte[] trailTiles = ProjectileTrailAtlasExtractor.Extract(validatedBus);
        byte[] flarePlacement = ChargeFlarePlacementExtractor.Extract(validatedBus);
        byte[] flareCompositions = ChargeFlareSpriteExtractor.Extract(validatedBus);
        byte[] hitBurstCompositions = HitBurstSpriteExtractor.Extract(validatedBus);
        byte[] grappleTiles = GrappleTileExtractor.Extract(validatedBus);
        byte[] grappleSprites = GrappleSpriteExtractor.Extract(validatedBus);
        byte[] grappleFlare = GrappleFlarePlacementExtractor.Extract(validatedBus);
        byte[] grappleSwing = GrappleSwingFrameExtractor.Extract(validatedBus);
        File.WriteAllBytes(Path.Combine(directory, GrappleSwingFrameExtractor.FileName), grappleSwing);
        File.WriteAllBytes(Path.Combine(directory, GrappleFlarePlacementDefinitions.FileName), grappleFlare);
        File.WriteAllBytes(Path.Combine(importData, GrappleSpriteDefinitions.FileName), grappleSprites);
        File.WriteAllBytes(Path.Combine(importData, GrappleTileDefinitions.FileName), grappleTiles);
        File.WriteAllBytes(Path.Combine(importData, ChargeFlareSpriteExtractor.FileName), flareCompositions);
        File.WriteAllBytes(Path.Combine(importData, HitBurstSpriteExtractor.FileName), hitBurstCompositions);
        File.WriteAllBytes(Path.Combine(directory, ChargeFlarePlacementExtractor.FileName), flarePlacement);
        File.WriteAllBytes(Path.Combine(importData, ProjectileTrailAtlasDefinitions.FileName), trailTiles);
        File.WriteAllBytes(Path.Combine(importData, ProjectileTrailVisualDefinitions.FileName), trails);
        File.WriteAllBytes(Path.Combine(importData, BeamPaletteFile.FileName), palettes);
        File.WriteAllBytes(Path.Combine(importData, HyperBeamFxColorFormat.FileName), hyperBeamFxColors);
        foreach (var file in beams) File.WriteAllBytes(Path.Combine(importData, file.Key), file.Value);
    }

    /// <summary>
    /// Loads the installed projectile documents, as the player may have edited them. No character page (#1636), beam colour
    /// (#1637), Hyper Beam colour (#1693), composition (#1638) or manifest (#1693) is read. An invalid file fails loudly with
    /// its path.
    /// </summary>
    public static InstalledProjectilePresentation Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ProjectileFile frameBindings = Read(ProjectileFrameBindingFormat.FileName);
        ProjectileFile flarePlacement = Read(ChargeFlarePlacementExtractor.FileName);
        ProjectileFile grappleFlare = Read(GrappleFlarePlacementDefinitions.FileName);
        ProjectileFile grappleSwing = Read(GrappleSwingFrameExtractor.FileName);
        return new(
            Identity(new ProjectilePresentationSourceFiles { FlarePlacement = flarePlacement, GrappleFlare = grappleFlare, GrappleSwing = grappleSwing, FrameBindings = frameBindings }),
            flarePlacement.Compile(ChargeFlarePlacementFile.Load),
            new InstalledGrapplePresentation(grappleFlare.Compile(ChargeFlarePlacementFile.LoadGrapple), grappleSwing.Compile(GrappleSwingFrameFile.Load)),
            frameBindings.Compile(ProjectileFrameBindingFile.Load));

        ProjectileFile Read(string name)
        {
            string path = Path.Combine(directory, name);
            return new(path, File.ReadAllBytes(path));
        }
    }

    /// <summary>
    /// Loads the projectile presentation as the import composes from it (#1636): the installed documents in
    /// <paramref name="directory"/> and the beam, trail and grapple characters, the beam colours, the Hyper Beam's colour
    /// cycle (#1693) and the compositions in its import data <paramref name="importData"/>.
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
        HyperBeamFxColours hyperBeamFxColors = Read(HyperBeamFxColorFormat.FileName).Compile(stream => HyperBeamFxColorFile.Read(stream).ToColours());
        return new ImportProjectileArtwork(presentation,
            BeamTileCatalog.FromAtlases(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount)
                .Select(index => Read(BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(index)))
                    .Compile(stream => BeamTileAtlas.Load(stream, BeamTileAtlasDefinitions.SelectionAt(index)))).ToArray(),
                beamPalettes, hyperBeamFxColors),
            beamPalettes,
            hyperBeamFxColors,
            Read(ProjectileTrailAtlasDefinitions.FileName).Compile(ProjectileTrailAtlas.Load),
            Read(GrappleTileDefinitions.FileName).Compile(GrappleTileAtlas.Load),
            new ProjectileCompositions(Read(ProjectileSpriteDefinitions.FileName).Compile(InstalledProjectileSprites.Load),
                Read(ChargeFlareSpriteExtractor.FileName).Compile(ChargeFlareSprites.Load),
                Read(ProjectileTrailVisualDefinitions.FileName).Compile(InstalledProjectileTrails.Load),
                Read(GrappleSpriteDefinitions.FileName).Compile(InstalledGrappleSprites.Load),
                Read(HitBurstSpriteExtractor.FileName).Compile(InstalledProjectileSprites.LoadHitBursts)));
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
        return Hash(System.Text.Encoding.ASCII.GetBytes(Hash(files.FlarePlacement.Bytes) + Hash(files.GrappleFlare.Bytes) + Hash(files.GrappleSwing.Bytes) + Hash(files.FrameBindings.Bytes)));
    }
    /// <summary>The installed projectile presentation documents whose contents form the presentation identity (#633, #1636).</summary>
    [ArchitectureReview("context-bag", "The projectile presentation's source files read for one extraction; every member is file content, not a service.")]
    private sealed record ProjectilePresentationSourceFiles
    {
        /// <summary>Charge-flare placement data relative to Samus's pose.</summary>
        public required ProjectileFile FlarePlacement { get; init; }

        /// <summary>Grapple flare placement independent of charge-flare placement.</summary>
        public required ProjectileFile GrappleFlare { get; init; }

        /// <summary>Grapple swinging frame selection and placement.</summary>
        public required ProjectileFile GrappleSwing { get; init; }

        /// <summary>Native projectile frame identities mapped to editable displayed frames.</summary>
        public required ProjectileFile FrameBindings { get; init; }
    }
}
