using System.Collections.Frozen;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The bytes of characters each picture of a looping room-FX animation holds in the installed atlas (#1584): the size each
/// object header copies per frame, which only the import needs to cut the atlas into pictures and read them from the
/// cartridge.
/// </summary>
internal static class RoomFxAnimatedTilePictureSizeDefinitions
{
    /// <summary>The bytes of each picture of each looping animation.</summary>
    internal static readonly FrozenDictionary<AnimatedTileObject, int> PictureByteCounts = new Dictionary<AnimatedTileObject, int>
    {
        [AnimatedTileObject.HorizontalSpikes] = 128,
        [AnimatedTileObject.MaridiaSandCeiling] = 64,
        [AnimatedTileObject.MaridiaSandFalling] = 32,
        [AnimatedTileObject.Lava] = 64,
        [AnimatedTileObject.Acid] = 64,
        [AnimatedTileObject.Rain] = 80,
        [AnimatedTileObject.Spores] = 48,
    }.ToFrozenDictionary();
}
