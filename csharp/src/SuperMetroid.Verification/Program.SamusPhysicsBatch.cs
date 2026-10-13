using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using static PlmsAndPostureVerification;
using static SamusAerialAndLiquidsVerification;
using static SamusAimVerification;
using static SamusGrappleVerification;
using static SamusMorphBallVerification;
using static SamusMotionCoreVerification;
using static SamusTerrainMovementVerification;
using static SamusTurnsAndKnockbackVerification;
/// <summary>Groups focused Samus movement checks while collecting every
/// failure rather than stopping at the first fixture.</summary>
internal static class SamusPhysicsBatchVerification
{
    /// <summary>Runs independent physics fixtures together while retaining every failure and a nonzero exit.</summary>
    internal static void VerifySamusPhysicsBatch()
    {
        Action[] checks =
        [
            VerifySamusHorizontalSpeed, VerifySamusExtraDisplacement, VerifySamusAerialMovement,
            VerifySamusSpaceJumpAndScrewAttack, VerifySamusLiquidPhysics, VerifySamusAerialTurnsAndWallJump,
            VerifySamusKnockbackAndDamageBoost, VerifySamusGrappleSwingAndRelease, VerifySamusPostureMovement,
            VerifySamusMorphBallMovement, VerifySamusStandingAimMovement, VerifySamusAimedAerialMovement,
            VerifySamusGunExtendedMovement, VerifySamusSlopePhysics, VerifySamusGroundedMovement,
            VerifySamusGroundedReversal, VerifySamusMoonwalking, VerifySamusRanIntoWall,
        ];
        var failures = new List<Exception>();
        foreach (Action check in checks)
        {
            try { check(); }
            catch (Exception error)
            {
                Console.Error.WriteLine($"FAILED {check.Method.Name}: {error}");
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
