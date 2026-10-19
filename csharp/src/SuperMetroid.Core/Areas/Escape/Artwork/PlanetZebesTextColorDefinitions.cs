using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>PLANET ZEBES text colors from three independently required bright endpoints.</summary>
internal static class PlanetZebesTextColorDefinitions
{
    /// <summary>The fade owner, frame, and RGB endpoint slot of a text color.</summary>
    internal static bool TryCoordinates(PaletteFxColorKey key, out PlanetZebesTextPaletteFxProgramOwner owner, out int frame, out int color)
    {
        foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
        {
            if (program.Rows.TryLocate(key, out frame, out color))
            {
                owner = program.Owner;
                return true;
            }
        }
        owner = default; frame = color = 0;
        return false;
    }

    /// <summary>The fade-in's last row: REQUIRED selected bright text endpoints. No color or intensity exception is claimed.</summary>
    internal static PaletteFxColorKey Endpoint(int color) => PlanetZebesTextPaletteFxColorDefinitions.All[0]
        .Color(PlanetZebesTextPaletteFxColorDefinitions.FrameCount - 1, color);

    /// <summary>Round each RGB5 endpoint channel to nearest over seven native intervals; fade-out reverses the phase.</summary>
    internal static bool TryCalculate(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> inputs, out Bgr555 color)
    {
        color = Bgr555.Black;
        if (!TryCoordinates(key, out var owner, out int frame, out int index)
            || !inputs.TryGetValue(Endpoint(index), out Bgr555 endpoint)) return false;
        int intervals = PlanetZebesTextPaletteFxColorDefinitions.FrameCount - 1;
        int phase = owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn ? frame : intervals - frame;
        int red = ((endpoint.Red) * phase + intervals / 2) / intervals;
        int green = ((endpoint.Green) * phase + intervals / 2) / intervals;
        int blue = ((endpoint.Blue) * phase + intervals / 2) / intervals;
        color = new Bgr555(red, green, blue);
        return true;
    }
}
