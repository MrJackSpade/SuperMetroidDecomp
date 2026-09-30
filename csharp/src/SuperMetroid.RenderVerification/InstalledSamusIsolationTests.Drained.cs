using SuperMetroid.Core.Game;

internal sealed partial class InstalledSamusIsolationTests
{
    /// <summary>
    /// The drained draw offsets have their own byte-indexed table. Exercise their
    /// real animation-triggered falling owner rather than assigning a plausible pose.
    /// </summary>
    private void CheckDrained(bool left)
    {
        Pair pair = Create(left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose);
        string context = $"drained left={left}";
        pair.Apply(context + " airborne setup", actor =>
        {
            actor.Samus.YPosition -= 80;
            SamusAerialMovement.ConfigureEnvironmentGravity(actor.Memory, actor.Samus);
            actor.Samus.Drained.LetFall(actor.Memory, actor.Samus);
        });
        bool fell = false;
        int frames = 0;
        while (pair.Stock.Samus.Drained.Phase != DrainedSamusPhase.OnFloor && frames < 180)
        {
            ushort frame = (ushort)frames++;
            pair.Apply(context + $" fall {frame}", actor =>
            {
                if (actor.Samus.Drained.Phase == DrainedSamusPhase.Falling)
                    actor.Samus.Drained.StepFalling(actor.Memory, actor.Level, actor.Samus, frame);
                Animate(actor, frame);
                actor.Samus.Drained.UpdatePalette(actor.Memory, actor.Colors, actor.Samus.EquippedItems, actor.Suits);
            });
            fell |= pair.Stock.Samus.Drained.Phase == DrainedSamusPhase.Falling;
        }
        Require(fell && pair.Stock.Samus.Drained.Phase == DrainedSamusPhase.OnFloor,
            context + ": animation command must install falling and terrain collision must end it");
        pair.Apply(context + " stand", actor => actor.Samus.Drained.PutStanding(actor.Memory, actor.Samus));
        for (ushort frame = 0; frame < 24; frame++)
            pair.Apply(context + $" standing {frame}", actor => Animate(actor, frame));
        pair.Apply(context + " crouch", actor => actor.Samus.Drained.PutCrouchingOrFalling(actor.Memory, actor.Samus));
        for (ushort frame = 0; frame < 24; frame++)
            pair.Apply(context + $" crouching {frame}", actor => Animate(actor, frame));
        pair.Apply(context + " release", actor => actor.Samus.Drained.Release(actor.Memory, actor.Samus));
        CompleteAnimation(pair, left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose,
            context + " released");
        Require(pair.Stock.Samus.Drained.Phase == DrainedSamusPhase.Inactive,
            context + ": compiled terminal animation must release the drained owner");
        Console.WriteLine($"  {context}: {frames} matching fall calls plus stand/crouch/release animation; edited draw offsets stay cosmetic.");
    }
}
