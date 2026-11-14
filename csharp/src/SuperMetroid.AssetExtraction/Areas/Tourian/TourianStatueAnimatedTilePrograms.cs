using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Areas.Tourian;

/// <summary>
/// The four Tourian entrance statues' animations, which the import walks to picture each statue's frames and the room
/// uploads its unlocking animations paint (#1502), and the animated-tile objects whose headers copy their pictures
/// (<see cref="TourianStatueTileStripDefinitions"/>).
/// </summary>
internal static class TourianStatueAnimatedTilePrograms
{
    /// <summary>The four statues' animations in authored header order.</summary>
    internal static IEnumerable<TourianStatueAnimationDefinition> All
    {
        get
        {
            yield return Require(TourianGreyStatues.Phantoon);
            yield return Require(TourianGreyStatues.Ridley);
            yield return Require(TourianGreyStatues.Kraid);
            yield return Require(TourianGreyStatues.Draygon);
        }
    }

    /// <summary>The animated-tile object whose header copies <paramref name="statue"/>'s pictures.</summary>
    /// <exception cref="InvalidDataException"><paramref name="statue"/> is not one statue.</exception>
    internal static BlockAnimation ObjectOf(TourianGreyStatues statue) => RoomFxBlockAnimationPictures.TourianStatueWindow(statue).Object;

    /// <summary>The animation of <paramref name="statue"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="statue"/> is not cataloged.</exception>
    private static TourianStatueAnimationDefinition Require(TourianGreyStatues statue) =>
        TourianStatueAnimationDefinitions.Statues.TryGetValue(statue, out TourianStatueAnimationDefinition? definition)
            ? definition
            : throw new InvalidOperationException($"Tourian statue {statue} has no definition in this catalog.");
}
