namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxOverlayState"/> members production does not use.</summary>
internal static class RoomFxOverlayStateAccess2
{
    /// <summary>Exposes <see cref="RoomFxOverlayState"/> state that only verification reads.</summary>
    extension(RoomFxOverlayState self)
    {
        /// <summary>The effect overlay the room shows (#1348, #1466, #1601): its effect's liquid or atmosphere when it has one, otherwise nothing.</summary>
        public FxOverlayPlane EffectPlane => FxOverlayPlane.Of(self.OverlayEffect);
    }
}
