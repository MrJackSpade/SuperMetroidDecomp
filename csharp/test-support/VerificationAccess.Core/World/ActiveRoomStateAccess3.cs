using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.World;

/// <summary>Verification access to <see cref="ActiveRoomState"/> members production does not use.</summary>
internal static class ActiveRoomStateAccess3
{
    /// <summary>Exposes <see cref="ActiveRoomState"/> state that only verification reads.</summary>
    extension(ActiveRoomState self)
    {
        /// <summary>The loaded room's Ceres elevator arrival; null while a load builds the next room's scope.</summary>
        public CeresElevatorArrivalState? CeresElevatorArrival => self.Construction is null ? self.Scope?.CeresElevatorArrival : null;
    }
}
