using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>The sprite colours the Tourian corpses draw in (#1464).</summary>
internal static class TourianCorpseColourDefinitions
{
    /// <summary>
    /// The drained corpses' colours, which the Shitroid encounter installs for its victim and in which the room's
    /// already-dead Sidehopper, Zoomers, Rippers and Skrees lie.
    /// </summary>
    internal static readonly SpriteColourRun DrainedCorpses = new(7, 0, 16);
}
