using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingTextPresentation"/> members production does not use.</summary>
internal static class EndingTextPresentationAccess2
{
    /// <summary>Reads the typewriter text an extracted ending-text document installs, as the game reads it.</summary>
    extension(EndingTextPresentation)
    {
        /// <summary>
        /// The characters the ending's typewriter types from the extracted ending-text document <paramref name="artwork"/>:
        /// the typewriter text the extraction installs from it, read as the game reads it (#1467, #1638).
        /// </summary>
        /// <exception cref="InvalidDataException">The document, or the typewriter text it installs, does not decode.</exception>
        public static EndingTypewriterText TypewriterOf(byte[] artwork) =>
            EndingTextPresentation.ReadTypewriter(new MemoryStream(EndingTextPresentation.ExtractTypewriter(artwork), writable: false));
    }
}
