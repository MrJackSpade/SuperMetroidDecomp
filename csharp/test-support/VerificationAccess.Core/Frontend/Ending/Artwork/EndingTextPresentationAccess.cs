using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingTextPresentation"/> members production does not use.</summary>
internal static class EndingTextPresentationAccess
{
    /// <summary>Exposes <see cref="EndingTextPresentation"/> state that only verification reads.</summary>
    extension(EndingTextPresentation self)
    {
        /// <summary>Gets the precompiled glyph placements for the item-percentage labels ($8C:DFDB) or final message ($8C:E0AF); dynamic percentage digits and cinematic timing are supplied by the frontend.</summary>
        /// <param name="sequence">The bounded ending typewriter program whose configured characters are requested.</param>
        /// <returns>Characters in drawing order, with their cells, styles and characters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The selector is not a supported ending text sequence.</exception>
        public ReadOnlySpan<EndingTextCharacter> Compile(EndingTextSequence sequence) => sequence switch
        {
            EndingTextSequence.ItemPercentage => self.percentage,
            EndingTextSequence.FinalMessage => self.finalMessage,
            _ => throw new ArgumentOutOfRangeException(nameof(sequence), sequence, null),
        };
    }

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
