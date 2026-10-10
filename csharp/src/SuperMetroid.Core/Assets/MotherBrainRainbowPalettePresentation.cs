using System.Text.Json;
using RedOriginLayout = SuperMetroid.Core.Assets.MotherBrainRainbowPaletteFormat.RedOriginLayout;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable rainbow, drain, revival, and normal-restoration colors for Mother Brain.</summary>
public sealed class MotherBrainRainbowPalettePresentation
{
    /// <summary>Ten full-CGRAM paint phases used while the rainbow attack runs.</summary>
    private readonly PaletteFrame[] rainbow;
    /// <summary>Drain palette frames, including their WRAM trailing words.</summary>
    private readonly PaletteFade toGrey;
    /// <summary>Revival frames, with the cartridge's held tissue shade and edited samples preserved.</summary>
    private readonly RevivalPaletteFade fromGrey;
    /// <summary>Brain-only palette fade used during fake death.</summary>
    private readonly PaletteFade fakeDeathToGrey;
    /// <summary>Normal body and rear-leg palette restored after the attack.</summary>
    private readonly PaletteFrame normal;
    /// <summary>Backdrop word displayed before the beam color cursor advances.</summary>
    private readonly ushort beamInitial;
    /// <summary>Sampled backdrop hue cycle, excluding its engine-owned terminator.</summary>
    private readonly BeamColors beamCycle;

    /// <summary>Installs compiled palette sequences and replaces only relationships verified against supplied colors.</summary>
    /// <param name="rainbow">Ten attack palette phases.</param><param name="toGrey">Drain transition frames.</param>
    /// <param name="fromGrey">Revival transition frames.</param><param name="fakeDeathToGrey">Brain-only fake-death descent.</param>
    /// <param name="normal">Restoration palette frame.</param><param name="beamInitial">Backdrop color before the beam cursor advances.</param>
    /// <param name="beamCycle">Sampled backdrop hue cycle.</param>
    private MotherBrainRainbowPalettePresentation(PaletteFrame[] rainbow, PaletteFrame[] toGrey,
        PaletteFrame[] fromGrey, PaletteFade fakeDeathToGrey, PaletteFrame normal,
        ushort beamInitial, ushort[] beamCycle)
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
        /// <summary>Original samples retained when any supplied word differs from the calculated hue wheel.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Number of authored backdrop color samples.</summary>
        public int Length { get; }
        /// <summary>Checks for a complete matching hue-wheel model before retaining calculated samples.</summary>
        /// <param name="colors">RGB5 words in native beam-cycle order.</param>
        public BeamColors(ushort[] colors)
        {
            Length = colors.Length;
            for (int index = 0; index < Length; index++)
                if (colors[index] != Calculate(index))
                { supplied = colors; return; }
        }
        /// <summary>Gets one authored backdrop color, preserving independent edits when present.</summary>
        /// <param name="index">Zero-based sample position.</param>
        /// <returns>The supplied or calculated RGB5 word.</returns>
        public ushort this[int index] => supplied is null ? Calculate(index) : supplied[index];

        /// <summary>Builds one word from the five asymmetric native hue-wheel legs.</summary>
        /// <param name="index">Zero-based position in the sampled cycle.</param>
        /// <returns>The packed RGB5 color.</returns>
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
        /// <summary>Rounds a fifteen-step channel rise to the nearest RGB5 level.</summary>
        private static int Rising(int step) => (31 * step + 7) / 15;
        /// <summary>Applies the cartridge's downward channel slope and late-step rounding bias.</summary>
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

    /// <summary>Copies a selected full palette frame after validating the frame index.</summary>
    /// <param name="cgram">CGRAM receiving body, brain, and rear-leg colors.</param>
    /// <param name="frames">Authored sequence from which to select.</param>
    /// <param name="frame">Zero-based sequence position.</param>
    private static void ApplyFull(SnesCgram cgram, PaletteFrame[] frames, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= frames.Length)
            throw new InvalidDataException($"Mother Brain rainbow frame {frame} is outside the authored loop.");
        ApplyColors(cgram, frames[frame], MotherBrainRainbowPaletteRomData.SecondaryColor);
    }

    /// <summary>Writes one drain or revival frame to CGRAM and publishes its trailing word to WRAM.</summary>
    /// <param name="bus">Address space receiving the native trailing word.</param>
    /// <param name="cgram">CGRAM receiving the body, brain, and rear-leg colors.</param>
    /// <param name="frames">Fade sequence whose dimensions define the copied ranges.</param>
    /// <param name="frame">Zero-based fade position.</param>
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

    /// <summary>Copies shared body/brain colors and legs to a caller-selected native palette slot.</summary>
    /// <param name="cgram">Palette memory to update.</param>
    /// <param name="selected">Frame supplying the body and leg colors.</param>
    /// <param name="legDestination">First CGRAM index for rear-leg colors.</param>
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

    /// <summary>Validates and compiles the brain-only fake-death sequence.</summary>
    /// <param name="source">Eight rows of the three native brain inks.</param>
    /// <returns>A fade that calculates regular interpolated words while preserving edited rows.</returns>
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

    /// <summary>Validates frame count, payload dimensions, and trailing-word presence before compilation.</summary>
    /// <param name="source">Document rows to compile.</param><param name="count">Required number of rows.</param>
    /// <param name="bodyCount">Required shared body/brain color count.</param><param name="legCount">Required rear-leg color count.</param>
    /// <param name="trailing">Whether each row must contain a WRAM trailing word.</param><param name="name">Sequence name used in validation errors.</param>
    /// <returns>Compiled palette frames in document order.</returns>
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

    /// <summary>Checks a color array's required length and packs its RGB5 components.</summary>
    /// <param name="source">Colors to compile.</param><param name="count">Required color count.</param><param name="name">Payload name used in validation errors.</param>
    /// <returns>Packed BGR555 words in source order.</returns>
    private static ushort[] CompileColors(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Mother Brain {name} requires {count} colors.");
        var colors = new ushort[count];
        for (int color = 0; color < count; color++)
            colors[color] = CompileColor(source[color], $"{name} color {color}");
        return colors;
    }

    /// <summary>Validates each five-bit component and packs a palette word.</summary>
    /// <param name="color">RGB5 components from the document.</param><param name="name">Color role used in validation errors.</param>
    /// <returns>The packed BGR555 word.</returns>
    private static ushort CompileColor(PaletteRgb5? color, string name)
    {
        if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
            throw new InvalidDataException($"Mother Brain {name} requires RGB5 components.");
        return (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
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

    /// <summary>One selected body/brain, rear-leg, and optional trailing-word palette row.</summary>
    private sealed class PaletteFrame
    {
        /// <summary>Independent leg words when the palette does not match a recognized source relationship.</summary>
        private readonly ushort[]? backLegs;
        /// <summary>Whether legs reuse the normal stock rear palette directly.</summary>
        private readonly bool stockRear;
        /// <summary>Whether legs are half-intensity stock rear colors during drain.</summary>
        private readonly bool drainedRear;
        /// <summary>Whether the revival endpoint selects the normal rear palette's five-color subset.</summary>
        private readonly bool normalRearSubset;
        /// <summary>Source frame used to derive a shared drain or revival rear-leg sequence.</summary>
        private readonly PaletteFrame? rearSource;
        /// <summary>Optional word written to the drain/revival WRAM slot.</summary>
        private readonly ushort? trailing;

        /// <summary>Creates a frame and recognizes rear-leg relationships only when every sample agrees.</summary>
        /// <param name="body">Shared body and brain inks.</param><param name="legs">Rear-leg inks supplied by the document.</param>
        /// <param name="trailingColor">Optional native trailing WRAM word.</param>
        public PaletteFrame(ushort[] body, ushort[] legs, ushort? trailingColor)
        {
            Body = new BodyColors(body);
            LegCount = legs.Length;
            trailing = trailingColor;
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

        /// <summary>Shared body/brain inks, with recognized calculated relationships compressed where exact.</summary>
        public BodyColors Body { get; private set; }
        /// <summary>Replaces an exact red-origin body palette with its calculated shade model when all samples match.</summary>
        internal void ReduceRedOrigin()
        {
            RedOriginColors? selected = RedOriginColors.TryCreate(Body);
            if (selected is not null) Body = new BodyColors(selected);
        }
        /// <summary>Shares selected RGB channels with another phase when the supplied frame proves that relationship.</summary>
        /// <param name="source">Phase providing shared channel values.</param><param name="profile">Profile for the independently authored channel.</param>
        /// <param name="red">Whether red is copied with the requested offset.</param><param name="green">Whether green is copied with the requested offset.</param>
        /// <param name="blue">Whether blue is copied unchanged.</param><param name="redAddition">Offset applied to shared red.</param><param name="greenAddition">Offset applied to shared green.</param>
        internal void ShareChannels(PaletteFrame source, MotherBrainRainbowShadeProfile profile, bool red, bool green, bool blue, int redAddition = 0, int greenAddition = 0)
        {
            if (Body.Length != source.Body.Length) return;
            var sharing = SharedBodyChannels.TryCreate(source.Body, Body, profile, red, green, blue, redAddition, greenAddition);
            if (sharing is not null) Body = new BodyColors(sharing);
        }
        /// <summary>Uses a tinted view of another phase only if every current body ink equals that tint.</summary>
        /// <param name="source">Phase supplying the original colors.</param><param name="red">Red channel offset.</param><param name="green">Green channel offset.</param><param name="blue">Blue channel offset.</param>
        internal void ShareTint(PaletteFrame source, int red, int green, int blue)
        {
            if (Body.Length != source.Body.Length) return;
            for (int color = 0; color < Body.Length; color++)
                if (Body[color] != BodyColors.Tinted(source.Body[color], red, green, blue)) return;
            Body = new BodyColors(source.Body, red, green, blue);
        }
        /// <summary>Number of rear-leg inks present in this frame.</summary>
        public int LegCount { get; }
        /// <summary>Gets the trailing word, deriving it from the matching native rear color when that relation was verified.</summary>
        public ushort? TrailingColor => rearSource is not null
            ? rearSource.Leg(MotherBrainDrainedPaletteRomData.TrailingRearSourceColor)
            : drainedRear || normalRearSubset ? Leg(1) : trailing;
        /// <summary>Gets a rear-leg ink from its retained, stock, drained, or verified shared representation.</summary>
        /// <param name="color">Zero-based rear-leg color index.</param>
        public ushort Leg(int color) => rearSource is not null
            ? rearSource.Leg(color + MotherBrainDrainedPaletteRomData.RearSourceColor)
            : drainedRear ? HalfIntensity(MotherBrainHealthPalettePresentation.StockBaseColor(true,
                color + MotherBrainDrainedPaletteRomData.RearSourceColor))
            : normalRearSubset ? MotherBrainHealthPalettePresentation.StockBaseColor(true,
                color + MotherBrainDrainedPaletteRomData.RearSourceColor)
            : stockRear ? MotherBrainHealthPalettePresentation.StockBaseColor(true, color)
            : backLegs is null ? HalfIntensity(Body[color]) : backLegs[color];

        // Endpoint views are selected only after every separately supplied rear and
        // trailing value matches; both sequences retain independent edit behavior.
        /// <summary>Shares the drain's initial paint with the rainbow endpoint when body, legs, and trailing word all match.</summary>
        /// <param name="drain">First drain frame.</param><param name="rainbow">Rainbow frame supplying the candidate endpoint.</param>
        internal static PaletteFrame ShareDrainStart(PaletteFrame drain, PaletteFrame rainbow)
        {
            for (int color = 0; color < drain.Body.Length; color++)
                if (drain.Body[color] != rainbow.Body[color]) return drain;
            for (int color = 0; color < drain.LegCount; color++)
                if (drain.Leg(color) != rainbow.Leg(color + MotherBrainDrainedPaletteRomData.RearSourceColor)) return drain;
            return drain.TrailingColor == rainbow.Leg(MotherBrainDrainedPaletteRomData.TrailingRearSourceColor)
                ? new PaletteFrame(rainbow, false) : drain;
        }
        /// <summary>Recognizes the drained stock rear endpoint and its matching trailing word.</summary>
        /// <param name="drain">Last drain frame to inspect.</param>
        internal static PaletteFrame ShareDrainEnd(PaletteFrame drain)
        {
            for (int color = 0; color < drain.LegCount; color++)
                if (drain.Leg(color) != HalfIntensity(MotherBrainHealthPalettePresentation.StockBaseColor(true,
                    color + MotherBrainDrainedPaletteRomData.RearSourceColor))) return drain;
            return drain.TrailingColor == drain.Leg(1) ? new PaletteFrame(drain, true) : drain;
        }
        /// <summary>Creates a drain endpoint view that shares body colors and derives its rear palette.</summary>
        /// <param name="source">Frame supplying the shared body inks and, for the start endpoint, source legs.</param><param name="drainedRear">Selects half-intensity stock legs instead of source legs.</param>
        private PaletteFrame(PaletteFrame source, bool drainedRear)
        {
            Body = source.Body;
            LegCount = MotherBrainDrainedPaletteRomData.BackLegCount;
            this.drainedRear = drainedRear;
            rearSource = drainedRear ? null : source;
        }

        /// <summary>Shares the normal rear-palette subset at the final revival endpoint when all entries match.</summary>
        /// <param name="frame">Final revival frame to inspect.</param>
        internal static PaletteFrame ShareNormalRearEndpoint(PaletteFrame frame)
        {
            for (int color = 0; color < frame.LegCount; color++)
                if (frame.Leg(color) != MotherBrainHealthPalettePresentation.StockBaseColor(true,
                    color + MotherBrainDrainedPaletteRomData.RearSourceColor)) return frame;
            return frame.TrailingColor == frame.Leg(1) ? new PaletteFrame(frame) : frame;
        }
        /// <summary>Creates a five-leg view backed by the matching normal rear palette subset.</summary>
        /// <param name="source">Frame whose body and verified rear colors are reused.</param>
        private PaletteFrame(PaletteFrame source)
        {
            Body = source.Body;
            LegCount = MotherBrainDrainedPaletteRomData.BackLegCount;
            normalRearSubset = true;
        }

        /// <summary>Halves each RGB5 component independently, rounding odd values upward.</summary>
        private static ushort HalfIntensity(ushort color) => (ushort)(
            ((color & 31) + 1) / 2 | (((color >> 5 & 31) + 1) / 2) << 5
            | (((color >> 10 & 31) + 1) / 2) << 10);
    }

    /// <summary>Exact red-origin shading from the narrowly reviewed temporal material paints; calculated shades are not retained rows.</summary>
    private sealed class RedOriginColors
    {
        /// <summary>Minimal native color anchors needed to calculate the red-origin paint sequence.</summary>
        private readonly ushort[] inputs;
        /// <summary>Captures anchors from an already validated body palette.</summary>
        /// <param name="colors">Candidate source phase.</param>
        private RedOriginColors(BodyColors colors)
        {
            inputs = [colors[0], (ushort)(colors[RedOriginLayout.DarkHead] & SnesColorMasks.GreenBlue), colors[RedOriginLayout.PlateStart], colors[RedOriginLayout.PlateEnd], colors[RedOriginLayout.TissueStart], colors[RedOriginLayout.TissueEnd],
                (ushort)(colors[RedOriginLayout.TissueStart + 1] & SnesColorMasks.Green), (ushort)(colors[RedOriginLayout.TissueStart + 2] & SnesColorMasks.Green), (ushort)(colors[RedOriginLayout.TissueStart + 3] & SnesColorMasks.Green), colors[RedOriginLayout.TailStart], colors[RedOriginLayout.TailStart + 1]];
        }
        /// <summary>Returns a calculated red-origin model only when it reproduces every supplied ink exactly.</summary>
        /// <param name="colors">Candidate full body palette.</param><returns>The exact model, or <see langword="null"/> if it does not match.</returns>
        internal static RedOriginColors? TryCreate(BodyColors colors)
        {
            if (colors.Length != MotherBrainRainbowPaletteRomData.ColorCount ||
                (colors[0] & 31) < (RedOriginLayout.PlateStart - 1) * MotherBrainRainbowPaletteFormat.HeadRedStep) return null;
            var selected = new RedOriginColors(colors);
            for (int color = 0; color < colors.Length; color++)
                if (selected[color] != colors[color]) return null;
            return selected;
        }
        /// <summary>Gets one body ink from the compact anchor model.</summary>
        /// <param name="color">Zero-based native body color index.</param>
        internal ushort this[int color]
        {
            get
            {
                if (color < RedOriginLayout.PlateStart)
                {
                    int red = (inputs[0] & 31) - color * MotherBrainRainbowPaletteFormat.HeadRedStep;
                    int greenBlue = color < RedOriginLayout.DarkHead ? inputs[0] & SnesColorMasks.GreenBlue : inputs[1];
                    return (ushort)(red | greenBlue);
                }
                if (color < RedOriginLayout.TissueStart)
                {
                    int result = 0, shade = color - RedOriginLayout.PlateStart;
                    for (int shift = 0; shift < 15; shift += 5)
                        result |= (((inputs[2] >> shift & 31) * (RedOriginLayout.PlateIntervals - shade) +
                            (inputs[3] >> shift & 31) * shade + RedOriginLayout.PlateIntervals - 1) / RedOriginLayout.PlateIntervals) << shift;
                    return (ushort)result;
                }
                if (color < RedOriginLayout.TailStart)
                {
                    int shade = color - RedOriginLayout.TissueStart;
                    int red = ((inputs[4] & 31) * (RedOriginLayout.TissueIntervals - shade) + (inputs[5] & 31) * shade + RedOriginLayout.TissueIntervals / 2) / RedOriginLayout.TissueIntervals;
                    int blue = ((inputs[4] >> 10) * (RedOriginLayout.TissueIntervals - shade) + (inputs[5] >> 10) * shade) / RedOriginLayout.TissueIntervals;
                    int green = shade == 0 ? inputs[4] & SnesColorMasks.Green : shade == RedOriginLayout.TissueIntervals ? inputs[5] & SnesColorMasks.Green : inputs[5 + shade];
                    return (ushort)(red | green | blue << 10);
                }
                return inputs[color - RedOriginLayout.TailStart + 9];
            }
        }
    }
    /// <summary>Exact shared channels with all unexplained RGB5 samples retained independently.</summary>
    private sealed class SharedBodyChannels
    {
        /// <summary>Source phase supplying the derived channels.</summary>
        private readonly BodyColors source;
        /// <summary>Channels reconstructed from the source phase.</summary>
        private readonly bool red, green, blue;
        /// <summary>Fixed offsets applied to shared red and green channels.</summary>
        private readonly int redAddition, greenAddition, independentCount;
        /// <summary>Profile-backed samples for the independently authored channel.</summary>
        private readonly MotherBrainRainbowShadeChannel independent;
        /// <summary>Number of body inks in the represented phase.</summary>
        internal int Length => source.Length;
        /// <summary>Captures the independently authored component values and profile.</summary>
        private SharedBodyChannels(BodyColors source, BodyColors selected, MotherBrainRainbowShadeProfile profile, bool red, bool green, bool blue, int redAddition, int greenAddition)
        {
            this.source = source; this.red = red; this.green = green; this.blue = blue; this.redAddition = redAddition; this.greenAddition = greenAddition;
            independentCount = (red ? 0 : 1) + (green ? 0 : 1) + (blue ? 0 : 1);
            if (independentCount != 1) throw new InvalidOperationException("A rainbow shade profile requires one independently supplied channel.");
            var samples = new byte[Length];
            int index = 0;
            for (int color = 0; color < Length; color++)
            {
                ushort word = selected[color];
                if (!red) samples[index++] = (byte)(word & 31);
                if (!green) samples[index++] = (byte)(word >> 5 & 31);
                if (!blue) samples[index++] = (byte)(word >> 10 & 31);
            }
            independent = new MotherBrainRainbowShadeChannel(samples, profile, color => (source[color] >> 5) & 31);
        }
        /// <summary>Uses shared channels only when every supplied value proves the proposed relationship.</summary>
        /// <param name="source">Palette supplying shared channel values.</param><param name="selected">Palette being represented.</param>
        /// <param name="profile">Shade profile for independent samples.</param><param name="red">Whether red is shared.</param>
        /// <param name="green">Whether green is shared.</param><param name="blue">Whether blue is shared.</param>
        /// <param name="redAddition">Offset to shared red.</param><param name="greenAddition">Offset to shared green.</param>
        /// <returns>The compact view, or <see langword="null"/> on any mismatch.</returns>
        internal static SharedBodyChannels? TryCreate(BodyColors source, BodyColors selected, MotherBrainRainbowShadeProfile profile, bool red, bool green, bool blue, int redAddition, int greenAddition)
        {
            for (int color = 0; color < source.Length; color++)
            {
                ushort from = source[color], to = selected[color];
                if (red && (from & 31) + redAddition != (to & 31) ||
                    green && (from >> 5 & 31) + greenAddition != (to >> 5 & 31) ||
                    blue && (from >> 10 & 31) != (to >> 10 & 31)) return null;
            }
            return new(source, selected, profile, red, green, blue, redAddition, greenAddition);
        }
        /// <summary>Reconstructs a packed color from shared channels and the independent sample.</summary>
        /// <param name="color">Zero-based body ink index.</param>
        internal ushort this[int color]
        {
            get
            {
                ushort from = source[color];
                int index = color * independentCount;
                int r = red ? (from & 31) + redAddition : independent[index++];
                int g = green ? (from >> 5 & 31) + greenAddition : independent[index++];
                int b = blue ? from >> 10 & 31 : independent[index];
                return (ushort)(r | g << 5 | b << 10);
            }
        }
    }
    /// <summary>Recognizes normal and drained body paint without repeated palette rows.</summary>
    private sealed class BodyColors
    {
        /// <summary>Exact supplied rows retained when no supported calculated representation fits.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Verified red-origin palette model.</summary>
        private readonly RedOriginColors? redOrigin;
        /// <summary>Wraps a validated red-origin model without storing its calculated rows.</summary>
        /// <summary>Creates a view over an already verified red-origin palette model.</summary>
        /// <param name="colors">Compact source model.</param>
        internal BodyColors(RedOriginColors colors)
        {
            redOrigin = colors;
            Length = MotherBrainRainbowPaletteRomData.ColorCount;
        }
        /// <summary>Verified channel-sharing model for this phase.</summary>
        private readonly SharedBodyChannels? sharedChannels;
        /// <summary>Wraps a verified shared-channel representation.</summary>
        /// <summary>Creates a view over an exact channel-sharing representation.</summary>
        /// <param name="channels">Verified shared-channel model.</param>
        internal BodyColors(SharedBodyChannels channels)
        {
            sharedChannels = channels;
            Length = channels.Length;
        }
        /// <summary>Source phase for an exactly matching tint relationship.</summary>
        private readonly BodyColors? tintSource;
        /// <summary>Channel offsets applied to the tint source.</summary>
        private readonly int redTint, greenTint, blueTint;
        /// <summary>Creates a view over a source palette using fixed RGB5 offsets.</summary>
        /// <summary>Creates a fixed-offset view over a source phase after its colors have been matched.</summary>
        /// <param name="source">Palette to tint.</param><param name="red">Red channel offset.</param><param name="green">Green channel offset.</param><param name="blue">Blue channel offset.</param>
        internal BodyColors(BodyColors source, int red, int green, int blue)
        {
            tintSource = source;
            redTint = red; greenTint = green; blueTint = blue;
            Length = source.Length;
        }
        /// <summary>Applies RGB5 offsets, returning a negative sentinel if any channel overflows.</summary>
        /// <param name="word">Packed source color.</param><param name="red">Red offset.</param><param name="green">Green offset.</param><param name="blue">Blue offset.</param>
        internal static int Tinted(ushort word, int red, int green, int blue)
        {
            int r = (word & 31) + red, g = (word >> 5 & 31) + green, b = (word >> 10 & 31) + blue;
            return (uint)r > 31 || (uint)g > 31 || (uint)b > 31 ? -1 : r | g << 5 | b << 10;
        }
        /// <summary>Verified drained-paint representation, if available.</summary>
        private readonly DrainedBodyColors? drained;
        /// <summary>Number of body inks represented.</summary>
        public int Length { get; }
        /// <summary>Recognizes stock or drained paint, otherwise retaining the supplied rows exactly.</summary>
        /// <param name="colors">Packed body and brain colors.</param>
        public BodyColors(ushort[] colors)
        {
            Length = colors.Length;
            bool normal = Length is MotherBrainFakeDeathPaletteRomData.ColorCount or MotherBrainDrainedPaletteRomData.RevivalColors or MotherBrainRainbowPaletteRomData.ColorCount;
            for (int color = 0; normal && color < Length; color++)
                normal = colors[color] == MotherBrainHealthPalettePresentation.StockBaseColor(false, color);
            if (normal) return;
            drained = new DrainedBodyColors(colors);
            if (!drained.Calculated) { drained = null; supplied = colors; }
        }
        /// <summary>Gets a body ink from its retained or verified calculated representation.</summary>
        /// <param name="color">Zero-based body ink index.</param>
        public ushort this[int color] => redOrigin is not null ? redOrigin[color]
            : sharedChannels is not null ? sharedChannels[color]
            : tintSource is not null ? (ushort)Tinted(tintSource[color], redTint, greenTint, blueTint)
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
        /// <summary>Unquantized highlight endpoint for the tissue gradient.</summary>
        private readonly Rgb8 tissueLight;
        /// <summary>Unquantized darkest tissue endpoint.</summary>
        private readonly Rgb8 tissueDark;
        /// <summary>RGB5 outline anchor used by the cortex shades.</summary>
        private readonly ushort outline;
        /// <summary>Exact colors retained when the supplied palette does not fit the model.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Whether colors are calculated from the anchors rather than retained verbatim.</summary>
        internal bool Calculated => supplied is null;

        /// <summary>Recognizes supported drained palettes or retains their complete supplied rows.</summary>
        /// <param name="colors">Packed body colors from a drain, revival, or fake-death frame.</param>
        internal DrainedBodyColors(ushort[] colors)
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
        /// <summary>Gets one color from the exact input or calculated drained-paint model.</summary>
        /// <param name="color">Zero-based body ink index.</param>
        internal ushort this[int color] => supplied is null ? Calculate(color) : supplied[color];

        /// <summary>Calculates the modeled paint for one native body index.</summary>
        /// <param name="color">Zero-based body ink index.</param><returns>Packed RGB5 paint.</returns>
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

        /// <summary>Unquantized RGB endpoint used when the cartridge interpolates tissue before RGB5 conversion.</summary>
        /// <param name="Red">Unquantized eight-bit red channel.</param>
        /// <param name="Green">Unquantized eight-bit green channel.</param>
        /// <param name="Blue">Unquantized eight-bit blue channel.</param>
        private readonly record struct Rgb8(int Red, int Green, int Blue)
        {
            /// <summary>Selects one channel component.</summary>
            /// <param name="index">Zero for red, one for green, two for blue.</param><returns>The selected RGB8 component.</returns>
            internal int Component(int index) => index switch
            { 0 => Red, 1 => Green, 2 => Blue, _ => throw new IndexOutOfRangeException() };
        }
    }
    /// <summary>Indexed view consumed by the common drain and revival application path.</summary>
    private interface IPaletteFade
    {
        /// <summary>Frame count.</summary>
        int Length { get; }
        /// <summary>Body/brain color count.</summary>
        int BodyCount { get; }
        /// <summary>Rear-leg color count.</summary>
        int LegCount { get; }
        /// <summary>Gets one body/brain sample.</summary>
        ushort Body(int frame, int color);
        /// <summary>Gets one rear-leg sample.</summary>
        ushort Leg(int frame, int color);
        /// <summary>Gets the frame's optional WRAM trailing word.</summary>
        ushort? Trailing(int frame);
    }

    // Revival rounds RGB5 endpoint interpolation to the nearest channel value. Stock
    // holds the prior shade at one independently reviewed tissue-ink/phase choice.
    // Sparse differences also preserve every independently edited output and endpoint.
    /// <summary>Calculates revival endpoint interpolation while retaining the native held-shade samples.</summary>
    private sealed class RevivalPaletteFade : IPaletteFade
    {
        /// <summary>First palette supplying interpolation start values.</summary>
        private readonly PaletteFrame first;
        /// <summary>Last palette supplying interpolation end values.</summary>
        private readonly PaletteFrame last;
        /// <summary>Samples that differ from calculated revival interpolation and must remain exact.</summary>
        private readonly Dictionary<(int Frame, int Color), ushort> suppliedOverrides = new();

        /// <summary>Builds an endpoint fade and records every independently authored difference.</summary>
        /// <param name="frames">Complete revival sequence in native order.</param>
        public RevivalPaletteFade(PaletteFrame[] frames)
        {
            first = frames[0];
            last = frames[^1];
            Length = frames.Length;
            for (int frame = 0; frame < Length; frame++)
                for (int color = 0; color < BodyCount + LegCount + 1; color++)
                {
                    ushort supplied = ReadColor(frames[frame], color);
                    if (Interpolate(frame, color) != supplied)
                        suppliedOverrides.Add((frame, color), supplied);
                }
        }

        /// <summary>Number of revival frames.</summary>
        public int Length { get; }
        /// <summary>Body and brain colors per frame.</summary>
        public int BodyCount => first.Body.Length;
        /// <summary>Rear-leg colors per frame.</summary>
        public int LegCount => first.LegCount;
        /// <summary>Gets one body/brain sample from a revival frame.</summary>
        public ushort Body(int frame, int color) => Color(frame, color);
        /// <summary>Gets one rear-leg sample from a revival frame.</summary>
        public ushort Leg(int frame, int color) => Color(frame, BodyCount + color);
        /// <summary>Gets the trailing WRAM word for a revival frame.</summary>
        public ushort? Trailing(int frame) => Color(frame, BodyCount + LegCount);

        /// <summary>Returns an exact override or calculates the selected fade color.</summary>
        private ushort Color(int frame, int color) => suppliedOverrides.TryGetValue((frame, color), out ushort supplied)
            ? supplied : Interpolate(frame, color);

        /// <summary>Reads a flat color position from a palette's body, legs, or trailing word.</summary>
        private ushort ReadColor(PaletteFrame palette, int color) => color < BodyCount ? palette.Body[color]
            : color < BodyCount + LegCount ? palette.Leg(color - BodyCount) : palette.TrailingColor!.Value;

        /// <summary>Interpolates one channel-packed word after applying the native per-color frame adjustment.</summary>
        private ushort Interpolate(int frame, int color)
        {
            frame = MotherBrainDrainedPaletteRomData.RevivalInterpolationFrame(frame, color);
            ushort start = ReadColor(first, color);
            ushort end = ReadColor(last, color);
            int intervals = Length - 1;
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= (((start >> shift & 31) * (intervals - frame)
                    + (end >> shift & 31) * frame + intervals / 2) / intervals) << shift;
            return (ushort)result;
        }
    }
    /// <summary>Uses straight endpoint interpolation when exact, otherwise reads every supplied frame verbatim.</summary>
    private sealed class PaletteFade : IPaletteFade
    {
        /// <summary>First endpoint for calculated channel interpolation.</summary>
        private readonly PaletteFrame first;
        /// <summary>Last endpoint for calculated channel interpolation.</summary>
        private readonly PaletteFrame last;
        /// <summary>Complete rows retained if any supplied intermediate color differs from interpolation.</summary>
        private readonly PaletteFrame[]? supplied;

        /// <summary>Uses endpoint interpolation only when it reproduces every supplied row.</summary>
        /// <param name="frames">Authored sequence to inspect.</param>
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

        /// <summary>Number of frames in the fade.</summary>
        public int Length { get; }
        /// <summary>Body and brain colors per frame.</summary>
        public int BodyCount => first.Body.Length;
        /// <summary>Rear-leg colors per frame.</summary>
        public int LegCount => first.LegCount;
        /// <summary>Gets a body/brain sample from an exact row or calculated fade.</summary>
        public ushort Body(int frame, int color) => supplied is null
            ? Interpolate(first.Body[color], last.Body[color], frame) : supplied[frame].Body[color];
        /// <summary>Gets a rear-leg sample from an exact row or calculated fade.</summary>
        public ushort Leg(int frame, int color) => supplied is null
            ? Interpolate(first.Leg(color), last.Leg(color), frame) : supplied[frame].Leg(color);
        /// <summary>Gets an exact or interpolated trailing word, if the endpoints contain one.</summary>
        public ushort? Trailing(int frame) => supplied is not null ? supplied[frame].TrailingColor
            : first.TrailingColor is ushort start && last.TrailingColor is ushort end
                ? Interpolate(start, end, frame) : null;

        /// <summary>Interpolates RGB5 channels independently, rounding to the nearest level.</summary>
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

/// <summary>Installed palette filename and fixed native sequence geometry; color content is editable while attack/fade timing and loop termination remain engine-owned.</summary>
public static class MotherBrainRainbowPaletteFormat
{
    /// <summary>Defines the body-color index ranges used to reconstruct the red-origin palette layout.</summary>
    internal static class RedOriginLayout
    {
        /// <summary>Index ranges dividing the source body into head, plates, tissue, and independent tail inks.</summary>
        /// <summary>$AD:E44E: darker head pair begins at body color index2.</summary>
        internal const int DarkHead = 2;
        /// <summary>$AD:E452-E459: four plate inks at body indices4..7.</summary>
        internal const int PlateStart = 4, PlateEnd = 7;
        /// <summary>Number of interpolation steps between the two plate endpoint inks.</summary>
        internal const int PlateIntervals = PlateEnd - PlateStart;
        /// <summary>$AD:E45A-E463: five tissue inks at body indices8..12.</summary>
        internal const int TissueStart = 8, TissueEnd = 12;
        /// <summary>Number of interpolation steps between the tissue endpoint inks.</summary>
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
    /// <summary>Current palette schema revision, requiring the brain-only fake-death sequence alongside all previous color families.</summary>
    public const int Version = 3;
    /// <summary>Legacy revision lacking fake-death colors, accepted only when loading with a verified current-stock fallback.</summary>
    public const int PreFakeDeathVersion = 2;
    /// <summary>Ten full-palette phases before the zero terminator in the native $AD:E434 rainbow pointer list.</summary>
    public const int RainbowFrameCount = 10;
    /// <summary>Eight authored steps in each native drain and revival palette pointer sequence; cadence and completion are controlled by boss AI.</summary>
    public const int GreyFrameCount = 8;
    /// <summary>38 sampled BGR555 words before the signed bank-$88 loop terminator.</summary>
    public const int BeamCycleColorCount = 38;
}
