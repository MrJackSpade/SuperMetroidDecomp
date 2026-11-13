using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The OBJ selection each ending sprite sheet's page was drawn with: where its characters begin and which sprite sizes its
/// compositions use, which the ending's sheets are composed in (#1463, #1626).
/// </summary>
[EncodingOwner("object-selection register values of the ending sprite pages")]
internal static class EndingSheetObjectSelectionDefinitions
{
    /// <summary>The sprite-sheet selection of the escape and explosion scenes.</summary>
    private const byte EscapeObjectSelection = 2;

    /// <summary>The sprite-sheet selection of the atmospheric cloud scenes.</summary>
    private const byte CloudObjectSelection = 0xa3;

    /// <summary>The sprite-sheet selection of the reward: the contiguous sheet at the start of sprite memory.</summary>
    private const byte RewardObjectSelection = 0;

    /// <summary>The sprite-sheet selection the reward jump installs when Samus passes above the screen and falls back.</summary>
    private const byte FallingRewardObjectSelection = 3;

    /// <summary>The OBJ selection each ending sprite sheet's page is drawn with.</summary>
    private static readonly IReadOnlyDictionary<SpriteSheetId, byte> BySheet = new Dictionary<SpriteSheetId, byte>
    {
        [SpriteSheetId.EndingCloudSprites] = CloudObjectSelection,
        [SpriteSheetId.EndingExplosionSprites] = EscapeObjectSelection,
        [SpriteSheetId.EndingSuitedRewardSprites] = RewardObjectSelection,
        [SpriteSheetId.EndingSuitlessRewardSprites] = RewardObjectSelection,
        [SpriteSheetId.EndingShootingSprites] = FallingRewardObjectSelection,
    };

    /// <summary>The OBJ selection the page of ending sheet <paramref name="sheet"/> is drawn with.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sheet"/> has no listed selection.</exception>
    internal static byte Of(SpriteSheetId sheet) => Ensure.Listed(BySheet, sheet);
}
