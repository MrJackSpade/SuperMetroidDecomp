namespace SuperMetroid.Core.Rendering;

/// <summary>Screen geometry values of <see cref="ScreenGeometry"/> that only verification reads.</summary>
internal static class ScreenGeometryConstants
{
    /// <summary>Room rows below the HUD.</summary>
    public const int GameplayViewportHeightPixels = ScreenGeometry.ScreenHeightPixels - ScreenGeometry.GameplayHudHeightPixels;

    /// <summary>Provides the gameplay viewport height verification compares against.</summary>
    extension(ScreenGeometry)
    {
        /// <inheritdoc cref="ScreenGeometryConstants.GameplayViewportHeightPixels"/>
        internal static int GameplayViewportHeightPixels => ScreenGeometryConstants.GameplayViewportHeightPixels;
    }
}
