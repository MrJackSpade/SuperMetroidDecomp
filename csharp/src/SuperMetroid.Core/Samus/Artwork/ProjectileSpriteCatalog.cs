using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable, ROM-independent projectile visual compositions; no damage, collision or timing fields.</summary>
public sealed class ProjectileSpriteCatalog
{
    /// <summary>Compiled OAM compositions keyed by bank-$93 spritemap offsets.</summary>
    internal readonly Dictionary<ushort, SpriteComposition> frames;
    /// <summary>Accepts the complete validated set of native projectile sprite identities.</summary>
    private ProjectileSpriteCatalog(Dictionary<ushort, SpriteComposition> frames) => this.frames = frames;

    /// <summary>One installed projectile composition as a whole frame.</summary>
    /// <param name="id">Native composition identity.</param>
    /// <returns>The composition as a whole frame whose characters are its OBJ tile numbers.</returns>
    public SpriteFrame Frame(ushort id)
    {
        if (!frames.TryGetValue(id, out var parts)) throw new InvalidDataException($"Missing projectile sprite {id:X4}.");
        return wholeFrames.GetOrAdd(id, key => parts.ToFrame($"Projectile.{key:X4}", 0));
    }

    /// <summary>Whole frames already built, by composition identity.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<ushort, SpriteFrame> wholeFrames = new();

    /// <summary>Validates and compiles the complete required projectile spritemap set into independently owned visual compositions, including the empty Nothing frame.</summary>
    /// <param name="json">Readable JSON stream at its current position; it remains open and caller-owned.</param>
    /// <returns>Installed artwork keyed by the 417 native timed-record spritemap identities. Matching stock geometry may use immutable calculated parts; authored changes retain their compiled parts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed or ambiguous, its schema or required identity set is invalid, or a part exceeds the supported OAM fields or count.</exception>
    public static ProjectileSpriteCatalog Load(Stream json)
        => LoadFrames(json, default, pointers: ProjectileFramePointers.NativeProjectile);

    /// <summary>
    /// Loads the native part format for either timed projectiles or charge flares. Charge
    /// flares have a separate required identity set, preserving existing projectile overrides.
    /// </summary>
    internal static ProjectileSpriteCatalog LoadFrames(Stream json, ReadOnlySpan<ushort> requiredPointers, ProjectileFramePointers pointers = ProjectileFramePointers.Required)
    {
        int requiredCount = pointers == ProjectileFramePointers.NativeProjectile ? ProjectileSpriteDefinitions.NativePointers.Length : requiredPointers.Length;
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
            throw new InvalidDataException("Projectile compositions require version 1 and every required sprite identity.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        for (int pointerIndex = 0; pointerIndex < requiredCount; pointerIndex++)
        {
            ushort id = pointers == ProjectileFramePointers.NativeProjectile ? ProjectileSpriteDefinitions.NativePointers[pointerIndex] : requiredPointers[pointerIndex];
            string name = ProjectileSpriteDefinitions.Name(id);
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
                var attributes = SnesObjAttributeWord.Create(p.TileRow * ProjectileSpriteDefinitions.TileColumns + p.TileColumn,
                    p.Palette.Value, p.Priority, (p.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (p.FlipY ? SnesTileFlipFlags.Vertical : 0));
                compiled[i] = new(SnesSpritemapXWord.Create(p.OffsetX, p.Size == 16), new SnesSpritemapYByte(unchecked((byte)p.OffsetY)), attributes, false);
            }
            var composition = new SpriteComposition(compiled);
            if (ProjectileSpritePartDefinitions.TrySingleBeamPhase(id, out int phase))
                composition = composition.CalculateIfMatching(ProjectileSpritePartDefinitions.SingleBeamPoses[phase]);
            else if (ProjectileSpritePartDefinitions.TryChargedBeamPhase(id, out phase, out bool ice))
                composition = composition.CalculateIfMatching(new ChargedBeamParts(phase, ice));
            else if (ProjectileSpritePartDefinitions.TryWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(ProjectileSpritePartDefinitions.WavePoses[phase]);
            else if (ProjectileSpritePartDefinitions.TryVerticalChargedWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(new VerticalChargedWaveParts(phase));
            else if (ProjectileSpritePartDefinitions.TryAxialSuperMissilePose(id, out phase))
                composition = composition.CalculateIfMatching(new AxialSuperMissileParts(phase));
            else if (ProjectileSpritePartDefinitions.TryDiagonalMissilePose(id, out phase, out bool super))
                composition = composition.CalculateIfMatching(new DiagonalMissileParts(phase, super));
            else if (ProjectileSpritePartDefinitions.TrySimpleEffectPose(id, out phase, out bool quad))
                composition = composition.CalculateIfMatching(new SimpleEffectParts(phase, quad));
            else if (ProjectileSpritePartDefinitions.TryHorizontalChargedWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(new HorizontalChargedWaveParts(phase));
            else if (ProjectileSpritePartDefinitions.TrySpazerSeedPose(id, out phase))
                composition = composition.CalculateIfMatching(new SpazerSeedParts(phase));
            else if (ProjectileSpritePartDefinitions.TrySpazerDiagonalSpread(id, out int pose, out phase))
                composition = composition.CalculateIfMatching(new SpazerDiagonalSpreadParts(pose, phase));
            else if (ProjectileSpritePartDefinitions.TrySpazerAxialSpread(id, out pose, out phase))
                composition = composition.CalculateIfMatching(new SpazerAxialSpreadParts(pose, phase));
            else if (ProjectileSpritePartDefinitions.TryHorizontalChargedSpazer(id, out phase))
                composition = composition.CalculateIfMatching(new HorizontalChargedSpazerParts(phase));
            else if (ProjectileSpritePartDefinitions.TryVerticalChargedSpazerSpread(id, out phase))
                composition = composition.CalculateIfMatching(new VerticalChargedSpazerSpreadParts(phase));
            else if (ProjectileSpritePartDefinitions.TrySpazerAxialStartup(id, out int group, out int length))
                composition = composition.CalculateIfMatching(new SpazerAxialStartupParts(group, length));
            else if (ProjectileSpritePartDefinitions.TrySpazerDiagonalStartup(id, out group, out length))
                composition = composition.CalculateIfMatching(new SpazerDiagonalStartupParts(group, length));
            else if (ProjectileSpritePartDefinitions.TryAlternateDiagonalStartup(id, out bool reflected, out phase))
                composition = composition.CalculateIfMatching(new AlternateDiagonalStartupParts(reflected, phase));
            else if (ProjectileSpritePartDefinitions.TryPlasmaStartupCore(id, out group))
                composition = composition.CalculateIfMatching(new PlasmaStartupCoreParts(group));
            else if (ProjectileSpritePartDefinitions.TryHorizontalPlasmaWaveShort(id, out phase))
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
