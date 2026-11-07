using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.WreckedShip;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The import's reading of the colours a <see cref="WreckedShipGhostFade"/> leaves the ghost showing, which it pre-colours
/// each of the fade's shades in (#1512).
/// </summary>
internal static class WreckedShipGhostFadeColours
{
    /// <summary>The colours of a Wrecked Ship ghost fade.</summary>
    extension(WreckedShipGhostFade fade)
    {
        /// <summary>The colours the ghost shows, colour zero first.</summary>
        internal ReadOnlySpan<RgbColor> Shown => fade.shown;
    }
}
