using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Verification access to <see cref="RoomCharacterAtlas"/> members production does not use.</summary>
internal static class RoomCharacterAtlasAccess
{
    /// <summary>Exposes <see cref="RoomCharacterAtlas"/> state that only verification reads.</summary>
    extension(RoomCharacterAtlas self)
    {
        /// <summary>Uploads the compiled characters to a fixed catalog destination.</summary>
        /// <param name="destination">VRAM region receiving the complete character stream.</param>
        public void LoadTo(VramRegion destination) => destination.Load(self.planar);
    }
}
