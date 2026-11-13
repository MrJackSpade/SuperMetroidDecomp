using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The shared strip of characters the four Tourian statues' pictures are cut from (#1395): each statue's three
/// oscillation pictures and its released picture are windows of the one strip the room-FX atlas packs (#1415: by
/// character position). Only the import pictures the statues' frames from it.
/// </summary>
internal static class TourianStatueTileStripDefinitions
{
    /// <summary>The bytes of one four-colour character.</summary>
    internal const int CharacterByteCount = 16;

    /// <summary>
    /// Each statue's windows of the strip: the animated-tile object that copies them, its first oscillation picture's
    /// first character, its pictures' width in characters and its released picture's first character
    /// (<see cref="RoomFxAnimatedTilePictures"/>).
    /// </summary>
    internal static readonly IReadOnlyDictionary<TourianGreyStatues, TourianStatueTileWindow> Windows =
        new Dictionary<TourianGreyStatues, TourianStatueTileWindow>
        {
            [TourianGreyStatues.Phantoon] = new(BlockAnimation.TourianStatuePhantoon, 0, 8, 72),
            [TourianGreyStatues.Ridley] = new(BlockAnimation.TourianStatueRidley, 24, 4, 80),
            [TourianGreyStatues.Kraid] = new(BlockAnimation.TourianStatueKraid, 60, 4, 84),
            [TourianGreyStatues.Draygon] = new(BlockAnimation.TourianStatueDraygon, 36, 8, 88),
        };
}
