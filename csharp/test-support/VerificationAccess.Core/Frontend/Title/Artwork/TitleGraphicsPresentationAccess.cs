using SuperMetroid.AssetExtraction.Frontend.Title;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Verification access to <see cref="TitleGraphicsCharacters"/> as the cartridge's title would load them.</summary>
internal static class TitleGraphicsPresentationAccess
{
    /// <summary>Exposes the characters the cartridge's title loads.</summary>
    extension(TitleGraphicsCharacters self)
    {
        /// <summary>Eight-bit Mode-7 character lane uploaded for the title backdrop.</summary>
        public ReadOnlySpan<byte> FloorCharacters => self.FloorCharacters;

        /// <summary>Four-bit title OBJ character graphics selected by sprite compositions.</summary>
        public ReadOnlySpan<byte> ObjectCharacters => self.ObjectPage.Characters;

        /// <summary>Separate eight-bit Mode-7 character page for the baby Metroid scene.</summary>
        public ReadOnlySpan<byte> BabyCharacters => self.BabyCharacters;
    }
}
