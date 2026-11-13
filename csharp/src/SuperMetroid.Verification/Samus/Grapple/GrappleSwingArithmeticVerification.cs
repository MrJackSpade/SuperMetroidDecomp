using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>
/// Checks the grapple swing's arithmetic (#1632) against the shifts it replaced: the quadrant gravity and velocity
/// correction for every angle, in and out of water, with the angular velocities around the hang's stop, and the bounce's
/// reversed half speed for every velocity. The release's lead angle and the anchor's bias and block are covered by
/// <see cref="GrappleCompiledMathVerification.VerifyCompiledGrappleMath"/>, which compares every release velocity and
/// angle and every radial point against the cartridge's trigonometry.
/// </summary>
internal static class GrappleSwingArithmeticVerification
{
    /// <summary>Compares the quadrant gravity and the bounce's reversed half speed with the replaced shifts.</summary>
    internal static void VerifyGrappleSwingArithmetic()
    {
        int[] velocities =
        [
            -GrappleBeamDefinitions.Physics.MaximumAngularVelocity, -257, -256, -255, -1, 0, 1, 255, 256, 257,
            GrappleBeamDefinitions.Physics.MaximumAngularVelocity,
        ];
        int gravityCompared = 0, gravityMismatches = 0;
        string? firstGravityMismatch = null;
        foreach (bool submerged in new[] { false, true })
        {
            foreach (int velocity in velocities)
            {
                for (int raw = 0; raw <= ushort.MaxValue; raw++)
                {
                    RotationAngle angle = RotationAngle.FromRaw(raw);
                    var expected = ReplacedGravity(angle, submerged, velocity);
                    var grapple = new SamusGrappleState { Angle = angle, Submerged = submerged, AngularVelocity = velocity };
                    SamusGrappleSwing.CalculateGravity(grapple);
                    gravityCompared++;
                    if ((grapple.GravityAcceleration, grapple.VelocityCorrection, grapple.AngularVelocity) != expected)
                    {
                        gravityMismatches++;
                        firstGravityMismatch ??= $"angle {raw}, submerged {submerged}, velocity {velocity}: expected {expected}, " +
                            $"got ({grapple.GravityAcceleration}, {grapple.VelocityCorrection}, {grapple.AngularVelocity})";
                    }
                }
            }
        }

        AssertEqual(0, gravityMismatches, $"swing gravity against the replaced shifts; first: {firstGravityMismatch}");

        int bounceMismatches = 0;
        string? firstBounceMismatch = null;
        for (int velocity = -ushort.MaxValue - 1; velocity <= ushort.MaxValue + 1; velocity++)
        {
            int expected = -(velocity >> 1);
            int actual = SamusGrappleSwing.ReversedAtHalfSpeed(velocity);
            if (actual != expected)
            {
                bounceMismatches++;
                firstBounceMismatch ??= $"velocity {velocity}: expected {expected}, got {actual}";
            }
        }

        AssertEqual(0, bounceMismatches, $"bounce reversed half speed against the replaced shift; first: {firstBounceMismatch}");
        Console.WriteLine($"Grapple swing arithmetic: {gravityCompared} gravity cases and every bounce velocity give the replaced shifts' results.");
    }

    /// <summary>The quadrant gravity, correction and hang stop as the replaced shifts computed them.</summary>
    private static (int Gravity, int Correction, int Velocity) ReplacedGravity(RotationAngle angle, bool submerged, int velocity)
    {
        const int gravity = GrappleBeamDefinitions.Physics.GravityMagnitude;
        const int correction = GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude;
        if (angle >= RotationAngle.ThreeQuarterTurn)
            return (submerged ? -(gravity >> 3) : -(gravity >> 2), -(correction >> 2), velocity);
        if (angle >= RotationAngle.HalfTurn)
        {
            if (angle.WholeStep == RotationAngle.HalfTurn)
                return (0, 0, Math.Abs(velocity) < 256 ? 0 : velocity);
            return (submerged ? -(gravity >> 1) : -gravity, -correction, velocity);
        }
        if (angle >= RotationAngle.QuarterTurn)
            return (submerged ? gravity >> 1 : gravity, correction, velocity);
        return (submerged ? gravity >> 3 : gravity >> 2, correction >> 2, velocity);
    }
}
