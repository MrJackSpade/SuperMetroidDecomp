using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's shared neck math. Both the live room-enemy actor
/// and the later rainbow-beam sequence use these exact angle handlers; keeping them here
/// prevents the two encounter halves from slowly acquiring different articulation rules.
/// </summary>
internal static class MotherBrainNeckKinematics
{
    /// <summary>Lowest angle the lower neck bobs down to before reversing upward.</summary>
    private static readonly RotationAngle LowerBobFloor = RotationAngle.FromSteps(40);

    /// <summary>Highest angle the lower neck reaches, both bobbing up and raising one way.</summary>
    private static readonly RotationAngle LowerCeiling = RotationAngle.FromSteps(144);

    /// <summary>Angle at which a one-way lowering of the lower neck stops.</summary>
    private static readonly RotationAngle LowerFloor = RotationAngle.FromSteps(48);

    /// <summary>Lowest angle the upper neck reaches, both bobbing down and lowering one way.</summary>
    private static readonly RotationAngle UpperFloor = RotationAngle.FromSteps(32);

    /// <summary>How far the upper neck's raised limit leads the lower neck's angle: a thirty-second of a turn.</summary>
    private static readonly RotationAngle UpperLeadOverLower = RotationAngle.FromSteps(8);

    /// <summary>
    /// Advances the lower and upper neck angles through bobbing or one-way raise/lower movement, each turning by
    /// <paramref name="angleDelta"/> 256ths of a step an update.
    /// </summary>
    /// <remarks>The upper step runs second and observes the lower angle and movement state changed in the same frame.</remarks>
    internal static void StepAngles(
        ref RotationAngle lowerAngle,
        ref RotationAngle upperAngle,
        ref MotherBrainNeckMovement lowerMovement,
        ref MotherBrainNeckMovement upperMovement,
        int angleDelta,
        int brainY,
        int samusY)
    {
        switch (lowerMovement)
        {
            case MotherBrainNeckMovement.None:
                break;
            case MotherBrainNeckMovement.BobDown: // Bob down to five thirty-seconds of a turn, then reverse.
            {
                RotationAngle candidate = lowerAngle.TurnedByFractionsOfStep(-angleDelta);
                if (candidate.CompareTo(LowerBobFloor) < 0)
                {
                    candidate = LowerBobFloor;
                    lowerMovement = MotherBrainNeckMovement.BobUp;
                }
                lowerAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.BobUp: // Bob up to nine sixteenths of a turn unless the brain is already above Y=60.
                if (CoordinateAxis.Difference(brainY, 60) < 0)
                {
                    lowerMovement = MotherBrainNeckMovement.BobDown;
                }
                else
                {
                    RotationAngle candidate = lowerAngle.TurnedByFractionsOfStep(angleDelta);
                    if (candidate.CompareTo(LowerCeiling) >= 0)
                    {
                        candidate = LowerCeiling;
                        lowerMovement = MotherBrainNeckMovement.BobDown;
                    }
                    lowerAngle = candidate;
                }
                break;
            case MotherBrainNeckMovement.Lower: // One-way lower used by beam setup and the resurrection stretch.
            {
                RotationAngle candidate = lowerAngle.TurnedByFractionsOfStep(-angleDelta);
                if (candidate.CompareTo(LowerFloor) < 0)
                {
                    candidate = LowerFloor;
                    lowerMovement = MotherBrainNeckMovement.None;
                }
                lowerAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Raise: // One-way raise used by fake-death ascent and Baby interruptions.
            {
                RotationAngle candidate = lowerAngle.TurnedByFractionsOfStep(angleDelta);
                if (candidate.CompareTo(LowerCeiling) >= 0)
                {
                    candidate = LowerCeiling;
                    lowerMovement = MotherBrainNeckMovement.None;
                }
                lowerAngle = candidate;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unsupported lower-neck movement {lowerMovement}.");
        }

        // Runs the upper step after the lower step, so it observes the
        // lower angle and movement-index mutations made immediately above on this frame.
        switch (upperMovement)
        {
            case MotherBrainNeckMovement.None:
                break;
            case MotherBrainNeckMovement.BobDown: // Bob down; Samus below the brain forces both halves back upward.
                if (CoordinateAxis.AsSigned(brainY + 4 - samusY) >= 0)
                {
                    lowerMovement = MotherBrainNeckMovement.BobUp;
                    upperMovement = MotherBrainNeckMovement.BobUp;
                }
                else
                {
                    RotationAngle candidate = upperAngle.TurnedByFractionsOfStep(-angleDelta);
                    if (candidate.CompareTo(UpperFloor) < 0)
                    {
                        candidate = UpperFloor;
                        upperMovement = MotherBrainNeckMovement.BobUp;
                    }
                    upperAngle = candidate;
                }
                break;
            case MotherBrainNeckMovement.BobUp: // Follow eight angle units above the newly updated lower segment.
            {
                RotationAngle target = lowerAngle + UpperLeadOverLower;
                RotationAngle candidate = upperAngle.TurnedByFractionsOfStep(angleDelta);
                if (candidate.CompareTo(target) >= 0)
                {
                    candidate = target;
                    upperMovement = MotherBrainNeckMovement.BobDown;
                }
                upperAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Lower: // One-way lower to an eighth of a turn.
            {
                RotationAngle candidate = upperAngle.TurnedByFractionsOfStep(-angleDelta);
                if (candidate.CompareTo(UpperFloor) < 0)
                {
                    candidate = UpperFloor;
                    upperMovement = MotherBrainNeckMovement.None;
                }
                upperAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Raise: // One-way raise to lower angle plus a thirty-second of a turn.
            {
                RotationAngle target = lowerAngle + UpperLeadOverLower;
                RotationAngle candidate = upperAngle.TurnedByFractionsOfStep(angleDelta);
                if (candidate.CompareTo(target) >= 0)
                {
                    candidate = target;
                    upperMovement = MotherBrainNeckMovement.None;
                }
                upperAngle = candidate;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unsupported upper-neck movement {upperMovement}.");
        }
    }

    /// <summary>Computes all five signed sine/cosine positions: each joint's room-pixel position, indexed by joint.</summary>
    /// <param name="body">The body's position.</param>
    /// <param name="lowerAngle">The angle the lower joints hang at.</param>
    /// <param name="upperAngle">The angle the upper joints hang at.</param>
    /// <param name="distances">Each joint's distance from its anchor, indexed by joint.</param>
    internal static ImmutableArray<Position16<RoomPixels>> CalculateGeometry(Position16<RoomPixels> body, RotationAngle lowerAngle, RotationAngle upperAngle, IReadOnlyList<int> distances)
    {
        // The body reference point is deliberately offset. Adding (112,-96)
        // below reduces it to body+(32,-50), but retaining the two stages keeps each
        // segment's equation separately readable.
        Position16<RoomPixels> reference = body + new Offset16<RoomPixels>(-80, 46);
        TrigonometryAngle lower = lowerAngle.ToTrigonometryAngle();
        TrigonometryAngle upper = upperAngle.ToTrigonometryAngle();

        var joints = new Position16<RoomPixels>[MotherBrainNeckJointDefinitions.Count];
        for (int joint = 0; joint < MotherBrainNeckJointDefinitions.LowerCount; joint++)
            joints[joint] = CalculateLowerSegment(reference, lower, distances[joint]);
        for (int joint = MotherBrainNeckJointDefinitions.LowerCount; joint < MotherBrainNeckJointDefinitions.Count; joint++)
            joints[joint] = joints[MotherBrainNeckJointDefinitions.UpperAnchor] + CalculatePolarOffset(upper, distances[joint]);
        return [.. joints];
    }

    /// <summary>Places a lower-neck joint from the body-relative reference point and its signed polar displacement.</summary>
    private static Position16<RoomPixels> CalculateLowerSegment(
        Position16<RoomPixels> reference,
        TrigonometryAngle angle,
        int distance) =>
        reference + new Offset16<RoomPixels>(112, -96) + CalculatePolarOffset(angle, distance);

    /// <summary>
    /// One joint's displacement: the sine multiply for the angle (X) and the angle plus a
    /// quarter turn (Y), each added to the anchor wrapping at 65536.
    /// </summary>
    private static Offset16<RoomPixels> CalculatePolarOffset(TrigonometryAngle angle, int distance) =>
        new(CalculateSignedComponent(angle, distance),
            CalculateSignedComponent(angle.TurnedBy(TrigonometryAngle.QuarterTurn), distance));

    /// <summary>
    /// The farthest a joint lies from its anchor, in pixels (#1627): the game multiplies the sine by the distance as a
    /// signed count of whole pixels, which goes no further, and the neck's distances are 2, 10 and 20, so a farther one is
    /// rejected rather than given a product the game never forms.
    /// </summary>
    private const int FarthestJointDistance = 127;

    /// <summary>
    /// The signed sine multiply, keeping the product's whole pixels exactly as the game does. A joint lies at most
    /// <see cref="FarthestJointDistance"/> pixels from its anchor.
    /// </summary>
    internal static int CalculateSignedComponent(
        TrigonometryAngle angle,
        int distance)
    {
        int sine = Trigonometry.SignedSine(angle);
        int product = sine * Ensure.BetweenInclusive(0, FarthestJointDistance, distance);
        return CoordinateAxis.AsSigned(EightEight.WholeOf(product));
    }
}
