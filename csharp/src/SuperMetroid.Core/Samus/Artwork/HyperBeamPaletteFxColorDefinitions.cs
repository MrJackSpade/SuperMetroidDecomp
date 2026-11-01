using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Compiled control skeleton for the Hyper Beam palette-FX program.
/// The ten eight-color payloads remain presentation data; this catalog owns the fixed
/// entry command, timing, record terminators, loop command, and valid pointer domain.
/// </summary>
internal static class HyperBeamPaletteFxColorDefinitions
{

    /// <summary>The ten records of the Hyper Beam program and the eight colours each writes.</summary>
    internal static PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(PaletteFxProgram.HyperBeam1, SamusColourDefinitions.HyperBeamColours);
}
