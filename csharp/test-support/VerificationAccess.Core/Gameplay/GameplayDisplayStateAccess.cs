using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Verification access to <see cref="GameplayDisplayState"/> members production does not use.</summary>
internal static class GameplayDisplayStateAccess
{
    /// <summary>Exposes <see cref="GameplayDisplayState"/> state that only verification reads.</summary>
    extension(GameplayDisplayState self)
    {
        /// <summary>The room's frame the most recent frame boundary latched.</summary>
        /// <exception cref="InvalidOperationException">No frame boundary has latched a room's frame.</exception>
        public RoomFrameLatch LatchedRoomFrame => self.Latched as RoomFrameLatch
            ?? throw new InvalidOperationException($"The latest frame boundary latched {self.Latched.GetType().Name}, not a room's frame.");

        /// <summary>The ordinary room frame the most recent frame boundary latched.</summary>
        /// <exception cref="InvalidOperationException">The latest frame boundary latched another kind of frame.</exception>
        public OrdinaryRoomFrameLatch LatchedOrdinaryRoom => self.Latched as OrdinaryRoomFrameLatch
            ?? throw new InvalidOperationException($"The latest frame boundary latched {self.Latched.GetType().Name}, not an ordinary room's frame.");

        /// <summary>The perspective floor's frame the most recent frame boundary latched.</summary>
        /// <exception cref="InvalidOperationException">The latest frame boundary latched another kind of frame.</exception>
        public PerspectiveFloorFrameLatch LatchedPerspectiveFloor => self.Latched as PerspectiveFloorFrameLatch
            ?? throw new InvalidOperationException($"The latest frame boundary latched {self.Latched.GetType().Name}, not a perspective floor's frame.");

        /// <summary>
        /// The Ceres perspective floor the most recent accepted frame shows, or null for another kind of frame. It is latched
        /// with <see cref="SuperMetroidRuntime.DisplayedSprites"/>: rendering uses both together rather than mixing the
        /// main loop's current transform with the preceding frame's sprites.
        /// </summary>
        public PerspectiveFloor? DisplayedPerspectiveFloor => (self.Latched as PerspectiveFloorFrameLatch)?.Floor;
        /// <summary>
        /// Ordinary BG scroll registers and room-shake deltas published by the most recent
        /// accepted NMI. These belong to the same PPU-visible frame as <see cref="SuperMetroidRuntime.DisplayedSprites"/>.
        /// </summary>
        public GameplayRenderSnapshot DisplayedGameplayScrolls => self.LatchedRoomFrame.Scrolls;
        /// <summary>Room-FX BG3 state published by the most recent accepted NMI.</summary>
        public RoomFxOverlayRenderSnapshot? DisplayedRoomFxOverlay => self.LatchedRoomFrame.RoomFxOverlay;
    }
}
