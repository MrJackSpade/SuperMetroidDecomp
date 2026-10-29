using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Verification access to <see cref="SamusColours"/> members production does not use.</summary>
internal static class SamusColoursAccess
{
    /// <summary>Reads Samus's colours as a check compares them.</summary>
    extension(SamusColours self)
    {
        /// <summary>The visor's colour, within the suit's.</summary>
        internal RgbColor Visor => self.Suit[SamusColourDefinitions.VisorColour];
    }
}
