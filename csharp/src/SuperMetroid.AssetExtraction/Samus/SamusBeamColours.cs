using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The colours Samus's beams show for each beam combination the game can select (#1512): a retail combination's installed
/// palette, the one the glitched Chainsaw beam shares (#1736), or the SpaceTime beam's defined black.
/// </summary>
internal static class SamusBeamColours
{
    /// <summary>
    /// Every colour of the SpaceTime beam. The game's beam palette load reads its colours through a colour word used as a
    /// pointer into live enemy-projectile timers and frame behaviours in memory. The port does not keep that layout, so
    /// the palette is a defined one instead (owner decision on #1318): black.
    /// </summary>
    private static readonly RgbColor SpacetimeColour = RgbColor.Black;

    /// <summary>The beam combinations with a beam shade of their own: the retail combinations and SpaceTime.</summary>
    internal static IEnumerable<SamusBeamCombination> Shown =>
        SamusBeamCombinationKeys.ListedOrder.Where(combination => combination.CanBeEquipped ||
            combination is SpacetimeBeamGraphicsDefinitions.Selection);

    /// <summary>
    /// The sixteen colours <paramref name="combination"/>'s beams show: SpaceTime's defined black, or the installed palette
    /// of the retail combination whose shade it shows (<see cref="SamusShades.ShownBeamShade"/>).
    /// </summary>
    internal static RgbColor[] Of(SamusBeamCombination combination, InstalledBeamPalettes palettes)
    {
        Ensure.NotNull(palettes);
        return combination == SpacetimeBeamGraphicsDefinitions.Selection
            ? Enumerable.Repeat(SpacetimeColour, BeamPaletteDefinitions.ColorCount).ToArray()
            : palettes.Colours(SamusShades.ShownBeamShade(combination));
    }
}
