using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Verification access to <see cref="InstalledSamusVisorColors"/> members production does not use.</summary>
internal static class InstalledSamusVisorColorsAccess
{
    /// <summary>Exposes <see cref="InstalledSamusVisorColors"/> state that only verification reads.</summary>
    extension(InstalledSamusVisorColors self)
    {
        /// <summary>Returns a stored visor colour, in the file's order: the X-ray widening glow's, then the steady cycle's.</summary>
        /// <param name="index">The colour's place in the file, 0..5.</param>
        /// <returns>The installed shade.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The color ordinal is outside 0..5.</exception>
        public RgbColor Resolve(int index)
        {
            if ((uint)index >= self.colors.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return self.colors[index];
        }
    }
}
