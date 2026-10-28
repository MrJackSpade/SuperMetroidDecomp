using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Verification access to <see cref="RoomFxBlendInks"/> members production does not use.</summary>
internal static class RoomFxPaletteBlendCatalogAccess
{
    /// <summary>Selects room-FX blend colors through the catalog's private branch.</summary>
    extension(RoomFxBlendInks self)
    {
        /// <summary>Resolves the selected blend and materializes its three RGB5 colors.</summary>
        internal ReadOnlySpan<RgbColor> Resolve(RoomFxPaletteBlend selection) => self.SelectColors(selection).CreateColors();
    }
}
