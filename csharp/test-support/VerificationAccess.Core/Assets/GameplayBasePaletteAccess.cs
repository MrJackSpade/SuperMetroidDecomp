

namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="GameplayBasePalette"/> members production does not use.</summary>
internal static class GameplayBasePaletteAccess
{
    /// <summary>Exposes the catalog's stock common-sprite CGRAM words for comparison.</summary>
    extension(GameplayBasePalette self)
    {
        /// <summary>Reads the common-sprite palette without copying or applying overrides.</summary>
        internal ReadOnlySpan<RgbColor> CommonSprites => self.commonSprites;
    }
}
