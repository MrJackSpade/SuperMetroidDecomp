using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable RGB5 artwork for the hurt flash and cinematic restoration.</summary>
public sealed class SamusHurtColorCatalog
{
    /// <summary>Optional independent edit to the hurt palette's transparent-slot word.</summary>
    internal readonly Bgr555? hurtZero;
    /// <summary>Optional independent edit to the intro palette's transparent-slot word.</summary>
    internal readonly Bgr555? introZero;
    /// <summary>Intro shade levels that differ from the calculated native levels.</summary>
    internal readonly Dictionary<int, byte> introLevels;
    /// <summary>Intro colors whose full RGB5 words cannot be derived from their shade levels.</summary>
    internal readonly Dictionary<int, Bgr555> introOverrides;
    /// <summary>Hurt colors that differ from the corresponding intro-derived native flash shade.</summary>
    internal readonly Dictionary<int, Bgr555> hurtOverrides;

    /// <summary>Retains only independent palette edits and derives unchanged native shade relationships.</summary>
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

    /// <summary>Strict camel-case JSON settings shared by hurt palette reading and writing.</summary>
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
        Ensure.NotNull(json);
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

    /// <summary>Gets an independently edited intro shade level or its native default.</summary>
    private byte IntroLevel(int index) => introLevels.TryGetValue(index, out byte level) ? level : SamusHurtColorDefinitions.DefaultIntroLevel(index);
    /// <summary>Gets an edited intro color or derives it from the selected shade level.</summary>
    private Bgr555 Intro(int index) => index == 0 ? introZero ?? SamusHurtColorDefinitions.IntroTransparentWord : introOverrides.TryGetValue(index, out Bgr555 value)
        ? value : SamusHurtColorDefinitions.IntroFromLevel(IntroLevel(index));
    /// <summary>Checks one complete sixteen-color RGB5 document array and converts it to packed colors.</summary>
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

    /// <summary>Rejects duplicate palette properties before JSON deserialization can overwrite them.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Samus hurt color property {name}."));
}
