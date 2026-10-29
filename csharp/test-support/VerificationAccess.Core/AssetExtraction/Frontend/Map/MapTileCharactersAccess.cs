using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Verification access to <see cref="MapTileCharacters"/> members production does not use.</summary>
internal static class MapTileCharactersAccess
{
    /// <summary>Exposes <see cref="MapTileCharacters"/> state that only verification reads.</summary>
    extension(MapTileCharacters self)
    {
        /// <summary>Copies compiled characters to the screen's own VRAM; source storage cannot be mutated by callers.</summary>
        /// <param name="destination">The screen's character region receiving the full 8192-byte atlas.</param>
        public void LoadTo(VramRegion destination) => destination.Load(self.Characters);
    }
}
