using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Verification access to <see cref="HudTileAtlas"/> members production does not use.</summary>
internal static class HudTileAtlasAccess
{
    /// <summary>Exposes <see cref="HudTileAtlas"/> state that only verification reads.</summary>
    extension(HudTileAtlas self)
    {
        /// <summary>Writes all $2000 bytes at the standard BG3 character base: the characters followed by the zero-filled BG2 tilemap-clearing half.</summary>
        /// <param name="vram">Destination VRAM whose existing bytes are overwritten.</param>
        public void LoadTo(SnesVram vram) => new VramRegion(vram, GameplayVramLayoutConstants.HudCharacterUpload).Load(self.bytes);
    }
}
