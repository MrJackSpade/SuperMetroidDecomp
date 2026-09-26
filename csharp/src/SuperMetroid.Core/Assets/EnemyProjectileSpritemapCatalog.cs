using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Named, editable bank-$8D enemy-projectile OAM compositions.</summary>
public sealed class EnemyProjectileSpritemapCatalog
{
    private readonly Dictionary<ushort, EnemySpritemapPart[]> frames;
    private readonly Dictionary<ushort, EnemySpritemapPart[]> programFrames;

    private EnemyProjectileSpritemapCatalog(Dictionary<ushort, EnemySpritemapPart[]> frames,
        Dictionary<ushort, EnemySpritemapPart[]> programFrames)
    {
        this.frames = frames;
        this.programFrames = programFrames;
    }

    public ReadOnlyMemory<EnemySpritemapPart> Get(ushort pointer) =>
        frames.TryGetValue(pointer, out EnemySpritemapPart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no composition at $8D:{pointer:X4}.");

    /// <summary>Draws a timed program frame without reading its bank-$86/$8D visual operands.</summary>
    public ReadOnlyMemory<EnemySpritemapPart> GetProgramFrame(ushort operandAddress) =>
        programFrames.TryGetValue(operandAddress, out EnemySpritemapPart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no frame for $86:{operandAddress:X4}.");

    public static EnemyProjectileSpritemapCatalog Load(Stream json,
        EnemyProjectileSpritemapCatalog? stock = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        EnemyProjectileSpritemapDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyProjectileSpritemapDocument>(Options)
                ?? throw new InvalidDataException("Enemy-projectile compositions are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid enemy-projectile compositions JSON.", error);
        }
        bool legacyOverride = document.Version is 1 or 2 && stock is not null;
        int expectedFrames = document.Version == 1 && legacyOverride
            ? EnemyProjectileSpritemapDefinitions.LegacyFrameCount
            : EnemyProjectileSpritemapDefinitions.Frames.Length;
        if ((!legacyOverride && document.Version != EnemyProjectileSpritemapDefinitions.Version) ||
            document.Frames is null || document.Frames.Count != expectedFrames)
            throw new InvalidDataException(
                "Enemy-projectile compositions have the wrong version or frame count.");
        // Older overrides retain their edited Ceres/debris frames. Newly introduced
        // shared-program frames come only from the hash-checked stock installation.
        var compiled = stock is not null && legacyOverride
            ? new Dictionary<ushort, EnemySpritemapPart[]>(stock.frames)
            : new Dictionary<ushort, EnemySpritemapPart[]>();
        var compiledPrograms = stock is not null && legacyOverride
            ? new Dictionary<ushort, EnemySpritemapPart[]>(stock.programFrames)
            : new Dictionary<ushort, EnemySpritemapPart[]>();
        foreach ((ushort pointer, string name) in
                 EnemyProjectileSpritemapDefinitions.Frames.Take(expectedFrames))
        {
            if (!document.Frames.TryGetValue(name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} is missing.");
            compiled[pointer] = CompileParts(name, visual);
        }
        if (!legacyOverride)
        {
            ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> definitions =
                EnemyProjectileInstructionMechanicsDefinitions.VisualFrames;
            if (document.ProgramFrames is null || document.ProgramFrames.Count != definitions.Length)
                throw new InvalidDataException(
                    "Enemy-projectile program frames have the wrong count.");
            foreach (EnemyProjectilePresentationFrameDefinition frame in definitions)
            {
                if (!document.ProgramFrames.TryGetValue(frame.Name,
                        out SpriteVisualPart[]? visual) || visual is null)
                    throw new InvalidDataException(
                        $"Enemy-projectile program frame {frame.Name} is missing.");
                compiledPrograms.Add(frame.OperandAddress, CompileParts(frame.Name, visual));
            }
        }
        return new EnemyProjectileSpritemapCatalog(compiled, compiledPrograms);
    }

    private static EnemySpritemapPart[] CompileParts(string name, SpriteVisualPart[] visual)
    {
        if (visual.Length > EnemyProjectileSpritemapDefinitions.MaximumParts)
            throw new InvalidDataException(
                $"Enemy-projectile composition {name} is oversized.");
        var parts = new EnemySpritemapPart[visual.Length];
        for (int index = 0; index < visual.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                part.TileColumn is < 0 or >= EnemyProjectileSpritemapDefinitions.TileColumns ||
                part.TileRow is < 0 or >= EnemyProjectileSpritemapDefinitions.TileRows)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} part {index} is invalid.");
            SnesTileFlipFlags flips =
                (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (part.FlipY ? SnesTileFlipFlags.Vertical : 0);
            parts[index] = new EnemySpritemapPart(
                SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                unchecked((byte)(sbyte)part.OffsetY),
                SnesObjAttributeWord.Create(
                    part.TileRow * EnemyProjectileSpritemapDefinitions.TileColumns +
                        part.TileColumn,
                    part.Palette.Value, part.Priority, flips));
        }
        return parts;
    }

    internal static byte[] Write(EnemyProjectileSpritemapDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(json, writable: false));
        return json;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

public sealed record EnemyProjectileSpritemapDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    public Dictionary<string, SpriteVisualPart[]>? ProgramFrames { get; init; }
}

/// <summary>Cartridge visual identities translated for bank-$8D projectile drawing.</summary>
public static class EnemyProjectileSpritemapDefinitions
{
    public const int Version = 3;
    public const string FileName = "enemy-projectile-compositions.json";
    public const int LegacyFrameCount = 3;
    public const int MaximumParts = 128;
    public const int TileColumns = 16;
    public const int TileRows = 64;

    /// <summary>Ceres arrival frames and Skree/Metaree debris selected by their bank-$86 programs.</summary>
    internal static readonly (ushort Pointer, string Name)[] Frames =
    [
        (0xb1ba, "ceres_elevator_pad_0"),
        (0xb1d0, "ceres_elevator_pad_1"),
        (0x846d, "ceres_elevator_platform"),
        (SkreeMetareeParticleVisualDefinitions.SkreeComposition, "skree_debris"),
        (SkreeMetareeParticleVisualDefinitions.MetareeComposition, "metaree_debris"),
    ];
}
