using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Verification access to <see cref="GameplayDisplayState"/> members production does not use.</summary>
internal static class GameplayDisplayStateAccess
{
    /// <summary>Exposes <see cref="GameplayDisplayState"/> state that only verification reads.</summary>
    extension(GameplayDisplayState self)
    {
        /// <summary>
        /// Ceres Mode 7 matrix made PPU-visible by the most recent accepted NMI. This is the
        /// register-phase companion to <see cref="SuperMetroidRuntime.DisplayedOam"/>: software rendering must use
        /// both displayed values together rather than mixing current main-loop shadow words
        /// with the preceding NMI's sprites.
        /// </summary>
        public SamusMode7Transform? DisplayedSamusMode7Transform => self.Latched.SamusMode7Transform;
        /// <summary>
        /// Ordinary BG scroll registers and room-shake deltas published by the most recent
        /// accepted NMI. These belong to the same PPU-visible frame as <see cref="SuperMetroidRuntime.DisplayedOam"/>.
        /// </summary>
        public GameplayPpuRenderSnapshot DisplayedGameplayPpu => self.Latched.Ppu;
        /// <summary>Room-FX BG3 state published by the most recent accepted NMI.</summary>
        public RoomLayer3FxRenderSnapshot? DisplayedRoomLayer3Fx => self.Latched.RoomLayer3Fx;
    }
}
