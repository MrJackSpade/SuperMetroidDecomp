using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Verification access: the logo crossfade read at the installed resource's colour positions (#1464), the native
/// boundary at which checks compare it with the cartridge's crossfade words. Production reads it by step and layer.
/// </summary>
internal static class EndingLogoPaletteFadeAccess
{
    /// <summary>A calculated fade's colours by resource position.</summary>
    extension(EndingLogoPaletteFade self)
    {
        /// <summary>The colour at resource position <paramref name="index"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the crossfade.</exception>
        internal RgbColor Color(int index) => EndingLogoCrossfadeImage.At(index, self.Color);
    }

    /// <summary>An installed crossfade palette's colours by resource position.</summary>
    extension(EndingPalette self)
    {
        /// <summary>The logo crossfade's colour at resource position <paramref name="index"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the crossfade.</exception>
        internal RgbColor CrossfadeColorAt(int index) => EndingLogoCrossfadeImage.At(index, self.LogoCrossfadeColour);
    }
}
