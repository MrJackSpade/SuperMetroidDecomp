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
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
        {
            content.AppendWords("spores", Enumerable.Range(0, SporeSpawnColorRomData.ColorsPerFrame).Select(ResolveSpore).ToArray());
            content.AppendWordFrames("health", health);
            content.AppendWordFrames("deathSprite", Enumerable.Range(0, SporeSpawnColorRomData.DeathSpriteFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorRomData.ColorsPerFrame)
                    .Select(color => ResolveDeathSprite(frame, color)).ToArray()).ToArray());
            content.AppendWordFrames("deathLevel", Enumerable.Range(0, SporeSpawnColorRomData.DeathSceneFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorRomData.ColorsPerFrame)
                    .Select(color => ResolveDeathLevel(frame, color)).ToArray()).ToArray());
            content.AppendWordFrames("deathBackground", Enumerable.Range(0, SporeSpawnColorRomData.DeathSceneFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorRomData.ColorsPerFrame)
                    .Select(color => ResolveDeathBackground(frame, color)).ToArray()).ToArray());
        });

    private readonly Dictionary<int, ushort> spores = new();
    private readonly ushort[][] health;
    // Endpoint colors remain required inputs; intermediate entries store independent edits only.
    private readonly Dictionary<int, ushort> deathSprite = new();
    private readonly Dictionary<int, ushort> deathLevel = new();
    private readonly Dictionary<int, ushort> deathBackground = new();

    private SporeSpawnColorCatalog(ushort[] spores, ushort[][] health,
        ushort[][] deathSprite, ushort[][] deathLevel, ushort[][] deathBackground)
    {
        for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
            if (spores[color] != health[0][color]) this.spores[color] = spores[color];
        this.health = health;
        int last = SporeSpawnColorRomData.DeathSpriteFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
        {
            if (deathSprite[0][color] != health[SporeSpawnColorRomData.HealthFrameCount - 1][color])
                this.deathSprite[color] = deathSprite[0][color];
            this.deathSprite[last * SporeSpawnColorRomData.ColorsPerFrame + color] = deathSprite[last][color];
            for (int frame = 1; frame < last; frame++)
                if (deathSprite[frame][color] != CalculateDeathSpriteColor(deathSprite[0][color], deathSprite[last][color], frame))
                    this.deathSprite[frame * SporeSpawnColorRomData.ColorsPerFrame + color] = deathSprite[frame][color];
        }
        int levelLast = SporeSpawnColorRomData.DeathSceneFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
        {
            this.deathLevel[levelLast * SporeSpawnColorRomData.ColorsPerFrame + color] = deathLevel[levelLast][color];
            for (int frame = 0; frame < levelLast; frame++)
                if (!SporeSpawnDeathColorDefinitions.TryLevelColor(deathLevel[levelLast][color], frame, color, out ushort calculated)
                    || deathLevel[frame][color] != calculated)
                    this.deathLevel[frame * SporeSpawnColorRomData.ColorsPerFrame + color] = deathLevel[frame][color];
        }
        int backgroundLast = SporeSpawnColorRomData.DeathSceneFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
        {
            this.deathBackground[backgroundLast * SporeSpawnColorRomData.ColorsPerFrame + color] = deathBackground[backgroundLast][color];
            for (int frame = 0; frame < backgroundLast; frame++)
                if (deathBackground[frame][color] != SporeSpawnDeathColorDefinitions.BackgroundColor(deathBackground[backgroundLast][color], frame, color))
                    this.deathBackground[frame * SporeSpawnColorRomData.ColorsPerFrame + color] = deathBackground[frame][color];
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>
    /// $A5:E359..E378 repeats the healthy body/spawner row at $E379..E398 in sprite
    /// palette7. The chosen healthy colors remain required under the health payload.
    /// Independently supplied spore colors override this original shared coloring.
    /// </summary>
    public ushort ResolveSpore(int color)
    {
        if ((uint)color >= SporeSpawnColorRomData.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return spores.TryGetValue(color, out ushort selected) ? selected : health[0][color];
    }
    public ushort ResolveHealth(int frame, int color) => Get(health[CheckFrame(
        frame, SporeSpawnColorRomData.HealthFrameCount)], color);
    public ushort ResolveDeathSprite(int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorRomData.DeathSpriteFrameCount);
        if ((uint)color >= SporeSpawnColorRomData.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (deathSprite.TryGetValue(frame * SporeSpawnColorRomData.ColorsPerFrame + color, out ushort selected))
            return selected;
        ushort first = deathSprite.TryGetValue(color, out ushort suppliedFirst)
            ? suppliedFirst : health[SporeSpawnColorRomData.HealthFrameCount - 1][color];
        return CalculateDeathSpriteColor(first, deathSprite[
            (SporeSpawnColorRomData.DeathSpriteFrameCount - 1) * SporeSpawnColorRomData.ColorsPerFrame + color], frame);
    }

    /// <summary>
    /// $A5:E3F9..E4F8, Palette_SporeSpawn_DeathSequence_0..7: interpolate each RGB5
    /// channel between the first and last rows, rounding to the nearest integer over
    /// seven intervals. The first endpoint repeats critical-health row3 (..E3F8);
    /// its sixteen colors remain required under health. Sixteen final colors also remain required.
    /// </summary>
    internal static ushort CalculateDeathSpriteColor(ushort first, ushort last, int frame)
    {
        int intervals = SporeSpawnColorRomData.DeathSpriteFrameCount - 1;
        int result = 0;
        for (int shift = 0; shift <= 10; shift += 5)
        {
            int start = (first >> shift) & 31;
            int end = (last >> shift) & 31;
            int channel = (start * (intervals - frame) + end * frame + intervals / 2) / intervals;
            result |= channel << shift;
        }
        return (ushort)result;
    }
    public ushort ResolveDeathLevel(int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorRomData.DeathSceneFrameCount);
        if ((uint)color >= SporeSpawnColorRomData.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (deathLevel.TryGetValue(frame * SporeSpawnColorRomData.ColorsPerFrame + color, out ushort selected))
            return selected;
        ushort finalColor = deathLevel[(SporeSpawnColorRomData.DeathSceneFrameCount - 1) * SporeSpawnColorRomData.ColorsPerFrame + color];
        if (SporeSpawnDeathColorDefinitions.TryLevelColor(finalColor, frame, color, out ushort calculated))
            return calculated;
        throw new InvalidOperationException("Required Spore Spawn level color is absent.");
    }
    public ushort ResolveDeathBackground(int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorRomData.DeathSceneFrameCount);
        if ((uint)color >= SporeSpawnColorRomData.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (deathBackground.TryGetValue(frame * SporeSpawnColorRomData.ColorsPerFrame + color, out ushort selected))
            return selected;
        ushort finalColor = deathBackground[(SporeSpawnColorRomData.DeathSceneFrameCount - 1) * SporeSpawnColorRomData.ColorsPerFrame + color];
        return SporeSpawnDeathColorDefinitions.BackgroundColor(finalColor, frame, color);
    }

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
