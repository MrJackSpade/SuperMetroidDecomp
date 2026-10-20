using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

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

    /// <summary>Sixteen-stage body and brain palette progression toward black.</summary>
    internal readonly ColorFade bodyFade;
    /// <summary>Separate sixteen-stage back-leg palette progression.</summary>
    internal readonly ColorFade legFade;
    /// <summary>Eight-stage detached-head progression toward the drained palette.</summary>
    internal readonly ColorFade corpseFade;
    /// <summary>Edited escape-door colors, or null when the canonical image suffices.</summary>
    internal readonly Bgr555[]? explodedDoor;

    /// <summary>Compresses stock fade images into calculated stages while retaining independently edited frames.</summary>
    private MotherBrainDeathColorCatalog(Bgr555[][] bodyFade, Bgr555[][] legFade,
        Bgr555[][] corpseFade, Bgr555[] explodedDoor)
    {
        this.bodyFade = new(bodyFade, MotherBrainFadeTarget.Black);
        this.legFade = new(legFade, MotherBrainFadeTarget.Black, MotherBrainPalettePart.BackLeg);
        this.corpseFade = new(corpseFade, MotherBrainFadeTarget.DrainedBody);
        this.explodedDoor = MotherBrainExplodedDoorPaintDefinitions.Matches(explodedDoor) ? null : explodedDoor;
    }

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
        Ensure.NotNull(json);
        MotherBrainDeathColorDocument document;
        document = JsonAssetDocument.Read<MotherBrainDeathColorDocument>(json, ArtworkJson.Options, "Mother Brain death color");
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
    public static byte[] Write(MotherBrainDeathColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Load(json));

    /// <summary>Checks authored frame and color bounds before resolving a fade color.</summary>
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
    internal sealed class ColorFade
    {
        /// <summary>Edited starting colors, or null when the health palette supplies the stock start.</summary>
        internal readonly Bgr555[]? first;
        /// <summary>Selects the back-leg rather than body starting palette.</summary>
        private readonly MotherBrainPalettePart part;
        /// <summary>Corpse fade endpoint; null selects black for the body and leg fades.</summary>
        internal readonly MotherBrainRainbowPalettePresentation.DrainedBodyColors? last;
        /// <summary>Exact edited frames retained when the calculated stock interpolation differs.</summary>
        internal readonly Bgr555[][]? supplied;
        /// <summary>Number of authored palette stages in this fade.</summary>
        internal int FrameCount { get; }
        /// <summary>Opaque colors in each authored stage.</summary>
        internal int ColorCount { get; }

        /// <summary>Detects whether the supplied stages follow stock interpolation or need exact storage.</summary>
        internal ColorFade(Bgr555[][] frames, MotherBrainFadeTarget target, MotherBrainPalettePart part = MotherBrainPalettePart.Body)
        {
            this.part = part;
            ColorCount = frames[0].Length;
            for (int color = 0; color < ColorCount; color++)
                if (frames[0][color] != MotherBrainHealthPalettePresentation.StockDeathStartColor(part, color))
                { first = frames[0]; break; }
            last = target == MotherBrainFadeTarget.Black ? null : new MotherBrainRainbowPalettePresentation.DrainedBodyColors(frames[^1]);
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

        /// <summary>Returns an exact edited color when present, otherwise calculates the stock stage.</summary>
        internal Bgr555 Resolve(int frame, int color) =>
            supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Interpolates a channel with the native black-fade bias or rounded corpse endpoint.</summary>
        private Bgr555 Calculate(int frame, int color)
        {
            int steps = FrameCount - 1;
            Bgr555 start = first is null ? MotherBrainHealthPalettePresentation.StockDeathStartColor(part, color) : first[color];
            // Without a supplied endpoint the fade ends at black with a one-unit bias.
            Bgr555 end = last is null ? Bgr555.Black : last[color];
            int bias = last is null ? 1 : steps / 2;
            return start.Zip(end, (_, from, to) => (from * (steps - frame) + to * frame + bias) / steps);
        }

        /// <summary>Appends resolved stage colors to the identity independent of storage representation.</summary>
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

    /// <summary>Checks the authored stage count and converts each RGB5 row.</summary>
    private static Bgr555[][] CompileFrames(PaletteRgb5[][]? source,
        int frameCount, int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {frameCount} frames.");
        return source.Select((frame, index) =>
            Compile(frame, colorCount, $"{name} frame {index}")).ToArray();
    }

    /// <summary>Checks row size and channel bounds before packing one palette image.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Mother Brain death {name}");

}
