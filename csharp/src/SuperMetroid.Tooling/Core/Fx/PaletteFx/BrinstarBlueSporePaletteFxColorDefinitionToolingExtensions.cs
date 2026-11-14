using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Development-tool instance members of <see cref="BrinstarBlueSporePaletteFxColorDefinition"/>.</summary>
internal static class BrinstarBlueSporePaletteFxColorDefinitionToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="BrinstarBlueSporePaletteFxColorDefinition"/>.</summary>
    extension(BrinstarBlueSporePaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
