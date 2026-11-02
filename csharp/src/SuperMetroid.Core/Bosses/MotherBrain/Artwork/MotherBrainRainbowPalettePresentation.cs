using RedOriginLayout = SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRainbowPaletteFormat.RedOriginLayout;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Editable rainbow, drain, revival, and normal-restoration colors for Mother Brain, as the import recognized them: each
/// phase keeps the calculated paint relationship that reproduces it exactly, or its supplied colours.
/// </summary>
public sealed class MotherBrainRainbowPalettePresentation
{
    /// <summary>Ten color phases read by the rainbow list.</summary>
    internal readonly PaletteFrame[] rainbow;
    /// <summary>Drain sequence whose last rear shade can share stock paint.</summary>
    internal readonly PaletteFade toGrey;
    /// <summary>Revival sequence with independently retained exceptions.</summary>
    internal readonly RevivalPaletteFade fromGrey;
    /// <summary>Brain-only descent used during the staged fake death.</summary>
    internal readonly PaletteFade fakeDeathToGrey;
    /// <summary>Body and rear-leg colors restored after the beam.</summary>
    internal readonly PaletteFrame normal;
    /// <summary>Backdrop hue cycle without its signed loop terminator.</summary>
    internal readonly BeamColors beamCycle;

    /// <summary>Keeps the recognized rainbow, drain, revival, fake-death, restoration and beam-backdrop colours.</summary>
    /// <param name="rainbow">The ten rainbow phases.</param>
    /// <param name="toGrey">The drain fade.</param>
    /// <param name="fromGrey">The revival fade.</param>
    /// <param name="fakeDeathToGrey">The fake-death brain fade.</param>
    /// <param name="normal">The restored normal phase.</param>
    /// <param name="beamInitial">The beam backdrop's first colour.</param>
    /// <param name="beamCycle">The beam backdrop's hue cycle.</param>
    internal MotherBrainRainbowPalettePresentation(PaletteFrame[] rainbow, PaletteFade toGrey,
        RevivalPaletteFade fromGrey, PaletteFade fakeDeathToGrey, PaletteFrame normal,
        RgbColor beamInitial, BeamColors beamCycle)
    {
        this.rainbow = rainbow;
        this.toGrey = toGrey;
        this.fromGrey = fromGrey;
        this.fakeDeathToGrey = fakeDeathToGrey;
        this.normal = normal;
        BeamInitialColor = beamInitial;
        this.beamCycle = beamCycle;
    }

    /// <summary>
    /// Set_RainbowBeam_ColorMathSubscreenBackdropColor.table:
    /// five linear hue legs sampled every other word. The blue-to-magenta leg
    /// delays red by one step and adds a one-step green tint in its upper half.
    /// Falling green/blue changes its rounding bias after local step eight.
    /// These phase boundaries preserve the source's asymmetric wheel exactly.
    /// </summary>
    internal sealed class BeamColors
    {
        /// <summary>Retained only when an edited sample differs from the five-leg wheel.</summary>
        internal readonly RgbColor[]? supplied;
        /// <summary>Number of actual color samples, excluding the control terminator.</summary>
        public int Length { get; }
        /// <summary>Keeps the cycle's length and, when one sample differs from the wheel, every supplied sample.</summary>
        /// <param name="length">The number of samples.</param>
        /// <param name="supplied">Every supplied sample when one differs from the calculated wheel, otherwise null.</param>
        internal BeamColors(int length, RgbColor[]? supplied)
        {
            Length = length;
            this.supplied = supplied;
        }
        /// <summary>Reads an authored override or reconstructs the matching wheel sample.</summary>
        public RgbColor this[int index] => supplied is null ? Calculate(index) : supplied[index];

        /// <summary>Reconstructs the five hue legs using the game's asymmetric blue-to-magenta shift.</summary>
        internal static RgbColor Calculate(int index)
        {
            int step = index * MotherBrainBeamDefinitions.HueStepsPerColor;
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
            return new RgbColor(red, green, blue);
        }
        /// <summary>Rounds an increasing channel to its RGB5 sample.</summary>
        private static int Rising(int step) => (31 * step + 7) / 15;
        /// <summary>Applies the descending channel's changed bias after step eight.</summary>
        private static int Falling(int step) => 31 - 2 * step - (step >= 9 ? 1 : 0);
    }
    /// <summary>Fixed-color backdrop used on the beam effect's first active frame.</summary>
    public RgbColor BeamInitialColor { get; }

    /// <summary>
    /// Reads beam cycle colour <paramref name="index"/>. The cycle's end is engine control, not an editable colour: the
    /// index one past the last colour returns false, and the beam performs the reset.
    /// </summary>
    public bool TryReadBeamColor(int index, out RgbColor color)
    {
        if (index < 0 || index > beamCycle.Length)
            throw new InvalidDataException($"Mother Brain beam color {index} is outside its authored cycle.");
        if (index == beamCycle.Length)
        {
            color = RgbColor.Black;
            return false;
        }
        color = beamCycle[index];
        return true;
    }

    /// <summary>Copies one rainbow-list entry to the three body/brain/leg slots.</summary>
    public void ApplyRainbow(GameplayColours gameplayColours, int frame) => ApplyFull(gameplayColours, rainbow, frame);

    /// <summary>Restores the two fixed normal-palette sources after the beam finishes.</summary>
    public void ApplyNormal(GameplayColours gameplayColours) =>
        ApplyColors(gameplayColours, normal);

    /// <summary>Copies a drain fade.</summary>
    public void ApplyToGrey(GameplayColours gameplayColours, int frame) =>
        ApplyGrey(gameplayColours, toGrey, frame);

    /// <summary>Copies a revival fade, preserving the two body/brain tail colors.</summary>
    public void ApplyFromGrey(GameplayColours gameplayColours, int frame) =>
        ApplyGrey(gameplayColours, fromGrey, frame);

    /// <summary>Changes only the three brain colors during the fake-death descent.</summary>
    public void ApplyFakeDeathToGrey(GameplayColours gameplayColours, int frame)
    {
        Ensure.NotNull(gameplayColours);
        if ((uint)frame >= fakeDeathToGrey.Length)
            throw new InvalidDataException($"Mother Brain fake-death grey frame {frame} is outside the authored sequence.");
        for (int color = 0; color < MotherBrainFakeDeathPaletteDefinitions.ColorCount; color++)
            new MotherBrainColours(gameplayColours).FakeDeathBrain.Write(color,
                fakeDeathToGrey.Body(frame, color));
    }

    /// <summary>Restores only those three colors, reusing the game's revival source table.</summary>
    public void ApplyFakeDeathFromGrey(GameplayColours gameplayColours, int frame)
    {
        Ensure.NotNull(gameplayColours);
        if ((uint)frame >= fromGrey.Length)
            throw new InvalidDataException($"Mother Brain fake-death revival frame {frame} is outside the authored sequence.");
        for (int color = 0; color < MotherBrainFakeDeathPaletteDefinitions.ColorCount; color++)
            new MotherBrainColours(gameplayColours).FakeDeathBrain.Write(color, fromGrey.Body(frame, color));
    }

    /// <summary>Copies a complete selected rainbow phase into body, brain, and rear-leg palette slots.</summary>
    private static void ApplyFull(GameplayColours gameplayColours, PaletteFrame[] frames, int frame)
    {
        Ensure.NotNull(gameplayColours);
        if ((uint)frame >= frames.Length)
            throw new InvalidDataException($"Mother Brain rainbow frame {frame} is outside the authored loop.");
        ApplyColors(gameplayColours, frames[frame]);
    }

    /// <summary>
    /// Copies a grey-transition phase. The original game also writes the trailing rear color to a spill
    /// colour nothing reads, so the port does not keep it.
    /// </summary>
    private static void ApplyGrey(GameplayColours gameplayColours, IPaletteFade frames, int frame)
    {
        Ensure.NotNull(gameplayColours);
        if ((uint)frame >= frames.Length)
            throw new InvalidDataException($"Mother Brain grey-transition frame {frame} is outside the authored sequence.");
        for (int color = 0; color < frames.BodyCount; color++)
        {
            RgbColor value = frames.Body(frame, color);
            new MotherBrainColours(gameplayColours).Body.Write(color, value);
            new MotherBrainColours(gameplayColours).Brain.Write(color, value);
        }
        for (int color = 0; color < frames.LegCount; color++)
            new MotherBrainColours(gameplayColours).DrainedRearLegs.Write(color, frames.Leg(frame, color));
    }

    /// <summary>Mirrors body inks to the brain palette and writes the selected rear-leg inks.</summary>
    private static void ApplyColors(GameplayColours gameplayColours, PaletteFrame selected)
    {
        for (int color = 0; color < selected.Body.Length; color++)
        {
            new MotherBrainColours(gameplayColours).Body.Write(color, selected.Body[color]);
            new MotherBrainColours(gameplayColours).Brain.Write(color, selected.Body[color]);
        }
        for (int color = 0; color < selected.LegCount; color++)
            new MotherBrainColours(gameplayColours).RearLegs.Write(color, selected.Leg(color));
    }

    /// <summary>One body and rear-leg phase, optionally recognized as shared stock or half-intensity paint.</summary>
    internal sealed class PaletteFrame
    {
        /// <summary>Retained rear inks when they do not match stock or half-intensity body paint.</summary>
        internal readonly RgbColor[]? backLegs;
        /// <summary>Where the rear legs take their colours from.</summary>
        internal readonly MotherBrainRearPaintKind rearKind;
        /// <summary>Rainbow phase supplying a drain-start rear subset and trailing color.</summary>
        internal readonly PaletteFrame? rearSource;

        /// <summary>Keeps a phase's body paint and the rear-leg relationship the import recognized.</summary>
        /// <param name="body">The body inks.</param>
        /// <param name="legCount">The rear-leg colours the phase publishes.</param>
        /// <param name="trailingColor">The supplied spill colour past the rear legs, read only when no shared rear supplies it.</param>
        /// <param name="rear">How the rear legs resolve.</param>
        internal PaletteFrame(BodyColors body, int legCount, RgbColor? trailingColor, RearPaint rear)
        {
            Body = body;
            LegCount = legCount;
            TrailingColor = trailingColor;
            backLegs = rear.BackLegs;
            rearKind = rear.Kind;
            rearSource = rear.Source;
        }

        /// <summary>Body inks, which may be backed by a matching calculated color relationship.</summary>
        public BodyColors Body { get; }
        /// <summary>Number of rear-leg colors this phase publishes.</summary>
        public int LegCount { get; }
        /// <summary>Spill color past the rear legs, drawn from the shared rear source when an endpoint matches.</summary>
        public RgbColor? TrailingColor => rearSource is not null
            ? rearSource.Leg(MotherBrainDrainedPaletteDefinitions.TrailingRearSourceColor)
            : rearKind is MotherBrainRearPaintKind.Drained or MotherBrainRearPaintKind.NormalSubset ? Leg(1) : field;
        /// <summary>Selects the appropriate rainbow, drained, stock, or independently edited rear ink.</summary>
        public RgbColor Leg(int color) => rearSource is not null
            ? rearSource.Leg(color + MotherBrainDrainedPaletteDefinitions.RearSourceColor)
            : rearKind == MotherBrainRearPaintKind.Drained ? HalfIntensity(MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg,
                color + MotherBrainDrainedPaletteDefinitions.RearSourceColor))
            : rearKind == MotherBrainRearPaintKind.NormalSubset ? MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg,
                color + MotherBrainDrainedPaletteDefinitions.RearSourceColor)
            : rearKind == MotherBrainRearPaintKind.Stock ? MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg, color)
            : backLegs is null ? HalfIntensity(Body[color]) : backLegs[color];

        /// <summary>Rounds each RGB5 body channel upward when deriving rainbow rear-leg paint.</summary>
        internal static RgbColor HalfIntensity(RgbColor color) => color.Map((_, channel) => (channel + 1) / 2);
    }

    /// <summary>
    /// How a phase's rear legs resolve: from retained colours, the full stock rear palette, the drained or normal stock
    /// subset, a rainbow phase's rear subset, or half-intensity body paint when every option is empty.
    /// </summary>
    /// <param name="BackLegs">Retained rear inks, or null.</param>
    /// <param name="Kind">Where the rear legs take their colours from.</param>
    /// <param name="Source">The rainbow phase whose rear subset and trailing colour the phase shares, or null.</param>
    internal sealed record RearPaint(RgbColor[]? BackLegs, MotherBrainRearPaintKind Kind, PaletteFrame? Source);

    /// <summary>Exact red-origin shading from the narrowly reviewed temporal material paints; calculated shades are not retained as colours.</summary>
    internal sealed class RedOriginColors
    {
        /// <summary>Independent head, plate, and tissue anchors used to reconstruct red-origin gradients.</summary>
        private readonly RgbColor head, plateStart, plateEnd, tissueStart, tissueEnd;
        /// <summary>The dark head keeps its own green/blue under the shared head red ramp.</summary>
        private readonly MotherBrainDarkHeadShade darkHead;
        /// <summary>The three interior tissue shades keep their own green levels.</summary>
        private readonly int[] tissueGreens;
        /// <summary>Final two inks lack a supported interpolation relationship.</summary>
        private readonly RgbColor[] tail;
        /// <summary>Keeps the anchors and independently varying channels of one phase's body colours.</summary>
        /// <param name="anchors">The head, plate and tissue anchors.</param>
        /// <param name="darkHead">The dark head's own green and blue.</param>
        /// <param name="tissueGreens">The three interior tissue greens.</param>
        /// <param name="tail">The final two inks.</param>
        internal RedOriginColors(RedOriginAnchors anchors, MotherBrainDarkHeadShade darkHead, int[] tissueGreens, RgbColor[] tail)
        {
            head = anchors.Head;
            plateStart = anchors.PlateStart;
            plateEnd = anchors.PlateEnd;
            tissueStart = anchors.TissueStart;
            tissueEnd = anchors.TissueEnd;
            this.darkHead = darkHead;
            this.tissueGreens = tissueGreens;
            this.tail = tail;
        }
        /// <summary>Calculates head, plate, or tissue shading, leaving the final two inks independent.</summary>
        internal RgbColor this[int color]
        {
            get
            {
                if (color < RedOriginLayout.PlateStart)
                {
                    int red = head.Red - color * MotherBrainRainbowPaletteFormat.HeadRedStep;
                    return color < RedOriginLayout.DarkHead
                        ? head.WithRed(red)
                        : new RgbColor(red, darkHead.Green, darkHead.Blue);
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
                    return new RgbColor(red, green, blue);
                }
                return tail[color - RedOriginLayout.TailStart];
            }
        }
    }

    /// <summary>The head, plate and tissue anchors a red-origin phase reconstructs its gradients from.</summary>
    /// <param name="Head">The brightest head ink.</param>
    /// <param name="PlateStart">The first plate ink.</param>
    /// <param name="PlateEnd">The last plate ink.</param>
    /// <param name="TissueStart">The first tissue ink.</param>
    /// <param name="TissueEnd">The last tissue ink.</param>
    internal readonly record struct RedOriginAnchors(RgbColor Head, RgbColor PlateStart, RgbColor PlateEnd, RgbColor TissueStart, RgbColor TissueEnd);

    /// <summary>Exact shared channels with all unexplained RGB5 samples retained independently.</summary>
    internal sealed class SharedBodyChannels
    {
        /// <summary>Earlier rainbow phase supplying the channels shared with this phase.</summary>
        private readonly BodyColors source;
        /// <summary>Which RGB5 components follow the source phase.</summary>
        private readonly bool red, green, blue;
        /// <summary>Shared channel offsets and count of independently supplied components.</summary>
        private readonly int redAddition, greenAddition, independentCount;
        /// <summary>Compressed independent samples and their selected shade profile.</summary>
        internal readonly MotherBrainRainbowShadeChannel independent;
        /// <summary>Matches the source phase's body color count.</summary>
        internal int Length => source.Length;
        /// <summary>Keeps the source phase, the shared channels and their offsets, and the independent shade channel.</summary>
        /// <param name="source">The earlier rainbow phase supplying the shared channels.</param>
        /// <param name="channels">Which channels follow the source, and their additions.</param>
        /// <param name="independent">The independently supplied channel's shades.</param>
        internal SharedBodyChannels(BodyColors source, RainbowChannelSelection channels, MotherBrainRainbowShadeChannel independent)
        {
            this.source = source; red = channels.Red; green = channels.Green; blue = channels.Blue; redAddition = channels.RedAddition; greenAddition = channels.GreenAddition;
            independentCount = (channels.Red ? 0 : 1) + (channels.Green ? 0 : 1) + (channels.Blue ? 0 : 1);
            this.independent = independent;
        }
        /// <summary>Combines source components with the retained independent shade at one body index.</summary>
        internal RgbColor this[int color]
        {
            get
            {
                RgbColor from = source[color];
                int index = color * independentCount;
                int r = red ? (from.Red) + redAddition : independent[index++];
                int g = green ? (from.Green) + greenAddition : independent[index++];
                int b = blue ? from.Blue : independent[index];
                return new RgbColor(r, g, b);
            }
        }
    }
    /// <summary>Recognizes normal and drained body paint without repeating every phase's colours.</summary>
    internal sealed class BodyColors
    {
        /// <summary>Raw authored inks retained when no recognized model reproduces the phase's colours.</summary>
        internal readonly RgbColor[]? supplied;
        /// <summary>Red-origin gradient model selected after exact comparison of every colour.</summary>
        internal readonly RedOriginColors? redOrigin;
        /// <summary>Uses a verified red-origin gradient as the body source.</summary>
        internal BodyColors(RedOriginColors colors)
        {
            redOrigin = colors;
            Length = MotherBrainRainbowPaletteDefinitions.ColorCount;
        }
        /// <summary>Rainbow phase modeled by shared channels plus independent paint.</summary>
        internal readonly SharedBodyChannels? sharedChannels;
        /// <summary>Uses a phase whose shared channels match the supplied colors.</summary>
        internal BodyColors(SharedBodyChannels channels)
        {
            sharedChannels = channels;
            Length = channels.Length;
        }
        /// <summary>Source phase for a constant RGB5 tint across the phase's body colours.</summary>
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
        internal static RgbColor? Tinted(RgbColor color, int red, int green, int blue)
        {
            int r = color.Red + red, g = color.Green + green, b = color.Blue + blue;
            return (uint)r > 31 || (uint)g > 31 || (uint)b > 31 ? null : new RgbColor(r, g, b);
        }
        /// <summary>Drained-paint interpolation selected when every colour agrees.</summary>
        internal readonly DrainedBodyColors? drained;
        /// <summary>Number of body/brain inks represented by this phase.</summary>
        public int Length { get; }
        /// <summary>
        /// Keeps stock body paint (both <paramref name="supplied"/> and <paramref name="drained"/> null), drained paint, or
        /// every supplied ink.
        /// </summary>
        /// <param name="length">The number of body inks.</param>
        /// <param name="supplied">Every supplied ink when no paint model reproduces them, otherwise null.</param>
        /// <param name="drained">The drained paint that reproduces them, otherwise null.</param>
        internal BodyColors(int length, RgbColor[]? supplied, DrainedBodyColors? drained)
        {
            Length = length;
            this.supplied = supplied;
            this.drained = drained;
        }
        /// <summary>Reads the selected calculated paint model or the independently authored ink.</summary>
        public RgbColor this[int color] => redOrigin is not null ? redOrigin[color]
            : sharedChannels is not null ? sharedChannels[color]
            : tintSource is not null ? Tinted(tintSource[color], redTint, greenTint, blueTint)
                ?? throw new InvalidOperationException("A shared rainbow tint left RGB5 after construction.")
            : supplied is not null ? supplied[color]
            : drained is not null ? drained[color]
            : MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, color);
    }

    /// <summary>
    /// The drained palettes share drained body paint. Cortex shades interpolate
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
        private readonly RgbColor outline;
        /// <summary>Exact edited colours when no common drained-paint anchors reproduce them.</summary>
        private readonly RgbColor[]? supplied;
        /// <summary>Whether the colours can be reconstructed from tissue and outline anchors.</summary>
        internal bool Calculated => supplied is null;

        /// <summary>Keeps the tissue and outline anchors, or every supplied colour when no anchors reproduce them.</summary>
        /// <param name="tissueLight">The eight-bit highlight anchor.</param>
        /// <param name="tissueDark">The eight-bit darkest tissue anchor.</param>
        /// <param name="outline">The cortex outline anchor.</param>
        /// <param name="supplied">Every supplied colour when the anchors do not reproduce them, otherwise null.</param>
        internal DrainedBodyColors(Rgb8 tissueLight, Rgb8 tissueDark, RgbColor outline, RgbColor[]? supplied)
        {
            this.tissueLight = tissueLight;
            this.tissueDark = tissueDark;
            this.outline = outline;
            this.supplied = supplied;
        }
        /// <summary>Reads a calculated drained shade or its exact authored replacement.</summary>
        internal RgbColor this[int color] => supplied is null ? Calculate(color) : supplied[color];

        /// <summary>Interpolates cortex and tissue paint while preserving the RGB5 rounding rules.</summary>
        private RgbColor Calculate(int color)
        {
            if (color is >= 4 and <= 7)
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, color);
            if (color == 13) return RgbColor.White;
            if (color == 14) return RgbColor.Black;
            if (color < 4)
                return outline.Map((channel, outlineChannel) =>
                    (tissueLight[channel] / 8 * (3 - color) + outlineChannel * color + 1) / 3);
            return new RgbColor(
                (tissueLight.Red * (12 - color) + tissueDark.Red * (color - 8)) / 32,
                (tissueLight.Green * (12 - color) + tissueDark.Green * (color - 8)) / 32,
                (tissueLight.Blue * (12 - color) + tissueDark.Blue * (color - 8)) / 32);
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
        RgbColor Body(int frame, int color);
        /// <summary>Reads a phase's rear-leg ink.</summary>
        RgbColor Leg(int frame, int color);
    }

    /// <summary>Revival rounds endpoint RGB5 channels to nearest, retaining sparse authored exceptions including a stock tissue hold.</summary>
    internal sealed class RevivalPaletteFade : IPaletteFade
    {
        /// <summary>Drained starting colors for interpolation.</summary>
        internal readonly PaletteFrame first;
        /// <summary>Normal ending colors for interpolation.</summary>
        internal readonly PaletteFrame last;
        /// <summary>Authored inks that differ from the revival interpolation.</summary>
        internal readonly Dictionary<RainbowPaletteColorKey, RgbColor> suppliedOverrides;

        /// <summary>Keeps the fade's endpoints, length and the authored inks that differ from its interpolation.</summary>
        /// <param name="first">The drained starting phase.</param>
        /// <param name="last">The normal ending phase.</param>
        /// <param name="length">The number of phases.</param>
        /// <param name="suppliedOverrides">The supplied inks that differ from the interpolation, by phase and flattened colour.</param>
        internal RevivalPaletteFade(PaletteFrame first, PaletteFrame last, int length,
            Dictionary<RainbowPaletteColorKey, RgbColor> suppliedOverrides)
        {
            this.first = first;
            this.last = last;
            Length = length;
            this.suppliedOverrides = suppliedOverrides;
        }

        /// <summary>Number of drain-revival phases.</summary>
        public int Length { get; }
        /// <summary>Number of shared body/brain colors in each phase.</summary>
        public int BodyCount => first.Body.Length;
        /// <summary>Number of rear-leg colors in each phase.</summary>
        public int LegCount => first.LegCount;
        /// <summary>Reads a body ink with any authored interpolation exception.</summary>
        public RgbColor Body(int frame, int color) => Color(frame, color);
        /// <summary>Reads a rear ink after the body range in the flattened fade.</summary>
        public RgbColor Leg(int frame, int color) => Color(frame, BodyCount + color);

        /// <summary>Prefers an authored exception to the calculated revival shade.</summary>
        private RgbColor Color(int frame, int color) => suppliedOverrides.TryGetValue(new RainbowPaletteColorKey(frame, color), out RgbColor supplied)
            ? supplied : Interpolate(frame, color);

        /// <summary>Maps a flattened body, rear, or trailing index into one phase.</summary>
        internal RgbColor ReadColor(PaletteFrame palette, int color) => color < BodyCount ? palette.Body[color]
            : color < BodyCount + LegCount ? palette.Leg(color - BodyCount)
            : palette.TrailingColor ?? throw new InvalidDataException("A supplied revival frame has no spill color.");

        /// <summary>The image whose shade revival colour <paramref name="color"/> shows on <paramref name="frame"/>: the held tissue colour repeats its preceding shade for one step.</summary>
        private static int RevivalInterpolationFrame(int frame, int color) =>
            frame == MotherBrainDrainedPaletteDefinitions.RevivalTissueHoldFrame &&
            color == MotherBrainDrainedPaletteDefinitions.RevivalHeldTissueColor
                ? frame - 1
                : frame;

        /// <summary>Applies the per-ink revival frame choice before nearest RGB5 interpolation.</summary>
        internal RgbColor Interpolate(int frame, int color)
        {
            frame = RevivalInterpolationFrame(frame, color);
            RgbColor start = ReadColor(first, color);
            RgbColor end = ReadColor(last, color);
            int intervals = Length - 1;
            return start.Zip(end, (_, a, b) => ((a * (intervals - frame)
                    + b * frame + intervals / 2) / intervals));
        }
    }
    /// <summary>Straight RGB5 endpoint fade that keeps every authored frame for independent edits.</summary>
    internal sealed class PaletteFade : IPaletteFade
    {
        /// <summary>First phase supplying one endpoint for every color.</summary>
        internal readonly PaletteFrame first;
        /// <summary>Last phase supplying the other endpoint for every color.</summary>
        internal readonly PaletteFrame last;
        /// <summary>All supplied frames when even one shade differs from straight interpolation.</summary>
        internal readonly PaletteFrame[]? supplied;

        /// <summary>Keeps the fade's endpoints and length, and every supplied phase when one differs from the interpolation.</summary>
        /// <param name="first">The first phase.</param>
        /// <param name="last">The last phase.</param>
        /// <param name="length">The number of phases.</param>
        /// <param name="supplied">Every supplied phase when one differs from the interpolation, otherwise null.</param>
        internal PaletteFade(PaletteFrame first, PaletteFrame last, int length, PaletteFrame[]? supplied)
        {
            this.first = first;
            this.last = last;
            Length = length;
            this.supplied = supplied;
        }

        /// <summary>Number of phases between the two endpoints, inclusive.</summary>
        public int Length { get; }
        /// <summary>Body/brain ink count inherited from the first endpoint.</summary>
        public int BodyCount => first.Body.Length;
        /// <summary>Rear-leg ink count inherited from the first endpoint.</summary>
        public int LegCount => first.LegCount;
        /// <summary>Reads an authored body colour or its straight-fade approximation.</summary>
        public RgbColor Body(int frame, int color) => supplied is null
            ? Interpolate(first.Body[color], last.Body[color], frame) : supplied[frame].Body[color];
        /// <summary>Reads an authored rear-leg colour or its straight-fade approximation.</summary>
        public RgbColor Leg(int frame, int color) => supplied is null
            ? Interpolate(first.Leg(color), last.Leg(color), frame) : supplied[frame].Leg(color);

        /// <summary>Rounds each channel to the nearest five-bit shade along the endpoint line.</summary>
        internal RgbColor Interpolate(RgbColor start, RgbColor end, int frame)
        {
            int intervals = Length - 1;
            return start.Zip(end, (_, a, b) => ((a * (intervals - frame)
                    + b * frame + intervals / 2) / intervals));
        }
    }
}
