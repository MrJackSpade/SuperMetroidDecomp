using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.World;

/// <summary>Verification access to <see cref="ActiveRoomState"/> members production does not use.</summary>
internal static class ActiveRoomStateAccess2
{
    /// <summary>Exposes <see cref="ActiveRoomState"/> state that only verification reads.</summary>
    extension(ActiveRoomState self)
    {
        /// <summary>The loading or loaded room's effect-overlay FX, for the accepted-frame publication.</summary>
        internal RoomFxOverlayState? FxOverlay => self.View(c => c.FxOverlay, s => s.FxOverlay);
    }
}
