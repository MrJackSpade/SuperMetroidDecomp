using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Verification access to <see cref="ElevatorBehavior"/> members production does not use.</summary>
internal static class ElevatorBehaviorAccess
{
    /// <summary>Exposes <see cref="ElevatorBehavior"/> state that only verification reads.</summary>
    extension(ElevatorBehavior self)
    {
        /// <summary>
        /// Debugger/test seam for the already-public runtime WRAM word. Production changes are
        /// still made by collision, actor AI, and room loading rather than arbitrary callers.
        /// </summary>
        public void SetElevatorStatusForDebugging(ushort status)
        {
            if (status > (ushort)ElevatorActorStatus.ReturningToRest)
                throw new ArgumentOutOfRangeException(nameof(status));
            self.ElevatorStatus = (ElevatorActorStatus)status;
            if (self.ElevatorStatus == ElevatorActorStatus.Inactive)
                self.ElevatorFlags = 0;
        }
    }
}
