using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Presentation geometry of the color payloads at $8D:D906..D9CA.</summary>
public static class HyperBeamFxColorFormat
{
    /// <summary>Supplies shared extrema for the remaining red,yellow,green and blue endpoint inks.</summary>
    /// <remarks>Original8D:D906+20*frame: red ink1 shares white red;
    /// yellow inks1/3 share red's minimum as blue; green inks1/3 share that
    /// minimum as red,with ink1 also sharing the red maximum as green.
    /// Blue inks1/3 share the red minimum as red; inks1/4 share white blue.
    /// Other components remain independent inputs,including zero-valued edits.</remarks>
    internal static bool TrySharedEndpoint(int frame, int ink, Bgr555 white, Bgr555 red, out Bgr555 basis, out int independentMask)
    {
        int minimum = red.Green, maximum = red.Red;
        HyperBeamMaskedColor selected = new HyperBeamInk(frame, ink) switch
        {
            (0, 1) => new HyperBeamMaskedColor(new Bgr555(white.Red, 0, 0), 6),
            (2, 1 or 3) => new HyperBeamMaskedColor(new Bgr555(0, 0, minimum), 3),
            (4, 1) => new HyperBeamMaskedColor(new Bgr555(minimum, maximum, 0), 4),
            (4, 3) or (6, 3) => new HyperBeamMaskedColor(new Bgr555(minimum, 0, 0), 6),
            (6, 1) => new HyperBeamMaskedColor(new Bgr555(minimum, 0, white.Blue), 2),
            (6, 4) => new HyperBeamMaskedColor(new Bgr555(0, 0, white.Blue), 3),
            _ => new HyperBeamMaskedColor(Bgr555.Black, 7),
        };
        basis = selected.Value;
        independentMask = selected.Mask;
        return selected.Mask != 7;
    }
    /// <summary>Builds a neutral RGB5 highlight from its single intensity.</summary>
    /// <remarks>The repeated white at8D:D906 has equal red,green,blue.
    /// Expand one0..31 intensity into all three channels. Edited unequal
    /// channels remain independent overrides; no generated color is stored.</remarks>
    internal static Bgr555 Neutral(int intensity)
    {
        if ((uint)intensity > 31) throw new ArgumentOutOfRangeException(nameof(intensity));
        return new Bgr555(intensity, intensity, intensity);
    }
    /// <summary>Rotates the red endpoint into green by exchanging red and green channels.</summary>
    /// <remarks>Original projectile frame4 ink7 ($8D:D964) derives from
    /// frame0 ink3 ($D90C). Blue stays fixed; RGB5 channel exchange has no
    /// rounding,saturation or overflow. Differing asset values remain inputs.</remarks>
    internal static Bgr555 GreenFromRed(Bgr555 red) =>
        new(red.Green, red.Red, red.Blue);

    /// <summary>Selects middle projectile shades calculated from their adjacent inks.</summary>
    /// <remarks>In the original rows8D:D906+20*frame, red frame0 and magenta
    /// frame8 ink2 interpolate inks1/3; green frame4 and magenta frame8 ink5
    /// interpolate inks4/6. Each RGB5 channel rounds its midpoint upward.
    /// Other even-frame ink2 shades and frame6 ink5 also share matching
    /// midpoint channels; six differing components remain independent inputs
    /// covered by the class-level ink-color disposition. Mismatch alone is not its rationale.</remarks>
    internal static bool IsShadeMidpoint(int frame, int ink) =>
        ink == 2 && frame is 0 or 2 or 4 or 6 or 8 || ink == 5 && frame is 4 or 6 or 8;
    /// <summary>Blends red and white into the four red-frame highlight inks.</summary>
    /// <remarks>Original8D:D90E..D914 (frame0 inks4..7) are4/5,3/5,2/5,1/5
    /// white from ink0 toward red ink3. Round each RGB5 channel to nearest:
    /// (red*(5-weight)+white*weight+2)/5. There are no half ties; numerator
    /// is at most157,with no saturation or overflow. Independent source edits
    /// preserve supplied targets through input overrides,not a generated cache.</remarks>
    internal static Bgr555 RedHighlight(Bgr555 red, Bgr555 white, int ink)
    {
        if (ink is < 4 or > 7) throw new ArgumentOutOfRangeException(nameof(ink));
        int whiteWeight = 8 - ink, redWeight = 5 - whiteWeight;
        int r = ((red.Red) * redWeight + (white.Red) * whiteWeight + 2) / 5;
        int g = ((red.Green) * redWeight + (white.Green) * whiteWeight + 2) / 5;
        int b = ((red.Blue) * redWeight + (white.Blue) * whiteWeight + 2) / 5;
        return new Bgr555(r, g, b);
    }
    /// <summary>Asset filename for the Hyper Beam projectile palette colors, separate from Samus's full-body Hyper Beam cycle.</summary>
    public const string FileName = "hyper-beam-fx-colors.json";
    /// <summary>Supported revision of the ten-row RGB5 projectile-color JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Ten palette rows in the native <c>$8D:D900</c> cycle; stock even rows supply red, yellow, green, blue, and magenta endpoints, with intervening midpoint rows.</summary>
    public const int FrameCount = 10;
    /// <summary>Eight colors per native palette record, written to OBJ palette 6 colors 1..8; control words and frame duration are not included.</summary>
    public const int ColorsPerFrame = 8;
    /// <summary>Required frame-by-color dimensions of the editable document.</summary>
    internal static FixedGridShape Shape => new(FrameCount, ColorsPerFrame);
}
