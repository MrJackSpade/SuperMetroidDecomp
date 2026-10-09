using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable, ROM-independent projectile visual compositions; no damage, collision or timing fields.</summary>
public sealed class ProjectileSpriteCatalog
{
    /// <summary>Compiled visual compositions keyed by their native spritemap identity.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Creates a catalog from the validated, compiled frame map.</summary>
    /// <param name="frames">Complete mapping from required spritemap identities to visual compositions.</param>
    private ProjectileSpriteCatalog(Dictionary<ushort, SpriteComposition> frames) => this.frames = frames;

    /// <summary>Emits one installed projectile composition in authored OAM order using native projectile coordinate wrapping, without advancing animation or collision state.</summary>
    /// <param name="id">Bank-$93 spritemap offset selected by a timed projectile record, not an instruction-list pointer or projectile family.</param>
    /// <param name="oam">Destination OAM buffer; the native wrapping insertion cursor advances for every part.</param>
    /// <param name="x">Screen-space horizontal origin in pixels, including unsigned representations of negative coordinates; individual parts retain nine-bit hardware X.</param>
    /// <param name="y">Screen-space vertical origin in pixels; native projectile insertion wraps the resulting Y to eight bits without vertical-origin clipping.</param>
    /// <exception cref="InvalidDataException">The requested sprite identity is not installed.</exception>
    public void Draw(ushort id, OamBuffer oam, ushort x, ushort y)
    {
        if (!frames.TryGetValue(id, out var parts)) throw new InvalidDataException($"Missing projectile sprite {id:X4}.");
        for (int index = 0; index < parts.PartCount; index++)
        {
            var part = parts.Part(index);
            oam.AddProjectileSpritePart(part.X, part.Y, part.Attributes, x, y);
        }
    }

    /// <summary>Validates and compiles the complete required projectile spritemap set into independently owned visual compositions, including the empty Nothing frame.</summary>
    /// <param name="json">Readable JSON stream at its current position; it remains open and caller-owned.</param>
    /// <returns>Installed artwork keyed by the 417 native timed-record spritemap identities. Matching stock geometry may use immutable calculated parts; authored changes retain their compiled parts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed or ambiguous, its schema or required identity set is invalid, or a part exceeds the supported OAM fields or count.</exception>
    public static ProjectileSpriteCatalog Load(Stream json)
        => LoadFrames(json, default, useProjectilePointers: true);

    // Charge flares use the same native part format but have a separate required
    // identity set, so existing projectile overrides remain compatible.
    /// <summary>
    /// Validates a projectile-style visual document and compiles every identity from the selected required set.
    /// </summary>
    /// <param name="json">JSON stream positioned at its document; the stream remains open.</param>
    /// <param name="requiredPointers">Identities required when <paramref name="useProjectilePointers"/> is false.</param>
    /// <param name="useProjectilePointers">When true, use the native projectile identity catalog instead of <paramref name="requiredPointers"/>.</param>
    /// <returns>A catalog containing a compiled composition for each required identity.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The document has duplicate or invalid JSON, the wrong schema or identity set, or an invalid visual part.</exception>
    internal static ProjectileSpriteCatalog LoadFrames(Stream json, ReadOnlySpan<ushort> requiredPointers, bool useProjectilePointers = false)
    {
        int requiredCount = useProjectilePointers ? ProjectileSpriteDefinitions.NativePointers.Length : requiredPointers.Length;
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
            ushort id = useProjectilePointers ? ProjectileSpriteDefinitions.NativePointers[pointerIndex] : requiredPointers[pointerIndex];
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
                compiled[i] = new(SnesSpritemapXWord.Create(p.OffsetX, p.Size == 16), unchecked((byte)(sbyte)p.OffsetY), attributes, false);
            }
            var composition = new SpriteComposition(compiled);
            if (ProjectileSpriteDefinitions.TrySingleBeamPhase(id, out int phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.SingleBeamParts(phase));
            else if (ProjectileSpriteDefinitions.TryChargedBeamPhase(id, out phase, out bool ice))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.ChargedBeamParts(phase, ice));
            else if (ProjectileSpriteDefinitions.TryWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.WaveParts(phase));
            else if (ProjectileSpriteDefinitions.TryVerticalChargedWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.VerticalChargedWaveParts(phase));
            else if (ProjectileSpriteDefinitions.TryAxialSuperMissilePose(id, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.AxialSuperMissileParts(phase));
            else if (ProjectileSpriteDefinitions.TryDiagonalMissilePose(id, out phase, out bool super))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.DiagonalMissileParts(phase, super));
            else if (ProjectileSpriteDefinitions.TrySimpleEffectPose(id, out phase, out bool quad))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.SimpleEffectParts(phase, quad));
            else if (ProjectileSpriteDefinitions.TryHorizontalChargedWavePhase(id, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.HorizontalChargedWaveParts(phase));
            else if (ProjectileSpriteDefinitions.TrySpazerSeedPose(id, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.SpazerSeedParts(phase));
            else if (ProjectileSpriteDefinitions.TrySpazerDiagonalSpread(id, out int pose, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.SpazerDiagonalSpreadParts(pose, phase));
            else if (ProjectileSpriteDefinitions.TrySpazerAxialSpread(id, out pose, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.SpazerAxialSpreadParts(pose, phase));
            else if (ProjectileSpriteDefinitions.TryHorizontalChargedSpazer(id, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.HorizontalChargedSpazerParts(phase));
            else if (ProjectileSpriteDefinitions.TryVerticalChargedSpazerSpread(id, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.VerticalChargedSpazerSpreadParts(phase));
            else if (ProjectileSpriteDefinitions.TrySpazerAxialStartup(id, out int group, out int length))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.SpazerAxialStartupParts(group, length));
            else if (ProjectileSpriteDefinitions.TrySpazerDiagonalStartup(id, out group, out length))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.SpazerDiagonalStartupParts(group, length));
            else if (ProjectileSpriteDefinitions.TryAlternateDiagonalStartup(id, out bool reflected, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.AlternateDiagonalStartupParts(reflected, phase));
            else if (ProjectileSpriteDefinitions.TryPlasmaStartupCore(id, out group))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.PlasmaStartupCoreParts(group));
            else if (ProjectileSpriteDefinitions.TryHorizontalPlasmaWaveShort(id, out phase))
                composition = composition.CalculateIfMatching(new ProjectileSpriteDefinitions.HorizontalPlasmaWaveShortParts(phase));
            frames.Add(id, composition);
        }
        return new(frames);
    }

    /// <summary>
    /// Rejects repeated property names throughout a parsed projectile document using ordinal name comparison.
    /// </summary>
    /// <param name="element">Parsed JSON subtree to validate, including objects nested within arrays.</param>
    /// <exception cref="InvalidDataException">An object in the document contains a repeated property name.</exception>
    private static void RejectDuplicateProperties(JsonElement element) =>
        JsonAssetDocument.RejectDuplicateProperties(element, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate projectile composition property {name}."));
}

/// <summary>Mutable JSON authoring data for projectile OAM appearance, separate from timed instruction records, damage, hitboxes, and motion.</summary>
public sealed record ProjectileSpriteDocument
{
    /// <summary>Gets the schema revision, which must equal <see cref="ProjectileSpriteDefinitions.Version"/> when loaded.</summary>
    public required int Version { get; init; }
    /// <summary>Gets caller-owned ordered parts keyed by <see cref="ProjectileSpriteDefinitions.Name"/> for every required native identity; zero-part compositions remain valid.</summary>
    /// <remarks>Each composition permits at most 128 parts with explicit palette selectors 0..7, pixel offsets, 8- or 16-pixel sizes, and tile coordinates in a 16-column by 32-row OBJ grid. Large parts may cross tile-grid boundaries using hardware wrapping. Loading compiles and copies values rather than retaining these mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
