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
    private readonly ushort[] hurt;
    private readonly ushort[] intro;

    private SamusHurtColorCatalog(ushort[] hurt, ushort[] intro)
    {
        this.hurt = hurt;
        this.intro = intro;
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
        ushort[] colors = variant switch
        {
            SamusHurtColorVariant.Hurt => hurt,
            SamusHurtColorVariant.Intro => intro,
            _ => throw new ArgumentOutOfRangeException(nameof(variant)),
        };
        return (uint)index < colors.Length
            ? colors[index] : throw new ArgumentOutOfRangeException(nameof(index));
    }

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

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Samus hurt color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
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
