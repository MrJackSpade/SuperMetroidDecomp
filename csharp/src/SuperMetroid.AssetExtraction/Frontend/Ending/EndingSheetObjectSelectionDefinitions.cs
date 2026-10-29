using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The OBJ selection each ending sprite page was drawn with: where its characters begin and which sprite sizes its
/// compositions use, which the ending's sheets are composed in (#1463).
/// </summary>
internal static class EndingSheetObjectSelectionDefinitions
{
    /// <summary>The sprite-sheet selection of the escape and explosion scenes.</summary>
    private const byte EscapeObjectSelection = 2;

    /// <summary>The sprite-sheet selection of the atmospheric cloud scenes.</summary>
    private const byte CloudObjectSelection = 0xa3;

    /// <summary>The sprite-sheet selection of the reward: the contiguous sheet at the start of sprite memory.</summary>
    private const byte RewardObjectSelection = 0;

    /// <summary>The sprite-sheet selection the reward jump installs when Samus passes above the screen and her falling sheet replaces the reward sheet.</summary>
    private const byte FallingRewardObjectSelection = 3;

    /// <summary>The OBJ selection each ending sprite page's actors are drawn with.</summary>
    private static readonly IReadOnlyDictionary<EndingObjectSheet, byte> BySheet = new Dictionary<EndingObjectSheet, byte>
    {
        [EndingObjectSheet.Clouds] = CloudObjectSelection,
        [EndingObjectSheet.Explosion] = EscapeObjectSelection,
        [EndingObjectSheet.Reward] = RewardObjectSelection,
        [EndingObjectSheet.RewardFalling] = FallingRewardObjectSelection,
    };

    /// <summary>The OBJ selection the actors of <paramref name="sheet"/> are drawn with.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sheet"/> has no listed selection.</exception>
    internal static byte Of(EndingObjectSheet sheet) => Ensure.Listed(BySheet, sheet);
}
