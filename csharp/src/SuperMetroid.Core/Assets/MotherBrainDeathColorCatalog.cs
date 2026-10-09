using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Mother Brain death-fade and exploded-door RGB5 images.</summary>
public sealed class MotherBrainDeathColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("MotherBrainDeathColorCatalog-v1", content =>
        {
            Span<ushort> door = stackalloc ushort[MotherBrainExplodedDoorPaintDefinitions.ColorCount];
            for (int color = 0; color < door.Length; color++) door[color] = ExplodedDoorColor(color);
            content.AppendWords("explodedDoor", door);
            bodyFade.AppendIdentity(content, "bodyFade");
            legFade.AppendIdentity(content, "legFade");
            corpseFade.AppendIdentity(content, "corpseFade");
        });

    /// <summary>Compiled body and brain death-fade stages, retaining edited colors when they differ from native interpolation.</summary>
    private readonly ColorFade bodyFade;
    /// <summary>Compiled back-leg death-fade stages, kept independent from the body palette segment.</summary>
    private readonly ColorFade legFade;
    /// <summary>Compiled detached-head fade stages, interpolating toward the drained gray endpoint.</summary>
    private readonly ColorFade corpseFade;
    /// <summary>Door palette override; null means the document matches the native palette definition.</summary>
    private readonly ushort[]? explodedDoor;

    /// <summary>Creates the catalog from validated RGB5 images and stores only the data needed to reproduce their colors.</summary>
    /// <param name="bodyFade">Compiled body/brain images in authored stage order.</param>
    /// <param name="legFade">Compiled back-leg images in authored stage order.</param>
    /// <param name="corpseFade">Compiled detached-head images in authored stage order.</param>
    /// <param name="explodedDoor">Compiled exploded-door palette colors.</param>
    private MotherBrainDeathColorCatalog(ushort[][] bodyFade, ushort[][] legFade,
        ushort[][] corpseFade, ushort[] explodedDoor)
    {
        this.bodyFade = new(bodyFade, toBlack: true);
        this.legFade = new(legFade, toBlack: true, backLeg: true);
        this.corpseFade = new(corpseFade, toBlack: false);
        this.explodedDoor = MotherBrainExplodedDoorPaintDefinitions.Matches(explodedDoor) ? null : explodedDoor;
    }

    /// <summary>JSON settings that require camel-case properties, reject unmapped fields, and produce readable asset files.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Returns one body-fade color from the first segment of the native $AD:EA0A images, applied to both CGRAM 65..78 and 145..158 during the death fade.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..15, not an elapsed gameplay-update count; the stock sequence ends at black.</param>
    /// <param name="color">Zero-based color within the fourteen-color body segment, 0..13.</param>
    /// <returns>Packed SNES BGR555 color, preserving any independently edited stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public ushort BodyColor(int frame, int color) =>
        Resolve(bodyFade, frame, color, nameof(BodyColor));

    /// <summary>Returns one back-leg fade color from the second fourteen-color segment of each native $AD:EA0A image, applied to CGRAM 177..190.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..15; the stock back-leg sequence fades to black alongside the body.</param>
    /// <param name="color">Zero-based color within the back-leg segment, 0..13.</param>
    /// <returns>Packed SNES BGR555 color from the selected back-leg image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public ushort LegColor(int frame, int color) =>
        Resolve(legFade, frame, color, nameof(LegColor));

    /// <summary>Returns one detached-head corpse color from the native $AD:F119 fade-to-gray images, applied to CGRAM 241..255 before corpse rotting.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..7, progressing from the death-start colors to the drained corpse appearance.</param>
    /// <param name="color">Zero-based opaque color in sprite palette seven, 0..14; transparent color zero is omitted.</param>
    /// <returns>Packed SNES BGR555 color from the selected corpse image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public ushort CorpseColor(int frame, int color) =>
        Resolve(corpseFade, frame, color, nameof(CorpseColor));

    /// <summary>Returns a color from the $A9:9534 exploded escape-door palette, installed at CGRAM 145..158 when the escape sequence opens the door.</summary>
    /// <param name="color">Zero-based palette color 0..13, corresponding to native sprite-palette-one colors 1..14.</param>
    /// <returns>Packed SNES BGR555 color from the selected fourteen-color image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color index is outside 0..13.</exception>
    public ushort ExplodedDoorColor(int color) =>
        (uint)color < MotherBrainExplodedDoorPaintDefinitions.ColorCount
            ? explodedDoor is null ? MotherBrainExplodedDoorPaintDefinitions.Color(color) : explodedDoor[color] :
            throw new ArgumentOutOfRangeException(nameof(color));

    /// <summary>Validates and compiles the death-fade and exploded-door RGB5 document, retaining edited images independently of the health-palette presentation.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled color catalog detached from the deserialized document arrays.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, incorrect image dimensions, null colors, or channels outside 0..31.</exception>
    public static MotherBrainDeathColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        MotherBrainDeathColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<MotherBrainDeathColorDocument>(JsonOptions) ??
                throw new InvalidDataException("Mother Brain death color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Mother Brain death color JSON.", error);
        }
        if (document.Version != MotherBrainDeathColorFormat.Version)
            throw new InvalidDataException(
                "Mother Brain death colors require the supported version.");
        return new(CompileFrames(document.BodyFade,
                MotherBrainDeathRomData.BodyFadeFrameCount,
                MotherBrainDeathRomData.BodyColorCount, "body fade"),
            CompileFrames(document.LegFade,
                MotherBrainDeathRomData.BodyFadeFrameCount,
                MotherBrainDeathRomData.BodyColorCount, "leg fade"),
            CompileFrames(document.CorpseFade,
                MotherBrainDeathRomData.CorpseFadeFrameCount,
                MotherBrainDeathRomData.CorpseColorCount, "corpse fade"),
            Compile(document.ExplodedDoor, MotherBrainDeathRomData.BodyColorCount,
                "exploded door"));
    }

    /// <summary>Serializes the editable death-color document to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">Palette-stage and door colors to serialize; their collections are not retained.</param>
    /// <returns>Validated JSON bytes ready to install under <see cref="MotherBrainDeathColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, dimension, or RGB5-channel validation.</exception>
    public static byte[] Write(MotherBrainDeathColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Bounds-checks a requested fade stage and color before resolving its packed palette word.</summary>
    /// <param name="frames">Compiled fade image to query.</param>
    /// <param name="frame">Zero-based stage index.</param>
    /// <param name="color">Zero-based color index.</param>
    /// <param name="name">Fade label included in an out-of-range diagnostic.</param>
    /// <returns>The resolved BGR555 color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside the compiled image dimensions.</exception>
    private static ushort Resolve(ColorFade frames, int frame, int color, string name) =>
        (uint)frame < frames.FrameCount && (uint)color < frames.ColorCount
            ? frames.Resolve(frame, color)
            : throw new ArgumentOutOfRangeException(nameof(frame),
                $"Mother Brain death {name} frame {frame}, color {color} is outside the authored images.");

    /// <summary>
    /// $AD:EA0A body/leg channels fade to black as (initial*(15-frame)+1)/15.
    /// $AD:F119 corpse channels interpolate between endpoints, rounded to nearest
    /// over seven intervals. These rules match every original intermediate color.
    /// Starting colors calculate from health state three; the corpse final palette calculates shared
    /// drained shade rules from the three narrowly approved drained-paint anchors.
    /// Unmatched edited endpoints and frames remain exact and independent of the health document.
    /// </summary>
    private sealed class ColorFade
    {
        /// <summary>Optional first-stage colors when the document changes the native death-start palette.</summary>
        private readonly ushort[]? first;
        /// <summary>Whether starting colors come from the separate back-leg segment of the health palette.</summary>
        private readonly bool backLeg;
        /// <summary>Optional drained corpse endpoint used for the detached-head interpolation.</summary>
        private readonly MotherBrainRainbowPalettePresentation.DrainedBodyColors? last;
        /// <summary>Full authored image set retained when any stage differs from calculated interpolation.</summary>
        private readonly ushort[][]? supplied;
        /// <summary>Gets the number of authored palette stages.</summary>
        internal int FrameCount { get; }
        /// <summary>Gets the number of colors in each stage.</summary>
        internal int ColorCount { get; }

        /// <summary>Builds a compact fade model and retains authored frames whenever calculated colors do not match exactly.</summary>
        /// <param name="frames">Compiled RGB5 stage rows in document order.</param>
        /// <param name="toBlack"><see langword="true"/> fades to black; otherwise the supplied final row is the endpoint.</param>
        /// <param name="backLeg"><see langword="true"/> selects the back-leg death-start palette segment.</param>
        internal ColorFade(ushort[][] frames, bool toBlack, bool backLeg = false)
        {
            this.backLeg = backLeg;
            ColorCount = frames[0].Length;
            for (int color = 0; color < ColorCount; color++)
                if (frames[0][color] != MotherBrainHealthPalettePresentation.StockDeathStartColor(backLeg, color))
                { first = frames[0]; break; }
            last = toBlack ? null : new MotherBrainRainbowPalettePresentation.DrainedBodyColors(frames[^1]);
            FrameCount = frames.Length;
            for (int frame = 0; frame < FrameCount; frame++)
            for (int color = 0; color < ColorCount; color++)
            {
                if (frames[frame][color] != Calculate(frame, color))
                {
                    supplied = frames;
                    return;
                }
            }
        }

        /// <summary>Returns a retained authored color or calculates the corresponding fade value.</summary>
        /// <param name="frame">Zero-based stage index.</param>
        /// <param name="color">Zero-based color index within that stage.</param>
        /// <returns>Packed BGR555 color for the requested stage and color.</returns>
        internal ushort Resolve(int frame, int color) =>
            supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Interpolates one five-bit channel triplet between the selected endpoints using the authored rounding rule.</summary>
        /// <param name="frame">Zero-based stage index among the fade intervals.</param>
        /// <param name="color">Zero-based color index within the stage.</param>
        /// <returns>The packed BGR555 color for this stage.</returns>
        private ushort Calculate(int frame, int color)
        {
            int steps = FrameCount - 1;
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
            {
                int start = ((first is null ? MotherBrainHealthPalettePresentation.StockDeathStartColor(backLeg, color) : first[color]) >> shift) & 31;
                int end = last is null ? 0 : (last[color] >> shift) & 31;
                int bias = last is null ? 1 : steps / 2;
                int channel = (start * (steps - frame) + end * frame + bias) / steps;
                result |= channel << shift;
            }
            return (ushort)result;
        }

        /// <summary>Appends the dimensions and resolved stage rows to the selected-presentation identity.</summary>
        /// <param name="content">Hash accumulator receiving the canonical fade content.</param>
        /// <param name="label">Stable component label distinguishing this palette image.</param>
        internal void AppendIdentity(SelectedPresentationHash content, string label)
        {
            content.Append(label, FrameCount);
            Span<ushort> row = stackalloc ushort[ColorCount];
            for (int frame = 0; frame < FrameCount; frame++)
            {
                for (int color = 0; color < ColorCount; color++)
                    row[color] = Resolve(frame, color);
                content.AppendWords("row", row);
            }
        }
    }

    /// <summary>Validates the stage count and compiles every palette row into packed BGR555 words.</summary>
    /// <param name="source">Nullable document rows to validate.</param>
    /// <param name="frameCount">Required number of stages for this fade.</param>
    /// <param name="colorCount">Required number of colors in each stage.</param>
    /// <param name="name">Fade label used in validation errors.</param>
    /// <returns>Compiled color rows in the original stage order.</returns>
    /// <exception cref="InvalidDataException">The stage count, row dimensions, or an RGB5 channel is invalid.</exception>
    private static ushort[][] CompileFrames(PaletteRgb5[][]? source,
        int frameCount, int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {frameCount} frames.");
        return source.Select((frame, index) =>
            Compile(frame, colorCount, $"{name} frame {index}")).ToArray();
    }

    /// <summary>Validates one RGB5 palette row and packs its channels into SNES BGR555 words.</summary>
    /// <param name="source">Nullable row of document colors.</param>
    /// <param name="count">Required number of colors.</param>
    /// <param name="name">Palette label used in validation errors.</param>
    /// <returns>Packed BGR555 words in source order.</returns>
    /// <exception cref="InvalidDataException">The row has the wrong size, a null entry, or a channel outside 0..31.</exception>
    private static ushort[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {count} RGB5 colors.");
        var compiled = new ushort[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Mother Brain death {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    /// <summary>Rejects duplicate JSON object properties before schema deserialization.</summary>
    /// <param name="value">Parsed JSON root value to validate recursively.</param>
    /// <exception cref="InvalidDataException">An object contains a duplicate property name.</exception>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Mother Brain death color property {name}."));
}

/// <summary>Editable RGB5 image schema for Mother Brain's body disappearance, detached-head gray transition, and escape-door explosion.</summary>
public sealed record MotherBrainDeathColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="MotherBrainDeathColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen ordered fourteen-color body/brain images from $AD:EA0A's first segments; each RGB5 channel is 0..31, with stock stages fading to black.</summary>
    public required PaletteRgb5[][] BodyFade { get; init; }
    /// <summary>Sixteen ordered fourteen-color back-leg images from $AD:EA0A's second segments, separate from body colors but selected by the same fade-stage index.</summary>
    public required PaletteRgb5[][] LegFade { get; init; }
    /// <summary>Eight ordered fifteen-color detached-head images corresponding to $AD:F119, ending in the stock drained appearance rather than the body fade's black.</summary>
    public required PaletteRgb5[][] CorpseFade { get; init; }
    /// <summary>Fourteen nonnull RGB5 colors corresponding to $A9:9534, replacing sprite-palette-one colors 1..14 for the exploded escape door.</summary>
    public required PaletteRgb5[] ExplodedDoor { get; init; }
}

/// <summary>Installed-resource identity and schema revision for Mother Brain death and escape-door colors.</summary>
public static class MotherBrainDeathColorFormat
{
    /// <summary>JSON resource filename used to install the death-fade and exploded-door color catalog.</summary>
    public const string FileName = "mother-brain-death-colors.json";
    /// <summary>Supported RGB5 document schema revision, one; it fixes the sixteen-stage body/leg and eight-stage corpse dimensions.</summary>
    public const int Version = 1;
}
