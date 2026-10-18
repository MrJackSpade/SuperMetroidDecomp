using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Native Maridia sand/waterfall color rotations. Sixteen first-row colors remain required source inputs.</summary>
internal static class MaridiaEnvironmentalColorDefinitions
{
    /// <summary>Maps a sand or waterfall color to its first-frame source after native cyclic rotation.</summary>
    /// <remarks>Sand falls take their edited colors from the lower sand-pit band; rejected colors clear the output.</remarks>
    internal static bool TrySource(PaletteFxColorKey key, out PaletteFxColorKey source)
    {
        foreach (var program in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        {
            if (!program.Rows.TryLocate(key, out int frame, out int color)) continue;
            int group = color / program.FrameCount;
            int rotated = (color + frame) % program.FrameCount;
            if (program.Owner == MaridiaEnvironmentalPaletteOwner.SandFalls)
            {
                var pits = MaridiaEnvironmentalPaletteFxColorDefinitions.All[0];
                source = pits.Color(0, pits.FrameCount + rotated);
            }
            else source = program.Color(0, group * program.FrameCount + rotated);
            return true;
        }
        source = default;
        return false;
    }

    /// <summary>Eight required sand colors and eight required waterfall colors, selected by cyclic phase.</summary>
    internal static bool TryReadColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> inputs, out Bgr555 color)
    {
        color = Bgr555.Black;
        return TrySource(key, out PaletteFxColorKey source) && inputs.TryGetValue(source, out color);
    }
}
