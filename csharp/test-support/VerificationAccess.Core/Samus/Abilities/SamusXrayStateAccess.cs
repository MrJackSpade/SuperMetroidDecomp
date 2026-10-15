
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Verification access to <see cref="SamusXrayState"/> members production does not use.</summary>
internal static class SamusXrayStateAccess
{
    /// <summary>Detects the reserve-recovery X-Ray state with all subsystems suspended but time unfrozen.</summary>
    extension(SamusXrayState self)
    {
        /// <summary>True after Reserve recovery has stranded X-Ray's subsystem disables.</summary>
        internal bool IsGMode => !self.TimeIsFrozen &&
            self.SuspendedSubsystems == XraySuspendedSubsystems.All;
    }
}
