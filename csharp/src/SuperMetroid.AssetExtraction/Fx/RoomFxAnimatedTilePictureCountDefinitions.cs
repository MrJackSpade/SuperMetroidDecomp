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
    internal static readonly FrozenDictionary<AnimatedTileObject, int> PictureCounts = new Dictionary<AnimatedTileObject, int>
    {
        [AnimatedTileObject.HorizontalSpikes] = 3,
        [AnimatedTileObject.MaridiaSandCeiling] = 4,
        [AnimatedTileObject.MaridiaSandFalling] = 4,
        [AnimatedTileObject.Lava] = 5,
        [AnimatedTileObject.Acid] = 5,
        [AnimatedTileObject.Rain] = 5,
        [AnimatedTileObject.Spores] = 3,
    }.ToFrozenDictionary();
}
