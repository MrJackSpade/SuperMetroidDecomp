using System.Text.Json;
using RedOriginLayout = SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRainbowPaletteFormat.RedOriginLayout;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable rainbow, drain, revival, and normal-restoration colors for Mother Brain.</summary>
public sealed class MotherBrainRainbowPalettePresentation
{
    /// <summary>Ten color phases read by the native rainbow list.</summary>
    internal readonly PaletteFrame[] rainbow;
    /// <summary>Drain sequence whose last rear shade can share stock paint.</summary>
    internal readonly PaletteFade toGrey;
    /// <summary>Revival sequence with independently retained native exceptions.</summary>
    internal readonly RevivalPaletteFade fromGrey;
    /// <summary>Brain-only descent used during the staged fake death.</summary>
    internal readonly PaletteFade fakeDeathToGrey;
    /// <summary>Body and rear-leg colors restored after the beam.</summary>
    internal readonly PaletteFrame normal;
    /// <summary>Backdrop hue cycle without its signed loop terminator.</summary>
    internal readonly BeamColors beamCycle;

    /// <summary>Recognizes shared phase and endpoint relationships without changing independently supplied colors.</summary>
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
    internal sealed class BeamColors
    {
        /// <summary>Retained only when an edited sample differs from the native five-leg wheel.</summary>
        internal readonly Bgr555[]? supplied;
        /// <summary>Number of actual color samples, excluding the control terminator.</summary>
        public int Length { get; }
        /// <summary>Uses the calculated wheel only when every supplied RGB5 sample agrees.</summary>
        public BeamColors(Bgr555[] colors)
        {
            Length = colors.Length;
            for (int index = 0; index < Length; index++)
                if (colors[index] != Calculate(index))
                { supplied = colors; return; }
        }
        /// <summary>Reads an authored override or reconstructs the matching native wheel sample.</summary>
        public Bgr555 this[int index] => supplied is null ? Calculate(index) : supplied[index];

        /// <summary>Reconstructs the five hue legs using the cartridge's asymmetric blue-to-magenta shift.</summary>
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
        /// <summary>Rounds an increasing channel to its RGB5 sample.</summary>
        private static int Rising(int step) => (31 * step + 7) / 15;
        /// <summary>Applies the descending channel's changed bias after step eight.</summary>
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
        ApplyColors(cgram, normal);

    /// <summary>Copies a drain fade and publishes its trailing palette word to WRAM $017C.</summary>
    public void ApplyToGrey(ISnesAddressSpace bus, SnesCgram cgram, int frame) =>
        ApplyGrey(bus, cgram, toGrey, frame);

    /// <summary>Copies a revival fade, preserving the two native body/brain tail colors.</summary>
    public void ApplyFromGrey(ISnesAddressSpace bus, SnesCgram cgram, int frame) =>
        ApplyGrey(bus, cgram, fromGrey, frame);

    /// <summary>Changes only the three brain colors during the fake-death descent.</summary>
    public void ApplyFakeDeathToGrey(SnesCgram cgram, int frame)
    {
        Ensure.NotNull(cgram);
        if ((uint)frame >= fakeDeathToGrey.Length)
            throw new InvalidDataException($"Mother Brain fake-death grey frame {frame} is outside the authored sequence.");
        for (int color = 0; color < MotherBrainFakeDeathPaletteRomData.ColorCount; color++)
            new MotherBrainPalettes(cgram).FakeDeathBrain.Write(color,
                fakeDeathToGrey.Body(frame, color));
    }

    /// <summary>Restores only those three colors, reusing the cartridge's revival source table.</summary>
    public void ApplyFakeDeathFromGrey(SnesCgram cgram, int frame)
    {
        Ensure.NotNull(cgram);
        if ((uint)frame >= fromGrey.Length)
            throw new InvalidDataException($"Mother Brain fake-death revival frame {frame} is outside the authored sequence.");
        for (int color = 0; color < MotherBrainFakeDeathPaletteRomData.ColorCount; color++)
            new MotherBrainPalettes(cgram).FakeDeathBrain.Write(color, fromGrey.Body(frame, color));
    }

    /// <summary>Copies a complete selected rainbow phase into body, brain, and rear-leg CGRAM slots.</summary>
    private static void ApplyFull(SnesCgram cgram, PaletteFrame[] frames, int frame)
    {
        Ensure.NotNull(cgram);
        if ((uint)frame >= frames.Length)
            throw new InvalidDataException($"Mother Brain rainbow frame {frame} is outside the authored loop.");
        ApplyColors(cgram, frames[frame]);
    }

    /// <summary>Copies a grey-transition phase and writes its trailing rear color to live WRAM.</summary>
    private static void ApplyGrey(ISnesAddressSpace bus, SnesCgram cgram,
        IPaletteFade frames, int frame)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(cgram);
        if ((uint)frame >= frames.Length)
            throw new InvalidDataException($"Mother Brain grey-transition frame {frame} is outside the authored sequence.");
        for (int color = 0; color < frames.BodyCount; color++)
        {
            Bgr555 value = frames.Body(frame, color);
            new MotherBrainPalettes(cgram).Body.Write(color, value);
            new MotherBrainPalettes(cgram).Brain.Write(color, value);
        }
        for (int color = 0; color < frames.LegCount; color++)
            new MotherBrainPalettes(cgram).DrainedRearLegs.Write(color, frames.Leg(frame, color));
        var spill = new MotherBrainPaletteMemoryDefinitions(
            SnesMemoryBusRequirement.Require(bus, "Mother Brain's grey palette spill requires live WRAM."));
        spill.GreyTrailingWord = frames.Trailing(frame) ??
            throw new InvalidDataException($"Mother Brain grey-transition frame {frame} has no WRAM spill color.");
    }

    /// <summary>Mirrors body inks to the brain palette and writes the selected rear-leg inks.</summary>
    private static void ApplyColors(SnesCgram cgram, PaletteFrame selected)
    {
        for (int color = 0; color < selected.Body.Length; color++)
        {
            new MotherBrainPalettes(cgram).Body.Write(color, selected.Body[color]);
            new MotherBrainPalettes(cgram).Brain.Write(color, selected.Body[color]);
        }
        for (int color = 0; color < selected.LegCount; color++)
            new MotherBrainPalettes(cgram).RearLegs.Write(color, selected.Leg(color));
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
                ? (currentStock ?? throw new InvalidDataException("A pre-fake-death Mother Brain rainbow override requires the current stock presentation.")).fakeDeathToGrey
                : CompileFakeDeathFrames(document.FakeDeathToGrey),
            CompileFrames([document.Normal], 1, MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.ColorCount, false, nameof(document.Normal))[0],
            CompileColor(document.BeamInitial, nameof(document.BeamInitial)),
            CompileColors(document.BeamCycle, MotherBrainRainbowPaletteFormat.BeamCycleColorCount,
                nameof(document.BeamCycle)));
    }

    /// <summary>Requires eight three-ink brain frames, with no rear-leg or trailing color.</summary>
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

    /// <summary>Validates palette geometry and whether this sequence carries a trailing WRAM word.</summary>
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

    /// <summary>Checks an authored row's exact length before converting its RGB5 colors.</summary>
    private static Bgr555[] CompileColors(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Mother Brain {name} requires {count} colors.");
        var colors = new Bgr555[count];
        for (int color = 0; color < count; color++)
            colors[color] = CompileColor(source[color], $"{name} color {color}");
        return colors;
    }

    /// <summary>Rejects absent colors and channels outside the five-bit hardware range.</summary>
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

    /// <summary>One body and rear-leg phase, optionally recognized as shared stock or half-intensity paint.</summary>
    internal sealed class PaletteFrame
    {
        /// <summary>Retained rear inks when they do not match stock or half-intensity body paint.</summary>
        internal readonly Bgr555[]? backLegs;
        /// <summary>Whether every rear ink equals the full stock rear-leg palette.</summary>
        private readonly bool stockRear;
        /// <summary>Whether the drained rear subset equals half-intensity stock paint.</summary>
        private readonly bool drainedRear;
        /// <summary>Whether the revival endpoint uses the stock rear-leg subset.</summary>
        private readonly bool normalRearSubset;
        /// <summary>Rainbow phase supplying a drain-start rear subset and trailing color.</summary>
        internal readonly PaletteFrame? rearSource;

        /// <summary>Preserves supplied legs unless all of them follow a recognized stock or body relation.</summary>
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

        /// <summary>Body inks, which may be backed by a matching calculated color relationship.</summary>
        public BodyColors Body { get; private set; }
        /// <summary>Uses the red-origin shading model only when it recreates every supplied ink.</summary>
        internal void ReduceRedOrigin()
        {
            RedOriginColors? selected = RedOriginColors.TryCreate(Body);
            if (selected is not null) Body = new BodyColors(selected);
        }
        /// <summary>Shares selected source channels while retaining the phase's independent shade channel.</summary>
        internal void ShareChannels(PaletteFrame source, MotherBrainRainbowShadeProfile profile, bool red, bool green, bool blue, int redAddition = 0, int greenAddition = 0)
        {
            if (Body.Length != source.Body.Length) return;
            var sharing = SharedBodyChannels.TryCreate(source.Body, Body, profile, new RainbowChannelSelection { Red = red, Green = green, Blue = blue, RedAddition = redAddition, GreenAddition = greenAddition });
            if (sharing is not null) Body = new BodyColors(sharing);
        }
        /// <summary>Shares a constant RGB5 offset only if it reproduces the entire supplied row.</summary>
        internal void ShareTint(PaletteFrame source, int red, int green, int blue)
        {
            if (Body.Length != source.Body.Length) return;
            for (int color = 0; color < Body.Length; color++)
                if (Body[color] != BodyColors.Tinted(source.Body[color], red, green, blue)) return;
            Body = new BodyColors(source.Body, red, green, blue);
        }
        /// <summary>Number of rear-leg colors this phase publishes.</summary>
        public int LegCount { get; }
        /// <summary>WRAM spill color, drawn from the shared rear source when an endpoint matches.</summary>
        public Bgr555? TrailingColor => rearSource is not null
            ? rearSource.Leg(MotherBrainDrainedPaletteRomData.TrailingRearSourceColor)
            : drainedRear || normalRearSubset ? Leg(1) : field;
        /// <summary>Selects the appropriate rainbow, drained, stock, or independently edited rear ink.</summary>
        public Bgr555 Leg(int color) => rearSource is not null
            ? rearSource.Leg(color + MotherBrainDrainedPaletteRomData.RearSourceColor)
            : drainedRear ? HalfIntensity(MotherBrainHealthPalettePresentation.StockBaseColor(true,
                color + MotherBrainDrainedPaletteRomData.RearSourceColor))
            : normalRearSubset ? MotherBrainHealthPalettePresentation.StockBaseColor(true,
                color + MotherBrainDrainedPaletteRomData.RearSourceColor)
            : stockRear ? MotherBrainHealthPalettePresentation.StockBaseColor(true, color)
            : backLegs is null ? HalfIntensity(Body[color]) : backLegs[color];

        /// <summary>Shares the drain's first body, rear subset, and trailing ink only after every supplied color matches the rainbow phase.</summary>
        internal static PaletteFrame ShareDrainStart(PaletteFrame drain, PaletteFrame rainbow)
        {
            for (int color = 0; color < drain.Body.Length; color++)
                if (drain.Body[color] != rainbow.Body[color]) return drain;
            for (int color = 0; color < drain.LegCount; color++)
                if (drain.Leg(color) != rainbow.Leg(color + MotherBrainDrainedPaletteRomData.RearSourceColor)) return drain;
            return drain.TrailingColor == rainbow.Leg(MotherBrainDrainedPaletteRomData.TrailingRearSourceColor)
                ? new PaletteFrame(rainbow, false) : drain;
        }
        /// <summary>Recognizes a drained stock rear subset, including its trailing ink, at the fade endpoint.</summary>
        internal static PaletteFrame ShareDrainEnd(PaletteFrame drain)
        {
            for (int color = 0; color < drain.LegCount; color++)
                if (drain.Leg(color) != HalfIntensity(MotherBrainHealthPalettePresentation.StockBaseColor(true,
                    color + MotherBrainDrainedPaletteRomData.RearSourceColor))) return drain;
            return drain.TrailingColor == drain.Leg(1) ? new PaletteFrame(drain, true) : drain;
        }
        /// <summary>Constructs a validated drain endpoint view over either rainbow or stock rear paint.</summary>
        private PaletteFrame(PaletteFrame source, bool drainedRear)
        {
            Body = source.Body;
            LegCount = MotherBrainDrainedPaletteRomData.BackLegCount;
            this.drainedRear = drainedRear;
            rearSource = drainedRear ? null : source;
        }

        /// <summary>Recognizes the stock rear-leg subset at the normal revival endpoint.</summary>
        internal static PaletteFrame ShareNormalRearEndpoint(PaletteFrame frame)
        {
            for (int color = 0; color < frame.LegCount; color++)
                if (frame.Leg(color) != MotherBrainHealthPalettePresentation.StockBaseColor(true,
                    color + MotherBrainDrainedPaletteRomData.RearSourceColor)) return frame;
            return frame.TrailingColor == frame.Leg(1) ? new PaletteFrame(frame) : frame;
        }
        /// <summary>Creates the validated normal rear subset while retaining its body colors.</summary>
        private PaletteFrame(PaletteFrame source)
        {
            Body = source.Body;
            LegCount = MotherBrainDrainedPaletteRomData.BackLegCount;
            normalRearSubset = true;
        }

        /// <summary>Rounds each RGB5 body channel upward when deriving rainbow rear-leg paint.</summary>
        private static Bgr555 HalfIntensity(Bgr555 color) => color.Map((_, channel) => (channel + 1) / 2);
    }

    /// <summary>Exact red-origin shading from the narrowly reviewed temporal material paints; calculated shades are not retained rows.</summary>
    internal sealed class RedOriginColors
    {
        /// <summary>Independent head, plate, and tissue anchors used to reconstruct red-origin gradients.</summary>
        private readonly Bgr555 head, plateStart, plateEnd, tissueStart, tissueEnd;
        /// <summary>The dark head keeps its own green/blue under the shared head red ramp.</summary>
        private readonly (int Green, int Blue) darkHead;
        /// <summary>The three interior tissue shades keep their own green levels.</summary>
        private readonly int[] tissueGreens;
        /// <summary>Final two inks lack a supported interpolation relationship.</summary>
        private readonly Bgr555[] tail;
        /// <summary>Extracts anchors and independently varying channels from one supplied body row.</summary>
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
        /// <summary>Accepts the red-origin model only when its red ramp and both gradients reproduce all fifteen inks.</summary>
        internal static RedOriginColors? TryCreate(BodyColors colors)
        {
            if (colors.Length != MotherBrainRainbowPaletteRomData.ColorCount ||
                colors[0].Red < (RedOriginLayout.PlateStart - 1) * MotherBrainRainbowPaletteFormat.HeadRedStep) return null;
            var selected = new RedOriginColors(colors);
            for (int color = 0; color < colors.Length; color++)
                if (selected[color] != colors[color]) return null;
            return selected;
        }
        /// <summary>Calculates head, plate, or tissue shading, leaving the final two inks independent.</summary>
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
    internal sealed class SharedBodyChannels
    {
        /// <summary>Earlier rainbow phase supplying the channels shared with this phase.</summary>
        private readonly BodyColors source;
        /// <summary>Which RGB5 components follow the source row.</summary>
        private readonly bool red, green, blue;
        /// <summary>Shared channel offsets and count of independently supplied components.</summary>
        private readonly int redAddition, greenAddition, independentCount;
        /// <summary>Compressed independent samples and their selected shade profile.</summary>
        internal readonly MotherBrainRainbowShadeChannel independent;
        /// <summary>Matches the source phase's body color count.</summary>
        internal int Length => source.Length;
        /// <summary>Separates one independent channel from two source-derived channels.</summary>
        private SharedBodyChannels(BodyColors source, BodyColors selected, MotherBrainRainbowShadeProfile profile, RainbowChannelSelection channels)
        {
            this.source = source; this.red = channels.Red; this.green = channels.Green; this.blue = channels.Blue; this.redAddition = channels.RedAddition; this.greenAddition = channels.GreenAddition;
            independentCount = (channels.Red ? 0 : 1) + (channels.Green ? 0 : 1) + (channels.Blue ? 0 : 1);
            if (independentCount != 1) throw new InvalidOperationException("A rainbow shade profile requires one independently supplied channel.");
            var samples = new byte[Length];
            int index = 0;
            for (int color = 0; color < Length; color++)
            {
                Bgr555 word = selected[color];
                if (!channels.Red) samples[index++] = (byte)(word.Red);
                if (!channels.Green) samples[index++] = (byte)(word.Green);
                if (!channels.Blue) samples[index++] = (byte)(word.Blue);
            }
            independent = new MotherBrainRainbowShadeChannel(samples, profile, color => source[color].Green);
        }
        /// <summary>Shares source channels only when every selected RGB5 ink satisfies the requested offsets.</summary>
        internal static SharedBodyChannels? TryCreate(BodyColors source, BodyColors selected, MotherBrainRainbowShadeProfile profile, RainbowChannelSelection channels)
        {
            for (int color = 0; color < source.Length; color++)
            {
                Bgr555 from = source[color], to = selected[color];
                if (channels.Red && (from.Red) + channels.RedAddition != (to.Red) ||
                    channels.Green && (from.Green) + channels.GreenAddition != (to.Green) ||
                    channels.Blue && (from.Blue) != (to.Blue)) return null;
            }
            return new(source, selected, profile, new RainbowChannelSelection { Red = channels.Red, Green = channels.Green, Blue = channels.Blue, RedAddition = channels.RedAddition, GreenAddition = channels.GreenAddition });
        }
        /// <summary>Combines source components with the retained independent shade at one body index.</summary>
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
    internal sealed class BodyColors
    {
        /// <summary>Raw authored inks retained when no recognized model reproduces the row.</summary>
        internal readonly Bgr555[]? supplied;
        /// <summary>Red-origin gradient model selected after exact row comparison.</summary>
        internal readonly RedOriginColors? redOrigin;
        /// <summary>Uses a verified red-origin gradient as the body source.</summary>
        internal BodyColors(RedOriginColors colors)
        {
            redOrigin = colors;
            Length = MotherBrainRainbowPaletteRomData.ColorCount;
        }
        /// <summary>Rainbow phase modeled by shared channels plus independent paint.</summary>
        internal readonly SharedBodyChannels? sharedChannels;
        /// <summary>Uses a phase whose shared channels match the supplied colors.</summary>
        internal BodyColors(SharedBodyChannels channels)
        {
            sharedChannels = channels;
            Length = channels.Length;
        }
        /// <summary>Source phase for a constant RGB5 tint across the body row.</summary>
        internal readonly BodyColors? tintSource;
        /// <summary>Per-channel offsets applied to the source phase.</summary>
        private readonly int redTint, greenTint, blueTint;
        /// <summary>Shares a verified constant channel shift from another phase.</summary>
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
        /// <summary>Drained-paint interpolation selected when the entire row agrees.</summary>
        internal readonly DrainedBodyColors? drained;
        /// <summary>Number of body/brain CGRAM inks represented by this row.</summary>
        public int Length { get; }
        /// <summary>Recognizes stock or drained paint, otherwise retains every supplied RGB5 ink.</summary>
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
        /// <summary>Reads the selected calculated paint model or the independently authored ink.</summary>
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
        /// <summary>Eight-bit highlight anchor before RGB5 tissue quantization.</summary>
        private readonly Rgb8 tissueLight;
        /// <summary>Eight-bit darkest tissue anchor before RGB5 quantization.</summary>
        private readonly Rgb8 tissueDark;
        /// <summary>Fourth cortex anchor between highlight and normal plates.</summary>
        private readonly Bgr555 outline;
        /// <summary>Exact edited row when no common drained-paint anchors reproduce it.</summary>
        private readonly Bgr555[]? supplied;
        /// <summary>Whether the row can be reconstructed from tissue and outline anchors.</summary>
        internal bool Calculated => supplied is null;

        /// <summary>Tests stock anchors, then seeks eight-bit endpoints that reproduce each edited tissue shade.</summary>
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
        /// <summary>Reads a calculated drained shade or its exact authored replacement.</summary>
        internal Bgr555 this[int color] => supplied is null ? Calculate(color) : supplied[color];

        /// <summary>Interpolates cortex and tissue paint while preserving the native RGB5 rounding rules.</summary>
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

        /// <summary>Unquantized tissue endpoint; interpolation occurs before conversion to RGB5.</summary>
        private readonly record struct Rgb8(int Red, int Green, int Blue)
        {
            /// <summary>Selects an eight-bit endpoint channel for interpolation.</summary>
            internal int this[ColorChannel channel] => channel switch
            {
                ColorChannel.Red => Red,
                ColorChannel.Green => Green,
                ColorChannel.Blue => Blue,
                _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
            };
        }
    }
    /// <summary>Common frame geometry for straight and revival-specific fade models.</summary>
    private interface IPaletteFade
    {
        /// <summary>Number of authored phases in the fade.</summary>
        int Length { get; }
        /// <summary>Body/brain inks written for each phase.</summary>
        int BodyCount { get; }
        /// <summary>Rear-leg inks written for each phase.</summary>
        int LegCount { get; }
        /// <summary>Reads a phase's body/brain ink.</summary>
        Bgr555 Body(int frame, int color);
        /// <summary>Reads a phase's rear-leg ink.</summary>
        Bgr555 Leg(int frame, int color);
        /// <summary>Reads the phase's optional WRAM spill ink.</summary>
        Bgr555? Trailing(int frame);
    }

    /// <summary>Revival rounds endpoint RGB5 channels to nearest, retaining sparse authored exceptions including a stock tissue hold.</summary>
    internal sealed class RevivalPaletteFade : IPaletteFade
    {
        /// <summary>Drained starting colors for interpolation.</summary>
        internal readonly PaletteFrame first;
        /// <summary>Normal ending colors for interpolation.</summary>
        internal readonly PaletteFrame last;
        /// <summary>Authored inks that differ from the native revival interpolation.</summary>
        internal readonly Dictionary<(int Frame, int Color), Bgr555> suppliedOverrides = new();

        /// <summary>Captures only phase/color differences from the native endpoint interpolation.</summary>
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

        /// <summary>Number of drain-revival phases.</summary>
        public int Length { get; }
        /// <summary>Number of shared body/brain colors in each phase.</summary>
        public int BodyCount => first.Body.Length;
        /// <summary>Number of rear-leg colors in each phase.</summary>
        public int LegCount => first.LegCount;
        /// <summary>Reads a body ink with any authored interpolation exception.</summary>
        public Bgr555 Body(int frame, int color) => Color(frame, color);
        /// <summary>Reads a rear ink after the body range in the flattened fade.</summary>
        public Bgr555 Leg(int frame, int color) => Color(frame, BodyCount + color);
        /// <summary>Reads the WRAM spill ink after body and rear colors.</summary>
        public Bgr555? Trailing(int frame) => Color(frame, BodyCount + LegCount);

        /// <summary>Prefers an authored exception to the calculated revival shade.</summary>
        private Bgr555 Color(int frame, int color) => suppliedOverrides.TryGetValue((frame, color), out Bgr555 supplied)
            ? supplied : Interpolate(frame, color);

        /// <summary>Maps a flattened body, rear, or trailing index into one supplied frame.</summary>
        private Bgr555 ReadColor(PaletteFrame palette, int color) => color < BodyCount ? palette.Body[color]
            : color < BodyCount + LegCount ? palette.Leg(color - BodyCount)
            : palette.TrailingColor ?? throw new InvalidDataException("A supplied revival frame has no WRAM spill color.");

        /// <summary>Applies the native per-ink revival frame choice before nearest RGB5 interpolation.</summary>
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
    /// <summary>Straight RGB5 endpoint fade with a full authored-row fallback for independent edits.</summary>
    internal sealed class PaletteFade : IPaletteFade
    {
        /// <summary>First phase supplying one endpoint for every color.</summary>
        internal readonly PaletteFrame first;
        /// <summary>Last phase supplying the other endpoint for every color.</summary>
        internal readonly PaletteFrame last;
        /// <summary>All supplied frames when even one shade differs from straight interpolation.</summary>
        internal readonly PaletteFrame[]? supplied;

        /// <summary>Tests every drain or fake-death shade against nearest-channel RGB5 interpolation.</summary>
        public PaletteFade(PaletteFrame[] frames)
        {
            first = frames[0];
            last = frames[^1];
            Length = frames.Length;
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

        /// <summary>Number of phases between the two endpoints, inclusive.</summary>
        public int Length { get; }
        /// <summary>Body/brain ink count inherited from the first endpoint.</summary>
        public int BodyCount => first.Body.Length;
        /// <summary>Rear-leg ink count inherited from the first endpoint.</summary>
        public int LegCount => first.LegCount;
        /// <summary>Reads an authored body row or its straight-fade approximation.</summary>
        public Bgr555 Body(int frame, int color) => supplied is null
            ? Interpolate(first.Body[color], last.Body[color], frame) : supplied[frame].Body[color];
        /// <summary>Reads an authored rear-leg row or its straight-fade approximation.</summary>
        public Bgr555 Leg(int frame, int color) => supplied is null
            ? Interpolate(first.Leg(color), last.Leg(color), frame) : supplied[frame].Leg(color);
        /// <summary>Interpolates the WRAM spill only when both endpoints provide it.</summary>
        public Bgr555? Trailing(int frame) => supplied is not null ? supplied[frame].TrailingColor
            : first.TrailingColor is Bgr555 start && last.TrailingColor is Bgr555 end
                ? Interpolate(start, end, frame) : null;

        /// <summary>Rounds each channel to the nearest five-bit shade along the endpoint line.</summary>
        private Bgr555 Interpolate(Bgr555 start, Bgr555 end, int frame)
        {
            int intervals = Length - 1;
            return start.Zip(end, (_, a, b) => ((a * (intervals - frame)
                    + b * frame + intervals / 2) / intervals));
        }
    }
}
