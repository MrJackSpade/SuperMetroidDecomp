using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable, ROM-independent projectile visual compositions; no damage, collision or timing fields.</summary>
public sealed class InstalledProjectileSprites
{
    /// <summary>Compiled OAM compositions keyed by composition.</summary>
    internal readonly Dictionary<ProjectileSpriteId, SpriteComposition> frames;
    /// <summary>Accepts the complete validated set of compositions.</summary>
    private InstalledProjectileSprites(Dictionary<ProjectileSpriteId, SpriteComposition> frames) => this.frames = frames;

    /// <summary>One installed projectile composition as a whole frame.</summary>
    /// <param name="id">The composition.</param>
    /// <returns>The composition as a whole frame whose characters are its OBJ tile numbers.</returns>
    public SpriteFrame Frame(ProjectileSpriteId id)
    {
        if (!frames.TryGetValue(id, out var parts)) throw new InvalidDataException($"Missing projectile sprite {id.Name}.");
        return wholeFrames.GetOrAdd(id, key => parts.ToFrame($"Projectile.{key.Name}", 0));
    }

    /// <summary>Whole frames already built, by composition.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<ProjectileSpriteId, SpriteFrame> wholeFrames = new();

    /// <summary>Validates and compiles the complete required projectile spritemap set into independently owned visual compositions, including the empty Nothing frame.</summary>
    /// <param name="json">Readable JSON stream at its current position; it remains open and caller-owned.</param>
    /// <returns>Installed artwork keyed by the 417 compositions timed records draw. Matching stock geometry may use immutable calculated parts; authored changes retain their compiled parts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed or ambiguous, its schema or required identity set is invalid, or a part exceeds the supported OAM fields or count.</exception>
    public static InstalledProjectileSprites Load(Stream json)
        => LoadFrames(json, ProjectileSpriteDefinitions.All);

    /// <summary>
    /// Loads the native part format for either timed projectiles or charge flares. Charge
    /// flares have a separate required composition set, preserving existing projectile overrides.
    /// </summary>
    internal static InstalledProjectileSprites LoadFrames(Stream json, IReadOnlyList<ProjectileSpriteId> required)
    {
        int requiredCount = required.Count;
        ProjectileSpriteDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ProjectileSpriteDocument>(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            }) ?? throw new InvalidDataException("Projectile composition document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid projectile composition JSON.", error); }
        if (document.Version != ProjectileSpriteDefinitions.Version || document.Frames is null || document.Frames.Count != requiredCount)
            throw new InvalidDataException($"Projectile compositions require version {ProjectileSpriteDefinitions.Version} and every required composition.");
        var frames = new Dictionary<ProjectileSpriteId, SpriteComposition>();
        foreach (ProjectileSpriteId id in required)
        {
            string name = id.Name;
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null || parts.Length > ProjectileSpriteDefinitions.MaximumParts)
                throw new InvalidDataException($"Projectile sprite {name} is missing or exceeds OAM capacity.");
            var compiled = new CompiledSpritePart[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                var p = parts[i];
                if (p is null || p.OffsetX is < -256 or > 255 || p.OffsetY is < -128 or > 127 ||
                    p.Size is not (8 or 16) || p.Priority is < 0 or > 3 || p.Palette is null or < 0 or > 7 ||
                    p.TileColumn is < 0 or >= ProjectileSpriteDefinitions.TileColumns || p.TileRow is < 0 or >= ProjectileSpriteDefinitions.TileRows)
                    throw new InvalidDataException($"Projectile sprite {name} part {i} has invalid visual fields.");
                // Hardware wraps a large part's adjacent tile at row/sheet boundaries.
                // Do not impose the menu atlas's contiguous-rectangle restriction here.
                compiled[i] = new(SpritePart.Create(Offset16<ScreenPixels>.Of(p.OffsetX, p.OffsetY), p.Size == 16,
                    p.TileRow * ProjectileSpriteDefinitions.TileColumns + p.TileColumn,
                    p.Palette.Value, p.Priority, (p.FlipX ? TileFlip.Horizontal : 0) | (p.FlipY ? TileFlip.Vertical : 0)), false);
            }
            var composition = new SpriteComposition(compiled);
            if (ProjectileSpriteParts.TrySingleBeamPhase(id, out int phase))
                composition = composition.CalculateIfMatching(ProjectileSpriteParts.SingleBeamPoses[phase]);
            else if (ProjectileSpriteParts.TryChargedBeamPhase(id, out phase, out bool ice))
                composition = composition.CalculateIfMatching(new ChargedBeamParts(phase, ice));
            else if (ProjectileSpriteParts.TryWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(ProjectileSpriteParts.WavePoses[phase]);
            else if (ProjectileSpriteParts.TryVerticalChargedWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(new VerticalChargedWaveParts(phase));
            else if (ProjectileSpriteParts.TryAxialSuperMissilePose(id, out phase))
                composition = composition.CalculateIfMatching(new AxialSuperMissileParts(phase));
            else if (ProjectileSpriteParts.TryDiagonalMissilePose(id, out phase, out bool super))
                composition = composition.CalculateIfMatching(new DiagonalMissileParts(phase, super));
            else if (ProjectileSpriteParts.TrySimpleEffectPose(id, out phase, out bool quad))
                composition = composition.CalculateIfMatching(new SimpleEffectParts(phase, quad));
            else if (ProjectileSpriteParts.TryHorizontalChargedWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(new HorizontalChargedWaveParts(phase));
            else if (ProjectileSpriteParts.TrySpazerSeedPose(id, out phase))
                composition = composition.CalculateIfMatching(new SpazerSeedParts(phase));
            else if (ProjectileSpriteParts.TrySpazerDiagonalSpread(id, out int pose, out phase))
                composition = composition.CalculateIfMatching(new SpazerDiagonalSpreadParts(pose, phase));
            else if (ProjectileSpriteParts.TrySpazerAxialSpread(id, out pose, out phase))
                composition = composition.CalculateIfMatching(new SpazerAxialSpreadParts(pose, phase));
            else if (ProjectileSpriteParts.TryHorizontalChargedSpazer(id, out phase))
                composition = composition.CalculateIfMatching(new HorizontalChargedSpazerParts(phase));
            else if (ProjectileSpriteParts.TryVerticalChargedSpazerSpread(id, out phase))
                composition = composition.CalculateIfMatching(new VerticalChargedSpazerSpreadParts(phase));
            else if (ProjectileSpriteParts.TrySpazerAxialStartup(id, out int group, out int length))
                composition = composition.CalculateIfMatching(new SpazerAxialStartupParts(group, length));
            else if (ProjectileSpriteParts.TrySpazerDiagonalStartup(id, out group, out length))
                composition = composition.CalculateIfMatching(new SpazerDiagonalStartupParts(group, length));
            else if (ProjectileSpriteParts.TryAlternateDiagonalStartup(id, out bool reflected, out phase))
                composition = composition.CalculateIfMatching(new AlternateDiagonalStartupParts(reflected, phase));
            else if (ProjectileSpriteParts.TryPlasmaStartupCore(id, out group))
                composition = composition.CalculateIfMatching(new PlasmaStartupCoreParts(group));
            else if (ProjectileSpriteParts.TryHorizontalPlasmaWaveShort(id, out phase))
                composition = composition.CalculateIfMatching(new HorizontalPlasmaWaveShortParts(phase));
            frames.Add(id, composition);
        }
        return new(frames);
    }

    /// <summary>Rejects ambiguous JSON fields before compiling the required sprite set.</summary>
    private static void RejectDuplicateProperties(JsonElement element) =>
        JsonAssetDocument.RejectDuplicateProperties(element, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate projectile composition property {name}."));
}
