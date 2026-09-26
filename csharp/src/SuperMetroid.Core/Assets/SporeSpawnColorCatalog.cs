using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable Spore Spawn RGB5 images. Health thresholds, death timing, and target/current
/// palette ownership remain compiled cartridge behavior.
/// </summary>
public sealed class SporeSpawnColorCatalog
{
    private readonly ushort[] spores;
    private readonly ushort[][] health;
    private readonly ushort[][] deathSprite;
    private readonly ushort[][] deathLevel;
    private readonly ushort[][] deathBackground;

    private SporeSpawnColorCatalog(ushort[] spores, ushort[][] health,
        ushort[][] deathSprite, ushort[][] deathLevel, ushort[][] deathBackground)
    {
        this.spores = spores;
        this.health = health;
        this.deathSprite = deathSprite;
        this.deathLevel = deathLevel;
        this.deathBackground = deathBackground;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveSpore(int color) => Get(spores, color);
    public ushort ResolveHealth(int frame, int color) => Get(health[CheckFrame(
        frame, SporeSpawnColorRomData.HealthFrameCount)], color);
    public ushort ResolveDeathSprite(int frame, int color) => Get(deathSprite[CheckFrame(
        frame, SporeSpawnColorRomData.DeathSpriteFrameCount)], color);
    public ushort ResolveDeathLevel(int frame, int color) => Get(deathLevel[CheckFrame(
        frame, SporeSpawnColorRomData.DeathSceneFrameCount)], color);
    public ushort ResolveDeathBackground(int frame, int color) => Get(deathBackground[
        CheckFrame(frame, SporeSpawnColorRomData.DeathSceneFrameCount)], color);

    public ushort ResolveDeath(SporeSpawnDeathPaletteLayer layer, int frame, int color) =>
        layer switch
        {
            SporeSpawnDeathPaletteLayer.Sprite => ResolveDeathSprite(frame, color),
            SporeSpawnDeathPaletteLayer.Level => ResolveDeathLevel(frame, color),
            SporeSpawnDeathPaletteLayer.Background => ResolveDeathBackground(frame, color),
            _ => throw new ArgumentOutOfRangeException(nameof(layer)),
        };

    public static SporeSpawnColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        SporeSpawnColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SporeSpawnColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Spore Spawn color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Spore Spawn color JSON.", error);
        }
        if (document.Version != SporeSpawnColorFormat.Version)
            throw new InvalidDataException("Spore Spawn colors require the supported version.");
        return new(
            Compile(document.Spores, "spores"),
            CompileFrames(document.Health, SporeSpawnColorRomData.HealthFrameCount, "health"),
            CompileFrames(document.DeathSprite, SporeSpawnColorRomData.DeathSpriteFrameCount,
                "death sprite"),
            CompileFrames(document.DeathLevel, SporeSpawnColorRomData.DeathSceneFrameCount,
                "death level"),
            CompileFrames(document.DeathBackground, SporeSpawnColorRomData.DeathSceneFrameCount,
                "death background"));
    }

    public static byte[] Write(SporeSpawnColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static int CheckFrame(int frame, int count) =>
        (uint)frame < count ? frame : throw new ArgumentOutOfRangeException(nameof(frame));

    private static ushort Get(ushort[] colors, int color) =>
        (uint)color < colors.Length
            ? colors[color]
            : throw new ArgumentOutOfRangeException(nameof(color));

    private static ushort[][] CompileFrames(PaletteRgb5[][]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Spore Spawn {name} requires {count} frames.");
        return source.Select((frame, index) => Compile(frame, $"{name} frame {index}"))
            .ToArray();
    }

    private static ushort[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SporeSpawnColorRomData.ColorsPerFrame)
            throw new InvalidDataException(
                $"Spore Spawn {name} requires {SporeSpawnColorRomData.ColorsPerFrame} RGB5 colors.");
        var compiled = new ushort[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Spore Spawn {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate Spore Spawn color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record SporeSpawnColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Spores { get; init; }
    public required PaletteRgb5[][] Health { get; init; }
    public required PaletteRgb5[][] DeathSprite { get; init; }
    public required PaletteRgb5[][] DeathLevel { get; init; }
    public required PaletteRgb5[][] DeathBackground { get; init; }
}

public static class SporeSpawnColorFormat
{
    public const string FileName = "spore-spawn-colors.json";
    public const int Version = 1;
}
