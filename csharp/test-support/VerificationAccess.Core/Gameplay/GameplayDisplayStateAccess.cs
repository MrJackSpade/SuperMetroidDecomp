using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Verification access to <see cref="GameplayDisplayState"/> members production does not use.</summary>
internal static class GameplayDisplayStateAccess
{
    /// <summary>Exposes <see cref="GameplayDisplayState"/> state that only verification reads.</summary>
    extension(GameplayDisplayState self)
    {
        /// <summary>
        /// The Ceres perspective floor the most recent accepted frame shows. It is latched with
        /// <see cref="SuperMetroidRuntime.DisplayedSprites"/>: rendering uses both together rather than mixing the
        /// main loop's current transform with the preceding frame's sprites.
        /// </summary>
        public PerspectiveFloor? DisplayedPerspectiveFloor => self.Latched.PerspectiveFloor;
        /// <summary>
        /// Ordinary BG scroll registers and room-shake deltas published by the most recent
        /// accepted NMI. These belong to the same PPU-visible frame as <see cref="SuperMetroidRuntime.DisplayedSprites"/>.
        /// </summary>
        public GameplayPpuRenderSnapshot DisplayedGameplayPpu => self.Latched.Ppu;
        /// <summary>Room-FX BG3 state published by the most recent accepted NMI.</summary>
        public RoomLayer3FxRenderSnapshot? DisplayedRoomLayer3Fx => self.Latched.RoomLayer3Fx;
    }
}
