using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Title;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Verification access to <see cref="TitleGraphicsPresentation"/> members production does not use.</summary>
internal static class TitleGraphicsPresentationAccess
{
    /// <summary>Exposes <see cref="TitleGraphicsPresentation"/> state that only verification reads.</summary>
    extension(TitleGraphicsPresentation self)
    {
        /// <summary>Eight-bit Mode-7 character lane uploaded for the title backdrop.</summary>
        public ReadOnlySpan<byte> FloorCharacters => TitleGraphicsCharacters.Of(self).FloorCharacters;

        /// <summary>Four-bit title OBJ character graphics selected by sprite compositions.</summary>
        public ReadOnlySpan<byte> ObjectCharacters => TitleGraphicsCharacters.Of(self).ObjectCharacters;

        /// <summary>Separate eight-bit Mode-7 character page for the baby Metroid scene.</summary>
        public ReadOnlySpan<byte> BabyCharacters => TitleGraphicsCharacters.Of(self).BabyCharacters;
    }
}
