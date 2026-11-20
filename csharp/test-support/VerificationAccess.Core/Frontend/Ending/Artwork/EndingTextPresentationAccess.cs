using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingTextPresentation"/> members production does not use.</summary>
internal static class EndingTextPresentationAccess
{

    /// <summary>Copies the Japanese subtitle's tiles for verification.</summary>
    extension(EndingTextPresentation.SubtitleSequence self)
    {
        /// <summary>Copies all 64 subtitle tiles in row-major order, leaving any additional destination cells unchanged.</summary>
        /// <param name="destination">Storage for at least two 32-cell tilemap rows.</param>
        /// <exception cref="ArgumentException">The destination has fewer than 64 cells.</exception>
        internal void CopyTo(Span<BackgroundTile> destination)
        {
            if (destination.Length < self.Count)
                throw new ArgumentException("Destination is too short.", nameof(destination));
            for (int cell = 0; cell < self.Count; cell++) destination[cell] = self[cell];
        }
    }
}
