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
            -GrappleBeamDefinitions.Physics.MaximumAngularVelocity.FractionsOfStep, -257, -256, -255, -1, 0, 1, 255, 256, 257,
            GrappleBeamDefinitions.Physics.MaximumAngularVelocity.FractionsOfStep,
        ];
        int gravityCompared = 0, gravityMismatches = 0;
        string? firstGravityMismatch = null;
        foreach (bool submerged in new[] { false, true })
        {
            foreach (int velocity in velocities)
            {
                for (int raw = 0; raw <= ushort.MaxValue; raw++)
                {
                    RotationAngle angle = RotationAngleConstants.FromRaw(raw);
                    var expected = ReplacedGravity(angle, submerged, velocity);
                    var grapple = new SamusGrappleState(global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Angle = angle, Submerged = submerged, AngularVelocity = AngularSpeedConstants.FromFractionsOfStep(velocity) };
                    SamusGrappleSwing.CalculateGravity(grapple);
                    gravityCompared++;
                    if ((grapple.GravityAcceleration.FractionsOfStep, grapple.VelocityCorrection.FractionsOfStep, grapple.AngularVelocity.FractionsOfStep) != expected)
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
        for (int velocity = short.MinValue; velocity <= short.MaxValue; velocity++)
        {
            AngularSpeed expected = AngularSpeedConstants.FromFractionsOfStep(-(velocity >> 1));
            AngularSpeed actual = SamusGrappleSwing.ReversedAtHalfSpeed(AngularSpeedConstants.FromFractionsOfStep(velocity));
            if (actual != expected)
            {
                bounceMismatches++;
                firstBounceMismatch ??= $"velocity {velocity}: expected {expected}, got {actual}";
            }
        }

        AssertEqual(0, bounceMismatches, $"bounce reversed half speed against the replaced shift; first: {firstBounceMismatch}");
        int typedCases = VerifyTypedSwingArithmetic();
        Console.WriteLine($"Grapple swing arithmetic: {gravityCompared} gravity cases, every bounce velocity and {typedCases} typed-speed cases give the replaced arithmetic's results.");
    }

    /// <summary>
    /// Compares the typed angular speeds, unit components and launch speeds (#2110) with the raw 256ths arithmetic they
    /// replaced: the submerged swing's share of its turn, the clamp to the fastest swing, the jump impulse's decay, the
    /// rope's segment step, the octant angle of an offset and the launch velocities, over every value each can take.
    /// </summary>
    /// <returns>The cases compared.</returns>
    private static int VerifyTypedSwingArithmetic()
    {
        int cases = 0;
        int fastest = GrappleBeamDefinitions.Physics.MaximumAngularVelocity.FractionsOfStep;
        int impulse = GrappleBeamDefinitions.Physics.JumpImpulseMagnitude.FractionsOfStep;
        int decay = GrappleBeamDefinitions.Physics.JumpImpulseDecay.FractionsOfStep;
        for (int combined = -(fastest + impulse); combined <= fastest + impulse; combined++)
        {
            // The replaced turn: the combined speed's size times 160/256, then its sign restored.
            int size = EightEightOracle.Multiply(Math.Abs(combined), 160);
            AssertEqual(AngularSpeedConstants.FromFractionsOfStep(combined < 0 ? -size : size),
                AngularSpeedConstants.FromFractionsOfStep(combined).ScaledBy(GrappleBeamDefinitions.Physics.SubmergedSwingShare, GrappleBeamDefinitions.Physics.SubmergedSwingParts),
                $"submerged swing share of {combined}");
            AssertEqual(AngularSpeedConstants.FromFractionsOfStep(Math.Clamp(combined, -fastest, fastest)),
                AngularSpeedConstants.FromFractionsOfStep(combined).LimitedTo(GrappleBeamDefinitions.Physics.MaximumAngularVelocity),
                $"swing speed {combined} held to the fastest swing");
            cases += 2;
        }
        for (int jump = -impulse; jump <= impulse; jump++)
        {
            int expected = jump > 0 ? Math.Max(0, jump - decay) : jump < 0 ? Math.Min(0, jump + decay) : 0;
            AssertEqual(AngularSpeedConstants.FromFractionsOfStep(expected),
                AngularSpeedConstants.FromFractionsOfStep(jump).SlowedTowardRest(GrappleBeamDefinitions.Physics.JumpImpulseDecay),
                $"jump impulse {jump} decays toward rest");
            cases++;
        }
        for (int angle = 0; angle < RotationAngle.StepsPerTurn; angle++)
        {
            int sine = Trigonometry.SignedSine(new TrigonometryAngle(angle));
            AssertEqual(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(sine * GrappleBeamDefinitions.Rendering.SegmentSpacing),
                UnitComponent.SineOf(new TrigonometryAngle(angle)).ExactReachOf(GrappleBeamDefinitions.Rendering.SegmentSpacing),
                $"rope segment step at angle {angle}");
            cases++;
        }
        for (int y = -160; y <= 160; y++)
        for (int x = -160; x <= 160; x++)
        {
            AssertEqual(ReplacedAngleFromXY(x, y), SamusGrappleMovement.CalculateAngleFromXY(Offset16<RoomPixels>.Of(x, y)),
                $"angle of offset ({x}, {y})");
            cases++;
        }
        // The replaced launch speeds in 256ths of a pixel an update: twelve times 255 along an axis, twelve times 181 along
        // each axis of a diagonal.
        var launches = new Dictionary<SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection, (int X, int Y)>
        {
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.UpFacingRight] = (0, -3060),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.UpRight] = (2172, -2172),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Right] = (3060, 0),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.DownRight] = (2172, 2172),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.DownFacingRight] = (0, 3060),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.DownFacingLeft] = (0, 3060),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.DownLeft] = (-2172, 2172),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Left] = (-3060, 0),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.UpLeft] = (-2172, -2172),
            [SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.UpFacingLeft] = (0, -3060),
        };
        foreach ((SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection direction, (int X, int Y) expected) in launches)
        {
            AssertEqual(new PixelVelocity<RoomPixels>(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(expected.X), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(expected.Y)), GrappleFiring.Launch(direction).Velocity,
                $"grapple launch velocity toward {direction}");
            cases++;
        }
        return cases;
    }

    /// <summary>The angle of an offset as the replaced arithmetic computed it: the tangent in 256ths, then its octant steps.</summary>
    private static RotationAngle ReplacedAngleFromXY(int x, int y)
    {
        int absoluteX = Math.Abs(x), absoluteY = Math.Abs(y);
        if (absoluteX == 0 && absoluteY == 0)
            return RotationAngle.Zero;
        bool leanSameWay = (x < 0) == (y < 0);
        if (absoluteY < absoluteX)
        {
            int steps = (256 * absoluteY / absoluteX) * 32 / 256;
            return (x < 0 ? RotationAngle.ThreeQuarterTurn : RotationAngle.QuarterTurn).TurnedBySteps(leanSameWay ? steps : -steps);
        }
        int reciprocal = (256 * absoluteX / absoluteY) * 32 / 256;
        return (y < 0 ? RotationAngle.Zero : RotationAngle.HalfTurn).TurnedBySteps(leanSameWay ? -reciprocal : reciprocal);
    }

    /// <summary>The quadrant gravity, correction and hang stop as the replaced shifts computed them.</summary>
    private static (int Gravity, int Correction, int Velocity) ReplacedGravity(RotationAngle angle, bool submerged, int velocity)
    {
        int gravity = GrappleBeamDefinitions.Physics.GravityMagnitude.FractionsOfStep;
        int correction = GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude.FractionsOfStep;
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
