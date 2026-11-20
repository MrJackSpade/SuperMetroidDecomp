using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// The Hyper Beam effect's colour frames and the colours each shows.
/// </summary>
internal static class HyperBeamPaletteFxColorDefinitions
{

    /// <summary>The ten records of the Hyper Beam program and the eight colours each writes.</summary>
    internal static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.HyperBeam, SamusColourSets.HyperBeamColours);
}
