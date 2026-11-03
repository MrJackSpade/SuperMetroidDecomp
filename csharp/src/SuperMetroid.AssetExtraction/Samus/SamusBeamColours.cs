using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The colours Samus's beams show for each beam combination the game can select (#1512): a retail combination's installed
/// palette, the glitched Chainsaw beam's colours the overread decoding holds, or the SpaceTime beam's defined black.
/// </summary>
internal static class SamusBeamColours
{
    /// <summary>
    /// Every colour of the SpaceTime beam. The game's beam palette load reads its colours through a colour word used as a
    /// pointer into live enemy-projectile timers and frame behaviours in memory. The port does not keep that layout, so
    /// the palette is a defined one instead (owner decision on #1318): black.
    /// </summary>
    private static readonly RgbColor SpacetimeColour = RgbColor.Black;

    /// <summary>The beam combinations the game can show a beam palette for, retail and glitched.</summary>
    internal static IEnumerable<SamusBeamCombination> Shown =>
        Enum.GetValues<SamusBeamCombination>().Where(combination => combination.IsRetail ||
            combination is ChainsawBeamGraphicsDefinitions.Selection or SpacetimeBeamGraphicsDefinitions.Selection);

    /// <summary>The sixteen colours <paramref name="combination"/>'s beams show.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The game shows no palette for the combination.</exception>
    internal static RgbColor[] Of(SamusBeamCombination combination, InstalledBeamPalettes palettes, ChainsawBeamColours<RgbColor> chainsaw)
    {
        Ensure.NotNull(palettes);
        Ensure.NotNull(chainsaw);
        return combination switch
        {
            ChainsawBeamGraphicsDefinitions.Selection => [.. chainsaw.Colours],
            SpacetimeBeamGraphicsDefinitions.Selection => Enumerable.Repeat(SpacetimeColour, BeamPaletteDefinitions.ColorCount).ToArray(),
            _ when combination.IsRetail => palettes.Colours(combination),
            _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "The game shows no palette for this beam combination."),
        };
    }
}
