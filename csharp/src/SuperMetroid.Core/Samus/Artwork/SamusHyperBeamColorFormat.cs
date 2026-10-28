using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Installed-resource identity and native row geometry for the full-body Hyper Beam RGB5 cycle.</summary>
public static class SamusHyperBeamColorFormat
{
    /// <summary>JSON resource filename containing the ten editable full-body Hyper Beam palettes.</summary>
    public const string FileName = "samus-hyper-beam-colors.json";
    /// <summary>Supported schema revision, one, requiring all ten complete RGB5 palette rows.</summary>
    public const int Version = 1;
    /// <summary>Ten authored palette phases selected by $91:D99E; this count does not specify the number of gameplay updates between phases.</summary>
    public const int FrameCount = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount;
    /// <summary>Sixteen colors in each complete OBJ palette, including transparent index zero and fifteen opaque ink slots.</summary>
    public const int ColorsPerFrame = SamusPaletteDefinitions.Common.ColoursPerRow;
    /// <summary>Required frame-by-color dimensions of the editable document.</summary>
    internal static FixedGridShape Shape => new(FrameCount, ColorsPerFrame);

    /// <summary>Selects a shadow channel shared with the same frame's middle ink13.</summary>
    /// <remarks>Native frame0 ink3 red and ink11 green at9BA366/A376
    /// share middle ink13 red21/green3. Frame8 ink11 blue at9BA276 shares
    /// middle blue3. Other channels keep their temporal blend. Return the
    /// RGB5 bit shift or-1 when no within-frame sharing applies.</remarks>
    internal static ColorChannel? SharedIntermediateShadowChannel(int frame, int ink) => new HyperBeamInk(frame, ink) switch
    {
        (0, 3) => ColorChannel.Red,
        (0, 11) => ColorChannel.Green,
        (8, 11) => ColorChannel.Blue,
        _ => null,
    };

    /// <summary>Interpolates the magenta-to-cyan middle shadow, rounding toward cyan.</summary>
    /// <remarks>Native frame2 ink13 at9BA33A is RGB10/7/18 between
    /// magenta19/0/19 and cyan2/13/17. Floor the red half-sum and ceil the
    /// green/blue half-sums: the quantization favors the cyan channels.
    /// No fixed correction value is added. Numerators0..63,RGB5 inputs only.
    /// This states the exact color construction,not the original tool's identity.</remarks>
    internal static RgbColor CyanTransitionMiddle(RgbColor magenta, RgbColor cyan)
    {
        return HueMidpoint(magenta, cyan).WithRed(HueMidpoint(magenta, cyan, direction: HueRounding.Down).Red);
    }

    /// <summary>Builds the lower and upper shadows around the magenta-cyan middle ink.</summary>
    /// <remarks>Original frame2 middle13 is10/7/18. Lower3 is the same
    /// red/blue with green one step lower; upper11 is one RGB step brighter.
    /// This gives10/6/18 and11/8/19 without independent intermediate inputs.
    /// Edited middle colors may exceed those operations' RGB5 bounds; return
    /// false so import preserves the supplied whole target,without saturation.</remarks>
    internal static bool TryCyanTransitionShadow(RgbColor middle, int ink, out RgbColor value)
    {
        if (ink is not (3 or 11)) throw new ArgumentOutOfRangeException(nameof(ink));
        int red = middle.Red, green = middle.Green, blue = middle.Blue;
        if (ink == 3)
        {
            if (green == 0) { value = RgbColor.Black; return false; }
            value = middle.WithGreen(green - 1);
        }
        else
        {
            if (red == 31 || green == 31 || blue == 31) { value = RgbColor.Black; return false; }
            value = new(red + 1, green + 1, blue + 1);
        }
        return true;
    }

    /// <summary>Derives shared hue channels and the middle shadow's endpoint components.</summary>
    /// <remarks>Native endpoint frames1/5/9 are magenta,green,red at
    /// 9BA340/A2C0/A240. Their shadow ink13 uses the midpoint of inks3/11:
    /// magenta red18/20 gives19; green green22/23 rounds up to23;
    /// red green4/6 gives5. Green ink13 shares ink3's red/blue minimum0.
    /// Existing hue rules share magenta green with green-hue red,and red-hue
    /// red with green-hue green. Blue derives from the appropriate resolved
    /// red/green channel. Null components remain independently supplied.
    /// Domain:frames1/5/9,shadow inks3/11/13,RGB5 inputs. The reused midpoint
    /// is bounded,no clamping or generated color storage is needed.</remarks>
    internal static HyperBeamEndpointChannels EndpointSourceChannels(int frame, int color,
        RgbColor greenHue, RgbColor lowShadow, RgbColor highShadow)
    {
        if (frame is not (1 or 5 or 9)) throw new ArgumentOutOfRangeException(nameof(frame));
        if (color is not (3 or 11 or 13)) throw new ArgumentOutOfRangeException(nameof(color));
        int? red = frame == 9 ? greenHue.Green : null;
        int? green = frame == 1 ? greenHue.Red : null;
        if (color == 13)
        {
            RgbColor middle = HueMidpoint(lowShadow, highShadow);
            if (frame == 1) red = middle.Red;
            if (frame == 5) red = lowShadow.Red;
            if (frame is 5 or 9) green = middle.Green;
        }
        return new HyperBeamEndpointChannels(red, green);
    }

    /// <summary>Selects the shared shadow ink and constant RGB brightening for an opaque sprite ink.</summary>
    /// <remarks>Across all ten native rows9B:A240..A37F, inks1/8 share ink11
    /// at +2/+4 in each channel; inks2/10/14 share ink3 at +8/+4/+2;
    /// inks4/5/9 share ink13 at +8/+2/+4. These are fixed shade assignments,
    /// independent of hue phase. Ink7 likewise uses ink3+7, with differing
    /// components in frames1/4/8 kept as independently selected highlight
    /// accents under the catalog's class-level artwork disposition.
    /// Other inks keep their own input.</remarks>
    internal static HyperBeamShadeSource ShadeSource(int ink) => ink switch
    {
        1 => new HyperBeamShadeSource(11, 2), 8 => new HyperBeamShadeSource(11, 4),
        2 => new HyperBeamShadeSource(3, 8), 7 => new HyperBeamShadeSource(3, 7), 10 => new HyperBeamShadeSource(3, 4), 14 => new HyperBeamShadeSource(3, 2),
        4 => new HyperBeamShadeSource(13, 8), 5 => new HyperBeamShadeSource(13, 2), 9 => new HyperBeamShadeSource(13, 4),
        _ => new HyperBeamShadeSource(ink, 0),
    };

    /// <summary>Adds a supported shade increment independently to all three RGB5 channels.</summary>
    /// <remarks>Original shades never overflow. A supplied edited source that
    /// would exceed31 cannot replace its independently supplied target; import
    /// keeps that target explicit instead of clamping or wrapping a channel.</remarks>
    internal static bool TryBrighten(RgbColor source, int brightness, out RgbColor value)
    {
        if (brightness is not (2 or 4 or 7 or 8)) throw new ArgumentOutOfRangeException(nameof(brightness));
        int red = (source.Red) + brightness;
        int green = (source.Green) + brightness;
        int blue = (source.Blue) + brightness;
        if (red > 31 || green > 31 || blue > 31) { value = RgbColor.Black; return false; }
        value = new RgbColor(red, green, blue);
        return true;
    }
    /// <summary>Turns the red endpoint into magenta by raising blue to red.</summary>
    /// <remarks>Original projectile frame8 ink1 ($8D:D9A8) derives from
    /// frame0 ink3 ($D90C). Body-cycle frame1 also has blue=red,including
    /// its independently edited shade-red channels. Red/green stay fixed; copying red into blue
    /// needs no rounding,saturation or overflow. Edits remain independent.</remarks>
    internal static RgbColor MagentaFromRed(RgbColor red) =>
        red.WithBlue(red.Red);
    /// <summary>Interpolates RGB5 colors by half with the caller's channel rounding convention.</summary>
    /// <remarks>Even frames interpolate the adjacent odd hue endpoints, with
    /// frame0 wrapping between9/1. Frame6 uses the green-to-yellow red ramp.
    /// Only independently edited channels differing from the selected color
    /// construction are stored; original intermediate colors need no corrections.
    /// matching channels are always calculated, never stored as generated words.
    /// Upward rounding is the default for hue blends. The magenta-cyan
    /// middle shadow selects downward red and upward green/blue rounding.
    /// Each independent channel numerator is0..63; no saturation or overflow.</remarks>
    internal static RgbColor HueMidpoint(RgbColor first, RgbColor second, HueRounding direction = HueRounding.Up)
    {
        int rounding = direction == HueRounding.Up ? 1 : 0;
        int red = ((first.Red) + (second.Red) + rounding) / 2;
        int green = ((first.Green) + (second.Green) + rounding) / 2;
        int blue = ((first.Blue) + (second.Blue) + rounding) / 2;
        return new RgbColor(red, green, blue);
    }

    /// <summary>Interpolates red halfway from green-frame red to its green value, rounding upward.</summary>
    /// <remarks>Original frame6 ($9B:A2A0) preserves frame5 green/blue;
    /// nine canonical opaque inks also have red=ceil((red5+green5)/2).
    /// Ink11 instead follows the within-frame red shadow ramp,with inks1/8
    /// deriving their brighter shades from it. Green/blue remain shared.
    /// The numerator is at most63; no saturation or
    /// overflow is needed. This reuses the green-frame input, not a generated cache.</remarks>
    internal static RgbColor GreenYellowMidpoint(RgbColor green) =>
        green.WithRed((green.Red + green.Green + 1) / 2);

    /// <summary>Extends the green-yellow red shadow ramp by its existing equal step.</summary>
    /// <remarks>Native frame6 inks3/13/11 at9BA2A6/A2BA/A2B6 have red11/12/13.
    /// Calculate the upper red as2*middle-low; green23/blue1 remain those of
    /// green frame5 ink11. This preserves the spatial shade ramp instead of
    /// storing a red correction to the temporal midpoint. RGB5 inputs only.
    /// Edited shadow endpoints may yield red-31..62; return false outside0..31
    /// so import keeps the independently supplied whole target,without clamping.</remarks>
    internal static bool TryGreenYellowHighShadow(RgbColor low, RgbColor middle, RgbColor green, out RgbColor value)
    {
        int red = 2 * (middle.Red) - (low.Red);
        if ((uint)red > 31) { value = RgbColor.Black; return false; }
        value = green.WithRed(red);
        return true;
    }

    /// <summary>Changes the green Hyper Beam hue into yellow by raising red to green.</summary>
    /// <remarks>Every opaque original frame7 word ($9B:A280) equals frame5
    /// ($9B:A2C0) with red replaced by green. Green/blue remain unchanged.
    /// RGB5 component copying needs no rounding or saturation. Transparent
    /// payloads are outside the hue transform; differing asset values override it.</remarks>
    internal static RgbColor YellowFromGreen(RgbColor green) =>
        green.WithRed(green.Green);

    /// <summary>Shares repeated Hyper Beam sprite inks and transparent payloads.</summary>
    /// <remarks>Original ten rows selected by91D99E have slots6=2,15=3,
    ///12=10. Transparent frames4..9 equal frame2. These cases preserve original
    /// input ownership with differing asset values stored as overrides. All other
    /// color/shade relationships still require independent review.</remarks>
    internal static int CanonicalColorIndex(int frame, int color)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= ColorsPerFrame) throw new ArgumentOutOfRangeException(nameof(color));
        if (color == 0 && frame >= 4) return 2 * 16;
        int ink = color switch { 6 => 2, 15 => 3, 12 => 10, _ => color };
        return frame * 16 + ink;
    }
}
