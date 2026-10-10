using SuperMetroid.Core.Hardware;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>The two mutually exclusive full-body palettes in Samus's hurt handler.</summary>
public enum SamusHurtColorVariant
{
    /// <summary>Bright flash from native <c>SamusPalettes_HurtFlash</c> at <c>$9B:A380</c>, selected on hurt-counter calls one, three, and five.</summary>
    Hurt,
    /// <summary>Gray-tinted restoration from native <c>SamusPalettes_Intro</c> at <c>$9B:A3A0</c>, selected on even hurt-counter calls below seven while a cinematic is active; ordinary gameplay restores the equipped suit instead.</summary>
    Intro,
}

/// <summary>Editable RGB5 artwork for the hurt flash and cinematic restoration.</summary>
public sealed class SamusHurtColorCatalog
{
    // Only independent installed edits are stored; native shade samples all calculate.
    private readonly Bgr555? hurtZero;
    private readonly Bgr555? introZero;
    private readonly Dictionary<int, byte> introLevels;
    private readonly Dictionary<int, Bgr555> introOverrides;
    private readonly Dictionary<int, Bgr555> hurtOverrides;

    private SamusHurtColorCatalog(Bgr555[] hurt, Bgr555[] intro)
    {
        hurtZero = hurt[0] == SamusHurtColorDefinitions.HurtTransparentWord ? null : hurt[0];
        introZero = intro[0] == SamusHurtColorDefinitions.IntroTransparentWord ? null : intro[0];
        introLevels = Enumerable.Range(1, 15)
            .Where(index => (intro[index].Red) != SamusHurtColorDefinitions.DefaultIntroLevel(index))
            .ToDictionary(index => index, index => (byte)(intro[index].Red));
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

    /// <summary>Compiles the installed hurt and cinematic palettes into independent color edits and calculated native shade relationships.</summary>
    /// <param name="json">UTF-8 JSON input, consumed from its current position and left open.</param>
    /// <returns>A catalog with private compiled color state, independent of the input document's arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON is malformed, null, has duplicate or unknown properties, uses an unsupported version, or does not supply two sixteen-color palettes with RGB channels from zero through 31.</exception>
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

    /// <summary>Serializes a palette document as indented, camel-case UTF-8 JSON and validates it through the same loader used for installed content.</summary>
    /// <param name="document">Hurt and cinematic RGB5 artwork to serialize; its arrays are not modified.</param>
    /// <returns>A newly allocated JSON byte array suitable for <see cref="SamusHurtColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document is null or fails the supported version, palette length, or RGB5 channel requirements.</exception>
    public static byte[] Write(SamusHurtColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Returns one display color; counter, phase, and sound rules remain in code.</summary>
    /// <param name="variant">Flash or cinematic restoration palette; this method does not select the palette from gameplay state.</param>
    /// <param name="index">Zero-based OBJ palette color index, from zero through 15; zero retains the independently editable transparent-slot payload.</param>
    /// <returns>A packed SNES color word with red in bits 0..4, green in bits 5..9, and blue in bits 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="variant"/> is undefined or <paramref name="index"/> is outside the sixteen-color palette.</exception>
    public Bgr555 Resolve(SamusHurtColorVariant variant, int index)
    {
        if (variant is not (SamusHurtColorVariant.Hurt or SamusHurtColorVariant.Intro))
            throw new ArgumentOutOfRangeException(nameof(variant));
        if ((uint)index >= SamusHurtColorFormat.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (variant == SamusHurtColorVariant.Intro) return Intro(index);
        return index == 0 ? hurtZero ?? SamusHurtColorDefinitions.HurtTransparentWord : hurtOverrides.TryGetValue(index, out Bgr555 value)
            ? value : SamusHurtColorDefinitions.HurtFromIntro(Intro(index));
    }

    private byte IntroLevel(int index) => introLevels.TryGetValue(index, out byte level) ? level : SamusHurtColorDefinitions.DefaultIntroLevel(index);
    private Bgr555 Intro(int index) => index == 0 ? introZero ?? SamusHurtColorDefinitions.IntroTransparentWord : introOverrides.TryGetValue(index, out Bgr555 value)
        ? value : SamusHurtColorDefinitions.IntroFromLevel(IntroLevel(index));
    private static Bgr555[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusHurtColorFormat.ColorsPerPalette)
            throw new InvalidDataException($"Samus {name} palette requires sixteen RGB5 colors.");
        var colors = new Bgr555[source.Length];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus {name} color {index} requires RGB components from zero through 31.");
            colors[index] = color.ToBgr555();
        }
        return colors;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Samus hurt color property {name}."));
}

/// <summary>Editable JSON payload for the two complete Samus OBJ palettes; initialized array references and their RGB5 entries remain caller-owned and mutable.</summary>
public sealed record SamusHurtColorDocument
{
    /// <summary>Schema version required to match <see cref="SamusHurtColorFormat.Version"/> when loading or writing.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen RGB5 colors in native hurt-flash pen order, including transparent slot zero; replaces Samus OBJ palette four on odd flash calls.</summary>
    public required PaletteRgb5[] Hurt { get; init; }
    /// <summary>Sixteen RGB5 colors in native intro pen order, including transparent slot zero; supplies cinematic restoration rather than the equipment-dependent normal suit palette.</summary>
    public required PaletteRgb5[] Intro { get; init; }
}

/// <summary>Installed JSON identity, schema dimensions, and extraction-only cartridge locations for Samus's hurt and cinematic full-body colors.</summary>
public static class SamusHurtColorFormat
{
    /// <summary>Installed resource filename loaded by the area-map presentation catalog for hurt-flash and cinematic restoration artwork.</summary>
    public const string FileName = "samus-hurt-colors.json";
    /// <summary>Supported schema version, requiring both complete sixteen-color RGB5 arrays.</summary>
    public const int Version = 1;
    /// <summary>Number of colors in one SNES OBJ palette: sixteen, including the transparent first slot.</summary>
    public const int ColorsPerPalette = SamusPaletteRomData.Common.ColorsPerObjPalette;
    /// <summary>Extraction address <c>$9B:A380</c>, native <c>SamusPalettes_HurtFlash</c>: sixteen little-endian SNES color words copied on odd hurt-counter calls below seven.</summary>
    public const int HurtSourceAddress = SamusPaletteRomData.HurtFlash.Colors;
    /// <summary>Extraction address <c>$9B:A3A0</c>, native <c>SamusPalettes_Intro</c>: sixteen little-endian SNES color words used for cinematic restoration, distinct from normal suit artwork.</summary>
    public const int IntroSourceAddress = SamusPaletteRomData.HurtFlash.IntroColors;
}
