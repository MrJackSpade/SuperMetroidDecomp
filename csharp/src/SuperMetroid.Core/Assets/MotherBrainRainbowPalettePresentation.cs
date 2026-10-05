using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable rainbow, drain, revival, and normal-restoration colors for Mother Brain.</summary>
public sealed class MotherBrainRainbowPalettePresentation
{
    private readonly PaletteFrame[] rainbow;
    private readonly PaletteFade toGrey;
    private readonly QuantizedPaletteFade fromGrey;
    private readonly PaletteFade fakeDeathToGrey;
    private readonly PaletteFrame normal;
    private readonly ushort beamInitial;
    private readonly BeamColors beamCycle;

    private MotherBrainRainbowPalettePresentation(PaletteFrame[] rainbow, PaletteFrame[] toGrey,
        PaletteFrame[] fromGrey, PaletteFade fakeDeathToGrey, PaletteFrame normal,
        ushort beamInitial, ushort[] beamCycle)
    {
        this.rainbow = rainbow;
        this.toGrey = new PaletteFade(toGrey);
        this.fromGrey = new QuantizedPaletteFade(fromGrey);
        this.fakeDeathToGrey = fakeDeathToGrey;
        this.normal = normal;
        this.beamInitial = beamInitial;
        this.beamCycle = new BeamColors(beamCycle);
    }

    /// <summary>
    /// $88:E833-E8C7, Set_RainbowBeam_ColorMathSubscreenBackdropColor.table:
    /// five linear hue legs sampled every other word. The native blue-to-magenta leg
    /// delays red by one step and adds a one-step green tint in its upper half.
    /// Falling green/blue changes its rounding bias after local step eight.
    /// These phase boundaries preserve the source's asymmetric wheel exactly.
    /// </summary>
    private sealed class BeamColors
    {
        private readonly ushort[]? supplied;
        public int Length { get; }
        public BeamColors(ushort[] colors)
        {
            Length = colors.Length;
            for (int index = 0; index < Length; index++)
                if (colors[index] != Calculate(index))
                { supplied = colors; return; }
        }
        public ushort this[int index] => supplied is null ? Calculate(index) : supplied[index];

        private static ushort Calculate(int index)
        {
            int step = index * MotherBrainBeamRomData.ColorStride / sizeof(ushort);
            int red, green, blue;
            if (step <= 15)
            {
                red = 31; green = Rising(step); blue = 0;
            }
            else if (step <= 30)
            {
                red = Rising(30 - step); green = 31; blue = 0;
            }
            else if (step <= 45)
            {
                red = 0; green = Falling(step - 30); blue = Rising(step - 30);
            }
            else if (step < 60)
            {
                int phase = step - 45;
                red = 2 * phase - (phase >= 7 ? 1 : 0);
                green = phase >= 8 ? 1 : 0;
                blue = 31;
            }
            else
            {
                red = 31; green = 0; blue = Falling(step - 60);
            }
            return (ushort)(red | green << 5 | blue << 10);
        }
        private static int Rising(int step) => (31 * step + 7) / 15;
        private static int Falling(int step) => 31 - 2 * step - (step >= 9 ? 1 : 0);
    }
    /// <summary>Fixed-color backdrop used on the beam's first active HDMA frame.</summary>
    public ushort BeamInitialColor => beamInitial;

    /// <summary>
    /// Resolves the native byte cursor. The signed terminator remains engine control,
    /// not an editable color; the HDMA owner performs the reset on that frame.
    /// </summary>
    public ushort BeamColorWord(int byteCursor)
    {
        if (byteCursor < 0 || byteCursor % MotherBrainBeamRomData.ColorStride != 0 ||
            byteCursor > beamCycle.Length * MotherBrainBeamRomData.ColorStride)
            throw new InvalidDataException($"Mother Brain beam color cursor ${byteCursor:X} is outside its authored cycle.");
        int index = byteCursor / MotherBrainBeamRomData.ColorStride;
        return index == beamCycle.Length ? ushort.MaxValue : beamCycle[index];
    }

    /// <summary>Copies one cartridge rainbow-list entry to the three body/brain/leg slots.</summary>
    public void ApplyRainbow(SnesCgram cgram, int frame) => ApplyFull(cgram, rainbow, frame);

    /// <summary>Restores the two fixed normal-palette sources after the beam finishes.</summary>
    public void ApplyNormal(SnesCgram cgram) =>
        ApplyColors(cgram, normal, MotherBrainRainbowPaletteRomData.SecondaryColor);

    /// <summary>Copies a drain fade and publishes its trailing palette word to WRAM $017C.</summary>
    public void ApplyToGrey(ISnesAddressSpace bus, SnesCgram cgram, int frame) =>
        ApplyGrey(bus, cgram, toGrey, frame);

    /// <summary>Copies a revival fade, preserving the two native body/brain tail colors.</summary>
    public void ApplyFromGrey(ISnesAddressSpace bus, SnesCgram cgram, int frame) =>
        ApplyGrey(bus, cgram, fromGrey, frame);

    /// <summary>Changes only the three brain colors during the fake-death descent.</summary>
    public void ApplyFakeDeathToGrey(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= fakeDeathToGrey.Length)
            throw new InvalidDataException($"Mother Brain fake-death grey frame {frame} is outside the authored sequence.");
        for (int color = 0; color < MotherBrainFakeDeathPaletteRomData.ColorCount; color++)
            cgram.SetColor(MotherBrainFakeDeathPaletteRomData.BrainColor + color,
                fakeDeathToGrey.Body(frame, color));
    }

    /// <summary>Restores only those three colors, reusing the cartridge's revival source table.</summary>
    public void ApplyFakeDeathFromGrey(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= fromGrey.Length)
            throw new InvalidDataException($"Mother Brain fake-death revival frame {frame} is outside the authored sequence.");
        for (int color = 0; color < MotherBrainFakeDeathPaletteRomData.ColorCount; color++)
            cgram.SetColor(MotherBrainFakeDeathPaletteRomData.BrainColor + color, fromGrey.Body(frame, color));
    }

    private static void ApplyFull(SnesCgram cgram, PaletteFrame[] frames, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= frames.Length)
            throw new InvalidDataException($"Mother Brain rainbow frame {frame} is outside the authored loop.");
        ApplyColors(cgram, frames[frame], MotherBrainRainbowPaletteRomData.SecondaryColor);
    }

    private static void ApplyGrey(ISnesAddressSpace bus, SnesCgram cgram,
        IPaletteFade frames, int frame)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= frames.Length)
            throw new InvalidDataException($"Mother Brain grey-transition frame {frame} is outside the authored sequence.");
        for (int color = 0; color < frames.BodyCount; color++)
        {
            ushort value = frames.Body(frame, color);
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + color, value);
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + color, value);
        }
        for (int color = 0; color < frames.LegCount; color++)
            cgram.SetColor(MotherBrainDrainedPaletteRomData.BackLegColor + color, frames.Leg(frame, color));
        ushort trailing = frames.Trailing(frame)!.Value;
        bus.WriteByte(MotherBrainDrainedPaletteRomData.TrailingWordWram, (byte)trailing);
        bus.WriteByte(MotherBrainDrainedPaletteRomData.TrailingWordWram + 1, (byte)(trailing >> 8));
    }

    private static void ApplyColors(SnesCgram cgram, PaletteFrame selected, int legDestination)
    {
        for (int color = 0; color < selected.Body.Length; color++)
        {
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + color, selected.Body[color]);
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + color, selected.Body[color]);
        }
        for (int color = 0; color < selected.LegCount; color++)
            cgram.SetColor(legDestination + color, selected.Leg(color));
    }

    public static MotherBrainRainbowPalettePresentation Load(Stream json,
        MotherBrainRainbowPalettePresentation? currentStock = null)
    {
        MotherBrainRainbowPaletteDocument document = JsonAssetDocument.Read<MotherBrainRainbowPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "Mother Brain rainbow palette");
        if (document.Version != MotherBrainRainbowPaletteFormat.Version &&
            !(document.Version == MotherBrainRainbowPaletteFormat.PreFakeDeathVersion && currentStock is not null))
            throw new InvalidDataException("Unsupported Mother Brain rainbow palette version.");
        return new(
            CompileFrames(document.Rainbow, MotherBrainRainbowPaletteFormat.RainbowFrameCount,
                MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.ColorCount, false, nameof(document.Rainbow)),
            CompileFrames(document.ToGrey, MotherBrainRainbowPaletteFormat.GreyFrameCount,
                MotherBrainDrainedPaletteRomData.DrainedColors,
                MotherBrainDrainedPaletteRomData.BackLegCount, true, nameof(document.ToGrey)),
            CompileFrames(document.FromGrey, MotherBrainRainbowPaletteFormat.GreyFrameCount,
                MotherBrainDrainedPaletteRomData.RevivalColors,
                MotherBrainDrainedPaletteRomData.BackLegCount, true, nameof(document.FromGrey)),
            document.Version == MotherBrainRainbowPaletteFormat.PreFakeDeathVersion
                ? currentStock!.fakeDeathToGrey
                : CompileFakeDeathFrames(document.FakeDeathToGrey),
            CompileFrames([document.Normal], 1, MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.ColorCount, false, nameof(document.Normal))[0],
            CompileColor(document.BeamInitial, nameof(document.BeamInitial)),
            CompileColors(document.BeamCycle, MotherBrainRainbowPaletteFormat.BeamCycleColorCount,
                nameof(document.BeamCycle)));
    }

    private static PaletteFade CompileFakeDeathFrames(PaletteRgb5[][]? source)
    {
        if (source is null || source.Length != MotherBrainFakeDeathPaletteRomData.FrameCount)
            throw new InvalidDataException("Mother Brain fake-death fade requires eight frames.");
        var frames = new PaletteFrame[source.Length];
        for (int frame = 0; frame < source.Length; frame++)
            frames[frame] = new PaletteFrame(CompileColors(source[frame],
                MotherBrainFakeDeathPaletteRomData.ColorCount, $"fake-death frame {frame}"), [], null);
        return new PaletteFade(frames);
    }

    private static PaletteFrame[] CompileFrames(MotherBrainRainbowPaletteFrameDocument[]? source,
        int count, int bodyCount, int legCount, bool trailing, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Mother Brain {name} requires {count} frames.");
        var frames = new PaletteFrame[count];
        for (int frame = 0; frame < count; frame++)
        {
            MotherBrainRainbowPaletteFrameDocument? item = source[frame];
            if (item is null || (item.TrailingColor is null) != !trailing)
                throw new InvalidDataException($"Mother Brain {name} frame {frame} has an invalid trailing color.");
            frames[frame] = new PaletteFrame(
                CompileColors(item.Body, bodyCount, $"{name} frame {frame} body"),
                CompileColors(item.BackLegs, legCount, $"{name} frame {frame} back legs"),
                item.TrailingColor is null ? null : CompileColor(item.TrailingColor,
                    $"{name} frame {frame} trailing color"));
        }
        return frames;
    }

    private static ushort[] CompileColors(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Mother Brain {name} requires {count} colors.");
        var colors = new ushort[count];
        for (int color = 0; color < count; color++)
            colors[color] = CompileColor(source[color], $"{name} color {color}");
        return colors;
    }

    private static ushort CompileColor(PaletteRgb5? color, string name)
    {
        if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
            throw new InvalidDataException($"Mother Brain {name} requires RGB5 components.");
        return (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
    }

    public static void Write(Stream json, MotherBrainRainbowPaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    private sealed class PaletteFrame
    {
        private readonly ushort[]? backLegs;
        private readonly bool stockRear;

        public PaletteFrame(ushort[] body, ushort[] legs, ushort? trailingColor)
        {
            Body = new BodyColors(body);
            LegCount = legs.Length;
            TrailingColor = trailingColor;
            bool matchesRear = legs.Length == MotherBrainRainbowPaletteRomData.ColorCount;
            for (int color = 0; matchesRear && color < legs.Length; color++)
                matchesRear = legs[color] == MotherBrainHealthPalettePresentation.StockBaseColor(true, color);
            if (matchesRear) { stockRear = true; return; }
            // Rainbow legs use half-intensity body colors, rounding RGB5 upward.
            // Select this relationship only when every supplied color agrees.
            if (body.Length != legs.Length)
            { backLegs = legs; return; }
            for (int color = 0; color < legs.Length; color++)
                if (HalfIntensity(body[color]) != legs[color])
                { backLegs = legs; return; }
        }

        public BodyColors Body { get; }
        public int LegCount { get; }
        public ushort? TrailingColor { get; }
        public ushort Leg(int color) => stockRear
            ? MotherBrainHealthPalettePresentation.StockBaseColor(true, color)
            : backLegs is null ? HalfIntensity(Body[color]) : backLegs[color];

        private static ushort HalfIntensity(ushort color) => (ushort)(
            ((color & 31) + 1) / 2 | (((color >> 5 & 31) + 1) / 2) << 5
            | (((color >> 10 & 31) + 1) / 2) << 10);
    }

    /// <summary>Recognizes normal and drained body paint without repeated palette rows.</summary>
    private sealed class BodyColors
    {
        private readonly ushort[]? supplied;
        private readonly DrainedBodyColors? drained;
        public int Length { get; }
        public BodyColors(ushort[] colors)
        {
            Length = colors.Length;
            bool normal = Length == MotherBrainRainbowPaletteRomData.ColorCount;
            for (int color = 0; normal && color < Length; color++)
                normal = colors[color] == MotherBrainHealthPalettePresentation.StockBaseColor(false, color);
            if (normal) return;
            drained = new DrainedBodyColors(colors);
            if (!drained.Calculated) { drained = null; supplied = colors; }
        }
        public ushort this[int color] => supplied is not null ? supplied[color]
            : drained is not null ? drained[color]
            : MotherBrainHealthPalettePresentation.StockBaseColor(false, color);
    }

    /// <summary>
    /// $AD:F0BF/F1EB and EEB8 share drained body paint. Cortex shades interpolate
    /// in RGB5 over three intervals; tissue shades interpolate before RGB5 quantization
    /// over four intervals. Plates reuse normal health paint; white/black are neutral.
    /// Stock uses the three narrowly approved drained paint anchors; independent edits
    /// retain their own calculated endpoints or exact supplied colors.
    /// </summary>
    internal sealed class DrainedBodyColors
    {
        private readonly Rgb8 tissueLight;
        private readonly Rgb8 tissueDark;
        private readonly ushort outline;
        private readonly ushort[]? supplied;
        internal bool Calculated => supplied is null;

        internal DrainedBodyColors(ushort[] colors)
        {
            if (colors.Length is not (13 or 15)) { supplied = colors; return; }
            var light = MotherBrainDrainedPaintDefinitions.HighlightRgb8;
            var dark = MotherBrainDrainedPaintDefinitions.DarkestTissueRgb8;
            tissueLight = new(light.Red, light.Green, light.Blue);
            tissueDark = new(dark.Red, dark.Green, dark.Blue);
            outline = MotherBrainDrainedPaintDefinitions.Outline;
            bool stock = true;
            for (int color = 0; stock && color < colors.Length; color++) stock = colors[color] == Calculate(color);
            if (stock) return;
            outline = colors[3];
            if (!TryChannel(0, out int lr, out int dr) ||
                !TryChannel(5, out int lg, out int dg) ||
                !TryChannel(10, out int lb, out int db))
            { supplied = colors; return; }
            tissueLight = new(lr, lg, lb);
            tissueDark = new(dr, dg, db);
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }

            bool TryChannel(int shift, out int first, out int last)
            {
                int low = (colors[8] >> shift & 31) * 8;
                int high = (colors[12] >> shift & 31) * 8;
                for (int start = low; start < low + 8; start++)
                    for (int end = high; end < high + 8; end++)
                    {
                        bool matches = true;
                        for (int step = 0; step <= 4; step++)
                            if ((start * (4 - step) + end * step) / 32 != (colors[8 + step] >> shift & 31))
                            { matches = false; break; }
                        if (matches) { first = start; last = end; return true; }
                    }
                first = last = 0;
                return false;
            }
        }
        internal ushort this[int color] => supplied is null ? Calculate(color) : supplied[color];

        private ushort Calculate(int color)
        {
            if (color is >= 4 and <= 7)
                return MotherBrainHealthPalettePresentation.StockBaseColor(false, color);
            if (color == 13) return (31 << 10) | (31 << 5) | 31;
            if (color == 14) return 0;
            int result = 0;
            for (int component = 0; component < 3; component++)
            {
                int channel = color < 4
                    ? ((tissueLight.Component(component) / 8) * (3 - color)
                        + (outline >> (component * 5) & 31) * color + 1) / 3
                    : (tissueLight.Component(component) * (12 - color)
                        + tissueDark.Component(component) * (color - 8)) / 32;
                result |= channel << (component * 5);
            }
            return (ushort)result;
        }

        private readonly record struct Rgb8(int Red, int Green, int Blue)
        {
            internal int Component(int index) => index switch
            { 0 => Red, 1 => Green, 2 => Blue, _ => throw new IndexOutOfRangeException() };
        }
    }
    private interface IPaletteFade
    {
        int Length { get; }
        int BodyCount { get; }
        int LegCount { get; }
        ushort Body(int frame, int color);
        ushort Leg(int frame, int color);
        ushort? Trailing(int frame);
    }

    // Revival colors interpolate before RGB5 quantization. Endpoint intervals are inferred
    // from supplied colors; every row must agree before a channel's samples are discarded.
    // Unmatched channels remain explicit supplied content, with no retention exemption.
    private sealed class QuantizedPaletteFade : IPaletteFade
    {
        private readonly Channel[] channels;

        public QuantizedPaletteFade(PaletteFrame[] frames)
        {
            Length = frames.Length;
            BodyCount = frames[0].Body.Length;
            LegCount = frames[0].LegCount;
            channels = new Channel[(BodyCount + LegCount + 1) * 3];
            for (int color = 0; color < channels.Length / 3; color++)
                for (int component = 0; component < 3; component++)
                {
                    var values = new byte[Length];
                    for (int frame = 0; frame < Length; frame++)
                    {
                        ushort packed = color < BodyCount ? frames[frame].Body[color]
                            : color < BodyCount + LegCount ? frames[frame].Leg(color - BodyCount)
                            : frames[frame].TrailingColor!.Value;
                        values[frame] = (byte)(packed >> (5 * component) & 31);
                    }
                    channels[color * 3 + component] = new Channel(values);
                }
        }

        public int Length { get; }
        public int BodyCount { get; }
        public int LegCount { get; }
        public ushort Body(int frame, int color) => Color(frame, color);
        public ushort Leg(int frame, int color) => Color(frame, BodyCount + color);
        public ushort? Trailing(int frame) => Color(frame, BodyCount + LegCount);
        private ushort Color(int frame, int color) => (ushort)(channels[color * 3].At(frame, Length)
            | channels[color * 3 + 1].At(frame, Length) << 5 | channels[color * 3 + 2].At(frame, Length) << 10);

        private sealed class Channel
        {
            private readonly int first;
            private readonly int last;
            private readonly byte[]? supplied;

            public Channel(byte[] values)
            {
                int intervals = values.Length - 1;
                for (int start = values[0] * 8; start < values[0] * 8 + 8; start++)
                    for (int end = values[^1] * 8; end < values[^1] * 8 + 8; end++)
                    {
                        bool matches = true;
                        for (int frame = 0; frame < values.Length; frame++)
                            if ((start * (intervals - frame) + end * frame) / (8 * intervals) != values[frame])
                            { matches = false; break; }
                        if (matches)
                        { first = start; last = end; return; }
                    }
                supplied = values;
            }

            public int At(int frame, int length) => supplied is not null ? supplied[frame]
                : (first * (length - 1 - frame) + last * frame) / (8 * (length - 1));
        }
    }
    private sealed class PaletteFade : IPaletteFade
    {
        private readonly PaletteFrame first;
        private readonly PaletteFrame last;
        private readonly PaletteFrame[]? supplied;

        public PaletteFade(PaletteFrame[] frames)
        {
            first = frames[0];
            last = frames[^1];
            Length = frames.Length;
            // The drain and fake-death rows round each RGB5 channel to the nearest
            // point on a straight endpoint fade. Keep independently edited rows verbatim.
            for (int frame = 0; frame < Length; frame++)
            {
                for (int color = 0; color < BodyCount; color++)
                    if (Body(frame, color) != frames[frame].Body[color])
                    { supplied = frames; return; }
                for (int color = 0; color < LegCount; color++)
                    if (Leg(frame, color) != frames[frame].Leg(color))
                    { supplied = frames; return; }
                if (Trailing(frame) != frames[frame].TrailingColor)
                { supplied = frames; return; }
            }
        }

        public int Length { get; }
        public int BodyCount => first.Body.Length;
        public int LegCount => first.LegCount;
        public ushort Body(int frame, int color) => supplied is null
            ? Interpolate(first.Body[color], last.Body[color], frame) : supplied[frame].Body[color];
        public ushort Leg(int frame, int color) => supplied is null
            ? Interpolate(first.Leg(color), last.Leg(color), frame) : supplied[frame].Leg(color);
        public ushort? Trailing(int frame) => supplied is not null ? supplied[frame].TrailingColor
            : first.TrailingColor is ushort start && last.TrailingColor is ushort end
                ? Interpolate(start, end, frame) : null;

        private ushort Interpolate(ushort start, ushort end, int frame)
        {
            int intervals = Length - 1;
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= (((start >> shift & 31) * (intervals - frame)
                    + (end >> shift & 31) * frame + intervals / 2) / intervals) << shift;
            return (ushort)result;
        }
    }
}

public sealed record MotherBrainRainbowPaletteDocument
{
    public required int Version { get; init; }
    public required MotherBrainRainbowPaletteFrameDocument[] Rainbow { get; init; }
    public required MotherBrainRainbowPaletteFrameDocument[] ToGrey { get; init; }
    public required MotherBrainRainbowPaletteFrameDocument[] FromGrey { get; init; }
    public PaletteRgb5[][]? FakeDeathToGrey { get; init; }
    public required MotherBrainRainbowPaletteFrameDocument Normal { get; init; }
    public required PaletteRgb5 BeamInitial { get; init; }
    public required PaletteRgb5[] BeamCycle { get; init; }
}

public sealed record MotherBrainRainbowPaletteFrameDocument
{
    public required PaletteRgb5[] Body { get; init; }
    public required PaletteRgb5[] BackLegs { get; init; }
    public PaletteRgb5? TrailingColor { get; init; }
}

public static class MotherBrainRainbowPaletteFormat
{
    public const string FileName = "mother-brain-rainbow-palette.json";
    public const int Version = 3;
    public const int PreFakeDeathVersion = 2;
    public const int RainbowFrameCount = 10;
    public const int GreyFrameCount = 8;
    /// <summary>38 sampled BGR555 words before the signed bank-$88 loop terminator.</summary>
    public const int BeamCycleColorCount = 38;
}
