using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="ChozoAndTubeColorCatalog"/> members production does not use.</summary>
internal static class ChozoAndTubeColorCatalogAccess
{
    /// <summary>Uses the catalog's private statue resolver for the two special palettes.</summary>
    extension(ChozoAndTubeColorCatalog self)
    {
        /// <summary>Selects Wrecked Ship statue colors through the production palette branch.</summary>
        internal Bgr555 ResolveWreckedShip(int color) => ((Bgr555)(self.ResolveStatue((ChozoStatuePalette)(ChozoStatuePalette.WreckedShip), (int)(color))));

        /// <summary>Selects Lower Norfair statue colors through the production palette branch.</summary>
        internal Bgr555 ResolveLowerNorfair(int color) => ((Bgr555)(self.ResolveStatue((ChozoStatuePalette)(ChozoStatuePalette.LowerNorfair), (int)(color))));
    }
}
