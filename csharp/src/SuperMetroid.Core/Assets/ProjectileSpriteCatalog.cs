using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable, ROM-independent projectile visual compositions; no damage, collision or timing fields.</summary>
public sealed class ProjectileSpriteCatalog
{
    private readonly Dictionary<ushort, SpriteComposition> frames;
    private ProjectileSpriteCatalog(Dictionary<ushort, SpriteComposition> frames) => this.frames = frames;

    public void Draw(ushort id, OamBuffer oam, ushort x, ushort y)
    {
        if (!frames.TryGetValue(id, out var parts)) throw new InvalidDataException($"Missing projectile sprite {id:X4}.");
        for (int index = 0; index < parts.PartCount; index++)
        {
            var part = parts.Part(index);
            oam.AddProjectileSpritePart(part.X, part.Y, part.Attributes, x, y);
        }
    }

    public static ProjectileSpriteCatalog Load(Stream json)
        => LoadFrames(json, default, useProjectilePointers: true);

    // Charge flares use the same native part format but have a separate required
    // identity set, so existing projectile overrides remain compatible.
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
            frames.Add(id, composition);
        }
        return new(frames);
    }

    private static void RejectDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new InvalidDataException($"Duplicate projectile composition property {property.Name}.");
                RejectDuplicateProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray()) RejectDuplicateProperties(item);
    }
}

public sealed record ProjectileSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
