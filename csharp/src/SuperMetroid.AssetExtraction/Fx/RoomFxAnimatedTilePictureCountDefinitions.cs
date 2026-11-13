using System.Collections.Frozen;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// How many distinct pictures each simple room-FX loop's frames show, each held once in the installed atlas (#1527): the
/// atlas's storage layout, which only the import needs to place and find a frame's characters.
/// </summary>
internal static class RoomFxAnimatedTilePictureCountDefinitions
{
    /// <summary>
    /// The distinct pictures of each simple loop. A loop with fewer pictures than frames plays them forward and back:
    /// the horizontal spikes' four frames show pictures 0, 1, 2 and 1.
    /// </summary>
    internal static readonly FrozenDictionary<BlockAnimation, int> PictureCounts = new Dictionary<BlockAnimation, int>
    {
        [BlockAnimation.HorizontalSpikes] = 3,
        [BlockAnimation.MaridiaSandCeiling] = 4,
        [BlockAnimation.MaridiaSandFalling] = 4,
        [BlockAnimation.Lava] = 5,
        [BlockAnimation.Acid] = 5,
        [BlockAnimation.Rain] = 5,
        [BlockAnimation.Spores] = 3,
    }.ToFrozenDictionary();
}
