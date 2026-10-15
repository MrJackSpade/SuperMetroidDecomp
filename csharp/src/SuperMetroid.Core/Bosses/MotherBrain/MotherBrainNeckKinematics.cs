using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Shared translation of Mother Brain's bank-$A9 neck math. Both the live room-enemy actor
/// and the later rainbow-beam sequence use these exact angle handlers; keeping them here
/// prevents the two encounter halves from slowly acquiring different articulation rules.
/// </summary>
internal static class MotherBrainNeckKinematics
{
    /// <summary>Advances the lower and upper neck angles through bobbing or one-way raise/lower movement at <c>$A9:9072-$91B7</c>.</summary>
    /// <remarks>The upper dispatcher runs second and observes the lower angle and movement state changed in the same frame.</remarks>
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
                if (candidate < 10240)
                {
                    candidate = 10240;
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
                    ushort candidate = unchecked((ushort)(lowerAngle + angleDelta));
                    if (candidate >= 36864)
                    {
                        candidate = 36864;
                        lowerMovementIndex = MotherBrainNeckMovement.BobDown;
                    }
                    lowerAngle = candidate;
                }
                break;
            case MotherBrainNeckMovement.Lower: // One-way lower used by beam setup and the resurrection stretch.
            {
                ushort candidate = unchecked((ushort)(lowerAngle - angleDelta));
                if (candidate < 12288)
                {
                    candidate = 12288;
                    lowerMovementIndex = MotherBrainNeckMovement.None;
                }
                lowerAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Raise: // One-way raise used by fake-death ascent and Baby interruptions.
            {
                ushort candidate = unchecked((ushort)(lowerAngle + angleDelta));
                if (candidate >= 36864)
                {
                    candidate = 36864;
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
                    if (candidate < 8192)
                    {
                        candidate = 8192;
                        upperMovementIndex = MotherBrainNeckMovement.BobUp;
                    }
                    upperAngle = candidate;
                }
                break;
            case MotherBrainNeckMovement.BobUp: // Follow eight angle units above the newly updated lower segment.
            {
                ushort target = unchecked((ushort)(lowerAngle + 2048));
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
                if (candidate < 8192)
                {
                    candidate = 8192;
                    upperMovementIndex = MotherBrainNeckMovement.None;
                }
                upperAngle = candidate;
                break;
            }
            case MotherBrainNeckMovement.Raise: // One-way raise to lower angle + $0800.
            {
                ushort target = unchecked((ushort)(lowerAngle + 2048));
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
    internal static MotherBrainNeckGeometry CalculateGeometry(Position16<RoomPixels> body, ushort lowerAngle, ushort upperAngle, MotherBrainNeckSegmentDistances distances)
    {
        // `$7E:7814/16` is a deliberately offset body reference point. Adding $70/$FFA0
        // below reduces it to body+($20,-$32), but retaining the two native stages makes the
        // individual segment equations auditable against the disassembly.
        Position16<RoomPixels> reference = body + new Offset16<RoomPixels>(-80, 46);
        byte lower = SnesAngle.FromRaw(lowerAngle).TableIndex;
        byte upper = SnesAngle.FromRaw(upperAngle).TableIndex;

        Position16<RoomPixels> segment0 = CalculateLowerSegment(reference, lower, distances.Segment0Distance);
        Position16<RoomPixels> segment1 = CalculateLowerSegment(reference, lower, distances.Segment1Distance);
        Position16<RoomPixels> segment2 = CalculateLowerSegment(reference, lower, distances.Segment2Distance);
        Position16<RoomPixels> segment3 = segment2 + CalculatePolarOffset(upper, distances.Segment3Distance);
        Position16<RoomPixels> segment4 = segment2 + CalculatePolarOffset(upper, distances.Segment4Distance);
        return new MotherBrainNeckGeometry(segment0, segment1, segment2, segment3, segment4);
    }

    /// <summary>Places a lower-neck joint from the body-relative reference point and its signed polar displacement.</summary>
    private static Position16<RoomPixels> CalculateLowerSegment(
        Position16<RoomPixels> reference,
        byte angle,
        ushort distance) =>
        reference + new Offset16<RoomPixels>(112, -96) + CalculatePolarOffset(angle, distance);

    /// <summary>
    /// One joint's displacement: <c>$A9:C46C</c> for the angle (X) and the angle plus a
    /// quarter turn (Y), each added to the anchor with 16-bit wrap.
    /// </summary>
    private static Offset16<RoomPixels> CalculatePolarOffset(byte angle, ushort distance) =>
        new(CalculateSignedComponent(angle, distance),
            CalculateSignedComponent(unchecked((byte)(angle + 64)), distance));

    /// <summary>Exact signed multiply/high-word behavior of <c>$A9:C46C</c>.</summary>
    internal static short CalculateSignedComponent(
        byte angle,
        ushort distance)
    {
        short sine = EnemyTrigonometryTables.SignedSine(angle);
        int product = sine * unchecked((sbyte)distance);
        return unchecked((short)SnesEightEight.WholeOf(product));
    }
}
