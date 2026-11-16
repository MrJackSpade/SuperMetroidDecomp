using SuperMetroid.Core.Samus.Movement;
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

        /// <summary>
        /// Runs one spark step on a frame whose gameplay clock is at its second's first frame and whose camera point is
        /// where Samus stands, then commits a timed-out windup's launch pose as the frame's pose update does when nothing
        /// outranks it.
        /// </summary>
        internal ShinesparkMovementResult StepAndCommit(SamusTerrain terrain, SamusState samus, int frameCounter,
            SamusProjectileSystem projectiles, bool samusCannotDie = false)
        {
            Ensure.NotNull(samus);
            ShinesparkMovementResult step = self.Step(terrain, samus, new ShinesparkFrame
            {
                FrameCounter = frameCounter,
                SamusCannotDie = samusCannotDie,
                Projectiles = projectiles,
                GameTimeFrames = 0,
                PreviousCheckpoint = samus.Position,
            });
            if (step.PendingLaunchPose is { } launchPose)
                SamusShinesparkState.ApplyDirectionalLaunchPose(samus, launchPose);
            return step;
        }
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
            foreach (SamusProjectile slot in self.Pool.InUpdateOrder)
            {
                if (slot.Behavior == SamusShotBehaviors.ShinesparkEcho)
                    self.Shinespark.StepShinesparkEcho(samus, slot, cameraPosition);
            }
        }
    }
}
