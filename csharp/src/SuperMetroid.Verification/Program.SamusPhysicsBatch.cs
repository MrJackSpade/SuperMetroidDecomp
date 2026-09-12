internal static partial class Program
{
    /// <summary>Runs independent physics fixtures together while retaining every failure and a nonzero exit.</summary>
    private static void VerifySamusPhysicsBatch()
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
}
