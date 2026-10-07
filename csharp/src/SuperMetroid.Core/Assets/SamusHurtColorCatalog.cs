using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>The two mutually exclusive full-body palettes in Samus's hurt handler.</summary>
public enum SamusHurtColorVariant
{
    Hurt,
    Intro,
}

/// <summary>Editable RGB5 artwork for the hurt flash and cinematic restoration.</summary>
public sealed class SamusHurtColorCatalog
{
    // Only independent installed edits are stored; native shade samples all calculate.
    private readonly ushort? hurtZero;
    private readonly ushort? introZero;
    private readonly Dictionary<int, byte> introLevels;
    private readonly Dictionary<int, ushort> introOverrides;
    private readonly Dictionary<int, ushort> hurtOverrides;

    private SamusHurtColorCatalog(ushort[] hurt, ushort[] intro)
    {
        hurtZero = hurt[0] == SamusHurtColorDefinitions.HurtTransparentWord ? null : hurt[0];
        introZero = intro[0] == SamusHurtColorDefinitions.IntroTransparentWord ? null : intro[0];
        introLevels = Enumerable.Range(1, 15)
            .Where(index => (intro[index] & 31) != SamusHurtColorDefinitions.DefaultIntroLevel(index))
            .ToDictionary(index => index, index => (byte)(intro[index] & 31));
        introOverrides = Enumerable.Range(1, 15)
            .Where(index => intro[index] != SamusHurtColorDefinitions.IntroFromLevel(IntroLevel(index)))
            .ToDictionary(index => index, index => intro[index]);
        hurtOverrides = Enumerable.Range(1, 15)
            .Where(index => hurt[index] != SamusHurtColorDefinitions.HurtFromIntro(intro[index]))
            .ToDictionary(index => index, index => hurt[index]);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public static SamusHurtColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        SamusHurtColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusHurtColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Samus hurt color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Samus hurt color JSON.", error);
        }
        if (document.Version != SamusHurtColorFormat.Version)
            throw new InvalidDataException("Samus hurt colors require the supported version.");
        return new(Compile(document.Hurt, "hurt"), Compile(document.Intro, "intro"));
    }

    public static byte[] Write(SamusHurtColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Returns one display color; counter, phase, and sound rules remain in code.</summary>
    public ushort Resolve(SamusHurtColorVariant variant, int index)
    {
        if (variant is not (SamusHurtColorVariant.Hurt or SamusHurtColorVariant.Intro))
            throw new ArgumentOutOfRangeException(nameof(variant));
        if ((uint)index >= SamusHurtColorFormat.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (variant == SamusHurtColorVariant.Intro) return Intro(index);
        return index == 0 ? hurtZero ?? SamusHurtColorDefinitions.HurtTransparentWord : hurtOverrides.TryGetValue(index, out ushort value)
            ? value : SamusHurtColorDefinitions.HurtFromIntro(Intro(index));
    }

    private byte IntroLevel(int index) => introLevels.TryGetValue(index, out byte level) ? level : SamusHurtColorDefinitions.DefaultIntroLevel(index);
    private ushort Intro(int index) => index == 0 ? introZero ?? SamusHurtColorDefinitions.IntroTransparentWord : introOverrides.TryGetValue(index, out ushort value)
        ? value : SamusHurtColorDefinitions.IntroFromLevel(IntroLevel(index));
    private static ushort[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusHurtColorFormat.ColorsPerPalette)
            throw new InvalidDataException($"Samus {name} palette requires sixteen RGB5 colors.");
        var colors = new ushort[source.Length];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus {name} color {index} requires RGB components from zero through 31.");
            colors[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return colors;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Samus hurt color property {name}."));
}

public sealed record SamusHurtColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Hurt { get; init; }
    public required PaletteRgb5[] Intro { get; init; }
}

public static class SamusHurtColorFormat
{
    public const string FileName = "samus-hurt-colors.json";
    public const int Version = 1;
    public const int ColorsPerPalette = SamusPaletteRomData.Common.ColorsPerObjPalette;
    /// <summary>$9B:A380, the sixteen-color hurt-flash palette copied on odd calls.</summary>
    public const int HurtSourceAddress = SamusPaletteRomData.HurtFlash.Colors;
    /// <summary>$9B:A3A0, the sixteen-color cinematic restoration palette.</summary>
    public const int IntroSourceAddress = SamusPaletteRomData.HurtFlash.IntroColors;
}
