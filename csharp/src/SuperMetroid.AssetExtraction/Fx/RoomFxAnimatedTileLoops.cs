using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The import's view of the simple room-FX animated-tile loops (#1502): the seven loops the atlas holds, in atlas order,
/// each loop's distinct pictures, and the byte lengths of the treadmill frames and the Tourian statue strip. The game
/// resolves one loop at a time and draws the installed pictures.
/// </summary>
public static class RoomFxAnimatedTileLoops
{
    /// <summary>The bytes of characters every Wrecked Ship treadmill frame shows: two characters.</summary>
    public const int TreadmillFrameByteCount = 32;

    /// <summary>The Tourian statue strip's length in bytes.</summary>
    public const int TourianStatueStripByteCount = TourianStatueTileStripDefinitions.TileCount * TourianStatueTileStripDefinitions.BytesPerTile;

    /// <summary>The loop of an object the atlas holds.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animatedObject"/> is not one of the simple loops.</exception>
    internal static RoomFxAnimatedTileLoop Require(AnimatedTileObject animatedObject) =>
        RoomFxAnimatedTiles.TryResolve(animatedObject, out RoomFxAnimatedTileLoop? loop) ? loop
            : throw new InvalidOperationException($"{animatedObject} has no definition in this catalog.");

    /// <summary>Enumerates the seven supported objects.</summary>
    public static IEnumerable<RoomFxAnimatedTileLoop> All
    {
        get
        {
            yield return Require(AnimatedTileObject.HorizontalSpikes);
            yield return Require(AnimatedTileObject.MaridiaSandCeiling);
            yield return Require(AnimatedTileObject.MaridiaSandFalling);
            yield return Require(AnimatedTileObject.Lava);
            yield return Require(AnimatedTileObject.Acid);
            yield return Require(AnimatedTileObject.Rain);
            yield return Require(AnimatedTileObject.Spores);
        }
    }

    /// <summary>The distinct pictures one loop's frames show.</summary>
    extension(RoomFxAnimatedTileLoop loop)
    {
        /// <summary>The distinct pictures the object's frames show, in order.</summary>
        internal IEnumerable<AnimatedTileImage> Images =>
            Enumerable.Range(0, loop.ImageCount).Select(index => new AnimatedTileImage(loop.AnimatedObject, index));
    }
}
