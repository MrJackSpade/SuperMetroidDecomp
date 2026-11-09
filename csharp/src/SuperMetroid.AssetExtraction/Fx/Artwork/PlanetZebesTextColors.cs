using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>PLANET ZEBES text colors from three independently required bright endpoints.</summary>
internal static class PlanetZebesTextColors
{
    /// <summary>The fade owner, frame, and RGB endpoint slot of a text color.</summary>
    internal static bool TryCoordinates(PaletteFxColorKey key, out PlanetZebesTextPaletteFxProgramOwner owner, out int frame, out int color)
    {
        foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
        {
            if (program.ColorFrames.TryLocate(key, out frame, out color))
            {
                owner = program.Owner;
                return true;
            }
        }
        owner = default; frame = color = 0;
        return false;
    }

    /// <summary>The fade-in's last frame: REQUIRED selected bright text endpoints. No color or intensity exception is claimed.</summary>
    internal static PaletteFxColorKey Endpoint(int color) => PlanetZebesTextPaletteFxColorDefinitions.All[0]
        .Color(PlanetZebesTextPaletteFxColorDefinitions.FrameCount - 1, color);

    /// <summary>Round each endpoint level to nearest over seven intervals; fade-out reverses the phase.</summary>
    internal static bool TryCalculate(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> inputs, out RgbColor color)
    {
        color = RgbColor.Black;
        if (!TryCoordinates(key, out var owner, out int frame, out int index)
            || !inputs.TryGetValue(Endpoint(index), out RgbColor endpoint)) return false;
        int intervals = PlanetZebesTextPaletteFxColorDefinitions.FrameCount - 1;
        int phase = owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn ? frame : intervals - frame;
        int red = ((endpoint.FadeLevels.Red) * phase + intervals / 2) / intervals;
        int green = ((endpoint.FadeLevels.Green) * phase + intervals / 2) / intervals;
        int blue = ((endpoint.FadeLevels.Blue) * phase + intervals / 2) / intervals;
        color = new FadeLevelColour(red, green, blue).ToColour();
        return true;
    }
}
