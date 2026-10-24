using SuperMetroid.AssetExtraction.Frontend.Ending;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Verification access to the characters the installed ending floors were read with (#1409).</summary>
internal static class EndingFloorArtworkAccess
{
    /// <summary>Reads one installed backdrop's characters.</summary>
    extension(EndingFloorSceneArtwork self)
    {
        /// <summary>The $4000 chunky pixel bytes for 256 ordered 8x8 tiles, written to the high VRAM byte lane.</summary>
        internal ReadOnlyMemory<byte> Characters => EndingFloorSceneCharacters.Of(self).Characters;
    }

    /// <summary>Reads the installed reward icon's transfer.</summary>
    extension(EndingRewardIconArtwork self)
    {
        /// <summary>The $8000-byte low-map/high-character word stream the sixteen reward uploads consume.</summary>
        internal ReadOnlyMemory<byte> Bytes => EndingRewardIconCharacters.Of(self).Bytes;
    }
}
