using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to <see cref="ChozoAndTubeColorCatalog"/> members production does not use.</summary>
internal static class ChozoAndTubeColorCatalogAccess
{
    /// <summary>Uses the catalog's private statue resolver for the two special palettes.</summary>
    extension(ChozoAndTubeColorCatalog self)
    {
        /// <summary>Selects Wrecked Ship statue colors through the production palette branch.</summary>
        internal RgbColor ResolveWreckedShip(int color) => self.ResolveStatue(ChozoStatuePalette.WreckedShip, color);

        /// <summary>Selects Lower Norfair statue colors through the production palette branch.</summary>
        internal RgbColor ResolveLowerNorfair(int color) => self.ResolveStatue(ChozoStatuePalette.LowerNorfair, color);
        /// <summary>Resolves a statue ink from an authored edit or the selected variant's calculated paint.</summary>
        internal RgbColor ResolveStatue(ChozoStatuePalette palette, int color) => ChozoAndTubeColors.Statue(self, palette, color);
    }
}
