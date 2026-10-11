namespace SuperMetroid.Core.Game;

/// <summary>
/// The five room-pixel joint positions written by <c>$A9:91B8-$92AA</c>. Segment four is the
/// brain's attachment point; segments one through three also own the encounter's neck hitboxes.
/// </summary>
internal readonly record struct MotherBrainNeckGeometry(
    Position16<RoomPixels> Segment0,
    Position16<RoomPixels> Segment1,
    Position16<RoomPixels> Segment2,
    Position16<RoomPixels> Segment3,
    Position16<RoomPixels> Segment4);

/// <summary>
/// Shared translation of Mother Brain's bank-$A9 neck math. Both the live room-enemy actor
/// and the later rainbow-beam sequence use these exact angle handlers; keeping them here
/// prevents the two encounter halves from slowly acquiring different articulation rules.
/// </summary>
internal static class MotherBrainNeckKinematics
{
    /// <summary>Ports the lower and upper dispatchers at <c>$A9:9072-$91B7</c>.</summary>
    internal static void StepAngles(
        ref ushort lowerAngle,
        ref ushort upperAngle,
        ref MotherBrainNeckMovement lowerMovementIndex,
        ref MotherBrainNeckMovement upperMovementIndex,
        ushort angleDelta,
        ushort brainY,
        ushort samusY)
    {
        switch (lowerMovementIndex)
        {
            case MotherBrainNeckMovement.None:
                break;
            case MotherBrainNeckMovement.BobDown: // Bob down to $2800, then reverse.
            {
                ushort candidate = unchecked((ushort)(lowerAngle - angleDelta));
                if (candidate < 0x2800)
                {
                    candidate = 0x2800;
                    lowerMovementIndex = MotherBrainNeckMovement.BobUp;
                }
                lowerAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.BobUp: // Bob up to $9000 unless the brain is already above Y=$3C.
                if (unchecked((short)(brainY - 0x003c)) < 0)
                {
                    lowerMovementIndex = MotherBrainNeckMovement.BobDown;
                }
                else
                {
                    ushort candidate = unchecked((ushort)(lowerAngle + angleDelta));
                    if (candidate >= 0x9000)
                    {
                        candidate = 0x9000;
                        lowerMovementIndex = MotherBrainNeckMovement.BobDown;
                    }
                    lowerAngle = candidate;
                }
                break;
            case MotherBrainNeckMovement.Lower: // One-way lower used by beam setup and the resurrection stretch.
            {
                ushort candidate = unchecked((ushort)(lowerAngle - angleDelta));
                if (candidate < 0x3000)
                {
                    candidate = 0x3000;
                    lowerMovementIndex = MotherBrainNeckMovement.None;
                }
                lowerAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Raise: // One-way raise used by fake-death ascent and Baby interruptions.
            {
                ushort candidate = unchecked((ushort)(lowerAngle + angleDelta));
                if (candidate >= 0x9000)
                {
                    candidate = 0x9000;
                    lowerMovementIndex = MotherBrainNeckMovement.None;
                }
                lowerAngle = candidate;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unsupported lower-neck movement index ${(int)lowerMovementIndex:X4}.");
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
                    ushort candidate = unchecked((ushort)(upperAngle - angleDelta));
                    if (candidate < 0x2000)
                    {
                        candidate = 0x2000;
                        upperMovementIndex = MotherBrainNeckMovement.BobUp;
                    }
                    upperAngle = candidate;
                }
                break;
            case MotherBrainNeckMovement.BobUp: // Follow eight angle units above the newly updated lower segment.
            {
                ushort target = unchecked((ushort)(lowerAngle + 0x0800));
                ushort candidate = unchecked((ushort)(upperAngle + angleDelta));
                if (candidate >= target)
                {
                    candidate = target;
                    upperMovementIndex = MotherBrainNeckMovement.BobDown;
                }
                upperAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Lower: // One-way lower to $2000.
            {
                ushort candidate = unchecked((ushort)(upperAngle - angleDelta));
                if (candidate < 0x2000)
                {
                    candidate = 0x2000;
                    upperMovementIndex = MotherBrainNeckMovement.None;
                }
                upperAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Raise: // One-way raise to lower angle + $0800.
            {
                ushort target = unchecked((ushort)(lowerAngle + 0x0800));
                ushort candidate = unchecked((ushort)(upperAngle + angleDelta));
                if (candidate >= target)
                {
                    candidate = target;
                    upperMovementIndex = MotherBrainNeckMovement.None;
                }
                upperAngle = candidate;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unsupported upper-neck movement index ${(int)upperMovementIndex:X4}.");
        }
    }

    /// <summary>Ports all five signed sine/cosine position writes at $A9:91B8-$92AA.</summary>
    internal static MotherBrainNeckGeometry CalculateGeometry(
        Position16<RoomPixels> body,
        ushort lowerAngle,
        ushort upperAngle,
        ushort segment0Distance = 2,
        ushort segment1Distance = 10,
        ushort segment2Distance = 20,
        ushort segment3Distance = 10,
        ushort segment4Distance = 20)
    {
        // `$7E:7814/16` is a deliberately offset body reference point. Adding $70/$FFA0
        // below reduces it to body+($20,-$32), but retaining the two native stages makes the
        // individual segment equations auditable against the disassembly.
        Position16<RoomPixels> reference = body + new Offset16<RoomPixels>(-0x0050, 0x002e);
        byte lower = unchecked((byte)(lowerAngle >> 8));
        byte upper = unchecked((byte)(upperAngle >> 8));

        Position16<RoomPixels> segment0 = CalculateLowerSegment(reference, lower, segment0Distance);
        Position16<RoomPixels> segment1 = CalculateLowerSegment(reference, lower, segment1Distance);
        Position16<RoomPixels> segment2 = CalculateLowerSegment(reference, lower, segment2Distance);
        Position16<RoomPixels> segment3 = segment2 + CalculatePolarOffset(upper, segment3Distance);
        Position16<RoomPixels> segment4 = segment2 + CalculatePolarOffset(upper, segment4Distance);
        return new MotherBrainNeckGeometry(segment0, segment1, segment2, segment3, segment4);
    }

    private static Position16<RoomPixels> CalculateLowerSegment(
        Position16<RoomPixels> reference,
        byte angle,
        ushort distance) =>
        reference + new Offset16<RoomPixels>(0x0070, -0x0060) + CalculatePolarOffset(angle, distance);

    /// <summary>
    /// One joint's displacement: <c>$A9:C46C</c> for the angle (X) and the angle plus a
    /// quarter turn (Y), each added to the anchor with 16-bit wrap.
    /// </summary>
    private static Offset16<RoomPixels> CalculatePolarOffset(byte angle, ushort distance) =>
        new(CalculateSignedComponent(angle, distance),
            CalculateSignedComponent(unchecked((byte)(angle + 0x40)), distance));

    /// <summary>Exact signed multiply/high-word behavior of <c>$A9:C46C</c>.</summary>
    internal static short CalculateSignedComponent(
        byte angle,
        ushort distance)
    {
        short sine = EnemyTrigonometryTables.SignedSine(angle);
        int product = sine * unchecked((sbyte)distance);
        return unchecked((short)(product >> 8));
    }
}
