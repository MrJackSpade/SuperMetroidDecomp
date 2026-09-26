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
    public const int Version = 13;
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
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
        Manifest manifest;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(stockDirectory, ManifestFileName)));
            ValidateObject(document.RootElement);
            manifest = document.RootElement.Deserialize<Manifest>(Options)
                ?? throw new InvalidDataException("Missing projectile manifest.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid projectile manifest JSON.", error); }
        if (manifest.Version != Version || manifest.RomSha256 != SupportedCartridge.Sha256)
            throw new InvalidDataException("Projectile manifest revision does not match the supported cartridge.");
        byte[] stock = File.ReadAllBytes(Path.Combine(stockDirectory, ProjectileSpriteDefinitions.FileName));
        string stockHash = Convert.ToHexString(SHA256.HashData(stock));
        if (!string.Equals(stockHash, manifest.ContentSha256, StringComparison.Ordinal))
            throw new InvalidDataException("Projectile stock composition hash mismatch.");
        _ = ProjectileSpriteCatalog.Load(new MemoryStream(stock, writable: false));
        byte[] stockFrameBindings = File.ReadAllBytes(Path.Combine(stockDirectory, ProjectileFrameBindingFormat.FileName));
        if (Hash(stockFrameBindings) != manifest.FrameBindingsSha256)
            throw new InvalidDataException("Projectile stock frame-binding hash mismatch.");
        _ = ProjectileFrameBindingCatalog.Load(new MemoryStream(stockFrameBindings, writable: false));
        if (manifest.BeamHashes is null || manifest.BeamHashes.Count != BeamTileAtlasDefinitions.SelectionCount)
            throw new InvalidDataException("Projectile manifest must identify every beam PNG.");
        var stockBeams = new Dictionary<string, byte[]>();
        for (int i = 0; i < BeamTileAtlasDefinitions.SelectionCount; i++)
        {
            string name = BeamTileAtlasDefinitions.FileName(i);
            byte[] bytes = File.ReadAllBytes(Path.Combine(stockDirectory, name));
            if (!manifest.BeamHashes.TryGetValue(name, out string? expected) || Hash(bytes) != expected)
                throw new InvalidDataException($"Beam PNG stock hash mismatch: {name}.");
            stockBeams.Add(name, bytes);
        }
        _ = BeamTileCatalog.Load(stockBeams);
        byte[] stockPalettes = File.ReadAllBytes(Path.Combine(stockDirectory, BeamPaletteDefinitions.FileName));
        if (Hash(stockPalettes) != manifest.PaletteSha256)
            throw new InvalidDataException("Beam palette stock hash mismatch.");
        _ = BeamPaletteCatalog.Load(new MemoryStream(stockPalettes));
        byte[] stockHyperBeamFxColors = File.ReadAllBytes(Path.Combine(stockDirectory, HyperBeamFxColorFormat.FileName));
        if (Hash(stockHyperBeamFxColors) != manifest.HyperBeamFxColorsSha256)
            throw new InvalidDataException("Hyper Beam FX color stock hash mismatch.");
        _ = HyperBeamFxColorCatalog.Load(new MemoryStream(stockHyperBeamFxColors));
        byte[] stockTrails = File.ReadAllBytes(Path.Combine(stockDirectory, ProjectileTrailVisualDefinitions.FileName));
        if (Hash(stockTrails) != manifest.TrailSha256)
            throw new InvalidDataException("Projectile trail stock hash mismatch.");
        _ = ProjectileTrailCatalog.Load(new MemoryStream(stockTrails));
        byte[] stockTrailTiles = File.ReadAllBytes(Path.Combine(stockDirectory, ProjectileTrailAtlasDefinitions.FileName));
        if (Hash(stockTrailTiles) != manifest.TrailTilesSha256)
            throw new InvalidDataException("Projectile trail PNG stock hash mismatch.");
        _ = ProjectileTrailAtlas.Load(new MemoryStream(stockTrailTiles));
        byte[] stockFlarePlacement = File.ReadAllBytes(Path.Combine(stockDirectory, ChargeFlarePlacementDefinitions.FileName));
        if (Hash(stockFlarePlacement) != manifest.FlarePlacementSha256)
            throw new InvalidDataException("Charge-flare placement stock hash mismatch.");
        _ = ChargeFlarePlacementCatalog.Load(new MemoryStream(stockFlarePlacement));
        byte[] stockFlareCompositions = File.ReadAllBytes(Path.Combine(stockDirectory, ChargeFlareSpriteDefinitions.FileName));
        if (Hash(stockFlareCompositions) != manifest.FlareCompositionsSha256)
            throw new InvalidDataException("Charge-flare compositions stock hash mismatch.");
        _ = ChargeFlareSpriteCatalog.Load(new MemoryStream(stockFlareCompositions));
        byte[] stockGrappleTiles = File.ReadAllBytes(Path.Combine(stockDirectory, GrappleTileDefinitions.FileName));
        if (Hash(stockGrappleTiles) != manifest.GrappleTilesSha256)
            throw new InvalidDataException("Grapple PNG stock hash mismatch.");
        _ = GrappleTileAtlas.Load(new MemoryStream(stockGrappleTiles));
        byte[] stockGrappleSprites = File.ReadAllBytes(Path.Combine(stockDirectory, GrappleSpriteDefinitions.FileName));
        if (Hash(stockGrappleSprites) != manifest.GrappleSpritesSha256)
            throw new InvalidDataException("Grapple sprite stock hash mismatch.");
        _ = GrappleSpriteCatalog.Load(new MemoryStream(stockGrappleSprites));
        byte[] stockGrappleFlare = File.ReadAllBytes(Path.Combine(stockDirectory, GrappleFlarePlacementDefinitions.FileName));
        if (Hash(stockGrappleFlare) != manifest.GrappleFlareSha256)
            throw new InvalidDataException("Grapple flare placement stock hash mismatch.");
        _ = ChargeFlarePlacementCatalog.Load(new MemoryStream(stockGrappleFlare));
        byte[] stockGrappleSwing = File.ReadAllBytes(Path.Combine(stockDirectory, GrappleSwingFrameDefinitions.FileName));
        if (Hash(stockGrappleSwing) != manifest.GrappleSwingSha256)
            throw new InvalidDataException("Grapple swing-frame stock hash mismatch.");
        _ = GrappleSwingFrameCatalog.Load(new MemoryStream(stockGrappleSwing));
        // Finish stock validation before opening any optional replacement.
        byte[] Select(string name, byte[] baseline)
        {
            string? path = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            return path is not null && File.Exists(path) ? File.ReadAllBytes(path) : baseline;
        }
        byte[] selected = Select(ProjectileSpriteDefinitions.FileName, stock);
        byte[] selectedFrameBindings = Select(ProjectileFrameBindingFormat.FileName, stockFrameBindings);
        var selectedBeams = stockBeams.ToDictionary(pair => pair.Key, pair => Select(pair.Key, pair.Value));
        byte[] selectedPalettes = Select(BeamPaletteDefinitions.FileName, stockPalettes);
        byte[] selectedHyperBeamFxColors = Select(HyperBeamFxColorFormat.FileName, stockHyperBeamFxColors);
        byte[] selectedTrails = Select(ProjectileTrailVisualDefinitions.FileName, stockTrails);
        byte[] selectedTrailTiles = Select(ProjectileTrailAtlasDefinitions.FileName, stockTrailTiles);
        byte[] selectedFlarePlacement = Select(ChargeFlarePlacementDefinitions.FileName, stockFlarePlacement);
        byte[] selectedFlareCompositions = Select(ChargeFlareSpriteDefinitions.FileName, stockFlareCompositions);
        byte[] selectedGrappleTiles = Select(GrappleTileDefinitions.FileName, stockGrappleTiles);
        byte[] selectedGrappleSprites = Select(GrappleSpriteDefinitions.FileName, stockGrappleSprites);
        byte[] selectedGrappleFlare = Select(GrappleFlarePlacementDefinitions.FileName, stockGrappleFlare);
        byte[] selectedGrappleSwing = Select(GrappleSwingFrameDefinitions.FileName, stockGrappleSwing);
        return new(ProjectileSpriteCatalog.Load(new MemoryStream(selected, writable: false)),
            Identity(stock, stockBeams, stockPalettes, stockHyperBeamFxColors, stockTrails, stockTrailTiles, stockFlarePlacement, stockFlareCompositions, stockGrappleTiles, stockGrappleSprites, stockGrappleFlare, stockGrappleSwing, stockFrameBindings), Identity(selected, selectedBeams, selectedPalettes, selectedHyperBeamFxColors, selectedTrails, selectedTrailTiles, selectedFlarePlacement, selectedFlareCompositions, selectedGrappleTiles, selectedGrappleSprites, selectedGrappleFlare, selectedGrappleSwing, selectedFrameBindings),
            BeamTileCatalog.Load(selectedBeams, BeamPaletteCatalog.Load(new MemoryStream(selectedPalettes)),
                HyperBeamFxColorCatalog.Load(new MemoryStream(selectedHyperBeamFxColors))),
            ProjectileTrailCatalog.Load(new MemoryStream(selectedTrails), ProjectileTrailAtlas.Load(new MemoryStream(selectedTrailTiles))),
            ChargeFlarePlacementCatalog.Load(new MemoryStream(selectedFlarePlacement)),
            ChargeFlareSpriteCatalog.Load(new MemoryStream(selectedFlareCompositions)),
            GrappleTileAtlas.Load(new MemoryStream(selectedGrappleTiles), GrappleSpriteCatalog.Load(new MemoryStream(selectedGrappleSprites)),
                ChargeFlarePlacementCatalog.Load(new MemoryStream(selectedGrappleFlare)), GrappleSwingFrameCatalog.Load(new MemoryStream(selectedGrappleSwing))),
            ProjectileFrameBindingCatalog.Load(new MemoryStream(selectedFrameBindings, writable: false)));
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static string Identity(byte[] composition, Dictionary<string, byte[]> beams, byte[] palettes, byte[] hyperBeamFxColors, byte[] trails, byte[] trailTiles, byte[] flarePlacement, byte[] flareCompositions, byte[] grappleTiles, byte[] grappleSprites, byte[] grappleFlare, byte[] grappleSwing, byte[] frameBindings)
    {
        // Fixed-size component hashes in fixed selection order prevent ambiguous concatenation.
        string hashes = Hash(composition);
        for (int i = 0; i < BeamTileAtlasDefinitions.SelectionCount; i++)
            hashes += Hash(beams[BeamTileAtlasDefinitions.FileName(i)]);
        return Hash(System.Text.Encoding.ASCII.GetBytes(hashes + Hash(palettes) + Hash(hyperBeamFxColors) + Hash(trails) + Hash(trailTiles) + Hash(flarePlacement) + Hash(flareCompositions) + Hash(grappleTiles) + Hash(grappleSprites) + Hash(grappleFlare) + Hash(grappleSwing) + Hash(frameBindings)));
    }
    private static void ValidateObject(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Projectile manifest requires an object.");
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!names.Add(property.Name)) throw new InvalidDataException("Duplicate projectile manifest property.");
            if (property.Value.ValueKind == JsonValueKind.Object) ValidateObject(property.Value);
        }
    }
    private sealed record Manifest(int Version, string RomSha256, string ContentSha256, Dictionary<string, string> BeamHashes, string PaletteSha256, string HyperBeamFxColorsSha256, string TrailSha256, string TrailTilesSha256, string FlarePlacementSha256, string FlareCompositionsSha256, string GrappleTilesSha256, string GrappleSpritesSha256, string GrappleFlareSha256, string GrappleSwingSha256, string FrameBindingsSha256);
}

/// <summary>Loaded content and separate original/selected byte identities for diagnostics.</summary>
public sealed record InstalledProjectilePresentation(ProjectileSpriteCatalog Catalog, string StockSha256, string SelectedSha256, BeamTileCatalog BeamTiles, ProjectileTrailCatalog Trails, ChargeFlarePlacementCatalog FlarePlacement, ChargeFlareSpriteCatalog FlareCompositions, GrappleTileAtlas GrappleTiles, ProjectileFrameBindingCatalog FrameBindings);
