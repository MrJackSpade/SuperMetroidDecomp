using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The colourings each enemy whose colours its encounter's code sets, rather than its graphics set's load, first shows its
/// frames in (#1513): the shades its encounter lays over the row it draws in, in the order they are laid, up to the moment
/// its frames first show. Their sheets are coloured in what those shades leave in that row, so a frame that shows before
/// any later shade does not show the zero colours its header palette leaves there.
/// </summary>
internal static class EnemyFirstShownColourDefinitions
{
    /// <summary>
    /// The shades laid over each such enemy's row before its frames first show, in laying order. The Baby's cutscene
    /// initialization lays its arrival colours before its first frame. Both Ridleys' initializations clear their row,
    /// then the eye fade ends on its brightest shade and the body fade on its full body colours before either encounter
    /// shows him; his row's last colour stays cleared.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, PictureShade[]> Shades { get; } = new Dictionary<EnemyDefinitionId, PictureShade[]>
    {
        [EnemyDefinitionId.BabyMetroidCutscene] = [MotherBrainShades.BabyArrives],
        [EnemyDefinitionId.RidleyCeres] =
        [
            RidleyShades.CeresHidden,
            RidleyShades.EyeFade(0),
            RidleyShades.BodyFade(CeresRidleyPaletteDefinitions.BodyFadeShadeCount - 1),
        ],
        [EnemyDefinitionId.Ridley] =
        [
            RidleyShades.NorfairHidden,
            RidleyShades.EyeFade(0),
            RidleyShades.BodyFade(CeresRidleyPaletteDefinitions.BodyFadeShadeCount - 1),
        ],
    };
}
