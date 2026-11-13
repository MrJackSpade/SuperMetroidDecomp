using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Palettes;
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
        Enum.GetValues<SamusBeamCombination>().Where(combination => combination.IsRetail ||
            combination is SpacetimeBeamGraphicsDefinitions.Selection);

    /// <summary>The sixteen colours <paramref name="combination"/>'s beams show.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The game shows no palette for the combination.</exception>
    internal static RgbColor[] Of(SamusBeamCombination combination, InstalledBeamPalettes palettes)
    {
        Ensure.NotNull(palettes);
        return combination switch
        {
            ChainsawBeamGraphicsDefinitions.Selection => palettes.Colours(SamusShades.ShownPalette(combination)),
            SpacetimeBeamGraphicsDefinitions.Selection => Enumerable.Repeat(SpacetimeColour, BeamPaletteDefinitions.ColorCount).ToArray(),
            SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or SamusBeamCombination.IceWave
                or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or SamusBeamCombination.SpazerIce
                or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaWave
                or SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave => palettes.Colours(combination),
            SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave =>
                throw new ArgumentOutOfRangeException(nameof(combination), combination, "The game shows no palette for this beam combination."),
            _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
        };
    }
}
