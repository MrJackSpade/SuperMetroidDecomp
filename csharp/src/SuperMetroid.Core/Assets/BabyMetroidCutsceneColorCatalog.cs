using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable initial and six fade RGB5 images for the Mother Brain cutscene Baby.</summary>
public sealed class BabyMetroidCutsceneColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
        {
            initial.AppendIdentity(content);
            fade.AppendIdentity(content);
        });

    /// <summary>Compiled fifteen-color initial image, resolved independently of the source JSON arrays.</summary>
    private readonly BabyMetroidInitialPalette initial;
    /// <summary>Compiled six-frame death fade, using supplied rows when they differ from the calculated native fade.</summary>
    private readonly ColorFade fade;

    /// <summary>Creates the runtime catalog from already validated packed initial colors and chronological fade rows.</summary>
    private BabyMetroidCutsceneColorCatalog(ushort[] initial, ushort[][] fade)
    {
        this.initial = new(initial);
        this.fade = new(fade);
    }

    /// <summary>JSON rules shared by catalog loading and writing: camel-case names, rejection of unknown members, and indented output.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Resolves one initial Baby Metroid sprite color, corresponding to the fifteen-word image at <c>$A9:94D4</c>; no CGRAM write or cutscene state change occurs.</summary>
    /// <param name="color">Zero-based image index from zero through 14, corresponding to colors 1..15 of OBJ palette seven; the transparent slot is excluded.</param>
    /// <returns>A packed SNES RGB5 word: red in bits 0..4, green in bits 5..9, and blue in bits 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is outside the fifteen-color initial image.</exception>
    public ushort InitialColor(int color) => initial.Resolve(color);

    /// <summary>Resolves one displayed death fade-to-black color from native <c>BabyMetroidFadingToBlackPalettes_1</c> through <c>_6</c> at <c>$AD:E90C-$E9B3</c>, without advancing the fade timer.</summary>
    /// <param name="paletteIndex">One-based displayed image selector from one through six; the unused native image zero is not installed.</param>
    /// <param name="color">Zero-based image index from zero through 13, corresponding to OBJ palette seven colors 1..14; transparent color zero and black color 15 are excluded.</param>
    /// <returns>A packed SNES RGB5 word from the selected image, retaining independently supplied artwork edits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either selector is outside the installed fade images; the exception names <paramref name="paletteIndex"/> for both invalid cases.</exception>
    public ushort FadeColor(int paletteIndex, int color) =>
        paletteIndex is >= 1 and <= BabyMetroidCutsceneColorRomData.FadeFrameCount &&
        (uint)color < BabyMetroidCutsceneColorRomData.FadeColorCount
            ? fade.Resolve(paletteIndex - 1, color)
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex),
                $"Cutscene Baby fade index {paletteIndex}, color {color} is outside the authored images.");

    /// <summary>$AD:E90C-E9B3 displays the separately identified final-health paints
    /// at five-sevenths down to zero intensity. Supplied edits remain independent;
    /// no latent RGB8 endpoint reconstruction or stored stock sample is needed.</summary>
    private sealed class ColorFade
    {
        /// <summary>Deep-copied authored rows, present only when at least one entry differs from the native calculated fade.</summary>
        private readonly ushort[][]? supplied;

        /// <summary>Uses calculated native colors unless the authored rows contain any independently supplied edit.</summary>
        /// <param name="frames">Validated chronological rows, each containing the displayed fade image's packed RGB5 colors.</param>
        internal ColorFade(ushort[][] frames)
        {
            for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < frames[frame].Length; color++)
                if (Calculate(frame, color) != frames[frame][color])
                {
                    supplied = frames.Select(row => row.ToArray()).ToArray();
                    return;
                }
        }

        /// <summary>Returns an authored color when edits are present, or calculates that frame's native fade color on demand.</summary>
        internal ushort Resolve(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Gets the final-health paint color from which this cutscene's fade frames are derived.</summary>
        private static ushort Endpoint(int color) => BabyMetroidFinalHealthPaintDefinitions.Color(color);

        /// <summary>Calculates one fade sample by scaling each RGB5 component of its endpoint toward black.</summary>
        /// <param name="frame">Zero-based displayed fade frame, ordered from the brightest image to black.</param>
        /// <param name="color">Zero-based color within the fourteen entries replaced by the fade.</param>
        /// <returns>The packed RGB5 sample for the requested frame and color.</returns>
        private static ushort Calculate(int frame, int color)
        {
            ushort endpoint = Endpoint(color);
            int remaining = BabyMetroidCutsceneColorRomData.FadeFrameCount - 1 - frame;
            int intervals = BabyMetroidCutsceneColorRomData.FadeFrameCount + 1;
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= (((endpoint >> shift & 31) * remaining / intervals) << shift);
            return (ushort)result;
        }

        /// <summary>Adds the frame count and every resolved row in display order to the artwork identity.</summary>
        internal void AppendIdentity(SelectedPresentationHash content)
        {
            content.Append("fade", BabyMetroidCutsceneColorRomData.FadeFrameCount);
            Span<ushort> row = stackalloc ushort[BabyMetroidCutsceneColorRomData.FadeColorCount];
            for (int frame = 0; frame < BabyMetroidCutsceneColorRomData.FadeFrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(frame, color);
                content.AppendWords("row", row);
            }
        }
    }

    /// <summary>Compiles initial and fade RGB5 artwork from installed JSON, retaining edits independently of the calculated native color compositions.</summary>
    /// <param name="json">UTF-8 JSON input read from its current position; the stream remains open.</param>
    /// <returns>A catalog whose private compiled images are independent of the source document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON is malformed, null, contains duplicate or unknown properties, uses an unsupported version, or lacks fifteen initial colors and six fourteen-color fade rows with RGB channels from zero through 31.</exception>
    public static BabyMetroidCutsceneColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        BabyMetroidCutsceneColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<BabyMetroidCutsceneColorDocument>(JsonOptions) ??
                throw new InvalidDataException("Cutscene Baby color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid cutscene Baby color JSON.", error);
        }
        if (document.Version != BabyMetroidCutsceneColorFormat.Version)
            throw new InvalidDataException("Cutscene Baby colors require the supported version.");
        if (document.Fade is null ||
            document.Fade.Length != BabyMetroidCutsceneColorRomData.FadeFrameCount)
            throw new InvalidDataException("Cutscene Baby fade requires six displayed frames.");
        return new(Compile(document.Initial,
                BabyMetroidCutsceneColorRomData.InitialColorCount, "initial"),
            document.Fade.Select((frame, index) => Compile(frame,
                BabyMetroidCutsceneColorRomData.FadeColorCount,
                $"fade frame {index + 1}")).ToArray());
    }

    /// <summary>Serializes editable artwork as indented, camel-case UTF-8 JSON, then validates the result using the installed-content loader.</summary>
    /// <param name="document">Initial and ordered fade images to serialize; the supplied arrays are not modified.</param>
    /// <returns>A newly allocated JSON byte array for <see cref="BabyMetroidCutsceneColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document is null or fails schema, image-dimension, or RGB5 channel validation.</exception>
    public static byte[] Write(BabyMetroidCutsceneColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Validates an RGB5 color row and packs its channel values into SNES color words.</summary>
    /// <param name="source">Nullable JSON entries in native image order.</param>
    /// <param name="required">Exact number of colors required by the image.</param>
    /// <param name="name">Image label included in validation errors.</param>
    /// <returns>A newly allocated packed row.</returns>
    private static ushort[] Compile(PaletteRgb5[]? source, int required, string name)
    {
        if (source is null || source.Length != required)
            throw new InvalidDataException(
                $"Cutscene Baby {name} requires {required} RGB5 colors.");
        var compiled = new ushort[required];
        for (int color = 0; color < required; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Cutscene Baby {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    /// <summary>Rejects repeated JSON property names before deserialization can obscure them.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate cutscene Baby color property {name}."));
}

/// <summary>Editable JSON artwork for the Mother Brain cutscene Baby's initial image and six displayed death-fade images; array references and RGB5 entries remain caller-owned and mutable.</summary>
public sealed record BabyMetroidCutsceneColorDocument
{
    /// <summary>Schema version required to match <see cref="BabyMetroidCutsceneColorFormat.Version"/> when loading or writing.</summary>
    public required int Version { get; init; }
    /// <summary>Fifteen RGB5 entries in native initial-image order, mapping to OBJ palette seven colors 1..15 and excluding transparent color zero.</summary>
    public required PaletteRgb5[] Initial { get; init; }
    /// <summary>Six chronological RGB5 rows of fourteen colors each; row zero corresponds to displayed fade selector one, and each row replaces OBJ palette seven colors 1..14 without changing color 15.</summary>
    public required PaletteRgb5[][] Fade { get; init; }
}

/// <summary>Installed resource identity and JSON schema version for the Mother Brain cutscene Baby's initial and death-fade artwork, separate from health and pulse palettes.</summary>
public static class BabyMetroidCutsceneColorFormat
{
    /// <summary>Installed artwork filename loaded with enemy tile resources for the Baby's initial colors and six death-fade images.</summary>
    public const string FileName = "baby-metroid-cutscene-colors.json";
    /// <summary>Supported schema version, requiring a fifteen-color initial image and six fourteen-color fade rows.</summary>
    public const int Version = 1;
}
