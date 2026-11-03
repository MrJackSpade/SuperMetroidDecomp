using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>Verification access to <see cref="GameplayStartingColours"/> members production does not use.</summary>
internal static class GameplayStartingColoursAccess
{
    /// <summary>Exposes <see cref="GameplayStartingColours"/> state that only verification reads.</summary>
    extension(GameplayStartingColours self)
    {
        /// <summary>The background colours, in the cartridge's order.</summary>
        public ReadOnlySpan<RgbColor> BackgroundColours => self.backgroundColours;
    }
}
