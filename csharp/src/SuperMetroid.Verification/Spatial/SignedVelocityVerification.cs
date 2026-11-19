using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #2569 conversions of magnitude-and-sign arithmetic into signed velocities: each equals the arithmetic it
/// replaces over the whole of its inputs. A share's reach rounds toward zero as the size-then-sign product did; a speed
/// kept from rest, sped up, dragged or taken along another velocity equals the sign-tested arithmetic; a share times a turn
/// is the signed product of the old size product; Samus's vertical velocity moves, gathers gravity and turns at its apex as
/// her speed and direction did; and a quantity taken toward a direction is the quantity or its reversal as the direction
/// tests chose it.
/// </summary>
internal static class SignedVelocityVerification
{
    /// <summary>The longest length a share's reach is compared at: the most a speed resolved along an angle is taken as.</summary>
    private const int LongestReach = ushort.MaxValue;

    /// <summary>The 256ths of the whole of a share's peak.</summary>
    private const int SharePeak = 256;

    /// <summary>Runs every comparison of the signed velocities with the arithmetic they replace.</summary>
    internal static void VerifySignedVelocities()
    {
        int compared = VerifyReachTowardZero() + VerifySpeedOperations() + VerifyTurnVelocity() + VerifyVerticalVelocity() +
            VerifyTowardDirection();
        Console.WriteLine($"  Signed velocities: {compared} reaches, speed operations, turn velocities, vertical velocities and directed quantities equal the magnitude-and-sign arithmetic they replace.");
    }

    /// <summary>
    /// A share's reach (<see cref="UnitComponent.ReachOf"/>) equals the replaced product, the share's size times the length
    /// with its 256ths dropped and the share's sign restored, at every angle and every length a speed along an angle is
    /// taken as; so <see cref="SpeedAlongAngle.Along"/> and the shinespark's circle keep their rounding toward zero.
    /// </summary>
    private static int VerifyReachTowardZero()
    {
        int[] mismatches = new int[byte.MaxValue + 1];
        ParallelSweep.For(0, byte.MaxValue + 1, raw =>
        {
            var angle = new TrigonometryAngle(raw);
            int sample = Trigonometry.SignedSine(angle);
            UnitComponent share = UnitComponent.SineOf(angle);
            for (int length = 0; length <= LongestReach; length++)
            {
                int product = length * Math.Abs(sample) / SharePeak;
                int old = sample < 0 ? -product : product;
                if (share.ReachOf(length) != old)
                    mismatches[raw]++;
            }
        });
        for (int raw = 0; raw <= byte.MaxValue; raw++)
            AssertEqual(0, mismatches[raw], $"the reach along angle {raw} rounds toward zero as the size-then-sign product did");
        return (byte.MaxValue + 1) * (LongestReach + 1);
    }

    /// <summary>
    /// Every velocity a signed 8.8 speed holds but the most negative, kept from rest, sped up, dragged and taken along,
    /// equals the sign-tested arithmetic it replaces: Ceres Ridley's fireball move at its least speed, Torizo's sonic boom's
    /// acceleration and the drool's, the orb's and the egg's drags, and a margin taken the way a velocity points.
    /// </summary>
    private static int VerifySpeedOperations()
    {
        SignedEightEight[] changes =
        [
            CeresRidleyFlightDefinitions.FireballMoveMinimumSpeed, TorizoMotionDefinitions.SonicBoomAcceleration,
            TorizoMotionDefinitions.DroolDrag, TorizoMotionDefinitions.GoldenOrbBounceDrag, TorizoMotionDefinitions.EggBounceDrag,
        ];
        int compared = 0;
        for (int raw = short.MinValue + 1; raw <= short.MaxValue; raw++)
        {
            SignedEightEight velocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(raw);
            foreach (SignedEightEight change in changes)
            {
                SignedEightEight kept = SignedEightEight.Max(change, velocity.Magnitude()).AlongDirectionOf(velocity);
                SignedEightEight spedUp = velocity.IsNegative ? velocity - change : velocity + change;
                SignedEightEight dragged = velocity.IsNegative ? velocity + change : velocity - change;
                if (velocity.WithSpeedAtLeast(change) != kept || velocity.SpedUpBy(change) != spedUp || velocity.DraggedBy(change) != dragged)
                    throw new InvalidDataException($"Velocity {raw}, change {change.RawValue}: a speed operation differs from the sign-tested arithmetic.");
                compared++;
            }
            int margin = raw & byte.MaxValue;
            if (velocity.Along(margin) != (velocity.IsNegative ? -margin : margin))
                throw new InvalidDataException($"Velocity {raw}: a margin taken along it differs from the margin signed by its sign.");
            compared++;
        }
        return compared;
    }

    /// <summary>
    /// A share times a turn (<see cref="UnitComponent.VelocityOf(AngularSpeed)"/>) is, at every angle and every turn a swing
    /// releases at, the size product it replaces with the direction the release's rule gave: down where the turn and the
    /// share point the same way, a still release included.
    /// </summary>
    private static int VerifyTurnVelocity()
    {
        int compared = 0;
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            UnitComponent share = UnitComponent.SineOf(new TrigonometryAngle(raw));
            int size = Math.Abs(Trigonometry.SignedSine(new TrigonometryAngle(raw)));
            for (int fractions = -2048; fractions <= 2047; fractions++)
            {
                AngularSpeed turn = AngularSpeed.FromFractionsOfStep(fractions);
                UnsignedSixteenSixteen speed = UnsignedSixteenSixteen.ProductOfTwoFiftySixths(size, Math.Abs(fractions));
                SamusVerticalDirection oldDirection = turn.IsNegative == share.IsNegative ? SamusVerticalDirection.Down : SamusVerticalDirection.Up;
                SamusVerticalVelocity released = SamusVerticalVelocity.FromDisplacement(share.VelocityOf(turn),
                    atRest: turn.IsNegative == share.IsNegative ? VerticalDirection.Down : VerticalDirection.Up);
                if (released != SamusVerticalVelocity.Of(speed, oldDirection) || share.VelocityOf(turn).Magnitude() != speed)
                    throw new InvalidDataException($"Angle {raw}, turn {fractions}: the turn's velocity differs from the size product and its release direction.");
                compared++;
            }
        }
        return compared;
    }

    /// <summary>
    /// Samus's vertical velocity, for every direction and a speed at each part's edges and wraps, moves as her speed and
    /// direction did (downward for a fall, upward otherwise), gathers or loses gravity as the old step did, the terminal
    /// whole speed holding a fall, and passes its apex only as a rise whose speed has wrapped past zero.
    /// </summary>
    private static int VerifyVerticalVelocity()
    {
        int[] parts = [0, 1, 4, 5, 6, 0x7fff, 0x8000, 0xfffe, 0xffff];
        UnsignedSixteenSixteen gravity = UnsignedSixteenSixteen.FromPixelFraction(7, 256);
        int compared = 0;
        foreach (SamusVerticalDirection direction in (SamusVerticalDirection[])[SamusVerticalDirection.None, SamusVerticalDirection.Up, SamusVerticalDirection.Down])
        foreach (int whole in parts)
        foreach (int fraction in parts)
        {
            var speed = new UnsignedSixteenSixteen(whole, fraction);
            SamusVerticalVelocity velocity = SamusVerticalVelocity.Of(speed, direction);
            SignedSixteenSixteen oldDisplacement = direction == SamusVerticalDirection.Down ? speed.AsDisplacement() : -speed.AsDisplacement();
            UnsignedSixteenSixteen oldSpeed = direction == SamusVerticalDirection.Down
                ? speed.Whole != SamusVerticalMotionDefinitions.TerminalFallWholeSpeed ? speed + gravity : speed
                : speed - gravity;
            bool oldApex = direction == SamusVerticalDirection.Up && SamusSpeedMagnitude.HasPassedZero(speed.Whole);
            SamusVerticalVelocity after = velocity.AfterGravity(gravity, SamusVerticalMotionDefinitions.TerminalFallWholeSpeed);
            if (velocity.Displacement != oldDisplacement || after.Speed != oldSpeed || after.Direction != direction ||
                velocity.HasPassedApex != oldApex || velocity.IsMoving != !speed.IsZero)
                throw new InvalidDataException($"Vertical velocity {whole}.{fraction} {direction}: it differs from the speed and direction it replaces.");
            compared++;
        }
        AssertEqual(SamusVerticalVelocity.Of(UnsignedSixteenSixteen.Zero, SamusVerticalDirection.None), SamusVerticalVelocity.Still, "a still velocity has no speed and no direction");
        AssertEqual(SamusVerticalVelocity.Of(UnsignedSixteenSixteen.Zero, SamusVerticalDirection.Down), SamusVerticalVelocity.FallFromRest, "a fall from rest heads down at no speed");
        return compared + 2;
    }

    /// <summary>
    /// A quantity taken toward each direction is the quantity rightward and downward and its reversal leftward and upward,
    /// for whole numbers and signed 8.8 and 16.16 values across their ranges.
    /// </summary>
    private static int VerifyTowardDirection()
    {
        int compared = 0;
        for (int raw = short.MinValue + 1; raw <= short.MaxValue; raw++)
        {
            SignedEightEight speed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(raw);
            SignedSixteenSixteen distance = SignedSixteenSixteen.FromParts(raw, raw & ushort.MaxValue);
            if (raw.Toward(HorizontalDirection.Right) != raw || raw.Toward(HorizontalDirection.Left) != -raw ||
                raw.Toward(VerticalDirection.Down) != raw || raw.Toward(VerticalDirection.Up) != -raw ||
                speed.Toward(HorizontalDirection.Left) != -speed || speed.Toward(VerticalDirection.Down) != speed ||
                distance.Toward(HorizontalDirection.Left) != -distance || distance.Toward(VerticalDirection.Down) != distance)
                throw new InvalidDataException($"{raw}: a quantity taken toward a direction differs from it or its reversal.");
            compared++;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => 1.Toward((HorizontalDirection)2), "an undefined horizontal direction is refused");
        AssertThrows<ArgumentOutOfRangeException>(() => 1.Toward((VerticalDirection)2), "an undefined vertical direction is refused");
        return compared + 2;
    }
}
