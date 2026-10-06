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
    /// <summary>Canonical selected colors preserve the five native transfer labels/order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("CrocomireColorCatalog-v1", content =>
    {
        Append(content, Band.FightBody, "fightBody");
        Append(content, Band.InitialWall, "initialWall");
        Append(content, Band.InitialProjectile, "initialProjectile");
        Append(content, Band.SkeletonArm, "skeletonArm");
        Append(content, Band.WallSpikes, "wallSpikes");
    });

    private enum Band { FightBody, InitialWall, InitialProjectile, SkeletonArm, WallSpikes }
    // Remaining paint and selected sharing relationships are REQUIRED source inputs.
    private readonly Dictionary<(Band Band, int Color), ushort> paint = [];
    private readonly Dictionary<(Band Band, int Color), ushort> edits = [];

    private CrocomireColorCatalog(ushort[] fightBody, ushort[] initialWall,
        ushort[] initialProjectile, ushort[] skeletonArm, ushort[] wallSpikes)
    {
        ushort[][] supplied = [fightBody, initialWall, initialProjectile, skeletonArm, wallSpikes];
        for (int band = 0; band < supplied.Length; band++)
            for (int color = 0; color < supplied[band].Length; color++)
            {
                var source = SharedSource((Band)band, color);
                paint.TryAdd(source, supplied[(int)source.Band][source.Color]);
            }
        for (int band = 0; band < supplied.Length; band++)
            for (int color = 0; color < supplied[band].Length; color++)
                if (supplied[band][color] != paint[SharedSource((Band)band, color)])
                    edits.Add(((Band)band, color), supplied[band][color]);
    }

    /// <summary>
    /// $A4:B8BD wall slots2..6 share neutral paint; $B8FD skeleton slots2..6
    /// repeat slots7..11 and share white/first bone paint with body $B89D.
    /// Seventeen-word initial transfers overlap the next palette at $B8DD/$B8FD.
    /// Supplied edits remain independent even across those native source overlaps.
    /// </summary>
    private static (Band Band, int Color) SharedSource(Band band, int color) => (band, color) switch
    {
        (Band.InitialWall, 16) => (Band.InitialProjectile, 0),
        (Band.InitialProjectile, 16) => (Band.SkeletonArm, 0),
        (Band.InitialWall, >= 3 and <= 6) => (Band.InitialWall, 2),
        (Band.SkeletonArm, 1) => (Band.FightBody, 1),
        (Band.SkeletonArm, 2 or 7) => (Band.FightBody, 7),
        (Band.SkeletonArm, >= 3 and <= 6) => (Band.SkeletonArm, color + 5),
        _ => (band, color),
    };

    private static int Count(Band band) => band switch
    {
        Band.FightBody => CrocomirePaletteRomData.FightBodyCount,
        Band.InitialWall => CrocomirePaletteRomData.InitialWallCount,
        Band.InitialProjectile => CrocomirePaletteRomData.InitialProjectileCount,
        Band.SkeletonArm => CrocomirePaletteRomData.SkeletonArmCount,
        _ => CrocomirePaletteRomData.WallSpikesCount,
    };

    private void Append(SelectedPresentationHash content, Band band, string label)
    {
        Span<ushort> transfer = stackalloc ushort[Count(band)];
        for (int color = 0; color < transfer.Length; color++) transfer[color] = Get(band, color);
        content.AppendWords(label, transfer);
    }
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveFightBody(int color) => Get(Band.FightBody, color);
    public ushort ResolveInitialWall(int color) => Get(Band.InitialWall, color);
    public ushort ResolveInitialProjectile(int color) => Get(Band.InitialProjectile, color);
    public ushort ResolveSkeletonArm(int color) => Get(Band.SkeletonArm, color);
    public ushort ResolveWallSpikes(int color) => Get(Band.WallSpikes, color);

    public void ApplyInitial(SnesCgram cgram)
    {
        Apply(cgram, Band.InitialWall, CrocomirePaletteRomData.InitialWallDestination);
        Apply(cgram, Band.InitialProjectile,
            CrocomirePaletteRomData.InitialProjectileDestination);
    }

    public void ApplyFightBody(SnesCgram cgram) =>
        Apply(cgram, Band.FightBody, CrocomirePaletteRomData.FightBodyDestination);

    public void ApplySkeletonArm(SnesCgram cgram) =>
        Apply(cgram, Band.SkeletonArm, CrocomirePaletteRomData.SkeletonArmDestination);

    public void ApplyWallSpikes(SnesCgram cgram) =>
        Apply(cgram, Band.WallSpikes, CrocomirePaletteRomData.WallSpikesDestination);

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

    private ushort Get(Band band, int index)
    {
        if ((uint)index >= Count(band)) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue((band, index), out ushort edited) ? edited : paint[SharedSource(band, index)];
    }

    private void Apply(SnesCgram cgram, Band band, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < Count(band); color++)
            cgram.SetColor(destination + color, Get(band, color));
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
