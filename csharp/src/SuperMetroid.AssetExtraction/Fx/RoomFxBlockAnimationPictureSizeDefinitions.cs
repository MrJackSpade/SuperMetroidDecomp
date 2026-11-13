using System.Collections.Frozen;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The bytes of characters each picture of a looping room-FX animation holds in the installed atlas (#1584): the size each
/// object header copies per frame, which only the import needs to cut the atlas into pictures and read them from the
/// cartridge.
/// </summary>
internal static class RoomFxBlockAnimationPictureSizeDefinitions
{
    /// <summary>The bytes of each picture of each looping animation.</summary>
    internal static readonly FrozenDictionary<BlockAnimation, int> PictureByteCounts = new Dictionary<BlockAnimation, int>
    {
        [BlockAnimation.HorizontalSpikes] = 128,
        [BlockAnimation.MaridiaSandCeiling] = 64,
        [BlockAnimation.MaridiaSandFalling] = 32,
        [BlockAnimation.Lava] = 64,
        [BlockAnimation.Acid] = 64,
        [BlockAnimation.Rain] = 80,
        [BlockAnimation.Spores] = 48,
    }.ToFrozenDictionary();
}
