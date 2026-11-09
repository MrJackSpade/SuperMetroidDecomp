using SuperMetroid.AssetExtraction.Frontend.Map;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Verification access to <see cref="WorldMapCharacters"/> as the cartridge's world map would load them.</summary>
internal static class WorldMapArtworkAccess
{
    /// <summary>Loads the characters where the cartridge's world map holds them.</summary>
    extension(WorldMapCharacters self)
    {
        /// <summary>Reconstructs both selected atlases, encodes their row-major 8x8 characters, and loads the foreground 4-bpp and background 2-bpp streams at their fixed VRAM byte offsets.</summary>
        /// <param name="vram">Destination VRAM receiving both character transfers; this method does not install palette colors or world-map layout words.</param>
        public void LoadTo(SnesVram vram)
        {
            new WorldMapVram(vram).Foreground.Load(self.ForegroundCharacters());
            new WorldMapVram(vram).Background.Load(self.BackgroundCharacters());
        }
    }
}
