using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectsAndPostureVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusAerialAndLiquidsVerification;
using static SuperMetroid.Verification.Samus.SamusAimVerification;
using static SuperMetroid.Verification.Samus.Grapple.SamusGrappleVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusMorphBallVerification;
using static SuperMetroid.Verification.Samus.SamusMotionCoreVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusTerrainMovementVerification;
using static SuperMetroid.Verification.Samus.SamusTurnsAndKnockbackVerification;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;
/// <summary>Groups focused Samus movement checks while collecting every
/// failure rather than stopping at the first fixture.</summary>
internal static class SamusPhysicsBatchVerification
{
    /// <summary>Runs independent physics fixtures together while retaining every failure and a nonzero exit.</summary>
    internal static void VerifySamusPhysicsBatch()
    {
        (string Name, Action Check)[] checks =
        [
            (nameof(VerifySamusHorizontalSpeed), VerifySamusHorizontalSpeed),
            (nameof(VerifySamusExtraDisplacement), VerifySamusExtraDisplacement),
            (nameof(VerifySamusAerialMovement), VerifySamusAerialMovement),
            (nameof(VerifySamusSpaceJumpAndScrewAttack), VerifySamusSpaceJumpAndScrewAttack),
            (nameof(VerifySamusLiquidPhysics), VerifySamusLiquidPhysics),
            (nameof(VerifySamusAerialTurnsAndWallJump), VerifySamusAerialTurnsAndWallJump),
            (nameof(VerifySamusKnockbackAndDamageBoost), VerifySamusKnockbackAndDamageBoost),
            (nameof(VerifySamusGrappleSwingAndRelease), VerifySamusGrappleSwingAndRelease),
            (nameof(VerifySamusPostureMovement), VerifySamusPostureMovement),
            (nameof(VerifySamusMorphBallMovement), VerifySamusMorphBallMovement),
            (nameof(VerifySamusStandingAimMovement), VerifySamusStandingAimMovement),
            (nameof(VerifySamusAimedAerialMovement), VerifySamusAimedAerialMovement),
            (nameof(VerifySamusGunExtendedMovement), VerifySamusGunExtendedMovement),
            (nameof(VerifySamusSlopePhysics), VerifySamusSlopePhysics),
            (nameof(VerifySamusGroundedMovement), VerifySamusGroundedMovement),
            (nameof(VerifySamusGroundedReversal), VerifySamusGroundedReversal),
            (nameof(VerifySamusMoonwalking), VerifySamusMoonwalking),
            (nameof(VerifySamusRanIntoWall), VerifySamusRanIntoWall),
        ];
        var failures = new List<Exception>();
        foreach ((string name, Action check) in checks)
        {
            try { check(); }
            catch (Exception error)
            {
                Console.Error.WriteLine($"FAILED {name}: {error}");
                failures.Add(error);
            }
        }
        if (failures.Count != 0)
            throw new AggregateException($"Samus physics: {failures.Count}/{checks.Length} fixtures failed.", failures);
        Console.WriteLine($"Samus physics: all {checks.Length} fixtures passed.");
    }

    /// <summary>Runs the grouped Samus physics fixtures from the verification CLI.</summary>
    [VerificationCommand("--samus-physics")]
    internal static int SamusPhysicsCommand()
    {
        Suite(nameof(VerifySamusPhysicsBatch), () => VerifySamusPhysicsBatch());
        return 0;
    }
}
