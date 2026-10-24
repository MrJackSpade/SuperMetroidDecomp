using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Verification access to <see cref="WorldMapArtwork"/> members production does not use.</summary>
internal static class WorldMapArtworkAccess
{
    /// <summary>Exposes <see cref="WorldMapArtwork"/> state that only verification reads.</summary>
    extension(WorldMapArtwork self)
    {
        /// <summary>Reconstructs both selected atlases, encodes their row-major 8x8 characters, and loads the foreground 4-bpp and background 2-bpp streams at their fixed VRAM byte offsets.</summary>
        /// <param name="vram">Destination VRAM receiving both character transfers; this method does not install palette colors or world-map layout words.</param>
        public void LoadTo(SnesVram vram)
        {
            WorldMapCharacters characters = WorldMapCharacters.Of(self);
            new WorldMapVram(vram).Foreground.Load(characters.ForegroundCharacters());
            new WorldMapVram(vram).Background.Load(characters.BackgroundCharacters());
        }
    }
}
