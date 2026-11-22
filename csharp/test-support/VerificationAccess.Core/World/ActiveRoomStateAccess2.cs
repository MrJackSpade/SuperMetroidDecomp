using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.World;

/// <summary>Verification access to <see cref="ActiveRoomState"/> members production does not use.</summary>
internal static class ActiveRoomStateAccess2
{
    /// <summary>Exposes <see cref="ActiveRoomState"/> state that only verification reads.</summary>
    extension(ActiveRoomState self)
    {
<<<<<<< HEAD
        /// <summary>
        /// The loaded room's effect-overlay FX, for the accepted-frame publication; none while the room loads, whose
        /// construction holds no overlay until it completes (#2554).
        /// </summary>
        internal RoomFxOverlayState? FxOverlay => self.View(_ => (RoomFxOverlayState?)null, s => s.FxOverlay);
||||||| 418ca79935
        /// <summary>The loading or loaded room's effect-overlay FX, for the accepted-frame publication.</summary>
        internal RoomFxOverlayState? FxOverlay => self.View(c => global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.World.RoomScopeConstruction, global::SuperMetroid.Core.Fx.RoomFxOverlayState?>(c, "FxOverlay"), s => s.FxOverlay);
=======
        /// <summary>The loading or loaded room's effect-overlay FX, for the accepted-frame publication.</summary>
        internal RoomFxOverlayState? FxOverlay => self.View(c => c.FxOverlay, s => s.FxOverlay);
>>>>>>> origin/main
    }
}
