using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>Cartridge values and helpers of <see cref="SamusShadeDefinitions"/> that only verification reads.</summary>
internal static class SamusShadeDefinitionsConstants
{
    /// <summary>The colour effect the Hyper Beam starts when Samus receives it.</summary>
    public static class HyperBeamFx
    {
        /// <summary>How the Hyper Beam's colour effect starts: plainly.</summary>
        public const ColourEffectStart Start = ColourEffectStart.Plain;
    }
}
