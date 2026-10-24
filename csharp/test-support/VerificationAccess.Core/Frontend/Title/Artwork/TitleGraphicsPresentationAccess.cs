using System.Text.Json;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Verification access to <see cref="TitleGraphicsPresentation"/> members production does not use.</summary>
internal static class TitleGraphicsPresentationAccess
{
    /// <summary>Exposes <see cref="TitleGraphicsPresentation"/> state that only verification reads.</summary>
    extension(TitleGraphicsPresentation self)
    {
        /// <summary>Four-bit title OBJ character graphics selected by sprite compositions.</summary>
        public ReadOnlySpan<byte> ObjectCharacters => self.objectCharacters;
    }
}
