using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable initial and six fade RGB5 images for the Mother Brain cutscene Baby.</summary>
public sealed class BabyMetroidCutsceneColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
        {
            initial.AppendIdentity(content);
            fade.AppendIdentity(content);
        });

    /// <summary>Compiled fifteen-color image shown before the death fade.</summary>
    internal readonly BabyMetroidInitialPalette initial;
    /// <summary>Six displayed fade rows, with native formula or supplied edited colors.</summary>
    internal readonly ColorFade fade;

    /// <summary>Copies compiled images into immutable catalog-owned palette views.</summary>
    private BabyMetroidCutsceneColorCatalog(Bgr555[] initial, Bgr555[][] fade)
    {
        this.initial = new(initial);
        this.fade = new(fade);
    }

    /// <summary>Strict camel-case artwork schema that rejects unknown JSON members.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Resolves one initial Baby Metroid sprite color, corresponding to the fifteen-word image at <c>$A9:94D4</c>; no CGRAM write or cutscene state change occurs.</summary>
    /// <param name="color">Zero-based image index from zero through 14, corresponding to colors 1..15 of OBJ palette seven; the transparent slot is excluded.</param>
    /// <returns>The image color.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is outside the fifteen-color initial image.</exception>
    public Bgr555 InitialColor(int color) => initial.Resolve(color);

    /// <summary>Resolves one displayed death fade-to-black color from native <c>BabyMetroidFadingToBlackPalettes_1</c> through <c>_6</c> at <c>$AD:E90C-$E9B3</c>, without advancing the fade timer.</summary>
    /// <param name="paletteIndex">One-based displayed image selector from one through six; the unused native image zero is not installed.</param>
    /// <param name="color">Zero-based image index from zero through 13, corresponding to OBJ palette seven colors 1..14; transparent color zero and black color 15 are excluded.</param>
    /// <returns>The selected image color, retaining independently supplied artwork edits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either selector is outside the installed fade images; the exception names <paramref name="paletteIndex"/> for both invalid cases.</exception>
    public Bgr555 FadeColor(int paletteIndex, int color) =>
        paletteIndex is >= 1 and <= BabyMetroidCutsceneColorRomData.FadeFrameCount &&
        (uint)color < BabyMetroidCutsceneColorRomData.FadeColorCount
            ? fade.Resolve(paletteIndex - 1, color)
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex),
                $"Cutscene Baby fade index {paletteIndex}, color {color} is outside the authored images.");

    /// <summary>$AD:E90C-E9B3 displays the separately identified final-health paints
    /// at five-sevenths down to zero intensity. Supplied edits remain independent;
    /// no latent RGB8 endpoint reconstruction or stored stock sample is needed.</summary>
    internal sealed class ColorFade
    {
        /// <summary>Stores edited rows only when they differ from the native final-health fade formula.</summary>
        internal readonly Bgr555[][]? supplied;

        /// <summary>Uses the native composition for stock rows and retains any independently authored edits.</summary>
        internal ColorFade(Bgr555[][] frames)
        {
            for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < frames[frame].Length; color++)
                if (Calculate(frame, color) != frames[frame][color])
                {
                    supplied = frames.Select(row => row.ToArray()).ToArray();
                    return;
                }
        }

        /// <summary>Retrieves one displayed color from edited rows or the native composition.</summary>
        internal Bgr555 Resolve(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Looks up the matching final-health paint before applying fade intensity.</summary>
        internal static Bgr555 Endpoint(int color) => BabyMetroidFinalHealthPaintDefinitions.Color(color);

        /// <summary>Scales each RGB5 channel by the native remaining-sevenths intensity.</summary>
        private static Bgr555 Calculate(int frame, int color)
        {
            int remaining = BabyMetroidCutsceneColorRomData.FadeFrameCount - 1 - frame;
            int intervals = BabyMetroidCutsceneColorRomData.FadeFrameCount + 1;
            return Endpoint(color).Map((_, channel) => channel * remaining / intervals);
        }

        /// <summary>Hashes the six resolved rows so native and equivalent supplied artwork share an identity.</summary>
        internal void AppendIdentity(SelectedPresentationHash content)
        {
            content.Append("fade", BabyMetroidCutsceneColorRomData.FadeFrameCount);
            Span<Bgr555> row = stackalloc Bgr555[BabyMetroidCutsceneColorRomData.FadeColorCount];
            for (int frame = 0; frame < BabyMetroidCutsceneColorRomData.FadeFrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(frame, color);
                content.AppendColors("row", row);
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
        Ensure.NotNull(json);
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

    /// <summary>Validates an RGB5 row's length and channels before copying it to packed colors.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int required, string name) =>
        Rgb5Rows.Compile(source, required, $"Cutscene Baby {name}");

    /// <summary>Rejects ambiguous property names before deserializing the artwork document.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate cutscene Baby color property {name}."));
}
