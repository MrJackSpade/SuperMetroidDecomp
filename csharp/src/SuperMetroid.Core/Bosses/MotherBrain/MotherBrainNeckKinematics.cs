using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Shared translation of Mother Brain's bank-$A9 neck math. Both the live room-enemy actor
/// and the later rainbow-beam sequence use these exact angle handlers; keeping them here
/// prevents the two encounter halves from slowly acquiring different articulation rules.
/// </summary>
internal static class MotherBrainNeckKinematics
{
    /// <summary>Lowest angle the lower neck bobs down to before reversing upward.</summary>
    private static readonly SnesAngle LowerBobFloor = SnesAngle.FromRaw(10240);

    /// <summary>Highest angle the lower neck reaches, both bobbing up and raising one way.</summary>
    private static readonly SnesAngle LowerCeiling = SnesAngle.FromRaw(36864);

    /// <summary>Angle at which a one-way lowering of the lower neck stops.</summary>
    private static readonly SnesAngle LowerFloor = SnesAngle.FromRaw(12288);

    /// <summary>Lowest angle the upper neck reaches, both bobbing down and lowering one way.</summary>
    private static readonly SnesAngle UpperFloor = SnesAngle.FromRaw(8192);

    /// <summary>How far the upper neck's raised limit leads the lower neck's angle, in 8.8 angle units.</summary>
    private const int UpperLeadOverLower = 2048;

    /// <summary>Advances the lower and upper neck angles through bobbing or one-way raise/lower movement at <c>$A9:9072-$91B7</c>.</summary>
    /// <remarks>The upper dispatcher runs second and observes the lower angle and movement state changed in the same frame.</remarks>
    internal static void StepAngles(
        ref SnesAngle lowerAngle,
        ref SnesAngle upperAngle,
        ref MotherBrainNeckMovement lowerMovementIndex,
        ref MotherBrainNeckMovement upperMovementIndex,
        int angleDelta,
        int brainY,
        int samusY)
    {
        switch (lowerMovementIndex)
        {
            case MotherBrainNeckMovement.None:
                break;
            case MotherBrainNeckMovement.BobDown: // Bob down to $2800, then reverse.
            {
                SnesAngle candidate = lowerAngle.AddRaw(-angleDelta);
                if (candidate.CompareTo(LowerBobFloor) < 0)
                {
                    candidate = LowerBobFloor;
                    lowerMovementIndex = MotherBrainNeckMovement.BobUp;
                }
                lowerAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.BobUp: // Bob up to $9000 unless the brain is already above Y=$3C.
                if (unchecked((short)(brainY - 60)) < 0)
                {
                    lowerMovementIndex = MotherBrainNeckMovement.BobDown;
                }
                else
                {
                    SnesAngle candidate = lowerAngle.AddRaw(angleDelta);
                    if (candidate.CompareTo(LowerCeiling) >= 0)
                    {
                        candidate = LowerCeiling;
                        lowerMovementIndex = MotherBrainNeckMovement.BobDown;
                    }
                    lowerAngle = candidate;
                }
                break;
            case MotherBrainNeckMovement.Lower: // One-way lower used by beam setup and the resurrection stretch.
            {
                SnesAngle candidate = lowerAngle.AddRaw(-angleDelta);
                if (candidate.CompareTo(LowerFloor) < 0)
                {
                    candidate = LowerFloor;
                    lowerMovementIndex = MotherBrainNeckMovement.None;
                }
                lowerAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Raise: // One-way raise used by fake-death ascent and Baby interruptions.
            {
                SnesAngle candidate = lowerAngle.AddRaw(angleDelta);
                if (candidate.CompareTo(LowerCeiling) >= 0)
                {
                    candidate = LowerCeiling;
                    lowerMovementIndex = MotherBrainNeckMovement.None;
                }
                lowerAngle = candidate;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unsupported lower-neck movement {lowerMovementIndex}.");
        }

        // Native runs the upper dispatcher after the lower dispatcher, so it observes the
        // lower angle and movement-index mutations made immediately above on this frame.
        switch (upperMovementIndex)
        {
            case MotherBrainNeckMovement.None:
                break;
            case MotherBrainNeckMovement.BobDown: // Bob down; Samus below the brain forces both halves back upward.
                if (unchecked((short)(brainY + 4 - samusY)) >= 0)
                {
                    lowerMovementIndex = MotherBrainNeckMovement.BobUp;
                    upperMovementIndex = MotherBrainNeckMovement.BobUp;
                }
                else
                {
                    SnesAngle candidate = upperAngle.AddRaw(-angleDelta);
                    if (candidate.CompareTo(UpperFloor) < 0)
                    {
                        candidate = UpperFloor;
                        upperMovementIndex = MotherBrainNeckMovement.BobUp;
                    }
                    upperAngle = candidate;
                }
                break;
            case MotherBrainNeckMovement.BobUp: // Follow eight angle units above the newly updated lower segment.
            {
                SnesAngle target = lowerAngle.AddRaw(UpperLeadOverLower);
                SnesAngle candidate = upperAngle.AddRaw(angleDelta);
                if (candidate.CompareTo(target) >= 0)
                {
                    candidate = target;
                    upperMovementIndex = MotherBrainNeckMovement.BobDown;
                }
                upperAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Lower: // One-way lower to $2000.
            {
                SnesAngle candidate = upperAngle.AddRaw(-angleDelta);
                if (candidate.CompareTo(UpperFloor) < 0)
                {
                    candidate = UpperFloor;
                    upperMovementIndex = MotherBrainNeckMovement.None;
                }
                upperAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Raise: // One-way raise to lower angle + $0800.
            {
                SnesAngle target = lowerAngle.AddRaw(UpperLeadOverLower);
                SnesAngle candidate = upperAngle.AddRaw(angleDelta);
                if (candidate.CompareTo(target) >= 0)
                {
                    candidate = target;
                    upperMovementIndex = MotherBrainNeckMovement.None;
                }
                upperAngle = candidate;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unsupported upper-neck movement {upperMovementIndex}.");
        }
    }

    /// <summary>Ports all five signed sine/cosine position writes at $A9:91B8-$92AA: each joint's room-pixel position, indexed by joint.</summary>
    /// <param name="body">The body's position.</param>
    /// <param name="lowerAngle">The angle the lower joints hang at.</param>
    /// <param name="upperAngle">The angle the upper joints hang at.</param>
    /// <param name="distances">Each joint's distance from its anchor, indexed by joint.</param>
    internal static ImmutableArray<Position16<RoomPixels>> CalculateGeometry(Position16<RoomPixels> body, SnesAngle lowerAngle, SnesAngle upperAngle, IReadOnlyList<int> distances)
    {
        // `$7E:7814/16` is a deliberately offset body reference point. Adding $70/$FFA0
        // below reduces it to body+($20,-$32), but retaining the two native stages makes the
        // individual segment equations auditable against the disassembly.
        Position16<RoomPixels> reference = body + new Offset16<RoomPixels>(-80, 46);
        SnesByteAngle lower = lowerAngle.ToByteAngle();
        SnesByteAngle upper = upperAngle.ToByteAngle();

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
        SnesByteAngle angle,
        int distance) =>
        reference + new Offset16<RoomPixels>(112, -96) + CalculatePolarOffset(angle, distance);

    /// <summary>
    /// One joint's displacement: <c>$A9:C46C</c> for the angle (X) and the angle plus a
    /// quarter turn (Y), each added to the anchor with 16-bit wrap.
    /// </summary>
    private static Offset16<RoomPixels> CalculatePolarOffset(SnesByteAngle angle, int distance) =>
        new(CalculateSignedComponent(angle, distance),
            CalculateSignedComponent(angle.AddTableUnits(SnesByteAngle.QuarterTurn.RawValue), distance));

    /// <summary>Exact signed multiply/high-word behavior of <c>$A9:C46C</c>.</summary>
    internal static short CalculateSignedComponent(
        SnesByteAngle angle,
        int distance)
    {
        int sine = EnemyTrigonometryTables.SignedSine(angle);
        int product = sine * unchecked((sbyte)distance);
        return unchecked((short)SnesEightEight.WholeOf(product));
    }
}
