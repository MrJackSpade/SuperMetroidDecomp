using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Map.Artwork;

/// <summary>Verification access to <see cref="MapTileAtlas"/> members production does not use.</summary>
internal static class MapTileAtlasAccess
{
    /// <summary>Exposes <see cref="MapTileAtlas"/> state that only verification reads.</summary>
    extension(MapTileAtlas self)
    {
        /// <summary>Copies compiled characters to the screen's own VRAM; source storage cannot be mutated by callers.</summary>
        /// <param name="destination">The screen's character region receiving the full 8192-byte atlas.</param>
        public void LoadTo(VramRegion destination) => destination.Load(self.planar);
    }
}
