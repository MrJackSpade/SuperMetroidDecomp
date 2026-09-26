using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable Crocomire fight, wall, projectile, skeleton-arm, and wall-spike RGB5 colors.
/// Phase transitions and the white hurt-flash decision remain engine-owned.
/// </summary>
public sealed class CrocomireColorCatalog
{
    private readonly ushort[] fightBody;
    private readonly ushort[] initialWall;
    private readonly ushort[] initialProjectile;
    private readonly ushort[] skeletonArm;
    private readonly ushort[] wallSpikes;

    private CrocomireColorCatalog(ushort[] fightBody, ushort[] initialWall,
        ushort[] initialProjectile, ushort[] skeletonArm, ushort[] wallSpikes)
    {
        this.fightBody = fightBody;
        this.initialWall = initialWall;
        this.initialProjectile = initialProjectile;
        this.skeletonArm = skeletonArm;
        this.wallSpikes = wallSpikes;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveFightBody(int color) => Get(fightBody, color);
    public ushort ResolveInitialWall(int color) => Get(initialWall, color);
    public ushort ResolveInitialProjectile(int color) => Get(initialProjectile, color);
    public ushort ResolveSkeletonArm(int color) => Get(skeletonArm, color);
    public ushort ResolveWallSpikes(int color) => Get(wallSpikes, color);

    public void ApplyInitial(SnesCgram cgram)
    {
        Apply(cgram, initialWall, CrocomirePaletteRomData.InitialWallDestination);
        Apply(cgram, initialProjectile,
            CrocomirePaletteRomData.InitialProjectileDestination);
    }

    public void ApplyFightBody(SnesCgram cgram) =>
        Apply(cgram, fightBody, CrocomirePaletteRomData.FightBodyDestination);

    public void ApplySkeletonArm(SnesCgram cgram) =>
        Apply(cgram, skeletonArm, CrocomirePaletteRomData.SkeletonArmDestination);

    public void ApplyWallSpikes(SnesCgram cgram) =>
        Apply(cgram, wallSpikes, CrocomirePaletteRomData.WallSpikesDestination);

    public static CrocomireColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CrocomireColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CrocomireColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Crocomire color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Crocomire color JSON.", error);
        }
        if (document.Version != CrocomireColorFormat.Version)
            throw new InvalidDataException("Crocomire colors require the supported version.");
        return new(
            Compile(document.FightBody, CrocomirePaletteRomData.FightBodyCount, "fight body"),
            Compile(document.InitialWall, CrocomirePaletteRomData.InitialWallCount,
                "initial wall"),
            Compile(document.InitialProjectile, CrocomirePaletteRomData.InitialProjectileCount,
                "initial projectile"),
            Compile(document.SkeletonArm, CrocomirePaletteRomData.SkeletonArmCount,
                "skeleton arm"),
            Compile(document.WallSpikes, CrocomirePaletteRomData.WallSpikesCount,
                "wall spikes"));
    }

    public static byte[] Write(CrocomireColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort Get(ushort[] colors, int index)
    {
        if ((uint)index >= colors.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors[index];
    }

    private static void Apply(SnesCgram cgram, ushort[] colors, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < colors.Length; color++)
            cgram.SetColor(destination + color, colors[color]);
    }

    private static ushort[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Crocomire {name} requires {count} RGB5 colors.");
        var compiled = new ushort[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Crocomire {name} color {color} requires RGB5 channels 0..31.");
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
                        $"Duplicate Crocomire color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record CrocomireColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] FightBody { get; init; }
    public required PaletteRgb5[] InitialWall { get; init; }
    public required PaletteRgb5[] InitialProjectile { get; init; }
    public required PaletteRgb5[] SkeletonArm { get; init; }
    public required PaletteRgb5[] WallSpikes { get; init; }
}

public static class CrocomireColorFormat
{
    public const string FileName = "crocomire-colors.json";
    public const int Version = 1;
}
