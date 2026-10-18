using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Compiled control skeleton for Hyper Beam palette-FX program <c>$8D:D900</c>.
/// The ten eight-color payloads remain presentation data; this catalog owns the fixed
/// entry command, timing, record terminators, loop command, and valid pointer domain.
/// </summary>
internal static class HyperBeamPaletteFxColorDefinitions
{

    /// <summary>The ten records of the Hyper Beam program and the eight colours each writes.</summary>
    internal static PaletteFxColorRows Rows { get; } = new(217, HyperBeamFxColorFormat.FrameCount, HyperBeamFxColorFormat.ColorsPerFrame);
}
