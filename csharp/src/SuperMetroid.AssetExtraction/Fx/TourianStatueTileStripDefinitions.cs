using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The shared strip of characters the four Tourian statues' pictures are cut from (#1395): each statue's three eye
/// pictures, bright, dim and dark, and its released picture are windows of the one strip the room-FX atlas packs
/// (#1415: by character position). Only the import pictures the statues' frames from it.
/// </summary>
internal static partial class TourianStatueTileStripDefinitions
{
    /// <summary>The bytes of one four-colour character.</summary>
    internal const int CharacterByteCount = 16;

    /// <summary>
    /// Each statue's windows of the strip: the animated-tile object that copies them, its bright-eyed picture's
    /// first character, its pictures' width in characters and its released picture's first character
    /// (<see cref="RoomFxBlockAnimationPictures"/>).
    /// </summary>
    internal static readonly IReadOnlyDictionary<TourianStatue, TourianStatueTileWindow> Windows =
        new Dictionary<TourianStatue, TourianStatueTileWindow>
        {
            [TourianStatue.Phantoon] = new(BlockAnimation.TourianStatuePhantoon, 0, 8, 72),
            [TourianStatue.Ridley] = new(BlockAnimation.TourianStatueRidley, 24, 4, 80),
            [TourianStatue.Kraid] = new(BlockAnimation.TourianStatueKraid, 60, 4, 84),
            [TourianStatue.Draygon] = new(BlockAnimation.TourianStatueDraygon, 36, 8, 88),
        };
}
