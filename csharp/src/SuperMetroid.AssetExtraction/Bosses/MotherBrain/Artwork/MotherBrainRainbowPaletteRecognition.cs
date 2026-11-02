using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using Phases = SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork.MotherBrainRainbowPhaseDefinitions;
using Presentation = SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRainbowPalettePresentation;
using RedOriginLayout = SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRainbowPaletteFormat.RedOriginLayout;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Recognizes the calculated paint relationships in Mother Brain's supplied rainbow, drain, revival, restoration and
/// beam colours: a relationship is kept only when it reproduces every supplied colour exactly, so stock phases store
/// their anchors and shared channels rather than rows, and independent edits stay exact.
/// </summary>
internal static class MotherBrainRainbowPaletteRecognition
{
    /// <summary>Builds the selected rainbow, drain, revival, fake-death, restoration and beam-backdrop colours.</summary>
    /// <param name="colours">The selected colours; a null fake-death fade is inherited from <paramref name="currentStock"/>.</param>
    /// <param name="currentStock">The current stock presentation a missing fake-death fade is inherited from.</param>
    internal static Presentation Create(MotherBrainRainbowPaletteColours colours, Presentation? currentStock)
    {
        Ensure.NotNull(colours);
        Presentation.PaletteFrame[] rainbow = ShareRainbow(Frames(colours.Rainbow));
        Presentation.PaletteFrame[] toGrey = Frames(colours.ToGrey);
        toGrey[0] = ShareDrainStart(toGrey[0], rainbow[MotherBrainRainbowPaletteDefinitions.DrainedRainbowFrame]);
        toGrey[^1] = ShareDrainEnd(toGrey[^1]);
        Presentation.PaletteFrame[] fromGrey = Frames(colours.FromGrey);
        fromGrey[0] = ShareDrainEnd(fromGrey[0]);
        fromGrey[^1] = ShareNormalRearEndpoint(fromGrey[^1]);
        Presentation.PaletteFade fakeDeathToGrey = colours.FakeDeathToGrey is null
            ? Ensure.NotNull(currentStock).fakeDeathToGrey
            : Fade(colours.FakeDeathToGrey.Select(brain => Frame(brain, [], null)).ToArray());
        return new(rainbow, Fade(toGrey), Revival(fromGrey), fakeDeathToGrey, Frame(colours.Normal),
            colours.BeamInitial, Beam(colours.BeamCycle));
    }

    /// <summary>
    /// Recognizes drained body paint: the stock anchors, or eight-bit tissue endpoints and an outline that reproduce every
    /// supplied colour, otherwise the supplied colours themselves.
    /// </summary>
    internal static Presentation.DrainedBodyColors Drained(RgbColor[] colors)
    {
        if (colors.Length is not (3 or 13 or 15))
            return new(default, default, default, colors);
        Rgb8 light = MotherBrainDrainedPaintDefinitions.HighlightRgb8;
        Rgb8 dark = MotherBrainDrainedPaintDefinitions.DarkestTissueRgb8;
        var stock = new Presentation.DrainedBodyColors(light, dark, MotherBrainDrainedPaintDefinitions.Outline, supplied: null);
        if (Reproduces(colors, color => stock[color]))
            return stock;
        if (colors.Length == MotherBrainFakeDeathPaletteDefinitions.ColorCount)
            return new(light, dark, MotherBrainDrainedPaintDefinitions.Outline, colors);
        RgbColor outline = colors[3];
        if (!TryChannel(colors, ColorChannel.Red, out int lr, out int dr) ||
            !TryChannel(colors, ColorChannel.Green, out int lg, out int dg) ||
            !TryChannel(colors, ColorChannel.Blue, out int lb, out int db))
            return new(light, dark, outline, colors);
        var edited = new Presentation.DrainedBodyColors(new(lr, lg, lb), new(dr, dg, db), outline, supplied: null);
        return Reproduces(colors, color => edited[color]) ? edited : new(new(lr, lg, lb), new(dr, dg, db), outline, colors);
    }

    /// <summary>Seeks eight-bit tissue endpoints whose four-interval gradient quantizes to every supplied tissue shade.</summary>
    private static bool TryChannel(RgbColor[] colors, ColorChannel channel, out int first, out int last)
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

    /// <summary>Whether <paramref name="resolve"/> gives every supplied colour.</summary>
    private static bool Reproduces(RgbColor[] colors, Func<int, RgbColor> resolve)
    {
        for (int color = 0; color < colors.Length; color++)
            if (resolve(color) != colors[color]) return false;
        return true;
    }

    /// <summary>Selected phases as palette frames, in order.</summary>
    private static Presentation.PaletteFrame[] Frames(MotherBrainPaletteFrameColours[] colours) =>
        colours.Select(Frame).ToArray();

    /// <summary>One selected phase as a palette frame.</summary>
    private static Presentation.PaletteFrame Frame(MotherBrainPaletteFrameColours colours) =>
        Frame(colours.Body, colours.BackLegs, colours.TrailingColor);

    /// <summary>Preserves supplied legs unless all of them follow a recognized stock or body relation.</summary>
    private static Presentation.PaletteFrame Frame(RgbColor[] body, RgbColor[] legs, RgbColor? trailingColor)
    {
        Presentation.BodyColors bodyColors = Body(body);
        bool matchesRear = legs.Length == MotherBrainRainbowPaletteDefinitions.ColorCount;
        for (int color = 0; matchesRear && color < legs.Length; color++)
            matchesRear = legs[color] == MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg, color);
        if (matchesRear)
            return new(bodyColors, legs.Length, trailingColor, new(null, Stock: true, Drained: false, NormalSubset: false, Source: null));
        // Rainbow legs use half-intensity body colors, rounding RGB5 upward.
        // Select this relationship only when every supplied color agrees.
        bool halfIntensity = body.Length == legs.Length;
        for (int color = 0; halfIntensity && color < legs.Length; color++)
            halfIntensity = Presentation.PaletteFrame.HalfIntensity(body[color]) == legs[color];
        return new(bodyColors, legs.Length, trailingColor,
            new(halfIntensity ? null : legs, Stock: false, Drained: false, NormalSubset: false, Source: null));
    }

    /// <summary>Recognizes stock or drained paint, otherwise retains every supplied RGB5 ink.</summary>
    private static Presentation.BodyColors Body(RgbColor[] colors)
    {
        int length = colors.Length;
        bool normal = length is MotherBrainFakeDeathPaletteDefinitions.ColorCount or MotherBrainDrainedPaletteDefinitions.RevivalColors
            or MotherBrainRainbowPaletteDefinitions.ColorCount;
        for (int color = 0; normal && color < length; color++)
            normal = colors[color] == MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, color);
        if (normal)
            return new(length, supplied: null, drained: null);
        Presentation.DrainedBodyColors drained = Drained(colors);
        return drained.Calculated ? new(length, supplied: null, drained) : new(length, colors, drained: null);
    }

    /// <summary>The same phase over different body paint, keeping its rear-leg relationship.</summary>
    private static Presentation.PaletteFrame WithBody(Presentation.PaletteFrame frame, Presentation.BodyColors body) =>
        new(body, frame.LegCount, frame.TrailingColor,
            new(frame.backLegs, frame.stockRear, frame.drainedRear, frame.normalRearSubset, frame.rearSource));

    /// <summary>Recognizes the shared phase relationships of the rainbow list without changing independently supplied colors.</summary>
    private static Presentation.PaletteFrame[] ShareRainbow(Presentation.PaletteFrame[] rainbow)
    {
        rainbow[Phases.RedOriginPhase] = ReduceRedOrigin(rainbow[Phases.RedOriginPhase]);
        rainbow[Phases.FirstGreenRisePhase] = ShareChannels(rainbow[Phases.FirstGreenRisePhase], rainbow[Phases.RedOriginPhase],
            MotherBrainRainbowShadeProfile.FirstGreenRise,
            new RainbowChannelSelection { Red = true, Green = false, Blue = true, RedAddition = 0, GreenAddition = 0 });
        rainbow[Phases.RedReductionSourcePhase] = ShareChannels(rainbow[Phases.RedReductionSourcePhase], rainbow[Phases.RedOriginPhase],
            MotherBrainRainbowShadeProfile.SecondGreenRise,
            new RainbowChannelSelection { Red = true, Green = false, Blue = true, RedAddition = 0, GreenAddition = 0 });
        rainbow[Phases.RedReducedPhase] = ShareTint(rainbow[Phases.RedReducedPhase], rainbow[Phases.RedReductionSourcePhase],
            -Phases.RedReduction, 0, 0);
        rainbow[Phases.BlueGreenShiftPhase] = ShareTint(rainbow[Phases.BlueGreenShiftPhase], rainbow[Phases.RedReducedPhase],
            0, -Phases.BlueGreenShift, Phases.BlueGreenShift);
        rainbow[Phases.MixedShiftPhase] = ShareChannels(rainbow[Phases.MixedShiftPhase], rainbow[Phases.RedOriginPhase],
            MotherBrainRainbowShadeProfile.MixedBlue,
            new RainbowChannelSelection { Red = true, Green = true, Blue = false, RedAddition = -Phases.RedReduction, GreenAddition = Phases.GreenAddition });
        rainbow[Phases.BlueMaximumPhase] = ShareChannels(rainbow[Phases.BlueMaximumPhase], rainbow[Phases.RedOriginPhase],
            MotherBrainRainbowShadeProfile.MaximumBlue,
            new RainbowChannelSelection { Red = true, Green = true, Blue = false, RedAddition = -Phases.RedReduction, GreenAddition = 0 });
        rainbow[Phases.RedRecoveryPhase] = ShareChannels(rainbow[Phases.RedRecoveryPhase], rainbow[Phases.BlueMaximumPhase],
            MotherBrainRainbowShadeProfile.RecoveringGreen,
            new RainbowChannelSelection { Red = true, Green = false, Blue = true, RedAddition = Phases.RedRecoveryAddition, GreenAddition = 0 });
        rainbow[Phases.RedRestoredPhase] = ShareTint(rainbow[Phases.RedRestoredPhase], rainbow[Phases.BlueMaximumPhase],
            Phases.RedReduction, 0, 0);
        rainbow[Phases.BlueRaisedPhase] = ShareTint(rainbow[Phases.BlueRaisedPhase], rainbow[Phases.RedOriginPhase],
            0, 0, Phases.BlueAddition);
        return rainbow;
    }

    /// <summary>Uses the red-origin shading model only when it recreates every supplied ink.</summary>
    private static Presentation.PaletteFrame ReduceRedOrigin(Presentation.PaletteFrame frame) =>
        RedOrigin(frame.Body) is { } selected ? WithBody(frame, new Presentation.BodyColors(selected)) : frame;

    /// <summary>Accepts the red-origin model only when its red ramp and both gradients reproduce all fifteen inks.</summary>
    private static Presentation.RedOriginColors? RedOrigin(Presentation.BodyColors colors)
    {
        if (colors.Length != MotherBrainRainbowPaletteDefinitions.ColorCount ||
            colors[0].Red < (RedOriginLayout.PlateStart - 1) * MotherBrainRainbowPaletteFormat.HeadRedStep) return null;
        var selected = new Presentation.RedOriginColors(
            new(colors[0], colors[RedOriginLayout.PlateStart], colors[RedOriginLayout.PlateEnd],
                colors[RedOriginLayout.TissueStart], colors[RedOriginLayout.TissueEnd]),
            new(colors[RedOriginLayout.DarkHead].Green, colors[RedOriginLayout.DarkHead].Blue),
            [colors[RedOriginLayout.TissueStart + 1].Green, colors[RedOriginLayout.TissueStart + 2].Green,
                colors[RedOriginLayout.TissueStart + 3].Green],
            [colors[RedOriginLayout.TailStart], colors[RedOriginLayout.TailStart + 1]]);
        for (int color = 0; color < colors.Length; color++)
            if (selected[color] != colors[color]) return null;
        return selected;
    }

    /// <summary>Shares selected source channels while retaining the phase's independent shade channel.</summary>
    private static Presentation.PaletteFrame ShareChannels(Presentation.PaletteFrame frame, Presentation.PaletteFrame source,
        MotherBrainRainbowShadeProfile profile, RainbowChannelSelection channels)
    {
        if (frame.Body.Length != source.Body.Length) return frame;
        Presentation.SharedBodyChannels? sharing = SharedChannels(source.Body, frame.Body, profile, channels);
        return sharing is null ? frame : WithBody(frame, new Presentation.BodyColors(sharing));
    }

    /// <summary>Shares source channels only when every selected RGB5 ink satisfies the requested offsets.</summary>
    private static Presentation.SharedBodyChannels? SharedChannels(Presentation.BodyColors source, Presentation.BodyColors selected,
        MotherBrainRainbowShadeProfile profile, RainbowChannelSelection channels)
    {
        for (int color = 0; color < source.Length; color++)
        {
            RgbColor from = source[color], to = selected[color];
            if (channels.Red && from.Red + channels.RedAddition != to.Red ||
                channels.Green && from.Green + channels.GreenAddition != to.Green ||
                channels.Blue && from.Blue != to.Blue) return null;
        }
        int independentCount = (channels.Red ? 0 : 1) + (channels.Green ? 0 : 1) + (channels.Blue ? 0 : 1);
        if (independentCount != 1) throw new InvalidOperationException("A rainbow shade profile requires one independently supplied channel.");
        var samples = new int[source.Length];
        int index = 0;
        for (int color = 0; color < source.Length; color++)
        {
            RgbColor word = selected[color];
            if (!channels.Red) samples[index++] = word.Red;
            if (!channels.Green) samples[index++] = word.Green;
            if (!channels.Blue) samples[index++] = word.Blue;
        }
        return new(source, channels, ShadeChannel(samples, profile, color => source[color].Green));
    }

    /// <summary>Retains independent samples only when the phase's supplied samples follow the shade profile.</summary>
    private static MotherBrainRainbowShadeChannel ShadeChannel(int[] samples, MotherBrainRainbowShadeProfile profile, Func<int, int> sourceGreen)
    {
        int[] inputs = profile == MotherBrainRainbowShadeProfile.RecoveringGreen
            ? [samples[0], samples[RedOriginLayout.TissueStart + 2], samples[RedOriginLayout.TissueStart + 3]]
            : [samples[0], samples[RedOriginLayout.PlateStart], samples[RedOriginLayout.PlateEnd],
                samples[RedOriginLayout.TissueStart], samples[RedOriginLayout.TissueEnd],
                samples[RedOriginLayout.TailStart], samples[RedOriginLayout.TailStart + 1]];
        if (MotherBrainRainbowShadeChannel.SharesDarkHeadTail(profile)) inputs = inputs[..^1];
        var calculated = new MotherBrainRainbowShadeChannel(profile, sourceGreen, inputs, calculated: true);
        for (int color = 0; color < samples.Length; color++)
            if (calculated.Calculate(color) != samples[color])
                return new MotherBrainRainbowShadeChannel(profile, sourceGreen, samples, calculated: false);
        return calculated;
    }

    /// <summary>Shares a constant RGB5 offset only if it reproduces every supplied body colour.</summary>
    private static Presentation.PaletteFrame ShareTint(Presentation.PaletteFrame frame, Presentation.PaletteFrame source,
        int red, int green, int blue)
    {
        if (frame.Body.Length != source.Body.Length) return frame;
        for (int color = 0; color < frame.Body.Length; color++)
            if (frame.Body[color] != Presentation.BodyColors.Tinted(source.Body[color], red, green, blue)) return frame;
        return WithBody(frame, new Presentation.BodyColors(source.Body, red, green, blue));
    }

    /// <summary>Shares the drain's first body, rear subset, and trailing ink only after every supplied color matches the rainbow phase.</summary>
    private static Presentation.PaletteFrame ShareDrainStart(Presentation.PaletteFrame drain, Presentation.PaletteFrame rainbow)
    {
        for (int color = 0; color < drain.Body.Length; color++)
            if (drain.Body[color] != rainbow.Body[color]) return drain;
        for (int color = 0; color < drain.LegCount; color++)
            if (drain.Leg(color) != rainbow.Leg(color + MotherBrainDrainedPaletteDefinitions.RearSourceColor)) return drain;
        return drain.TrailingColor == rainbow.Leg(MotherBrainDrainedPaletteDefinitions.TrailingRearSourceColor)
            ? new(rainbow.Body, MotherBrainDrainedPaletteDefinitions.BackLegCount, null,
                new(null, Stock: false, Drained: false, NormalSubset: false, Source: rainbow))
            : drain;
    }

    /// <summary>Recognizes a drained stock rear subset, including its trailing ink, at the fade endpoint.</summary>
    private static Presentation.PaletteFrame ShareDrainEnd(Presentation.PaletteFrame drain)
    {
        for (int color = 0; color < drain.LegCount; color++)
            if (drain.Leg(color) != Presentation.PaletteFrame.HalfIntensity(MotherBrainHealthPalettePresentation.StockBaseColor(
                    MotherBrainPalettePart.BackLeg, color + MotherBrainDrainedPaletteDefinitions.RearSourceColor))) return drain;
        return drain.TrailingColor == drain.Leg(1)
            ? new(drain.Body, MotherBrainDrainedPaletteDefinitions.BackLegCount, null,
                new(null, Stock: false, Drained: true, NormalSubset: false, Source: null))
            : drain;
    }

    /// <summary>Recognizes the stock rear-leg subset at the normal revival endpoint.</summary>
    private static Presentation.PaletteFrame ShareNormalRearEndpoint(Presentation.PaletteFrame frame)
    {
        for (int color = 0; color < frame.LegCount; color++)
            if (frame.Leg(color) != MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg,
                    color + MotherBrainDrainedPaletteDefinitions.RearSourceColor)) return frame;
        return frame.TrailingColor == frame.Leg(1)
            ? new(frame.Body, MotherBrainDrainedPaletteDefinitions.BackLegCount, null,
                new(null, Stock: false, Drained: false, NormalSubset: true, Source: null))
            : frame;
    }

    /// <summary>Tests every drain or fake-death shade against nearest-channel RGB5 interpolation.</summary>
    private static Presentation.PaletteFade Fade(Presentation.PaletteFrame[] frames)
    {
        var calculated = new Presentation.PaletteFade(frames[0], frames[^1], frames.Length, supplied: null);
        for (int frame = 0; frame < frames.Length; frame++)
        {
            for (int color = 0; color < calculated.BodyCount; color++)
                if (calculated.Body(frame, color) != frames[frame].Body[color])
                    return new(frames[0], frames[^1], frames.Length, frames);
            for (int color = 0; color < calculated.LegCount; color++)
                if (calculated.Leg(frame, color) != frames[frame].Leg(color))
                    return new(frames[0], frames[^1], frames.Length, frames);
            RgbColor? trailing = calculated.first.TrailingColor is RgbColor start && calculated.last.TrailingColor is RgbColor end
                ? calculated.Interpolate(start, end, frame) : null;
            if (trailing != frames[frame].TrailingColor)
                return new(frames[0], frames[^1], frames.Length, frames);
        }
        return calculated;
    }

    /// <summary>Captures only phase/color differences from the revival's endpoint interpolation.</summary>
    private static Presentation.RevivalPaletteFade Revival(Presentation.PaletteFrame[] frames)
    {
        var overrides = new Dictionary<RainbowPaletteColorKey, RgbColor>();
        var fade = new Presentation.RevivalPaletteFade(frames[0], frames[^1], frames.Length, overrides);
        for (int frame = 0; frame < fade.Length; frame++)
            for (int color = 0; color < fade.BodyCount + fade.LegCount + 1; color++)
            {
                RgbColor supplied = fade.ReadColor(frames[frame], color);
                if (fade.Interpolate(frame, color) != supplied)
                    overrides.Add(new RainbowPaletteColorKey(frame, color), supplied);
            }
        return fade;
    }

    /// <summary>Uses the calculated wheel only when every supplied RGB5 sample agrees.</summary>
    private static Presentation.BeamColors Beam(RgbColor[] colors)
    {
        for (int index = 0; index < colors.Length; index++)
            if (colors[index] != Presentation.BeamColors.Calculate(index))
                return new(colors.Length, colors);
        return new(colors.Length, supplied: null);
    }
}
