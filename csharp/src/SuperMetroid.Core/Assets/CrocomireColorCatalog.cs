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
    // Only independently supplied differences are stored; every stock ink calculates.
    private readonly Dictionary<(Band Band, int Color), Bgr555> edits = [];

    private CrocomireColorCatalog(Bgr555[] fightBody, Bgr555[] initialWall,
        Bgr555[] initialProjectile, Bgr555[] skeletonArm, Bgr555[] wallSpikes)
    {
        Bgr555[][] supplied = [fightBody, initialWall, initialProjectile, skeletonArm, wallSpikes];
        for (int band = 0; band < supplied.Length; band++)
            for (int color = 0; color < supplied[band].Length; color++)
                if (supplied[band][color] != Stock((Band)band, color))
                    edits.Add(((Band)band, color), supplied[band][color]);
    }

    /// <summary>Each named material calculates its native paint and transfer overlaps; supplied edits never alias across fields.</summary>
    private static Bgr555 Stock(Band band, int color) => band switch
    {
        Band.FightBody => CrocomirePaintDefinitions.FightBody(color),
        Band.InitialWall => CrocomirePaintDefinitions.InitialWall(color),
        Band.InitialProjectile => CrocomirePaintDefinitions.InitialProjectile(color),
        Band.SkeletonArm => CrocomirePaintDefinitions.SkeletonArm(color),
        Band.WallSpikes => CrocomirePaintDefinitions.WallSpikes(color),
        _ => throw new ArgumentOutOfRangeException(nameof(band)),
    };
    private static int Count(Band band) => band switch
    {
        Band.FightBody => CrocomirePaletteRomData.FightBodyCount,
        Band.InitialWall => CrocomirePaletteRomData.InitialWallCount,
        Band.InitialProjectile => CrocomirePaletteRomData.InitialProjectileCount,
        Band.SkeletonArm => CrocomirePaletteRomData.SkeletonArmCount,
        Band.WallSpikes => CrocomirePaletteRomData.WallSpikesCount,
        _ => throw new ArgumentOutOfRangeException(nameof(band)),
    };

    private void Append(SelectedPresentationHash content, Band band, string label)
    {
        Span<Bgr555> transfer = stackalloc Bgr555[Count(band)];
        for (int color = 0; color < transfer.Length; color++) transfer[color] = Get(band, color);
        content.AppendColors(label, transfer);
    }
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Installs the native initialization transfers in order: seventeen wall words at CGRAM 160–176, then seventeen projectile words at 208–224, retaining each source overlap word.</summary>
    public void ApplyInitial(SnesCgram cgram)
    {
        Apply(cgram, Band.InitialWall, CrocomirePaletteRomData.InitialWallDestination);
        Apply(cgram, Band.InitialProjectile,
            CrocomirePaletteRomData.InitialProjectileDestination);
    }

    /// <summary>Restores the eight selected body/head BG inks from the $A4:B89D role at CGRAM 112–119; the actor separately decides whether a hurt update should show white instead.</summary>
    public void ApplyFightBody(SnesCgram cgram) =>
        Apply(cgram, Band.FightBody, CrocomirePaletteRomData.FightBodyDestination);

    /// <summary>Installs the sixteen skeleton-arm OBJ inks corresponding to $A4:B8FD at CGRAM 144–159 when the wall-break sequence begins.</summary>
    public void ApplySkeletonArm(SnesCgram cgram) =>
        Apply(cgram, Band.SkeletonArm, CrocomirePaletteRomData.SkeletonArmDestination);

    /// <summary>Installs the sixteen wall-spike OBJ inks corresponding to $A4:B91D at CGRAM 176–191 when the rumble script reaches its terminator.</summary>
    public void ApplyWallSpikes(SnesCgram cgram) =>
        Apply(cgram, Band.WallSpikes, CrocomirePaletteRomData.WallSpikesDestination);

    /// <summary>Loads version-1 <c>crocomire-colors.json</c>, validating the five native transfer lengths and RGB5 channel bounds while keeping hurt-flash and death transitions in actor code.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Selected compiled inks, preserving independent supplied edits even where stock source transfers overlap.</returns>
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

    /// <summary>Serializes the five editable color bands as indented camel-case UTF-8 JSON, validating version, transfer lengths, and RGB5 channels before returning the bytes.</summary>
    public static byte[] Write(CrocomireColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private Bgr555 Get(Band band, int index)
    {
        if ((uint)index >= Count(band)) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue((band, index), out Bgr555 edited) ? edited : Stock(band, index);
    }

    private void Apply(SnesCgram cgram, Band band, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < Count(band); color++)
            cgram.SetColor(destination + color, Get(band, color));
    }
    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Crocomire {name} requires {count} RGB5 colors.");
        var compiled = new Bgr555[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Crocomire {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToBgr555();
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Crocomire color property {name}."));
}

/// <summary>Editable RGB5 payloads for Crocomire's five palette transfers; each channel must be 0–31, and the native seventeen-word initialization overlaps remain explicit inputs.</summary>
public sealed record CrocomireColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Eight body/head BG inks corresponding to $A4:B89D, restored at CGRAM 112–119 on nonwhite hurt-flash updates.</summary>
    public required PaletteRgb5[] FightBody { get; init; }
    /// <summary>Seventeen ordered wall initialization words corresponding to $A4:B8BD; word 16 overlaps the stock projectile source's first word but remains independently editable here.</summary>
    public required PaletteRgb5[] InitialWall { get; init; }
    /// <summary>Seventeen ordered projectile initialization words corresponding to $A4:B8DD; word 16 overlaps the stock skeleton-arm source's first word but remains independently editable here.</summary>
    public required PaletteRgb5[] InitialProjectile { get; init; }
    /// <summary>Sixteen skeleton-arm OBJ inks corresponding to $A4:B8FD, installed in OBJ palette 1 during wall break.</summary>
    public required PaletteRgb5[] SkeletonArm { get; init; }
    /// <summary>Sixteen wall-spike OBJ inks corresponding to $A4:B91D, installed in OBJ palette 3 at the rumble terminator.</summary>
    public required PaletteRgb5[] WallSpikes { get; init; }
}

/// <summary>Installed filename and supported schema revision for Crocomire's bounded editable palette-transfer payloads.</summary>
public static class CrocomireColorFormat
{
    /// <summary>Installed editable JSON filename for fight-body, initial-wall/projectile, skeleton-arm, and wall-spike colors.</summary>
    public const string FileName = "crocomire-colors.json";
    /// <summary>Supported color schema revision, requiring transfer lengths of 8, 17, 17, 16, and 16 RGB5 words.</summary>
    public const int Version = 1;
}
