using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Verification access to <see cref="RoomFxPaletteBlendCatalog"/> members production does not use.</summary>
internal static class RoomFxPaletteBlendCatalogAccess
{
    /// <summary>Selects room-FX blend colors through the catalog's private branch.</summary>
    extension(RoomFxPaletteBlendCatalog self)
    {
        /// <summary>Resolves the selected blend and materializes its three RGB5 colors.</summary>
        internal ReadOnlySpan<Bgr555> Resolve(RoomFxPaletteBlend selection) => ((RoomFxBlendColors)(self.SelectColors(selection))).CreateColors();
    }
}
