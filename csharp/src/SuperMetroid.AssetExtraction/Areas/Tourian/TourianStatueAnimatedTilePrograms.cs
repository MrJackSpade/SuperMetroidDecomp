using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Areas.Tourian;

/// <summary>
/// The four Tourian entrance statues' animated-tile programs, which the import walks to picture each statue's frames and
/// the room uploads its unlocking animations paint (#1502).
/// </summary>
internal static class TourianStatueAnimatedTilePrograms
{
    /// <summary>The four boss-statue programs in authored header order.</summary>
    internal static IEnumerable<TourianStatueAnimatedTileProgramDefinition> All
    {
        get
        {
            yield return Require(AnimatedTileObject.TourianStatuePhantoon);
            yield return Require(AnimatedTileObject.TourianStatueRidley);
            yield return Require(AnimatedTileObject.TourianStatueKraid);
            yield return Require(AnimatedTileObject.TourianStatueDraygon);
        }
    }

    /// <summary>The definition of a statue's animated-tile object.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animatedObject"/> is not a statue's object.</exception>
    private static TourianStatueAnimatedTileProgramDefinition Require(AnimatedTileObject animatedObject) =>
        TourianStatueAnimatedTiles.TryResolveObjectHeader(animatedObject, out TourianStatueAnimatedTileProgramDefinition? definition)
            ? definition
            : throw new InvalidOperationException($"{animatedObject} has no definition in this catalog.");
}
