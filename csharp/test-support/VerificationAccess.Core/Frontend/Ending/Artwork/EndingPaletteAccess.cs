
namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingPalette"/> members production does not use.</summary>
internal static class EndingPaletteAccess
{
    /// <summary>Exposes <see cref="EndingPalette"/> state that only verification reads.</summary>
    extension(EndingPalette self)
    {
        /// <summary>Number of colours in the complete resource, including all 512 ordered colours of the sixteen-step logo crossfade.</summary>
        public int ColorCount => self.colours.Length;
    }
}
