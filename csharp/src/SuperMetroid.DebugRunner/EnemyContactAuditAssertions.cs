using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks the native damage-publication / later standing-air knockback boundary.</summary>
internal static class EnemyContactAuditAssertions
{
    internal readonly record struct BeforeHit(ushort Health, byte Pose, ushort X, ushort SubX, ushort Y, ushort SubY);

    public static BeforeHit Capture(SamusState samus) => new(samus.Health, samus.Pose,
        samus.XPosition, samus.Kinematics.XSubposition, samus.YPosition, samus.Kinematics.YSubposition);

    public static void VerifyStandingAirHit(ISnesAddressSpace bus, SamusState samus,
        BeforeHit before, ushort damage, ushort expectedSide, string context)
    {
        void Fail(string phase) => throw new InvalidDataException($"{context}: {phase}; health={samus.Health}, pose={samus.Pose:X2}, active={samus.KnockbackActive}, timer={samus.KnockbackTimer}, side={samus.KnockbackXDirection}, direction={samus.KnockbackDirection}.");
        bool SamePosition() => samus.XPosition == before.X && samus.Kinematics.XSubposition == before.SubX &&
            samus.YPosition == before.Y && samus.Kinematics.YSubposition == before.SubY;
        ushort expectedHealth = checked((ushort)(before.Health - damage));
        if (before.Pose != SamusPoseIds.FacingRightNormalPose || expectedSide > 1)
            Fail("fixture is not the explicitly supported right-facing standing case");

        // $A0:A4A1 body touch and $A0:9923 projectile touch publish the same
        // timer/side request. Neither routine installs a pose or moves Samus.
        if (samus.Health != expectedHealth || samus.InvincibilityTimer != 96 ||
            samus.KnockbackActive || samus.KnockbackTimer != 5 ||
            samus.KnockbackXDirection != expectedSide || samus.KnockbackDirection != 0 ||
            samus.Pose != before.Pose || !SamePosition())
            Fail("native contact publication differs");
        if (SamusKnockbackMovement.TryStartPendingHitInterruption(bus, samus, 0, timeIsFrozen: true) ||
            samus.KnockbackActive || samus.Pose != before.Pose || samus.KnockbackTimer != 5 || !SamePosition())
            Fail("frozen time admitted or mutated pending movement");

        // $90:DDE9 / $91:ED4E admit exactly once. With no direction held, a
        // standing air body starts upward at 5.0000; source side chooses X only.
        if (!SamusKnockbackMovement.TryStartPendingHitInterruption(bus, samus, 0, timeIsFrozen: false) ||
            !samus.KnockbackActive || samus.Pose != SamusPoseIds.KnockbackRightPose ||
            samus.KnockbackDirection != expectedSide + 1 || samus.HurtFlashCounter != 1 ||
            samus.Kinematics.YSpeed != 5 || samus.Kinematics.YSubspeed != 0 || samus.Kinematics.YDirection != 1 ||
            samus.Health != expectedHealth || !SamePosition() ||
            SamusKnockbackMovement.TryStartPendingHitInterruption(bus, samus, 0, timeIsFrozen: false))
            Fail("single later knockback admission differs");
    }
}
