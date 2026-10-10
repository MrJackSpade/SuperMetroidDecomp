using SuperMetroid.Core.Hardware;
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
            Span<Bgr555> door = stackalloc Bgr555[MotherBrainExplodedDoorPaintDefinitions.ColorCount];
            for (int color = 0; color < door.Length; color++) door[color] = ExplodedDoorColor(color);
            content.AppendColors("explodedDoor", door);
            bodyFade.AppendIdentity(content, "bodyFade");
            legFade.AppendIdentity(content, "legFade");
            corpseFade.AppendIdentity(content, "corpseFade");
        });

    private readonly ColorFade bodyFade;
    private readonly ColorFade legFade;
    private readonly ColorFade corpseFade;
    private readonly Bgr555[]? explodedDoor;

    private MotherBrainDeathColorCatalog(Bgr555[][] bodyFade, Bgr555[][] legFade,
        Bgr555[][] corpseFade, Bgr555[] explodedDoor)
    {
        this.bodyFade = new(bodyFade, toBlack: true);
        this.legFade = new(legFade, toBlack: true, backLeg: true);
        this.corpseFade = new(corpseFade, toBlack: false);
        this.explodedDoor = MotherBrainExplodedDoorPaintDefinitions.Matches(explodedDoor) ? null : explodedDoor;
    }

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
    public Bgr555 BodyColor(int frame, int color) =>
        Resolve(bodyFade, frame, color, nameof(BodyColor));

    /// <summary>Returns one back-leg fade color from the second fourteen-color segment of each native $AD:EA0A image, applied to CGRAM 177..190.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..15; the stock back-leg sequence fades to black alongside the body.</param>
    /// <param name="color">Zero-based color within the back-leg segment, 0..13.</param>
    /// <returns>Packed SNES BGR555 color from the selected back-leg image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public Bgr555 LegColor(int frame, int color) =>
        Resolve(legFade, frame, color, nameof(LegColor));

    /// <summary>Returns one detached-head corpse color from the native $AD:F119 fade-to-gray images, applied to CGRAM 241..255 before corpse rotting.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..7, progressing from the death-start colors to the drained corpse appearance.</param>
    /// <param name="color">Zero-based opaque color in sprite palette seven, 0..14; transparent color zero is omitted.</param>
    /// <returns>Packed SNES BGR555 color from the selected corpse image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public Bgr555 CorpseColor(int frame, int color) =>
        Resolve(corpseFade, frame, color, nameof(CorpseColor));

    /// <summary>Returns a color from the $A9:9534 exploded escape-door palette, installed at CGRAM 145..158 when the escape sequence opens the door.</summary>
    /// <param name="color">Zero-based palette color 0..13, corresponding to native sprite-palette-one colors 1..14.</param>
    /// <returns>Packed SNES BGR555 color from the selected fourteen-color image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color index is outside 0..13.</exception>
    public Bgr555 ExplodedDoorColor(int color) =>
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

    private static Bgr555 Resolve(ColorFade frames, int frame, int color, string name) =>
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
        private readonly Bgr555[]? first;
        private readonly bool backLeg;
        private readonly MotherBrainRainbowPalettePresentation.DrainedBodyColors? last;
        private readonly Bgr555[][]? supplied;
        internal int FrameCount { get; }
        internal int ColorCount { get; }

        internal ColorFade(Bgr555[][] frames, bool toBlack, bool backLeg = false)
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

        internal Bgr555 Resolve(int frame, int color) =>
            supplied is null ? Calculate(frame, color) : supplied[frame][color];

        private Bgr555 Calculate(int frame, int color)
        {
            int steps = FrameCount - 1;
            Bgr555 start = first is null ? MotherBrainHealthPalettePresentation.StockDeathStartColor(backLeg, color) : first[color];
            // Without a supplied endpoint the fade ends at black with a one-unit bias.
            Bgr555 end = last is null ? Bgr555.Black : last[color];
            int bias = last is null ? 1 : steps / 2;
            return start.Zip(end, (_, from, to) => (from * (steps - frame) + to * frame + bias) / steps);
        }

        internal void AppendIdentity(SelectedPresentationHash content, string label)
        {
            content.Append(label, FrameCount);
            Span<Bgr555> row = stackalloc Bgr555[ColorCount];
            for (int frame = 0; frame < FrameCount; frame++)
            {
                for (int color = 0; color < ColorCount; color++)
                    row[color] = Resolve(frame, color);
                content.AppendColors("row", row);
            }
        }
    }

    private static Bgr555[][] CompileFrames(PaletteRgb5[][]? source,
        int frameCount, int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {frameCount} frames.");
        return source.Select((frame, index) =>
            Compile(frame, colorCount, $"{name} frame {index}")).ToArray();
    }

    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {count} RGB5 colors.");
        var compiled = new Bgr555[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Mother Brain death {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToBgr555();
        }
        return compiled;
    }

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
