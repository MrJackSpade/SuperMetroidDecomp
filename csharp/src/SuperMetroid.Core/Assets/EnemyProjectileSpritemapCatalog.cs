using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Named, editable bank-$8D enemy-projectile OAM compositions.</summary>
public sealed class EnemyProjectileSpritemapCatalog
{
    private readonly Dictionary<ushort, EnemySpritemapPart[]> frames;

    private EnemyProjectileSpritemapCatalog(Dictionary<ushort, EnemySpritemapPart[]> frames) =>
        this.frames = frames;

    public ReadOnlyMemory<EnemySpritemapPart> Get(ushort pointer) =>
        frames.TryGetValue(pointer, out EnemySpritemapPart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no composition at $8D:{pointer:X4}.");

    public static EnemyProjectileSpritemapCatalog Load(Stream json)
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
        if (document.Version != EnemyProjectileSpritemapDefinitions.Version ||
            document.Frames is null ||
            document.Frames.Count != EnemyProjectileSpritemapDefinitions.Frames.Length)
            throw new InvalidDataException(
                "Enemy-projectile compositions have the wrong version or frame count.");
        var compiled = new Dictionary<ushort, EnemySpritemapPart[]>();
        foreach ((ushort pointer, string name) in EnemyProjectileSpritemapDefinitions.Frames)
        {
            if (!document.Frames.TryGetValue(name, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > EnemyProjectileSpritemapDefinitions.MaximumParts)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} is missing or oversized.");
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
            compiled.Add(pointer, parts);
        }
        return new EnemyProjectileSpritemapCatalog(compiled);
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
}

/// <summary>Cartridge visual identities translated for bank-$8D projectile drawing.</summary>
public static class EnemyProjectileSpritemapDefinitions
{
    public const int Version = 1;
    public const string FileName = "enemy-projectile-compositions.json";
    public const int MaximumParts = 128;
    public const int TileColumns = 16;
    public const int TileRows = 64;

    /// <summary>The two pad frames and stationary concealer selected by $86:A28D-A29D.</summary>
    internal static readonly (ushort Pointer, string Name)[] Frames =
    [
        (0xb1ba, "ceres_elevator_pad_0"),
        (0xb1d0, "ceres_elevator_pad_1"),
        (0x846d, "ceres_elevator_platform"),
    ];
}
