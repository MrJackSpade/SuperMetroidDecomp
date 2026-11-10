using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Verification access to <see cref="SamusShinesparkState"/> members production does not use.</summary>
internal static class SamusShinesparkStateAccess
{
    /// <summary>Counts the crash's shown released echoes.</summary>
    extension(SamusShinesparkState self)
    {
        /// <summary>Number of released echoes shown, each while its shot still carries it.</summary>
        internal int ReleasedCrashEchoCount =>
            (self.FirstReleasedEcho.ShownPosition is null ? 0 : 1) +
            (self.SecondReleasedEcho.ShownPosition is null ? 0 : 1);
    }

    /// <summary>Runs the released crash echoes' shots without the rest of the projectile pass.</summary>
    extension(SamusProjectileSystem self)
    {
        /// <summary>
        /// Runs the echo update of every shot carrying a released crash echo, in the projectile pass's update order,
        /// clearing each shot whose echo leaves the screen.
        /// </summary>
        internal void StepReleasedCrashEchoProjectiles(SamusState samus, Position16<RoomPixels> cameraPosition)
        {
            Ensure.NotNull(samus);
            foreach (SamusProjectileSlot slot in self.SlotTable.InUpdateOrder)
            {
                if (slot.FrameBehavior == SamusProjectileFrameBehavior.ShinesparkEcho)
                    self.Shinespark.StepShinesparkEcho(samus, slot, cameraPosition);
            }
        }
    }
}
