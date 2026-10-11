using System.Text.Json;
using RedOriginLayout = SuperMetroid.Core.Assets.MotherBrainRainbowPaletteFormat.RedOriginLayout;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable rainbow, drain, revival, and normal-restoration colors for Mother Brain.</summary>
public sealed class MotherBrainRainbowPalettePresentation
{
    private readonly PaletteFrame[] rainbow;
    private readonly PaletteFade toGrey;
    private readonly RevivalPaletteFade fromGrey;
    private readonly PaletteFade fakeDeathToGrey;
    private readonly PaletteFrame normal;
    private readonly BeamColors beamCycle;

    private MotherBrainRainbowPalettePresentation(PaletteFrame[] rainbow, PaletteFrame[] toGrey,
        PaletteFrame[] fromGrey, PaletteFade fakeDeathToGrey, PaletteFrame normal,
        Bgr555 beamInitial, Bgr555[] beamCycle)
    {
        rainbow[MotherBrainRainbowPaletteFormat.RedOriginPhase].ReduceRedOrigin();
        rainbow[MotherBrainRainbowPaletteFormat.FirstGreenRisePhase].ShareChannels(rainbow[MotherBrainRainbowPaletteFormat.RedOriginPhase], MotherBrainRainbowShadeProfile.FirstGreenRise, true, false, true);
        rainbow[MotherBrainRainbowPaletteFormat.RedReductionSourcePhase].ShareChannels(rainbow[MotherBrainRainbowPaletteFormat.RedOriginPhase], MotherBrainRainbowShadeProfile.SecondGreenRise, true, false, true);
        rainbow[MotherBrainRainbowPaletteFormat.RedReducedPhase].ShareTint(
            rainbow[MotherBrainRainbowPaletteFormat.RedReductionSourcePhase],
            -MotherBrainRainbowPaletteFormat.RedReduction, 0, 0);
        rainbow[MotherBrainRainbowPaletteFormat.BlueGreenShiftPhase].ShareTint(
            rainbow[MotherBrainRainbowPaletteFormat.RedReducedPhase], 0,
            -MotherBrainRainbowPaletteFormat.BlueGreenShift, MotherBrainRainbowPaletteFormat.BlueGreenShift);
        rainbow[MotherBrainRainbowPaletteFormat.MixedShiftPhase].ShareChannels(rainbow[MotherBrainRainbowPaletteFormat.RedOriginPhase], MotherBrainRainbowShadeProfile.MixedBlue, true, true, false,
            -MotherBrainRainbowPaletteFormat.RedReduction, MotherBrainRainbowPaletteFormat.GreenAddition);
        rainbow[MotherBrainRainbowPaletteFormat.BlueMaximumPhase].ShareChannels(rainbow[MotherBrainRainbowPaletteFormat.RedOriginPhase], MotherBrainRainbowShadeProfile.MaximumBlue, true, true, false,
            -MotherBrainRainbowPaletteFormat.RedReduction);
        rainbow[MotherBrainRainbowPaletteFormat.RedRecoveryPhase].ShareChannels(rainbow[MotherBrainRainbowPaletteFormat.BlueMaximumPhase], MotherBrainRainbowShadeProfile.RecoveringGreen, true, false, true,
            MotherBrainRainbowPaletteFormat.RedRecoveryAddition);
        rainbow[MotherBrainRainbowPaletteFormat.RedRestoredPhase].ShareTint(rainbow[MotherBrainRainbowPaletteFormat.BlueMaximumPhase],
            MotherBrainRainbowPaletteFormat.RedReduction, 0, 0);
        rainbow[MotherBrainRainbowPaletteFormat.BlueRaisedPhase].ShareTint(rainbow[MotherBrainRainbowPaletteFormat.RedOriginPhase], 0, 0,
            MotherBrainRainbowPaletteFormat.BlueAddition);
        this.rainbow = rainbow;
        toGrey[0] = PaletteFrame.ShareDrainStart(toGrey[0],
            rainbow[MotherBrainRainbowPaletteRomData.DrainedPointerOffset / sizeof(ushort)]);
        toGrey[^1] = PaletteFrame.ShareDrainEnd(toGrey[^1]);
        this.toGrey = new PaletteFade(toGrey);
        fromGrey[0] = PaletteFrame.ShareDrainEnd(fromGrey[0]);
        fromGrey[^1] = PaletteFrame.ShareNormalRearEndpoint(fromGrey[^1]);
        this.fromGrey = new RevivalPaletteFade(fromGrey);
        this.fakeDeathToGrey = fakeDeathToGrey;
        this.normal = normal;
        BeamInitialColor = beamInitial;
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
        private readonly Bgr555[]? supplied;
        public int Length { get; }
        public BeamColors(Bgr555[] colors)
        {
            Length = colors.Length;
            for (int index = 0; index < Length; index++)
                if (colors[index] != Calculate(index))
                { supplied = colors; return; }
        }
        public Bgr555 this[int index] => supplied is null ? Calculate(index) : supplied[index];

        private static Bgr555 Calculate(int index)
        {
            int step = index * MotherBrainBeamRomData.ColorStride / Bgr555.ByteCount;
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
            return new Bgr555(red, green, blue);
        }
        private static int Rising(int step) => (31 * step + 7) / 15;
        private static int Falling(int step) => 31 - 2 * step - (step >= 9 ? 1 : 0);
    }
    /// <summary>Fixed-color backdrop used on the beam's first active HDMA frame.</summary>
    public Bgr555 BeamInitialColor { get; }

    /// <summary>
    /// Resolves the native byte cursor. The cycle's signed terminator is engine control,
    /// not an editable color: it returns false, and the HDMA owner performs the reset.
    /// </summary>
    public bool TryReadBeamColor(int byteCursor, out Bgr555 color)
    {
        if (byteCursor < 0 || byteCursor % MotherBrainBeamRomData.ColorStride != 0 ||
            byteCursor > beamCycle.Length * MotherBrainBeamRomData.ColorStride)
            throw new InvalidDataException($"Mother Brain beam color cursor ${byteCursor:X} is outside its authored cycle.");
        int index = byteCursor / MotherBrainBeamRomData.ColorStride;
        if (index == beamCycle.Length)
        {
            color = Bgr555.Black;
            return false;
        }
        color = beamCycle[index];
        return true;
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
            Bgr555 value = frames.Body(frame, color);
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + color, value);
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + color, value);
        }
        for (int color = 0; color < frames.LegCount; color++)
            cgram.SetColor(MotherBrainDrainedPaletteRomData.BackLegColor + color, frames.Leg(frame, color));
        ushort trailing = frames.Trailing(frame)!.Value.ToWord();
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

    /// <summary>Loads and compiles bounded RGB5 rainbow, drain, revival, restoration, and beam-backdrop colors; validates frame geometry and channels without importing engine timers or signed loop terminators.</summary>
    /// <param name="json">Caller-owned <c>mother-brain-rainbow-palette.json</c> stream, read from its current position and left open.</param>
    /// <param name="currentStock">Verified current presentation supplying only the missing fake-death fade when loading legacy version 2; without it, version 3 is required.</param>
    /// <returns>Compiled colors retaining independent edits while sharing calculated native color relationships when their supplied values agree.</returns>
    public static MotherBrainRainbowPalettePresentation Load(Stream json,
        MotherBrainRainbowPalettePresentation? currentStock = null)
    {
        MotherBrainRainbowPaletteDocument document = JsonAssetDocument.Read<MotherBrainRainbowPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "Mother Brain rainbow palette");
        var version = (MotherBrainRainbowPaletteVersion)document.Version;
        if (version != MotherBrainRainbowPaletteVersion.Current &&
            !(version == MotherBrainRainbowPaletteVersion.PreFakeDeath && currentStock is not null))
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
            version == MotherBrainRainbowPaletteVersion.PreFakeDeath
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

    private static Bgr555[] CompileColors(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Mother Brain {name} requires {count} colors.");
        var colors = new Bgr555[count];
        for (int color = 0; color < count; color++)
            colors[color] = CompileColor(source[color], $"{name} color {color}");
        return colors;
    }

    private static Bgr555 CompileColor(PaletteRgb5? color, string name)
    {
        if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
            throw new InvalidDataException($"Mother Brain {name} requires RGB5 components.");
        return color.ToBgr555();
    }

    /// <summary>Serializes UTF-8 palette JSON and validates it without a legacy fallback before writing any bytes; consequently the document must satisfy the complete version-3 schema.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Bounded RGB5 palette and beam-color sequences to serialize.</param>
    public static void Write(Stream json, MotherBrainRainbowPaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    private sealed class PaletteFrame
    {
        private readonly Bgr555[]? backLegs;
        private readonly bool stockRear;
        private readonly bool drainedRear;
        private readonly bool normalRearSubset;
        private readonly PaletteFrame? rearSource;

        public PaletteFrame(Bgr555[] body, Bgr555[] legs, Bgr555? trailingColor)
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

        public BodyColors Body { get; private set; }
        internal void ReduceRedOrigin()
        {
            RedOriginColors? selected = RedOriginColors.TryCreate(Body);
            if (selected is not null) Body = new BodyColors(selected);
        }
        internal void ShareChannels(PaletteFrame source, MotherBrainRainbowShadeProfile profile, bool red, bool green, bool blue, int redAddition = 0, int greenAddition = 0)
        {
            if (Body.Length != source.Body.Length) return;
            var sharing = SharedBodyChannels.TryCreate(source.Body, Body, profile, red, green, blue, redAddition, greenAddition);
            if (sharing is not null) Body = new BodyColors(sharing);
        }
        internal void ShareTint(PaletteFrame source, int red, int green, int blue)
        {
            if (Body.Length != source.Body.Length) return;
            for (int color = 0; color < Body.Length; color++)
                if (Body[color] != BodyColors.Tinted(source.Body[color], red, green, blue)) return;
            Body = new BodyColors(source.Body, red, green, blue);
        }
        public int LegCount { get; }
        public Bgr555? TrailingColor => rearSource is not null
            ? rearSource.Leg(MotherBrainDrainedPaletteRomData.TrailingRearSourceColor)
            : drainedRear || normalRearSubset ? Leg(1) : field;
        public Bgr555 Leg(int color) => rearSource is not null
            ? rearSource.Leg(color + MotherBrainDrainedPaletteRomData.RearSourceColor)
            : drainedRear ? HalfIntensity(MotherBrainHealthPalettePresentation.StockBaseColor(true,
                color + MotherBrainDrainedPaletteRomData.RearSourceColor))
            : normalRearSubset ? MotherBrainHealthPalettePresentation.StockBaseColor(true,
                color + MotherBrainDrainedPaletteRomData.RearSourceColor)
            : stockRear ? MotherBrainHealthPalettePresentation.StockBaseColor(true, color)
            : backLegs is null ? HalfIntensity(Body[color]) : backLegs[color];

        // Endpoint views are selected only after every separately supplied rear and
        // trailing value matches; both sequences retain independent edit behavior.
        internal static PaletteFrame ShareDrainStart(PaletteFrame drain, PaletteFrame rainbow)
        {
            for (int color = 0; color < drain.Body.Length; color++)
                if (drain.Body[color] != rainbow.Body[color]) return drain;
            for (int color = 0; color < drain.LegCount; color++)
                if (drain.Leg(color) != rainbow.Leg(color + MotherBrainDrainedPaletteRomData.RearSourceColor)) return drain;
            return drain.TrailingColor == rainbow.Leg(MotherBrainDrainedPaletteRomData.TrailingRearSourceColor)
                ? new PaletteFrame(rainbow, false) : drain;
        }
        internal static PaletteFrame ShareDrainEnd(PaletteFrame drain)
        {
            for (int color = 0; color < drain.LegCount; color++)
                if (drain.Leg(color) != HalfIntensity(MotherBrainHealthPalettePresentation.StockBaseColor(true,
                    color + MotherBrainDrainedPaletteRomData.RearSourceColor))) return drain;
            return drain.TrailingColor == drain.Leg(1) ? new PaletteFrame(drain, true) : drain;
        }
        private PaletteFrame(PaletteFrame source, bool drainedRear)
        {
            Body = source.Body;
            LegCount = MotherBrainDrainedPaletteRomData.BackLegCount;
            this.drainedRear = drainedRear;
            rearSource = drainedRear ? null : source;
        }

        internal static PaletteFrame ShareNormalRearEndpoint(PaletteFrame frame)
        {
            for (int color = 0; color < frame.LegCount; color++)
                if (frame.Leg(color) != MotherBrainHealthPalettePresentation.StockBaseColor(true,
                    color + MotherBrainDrainedPaletteRomData.RearSourceColor)) return frame;
            return frame.TrailingColor == frame.Leg(1) ? new PaletteFrame(frame) : frame;
        }
        private PaletteFrame(PaletteFrame source)
        {
            Body = source.Body;
            LegCount = MotherBrainDrainedPaletteRomData.BackLegCount;
            normalRearSubset = true;
        }

        private static Bgr555 HalfIntensity(Bgr555 color) => color.Map((_, channel) => (channel + 1) / 2);
    }

    /// <summary>Exact red-origin shading from the narrowly reviewed temporal material paints; calculated shades are not retained rows.</summary>
    private sealed class RedOriginColors
    {
        private readonly Bgr555 head, plateStart, plateEnd, tissueStart, tissueEnd;
        /// <summary>The dark head keeps its own green/blue under the shared head red ramp.</summary>
        private readonly (int Green, int Blue) darkHead;
        /// <summary>The three interior tissue shades keep their own green levels.</summary>
        private readonly int[] tissueGreens;
        private readonly Bgr555[] tail;
        private RedOriginColors(BodyColors colors)
        {
            head = colors[0];
            darkHead = (colors[RedOriginLayout.DarkHead].Green, colors[RedOriginLayout.DarkHead].Blue);
            plateStart = colors[RedOriginLayout.PlateStart];
            plateEnd = colors[RedOriginLayout.PlateEnd];
            tissueStart = colors[RedOriginLayout.TissueStart];
            tissueEnd = colors[RedOriginLayout.TissueEnd];
            tissueGreens = [colors[RedOriginLayout.TissueStart + 1].Green, colors[RedOriginLayout.TissueStart + 2].Green,
                colors[RedOriginLayout.TissueStart + 3].Green];
            tail = [colors[RedOriginLayout.TailStart], colors[RedOriginLayout.TailStart + 1]];
        }
        internal static RedOriginColors? TryCreate(BodyColors colors)
        {
            if (colors.Length != MotherBrainRainbowPaletteRomData.ColorCount ||
                colors[0].Red < (RedOriginLayout.PlateStart - 1) * MotherBrainRainbowPaletteFormat.HeadRedStep) return null;
            var selected = new RedOriginColors(colors);
            for (int color = 0; color < colors.Length; color++)
                if (selected[color] != colors[color]) return null;
            return selected;
        }
        internal Bgr555 this[int color]
        {
            get
            {
                if (color < RedOriginLayout.PlateStart)
                {
                    int red = head.Red - color * MotherBrainRainbowPaletteFormat.HeadRedStep;
                    return color < RedOriginLayout.DarkHead
                        ? head.WithRed(red)
                        : new Bgr555(red, darkHead.Green, darkHead.Blue);
                }
                if (color < RedOriginLayout.TissueStart)
                {
                    int shade = color - RedOriginLayout.PlateStart;
                    return plateStart.Zip(plateEnd, (_, from, to) => (from * (RedOriginLayout.PlateIntervals - shade) +
                        to * shade + RedOriginLayout.PlateIntervals - 1) / RedOriginLayout.PlateIntervals);
                }
                if (color < RedOriginLayout.TailStart)
                {
                    int shade = color - RedOriginLayout.TissueStart;
                    int red = (tissueStart.Red * (RedOriginLayout.TissueIntervals - shade) + tissueEnd.Red * shade + RedOriginLayout.TissueIntervals / 2) / RedOriginLayout.TissueIntervals;
                    int blue = (tissueStart.Blue * (RedOriginLayout.TissueIntervals - shade) + tissueEnd.Blue * shade) / RedOriginLayout.TissueIntervals;
                    int green = shade == 0 ? tissueStart.Green : shade == RedOriginLayout.TissueIntervals ? tissueEnd.Green : tissueGreens[shade - 1];
                    return new Bgr555(red, green, blue);
                }
                return tail[color - RedOriginLayout.TailStart];
            }
        }
    }
    /// <summary>Exact shared channels with all unexplained RGB5 samples retained independently.</summary>
    private sealed class SharedBodyChannels
    {
        private readonly BodyColors source;
        private readonly bool red, green, blue;
        private readonly int redAddition, greenAddition, independentCount;
        private readonly MotherBrainRainbowShadeChannel independent;
        internal int Length => source.Length;
        private SharedBodyChannels(BodyColors source, BodyColors selected, MotherBrainRainbowShadeProfile profile, bool red, bool green, bool blue, int redAddition, int greenAddition)
        {
            this.source = source; this.red = red; this.green = green; this.blue = blue; this.redAddition = redAddition; this.greenAddition = greenAddition;
            independentCount = (red ? 0 : 1) + (green ? 0 : 1) + (blue ? 0 : 1);
            if (independentCount != 1) throw new InvalidOperationException("A rainbow shade profile requires one independently supplied channel.");
            var samples = new byte[Length];
            int index = 0;
            for (int color = 0; color < Length; color++)
            {
                Bgr555 word = selected[color];
                if (!red) samples[index++] = (byte)(word.Red);
                if (!green) samples[index++] = (byte)(word.Green);
                if (!blue) samples[index++] = (byte)(word.Blue);
            }
            independent = new MotherBrainRainbowShadeChannel(samples, profile, color => source[color].Green);
        }
        internal static SharedBodyChannels? TryCreate(BodyColors source, BodyColors selected, MotherBrainRainbowShadeProfile profile, bool red, bool green, bool blue, int redAddition, int greenAddition)
        {
            for (int color = 0; color < source.Length; color++)
            {
                Bgr555 from = source[color], to = selected[color];
                if (red && (from.Red) + redAddition != (to.Red) ||
                    green && (from.Green) + greenAddition != (to.Green) ||
                    blue && (from.Blue) != (to.Blue)) return null;
            }
            return new(source, selected, profile, red, green, blue, redAddition, greenAddition);
        }
        internal Bgr555 this[int color]
        {
            get
            {
                Bgr555 from = source[color];
                int index = color * independentCount;
                int r = red ? (from.Red) + redAddition : independent[index++];
                int g = green ? (from.Green) + greenAddition : independent[index++];
                int b = blue ? from.Blue : independent[index];
                return new Bgr555(r, g, b);
            }
        }
    }
    /// <summary>Recognizes normal and drained body paint without repeated palette rows.</summary>
    private sealed class BodyColors
    {
        private readonly Bgr555[]? supplied;
        private readonly RedOriginColors? redOrigin;
        internal BodyColors(RedOriginColors colors)
        {
            redOrigin = colors;
            Length = MotherBrainRainbowPaletteRomData.ColorCount;
        }
        private readonly SharedBodyChannels? sharedChannels;
        internal BodyColors(SharedBodyChannels channels)
        {
            sharedChannels = channels;
            Length = channels.Length;
        }
        private readonly BodyColors? tintSource;
        private readonly int redTint, greenTint, blueTint;
        internal BodyColors(BodyColors source, int red, int green, int blue)
        {
            tintSource = source;
            redTint = red; greenTint = green; blueTint = blue;
            Length = source.Length;
        }
        /// <summary>The tinted color, or null when a channel leaves RGB5 (never a shared tint).</summary>
        internal static Bgr555? Tinted(Bgr555 color, int red, int green, int blue)
        {
            int r = color.Red + red, g = color.Green + green, b = color.Blue + blue;
            return (uint)r > 31 || (uint)g > 31 || (uint)b > 31 ? null : new Bgr555(r, g, b);
        }
        private readonly DrainedBodyColors? drained;
        public int Length { get; }
        public BodyColors(Bgr555[] colors)
        {
            Length = colors.Length;
            bool normal = Length is MotherBrainFakeDeathPaletteRomData.ColorCount or MotherBrainDrainedPaletteRomData.RevivalColors or MotherBrainRainbowPaletteRomData.ColorCount;
            for (int color = 0; normal && color < Length; color++)
                normal = colors[color] == MotherBrainHealthPalettePresentation.StockBaseColor(false, color);
            if (normal) return;
            drained = new DrainedBodyColors(colors);
            if (!drained.Calculated) { drained = null; supplied = colors; }
        }
        public Bgr555 this[int color] => redOrigin is not null ? redOrigin[color]
            : sharedChannels is not null ? sharedChannels[color]
            : tintSource is not null ? Tinted(tintSource[color], redTint, greenTint, blueTint)
                ?? throw new InvalidOperationException("A shared rainbow tint left RGB5 after construction.")
            : supplied is not null ? supplied[color]
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
        private readonly Bgr555 outline;
        private readonly Bgr555[]? supplied;
        internal bool Calculated => supplied is null;

        internal DrainedBodyColors(Bgr555[] colors)
        {
            if (colors.Length is not (3 or 13 or 15)) { supplied = colors; return; }
            var light = MotherBrainDrainedPaintDefinitions.HighlightRgb8;
            var dark = MotherBrainDrainedPaintDefinitions.DarkestTissueRgb8;
            tissueLight = new(light.Red, light.Green, light.Blue);
            tissueDark = new(dark.Red, dark.Green, dark.Blue);
            outline = MotherBrainDrainedPaintDefinitions.Outline;
            bool stock = true;
            for (int color = 0; stock && color < colors.Length; color++) stock = colors[color] == Calculate(color);
            if (stock) return;
            if (colors.Length == MotherBrainFakeDeathPaletteRomData.ColorCount) { supplied = colors; return; }
            outline = colors[3];
            if (!TryChannel(ColorChannel.Red, out int lr, out int dr) ||
                !TryChannel(ColorChannel.Green, out int lg, out int dg) ||
                !TryChannel(ColorChannel.Blue, out int lb, out int db))
            { supplied = colors; return; }
            tissueLight = new(lr, lg, lb);
            tissueDark = new(dr, dg, db);
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }

            bool TryChannel(ColorChannel channel, out int first, out int last)
            {
                int low = colors[8][channel] * 8;
                int high = colors[12][channel] * 8;
                for (int start = low; start < low + 8; start++)
                    for (int end = high; end < high + 8; end++)
                    {
                        bool matches = true;
                        for (int step = 0; step <= 4; step++)
                            if ((start * (4 - step) + end * step) / 32 != colors[8 + step][channel])
                            { matches = false; break; }
                        if (matches) { first = start; last = end; return true; }
                    }
                first = last = 0;
                return false;
            }
        }
        internal Bgr555 this[int color] => supplied is null ? Calculate(color) : supplied[color];

        private Bgr555 Calculate(int color)
        {
            if (color is >= 4 and <= 7)
                return MotherBrainHealthPalettePresentation.StockBaseColor(false, color);
            if (color == 13) return Bgr555.White;
            if (color == 14) return Bgr555.Black;
            if (color < 4)
                return outline.Map((channel, outlineChannel) =>
                    (tissueLight[channel] / 8 * (3 - color) + outlineChannel * color + 1) / 3);
            return new Bgr555(
                (tissueLight.Red * (12 - color) + tissueDark.Red * (color - 8)) / 32,
                (tissueLight.Green * (12 - color) + tissueDark.Green * (color - 8)) / 32,
                (tissueLight.Blue * (12 - color) + tissueDark.Blue * (color - 8)) / 32);
        }

        private readonly record struct Rgb8(int Red, int Green, int Blue)
        {
            internal int this[ColorChannel channel] => channel switch
            {
                ColorChannel.Red => Red,
                ColorChannel.Green => Green,
                ColorChannel.Blue => Blue,
                _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
            };
        }
    }
    private interface IPaletteFade
    {
        int Length { get; }
        int BodyCount { get; }
        int LegCount { get; }
        Bgr555 Body(int frame, int color);
        Bgr555 Leg(int frame, int color);
        Bgr555? Trailing(int frame);
    }

    // Revival rounds RGB5 endpoint interpolation to the nearest channel value. Stock
    // holds the prior shade at one independently reviewed tissue-ink/phase choice.
    // Sparse differences also preserve every independently edited output and endpoint.
    private sealed class RevivalPaletteFade : IPaletteFade
    {
        private readonly PaletteFrame first;
        private readonly PaletteFrame last;
        private readonly Dictionary<(int Frame, int Color), Bgr555> suppliedOverrides = new();

        public RevivalPaletteFade(PaletteFrame[] frames)
        {
            first = frames[0];
            last = frames[^1];
            Length = frames.Length;
            for (int frame = 0; frame < Length; frame++)
                for (int color = 0; color < BodyCount + LegCount + 1; color++)
                {
                    Bgr555 supplied = ReadColor(frames[frame], color);
                    if (Interpolate(frame, color) != supplied)
                        suppliedOverrides.Add((frame, color), supplied);
                }
        }

        public int Length { get; }
        public int BodyCount => first.Body.Length;
        public int LegCount => first.LegCount;
        public Bgr555 Body(int frame, int color) => Color(frame, color);
        public Bgr555 Leg(int frame, int color) => Color(frame, BodyCount + color);
        public Bgr555? Trailing(int frame) => Color(frame, BodyCount + LegCount);

        private Bgr555 Color(int frame, int color) => suppliedOverrides.TryGetValue((frame, color), out Bgr555 supplied)
            ? supplied : Interpolate(frame, color);

        private Bgr555 ReadColor(PaletteFrame palette, int color) => color < BodyCount ? palette.Body[color]
            : color < BodyCount + LegCount ? palette.Leg(color - BodyCount) : palette.TrailingColor!.Value;

        private Bgr555 Interpolate(int frame, int color)
        {
            frame = MotherBrainDrainedPaletteRomData.RevivalInterpolationFrame(frame, color);
            Bgr555 start = ReadColor(first, color);
            Bgr555 end = ReadColor(last, color);
            int intervals = Length - 1;
            return start.Zip(end, (_, a, b) => ((a * (intervals - frame)
                    + b * frame + intervals / 2) / intervals));
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
        public Bgr555 Body(int frame, int color) => supplied is null
            ? Interpolate(first.Body[color], last.Body[color], frame) : supplied[frame].Body[color];
        public Bgr555 Leg(int frame, int color) => supplied is null
            ? Interpolate(first.Leg(color), last.Leg(color), frame) : supplied[frame].Leg(color);
        public Bgr555? Trailing(int frame) => supplied is not null ? supplied[frame].TrailingColor
            : first.TrailingColor is Bgr555 start && last.TrailingColor is Bgr555 end
                ? Interpolate(start, end, frame) : null;

        private Bgr555 Interpolate(Bgr555 start, Bgr555 end, int frame)
        {
            int intervals = Length - 1;
            return start.Zip(end, (_, a, b) => ((a * (intervals - frame)
                    + b * frame + intervals / 2) / intervals));
        }
    }
}

/// <summary>Editable Mother Brain palette schema, separating selected RGB5 colors from engine-owned attack/fade cadence and HDMA loop control.</summary>
public sealed record MotherBrainRainbowPaletteDocument
{
    /// <summary>Schema revision: version 3 includes the fake-death fade; version 2 is loadable only with a current-stock presentation supplying that missing sequence.</summary>
    public required int Version { get; init; }
    /// <summary>Ten native $AD:E434 rainbow-list phases, each containing 15 body/brain colors and 15 rear-leg colors with no trailing word.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] Rainbow { get; init; }
    /// <summary>Eight $AD:EF87 drain phases, each containing 15 body/brain colors, five rear-leg colors, and a trailing word written to WRAM $017C.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] ToGrey { get; init; }
    /// <summary>Eight $AD:ED9C revival phases, each containing 13 body/brain colors, five rear-leg colors, and a trailing WRAM word; the final two body/brain inks are preserved.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] FromGrey { get; init; }
    /// <summary>Eight ordered rows of three brain-only RGB5 inks corresponding to $AD:ED8A, required by version 3 and applied beginning at CGRAM color $91.</summary>
    public PaletteRgb5[][]? FakeDeathToGrey { get; init; }
    /// <summary>Normal restoration frame with 15 shared body/brain inks and 15 rear-leg inks, matching the roles of $A9:9474/$9494; no trailing word is allowed.</summary>
    public required MotherBrainRainbowPaletteFrameDocument Normal { get; init; }
    /// <summary>RGB5 fixed backdrop color for the first active rainbow-beam HDMA frame, before the native color cursor advances.</summary>
    public required PaletteRgb5 BeamInitial { get; init; }
    /// <summary>Exactly 38 RGB5 backdrop samples from the native $88:E833 hue cycle; the terminating signed $FFFF word is engine control and is not included.</summary>
    public required PaletteRgb5[] BeamCycle { get; init; }
}

/// <summary>One RGB5 palette payload whose array dimensions depend on its rainbow, drain, revival, or normal-restoration role; each channel must be from 0 through 31.</summary>
public sealed record MotherBrainRainbowPaletteFrameDocument
{
    /// <summary>Shared body-BG and brain/neck-OBJ inks copied starting at CGRAM $41 and $91: 15 colors for rainbow/drain/normal, or 13 for revival.</summary>
    public required PaletteRgb5[] Body { get; init; }
    /// <summary>Rear-leg OBJ inks: 15 copied to CGRAM starting at $B1 for rainbow/normal, or five starting at $B4 for drain/revival.</summary>
    public required PaletteRgb5[] BackLegs { get; init; }
    /// <summary>Required RGB5 word for drain/revival, written directly to WRAM $7E:017C rather than CGRAM; must be absent for rainbow and normal frames.</summary>
    public PaletteRgb5? TrailingColor { get; init; }
}

/// <summary>Schema revisions of the editable Mother Brain rainbow palette document.</summary>
public enum MotherBrainRainbowPaletteVersion
{
    /// <summary>Legacy revision lacking fake-death colors, accepted only when loading with a verified current-stock fallback.</summary>
    PreFakeDeath = 2,
    /// <summary>Current palette schema revision, requiring the brain-only fake-death sequence alongside all previous color families.</summary>
    Current = 3,
}

/// <summary>Installed palette filename and fixed native sequence geometry; color content is editable while attack/fade timing and loop termination remain engine-owned.</summary>
public static class MotherBrainRainbowPaletteFormat
{
    internal static class RedOriginLayout
    {
        /// <summary>$AD:E44E: darker head pair begins at body color index2.</summary>
        internal const int DarkHead = 2;
        /// <summary>$AD:E452-E459: four plate inks at body indices4..7.</summary>
        internal const int PlateStart = 4, PlateEnd = 7;
        internal const int PlateIntervals = PlateEnd - PlateStart;
        /// <summary>$AD:E45A-E463: five tissue inks at body indices8..12.</summary>
        internal const int TissueStart = 8, TissueEnd = 12;
        internal const int TissueIntervals = TissueEnd - TissueStart;
        /// <summary>$AD:E464-E467: two remaining independent body inks at indices13..14.</summary>
        internal const int TailStart = 13;
    }
    /// <summary>$AD:E44A-E451: first four head/outline inks have red31/25/19/13; chosen decrement6 and paired other-channel policy are chosen paint-animation content.</summary>
    internal const int HeadRedStep = 6;
    /// <summary>$AD:E44A: original red phase, supplying shared channels; selected phase role is chosen paint-animation content.</summary>
    internal const int RedOriginPhase = 0;
    /// <summary>$AD:E486: first green rise shares original red/blue; its independent green inputs are chosen paint-animation content.</summary>
    internal const int FirstGreenRisePhase = 1;
    /// <summary>$AD:E576: phase5 retains original red minus10/green plus5; its independent blue inputs and tint choices are chosen paint-animation content.</summary>
    internal const int MixedShiftPhase = 5;
    /// <summary>$AD:E5B2: phase6 retains original green and reduced red; its independent blue inputs are chosen paint-animation content.</summary>
    internal const int BlueMaximumPhase = 6;
    /// <summary>$AD:E5EE: phase7 retains phase6 blue and increases red; its independent green inputs are chosen paint-animation content.</summary>
    internal const int RedRecoveryPhase = 7;
    /// <summary>$AD:E62A: phase8 is phase6 with restored red plus10; selected phase mapping/tint are chosen paint-animation content.</summary>
    internal const int RedRestoredPhase = 8;
    /// <summary>$AD:E576 versusE44A: selected green addition5 across all15 inks; independent of red/blue additions5 and chosen paint-animation content.</summary>
    internal const int GreenAddition = 5;
    /// <summary>$AD:E5EE versusE5B2: selected red recovery addition5; magnitude is chosen paint-animation content independently of blue tint5.</summary>
    internal const int RedRecoveryAddition = 5;
    /// <summary>$AD:E4C2/E4FE: phase3 copies phase2 with a selected red reduction; phase mapping is chosen paint-animation content.</summary>
    internal const int RedReductionSourcePhase = 2, RedReducedPhase = 3;
    /// <summary>$AD:E4FE: all15 body inks reduce red by10; chosen magnitude is chosen paint-animation content.</summary>
    internal const int RedReduction = 10;
    /// <summary>$AD:E53A: phase4 shifts phase3 from green toward blue; phase mapping is chosen paint-animation content.</summary>
    internal const int BlueGreenShiftPhase = 4;
    /// <summary>$AD:E53A: all15 body inks subtract2 green/add2 blue; selected amount is chosen paint-animation content.</summary>
    internal const int BlueGreenShift = 2;
    /// <summary>$AD:E666: phase9 copies phase0 with increased blue; selected mapping is chosen paint-animation content.</summary>
    internal const int BlueRaisedPhase = 9;
    /// <summary>$AD:E666 versusE44A: all15 body inks add5 blue; selected magnitude is chosen paint-animation content.</summary>
    internal const int BlueAddition = 5;
    /// <summary>Installed editable JSON filename for rainbow, drain/revival, fake-death, restoration, and beam-backdrop colors.</summary>
    public const string FileName = "mother-brain-rainbow-palette.json";
    /// <summary>Ten full-palette phases before the zero terminator in the native $AD:E434 rainbow pointer list.</summary>
    public const int RainbowFrameCount = 10;
    /// <summary>Eight authored steps in each native drain and revival palette pointer sequence; cadence and completion are controlled by boss AI.</summary>
    public const int GreyFrameCount = 8;
    /// <summary>38 sampled BGR555 words before the signed bank-$88 loop terminator.</summary>
    public const int BeamCycleColorCount = 38;
}
