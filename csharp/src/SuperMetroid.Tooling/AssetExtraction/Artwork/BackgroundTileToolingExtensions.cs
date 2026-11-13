using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Development-tool instance members of <see cref="BackgroundTile"/>.</summary>
internal static class BackgroundTileToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="BackgroundTile"/>.</summary>
    extension(BackgroundTile self)
    {
        /// <summary>
        /// The same placement with the mirrorings <paramref name="flip"/> names reversed: a 16x16 block mirrored on an axis
        /// mirrors each of its four tiles on that axis as well as swapping their places.
        /// </summary>
        internal BackgroundTile WithFlipsToggled(PictureFlip flip) => self with
        {
            FlipHorizontally = self.FlipHorizontally ^ ((flip & PictureFlip.Horizontal) != PictureFlip.None),
            FlipVertically = self.FlipVertically ^ ((flip & PictureFlip.Vertical) != PictureFlip.None),
        };
    }
}
