using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>One world-space joint produced by Mother Brain's articulated neck solver.</summary>
public readonly record struct MotherBrainNeckPoint(ushort X, ushort Y);

/// <summary>
/// The five positions written by <c>$A9:91B8-$92AA</c>. Segment four is the brain's
/// attachment point; segments one through three also own the encounter's neck hitboxes.
/// </summary>
internal readonly record struct MotherBrainNeckGeometry(
    MotherBrainNeckPoint Segment0,
    MotherBrainNeckPoint Segment1,
    MotherBrainNeckPoint Segment2,
    MotherBrainNeckPoint Segment3,
    MotherBrainNeckPoint Segment4);

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
        ref ushort lowerMovementIndex,
        ref ushort upperMovementIndex,
        ushort angleDelta,
        ushort brainY,
        ushort samusY)
    {
        switch (lowerMovementIndex)
        {
            case 0:
                break;
            case 2: // Bob down to $2800, then reverse.
            {
                ushort candidate = unchecked((ushort)(lowerAngle - angleDelta));
                if (candidate < 0x2800)
                {
                    candidate = 0x2800;
                    lowerMovementIndex = 4;
                }
                lowerAngle = candidate;
                break;
            }
            case 4: // Bob up to $9000 unless the brain is already above Y=$3C.
                if (unchecked((short)(brainY - 0x003c)) < 0)
                {
                    lowerMovementIndex = 2;
                }
                else
                {
                    ushort candidate = unchecked((ushort)(lowerAngle + angleDelta));
                    if (candidate >= 0x9000)
                    {
                        candidate = 0x9000;
                        lowerMovementIndex = 2;
                    }
                    lowerAngle = candidate;
                }
                break;
            case 6: // One-way lower used by beam setup and the resurrection stretch.
            {
                ushort candidate = unchecked((ushort)(lowerAngle - angleDelta));
                if (candidate < 0x3000)
                {
                    candidate = 0x3000;
                    lowerMovementIndex = 0;
                }
                lowerAngle = candidate;
                break;
            }
            case 8: // One-way raise used by fake-death ascent and Baby interruptions.
            {
                ushort candidate = unchecked((ushort)(lowerAngle + angleDelta));
                if (candidate >= 0x9000)
                {
                    candidate = 0x9000;
                    lowerMovementIndex = 0;
                }
                lowerAngle = candidate;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unsupported lower-neck movement index ${lowerMovementIndex:X4}.");
        }

        // Native runs the upper dispatcher after the lower dispatcher, so it observes the
        // lower angle and movement-index mutations made immediately above on this frame.
        switch (upperMovementIndex)
        {
            case 0:
                break;
            case 2: // Bob down; Samus below the brain forces both halves back upward.
                if (unchecked((short)(brainY + 4 - samusY)) >= 0)
                {
                    lowerMovementIndex = 4;
                    upperMovementIndex = 4;
                }
                else
                {
                    ushort candidate = unchecked((ushort)(upperAngle - angleDelta));
                    if (candidate < 0x2000)
                    {
                        candidate = 0x2000;
                        upperMovementIndex = 4;
                    }
                    upperAngle = candidate;
                }
                break;
            case 4: // Follow eight angle units above the newly updated lower segment.
            {
                ushort target = unchecked((ushort)(lowerAngle + 0x0800));
                ushort candidate = unchecked((ushort)(upperAngle + angleDelta));
                if (candidate >= target)
                {
                    candidate = target;
                    upperMovementIndex = 2;
                }
                upperAngle = candidate;
                break;
            }
            case 6: // One-way lower to $2000.
            {
                ushort candidate = unchecked((ushort)(upperAngle - angleDelta));
                if (candidate < 0x2000)
                {
                    candidate = 0x2000;
                    upperMovementIndex = 0;
                }
                upperAngle = candidate;
                break;
            }
            case 8: // One-way raise to lower angle + $0800.
            {
                ushort target = unchecked((ushort)(lowerAngle + 0x0800));
                ushort candidate = unchecked((ushort)(upperAngle + angleDelta));
                if (candidate >= target)
                {
                    candidate = target;
                    upperMovementIndex = 0;
                }
                upperAngle = candidate;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Unsupported upper-neck movement index ${upperMovementIndex:X4}.");
        }
    }

    /// <summary>Ports all five signed sine/cosine position writes at $A9:91B8-$92AA.</summary>
    internal static MotherBrainNeckGeometry CalculateGeometry(
        ISnesAddressSpace bus,
        ushort bodyX,
        ushort bodyY,
        ushort lowerAngle,
        ushort upperAngle,
        ushort segment0Distance = 2,
        ushort segment1Distance = 10,
        ushort segment2Distance = 20,
        ushort segment3Distance = 10,
        ushort segment4Distance = 20)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // `$7E:7814/16` is a deliberately offset body reference point. Adding $70/$FFA0
        // below reduces it to body+($20,-$32), but retaining the two native stages makes the
        // individual segment equations auditable against the disassembly.
        ushort referenceX = unchecked((ushort)(bodyX - 0x0050));
        ushort referenceY = unchecked((ushort)(bodyY + 0x002e));
        byte lower = unchecked((byte)(lowerAngle >> 8));
        byte upper = unchecked((byte)(upperAngle >> 8));

        MotherBrainNeckPoint segment0 = CalculateLowerSegment(
            bus, referenceX, referenceY, lower, segment0Distance);
        MotherBrainNeckPoint segment1 = CalculateLowerSegment(
            bus, referenceX, referenceY, lower, segment1Distance);
        MotherBrainNeckPoint segment2 = CalculateLowerSegment(
            bus, referenceX, referenceY, lower, segment2Distance);
        MotherBrainNeckPoint segment3 = CalculateUpperSegment(
            bus, segment2, upper, segment3Distance);
        MotherBrainNeckPoint segment4 = CalculateUpperSegment(
            bus, segment2, upper, segment4Distance);
        return new MotherBrainNeckGeometry(segment0, segment1, segment2, segment3, segment4);
    }

    private static MotherBrainNeckPoint CalculateLowerSegment(
        ISnesAddressSpace bus,
        ushort referenceX,
        ushort referenceY,
        byte angle,
        ushort distance) =>
        new(
            unchecked((ushort)(referenceX + 0x0070 +
                CalculateSignedComponent(bus, angle, distance))),
            unchecked((ushort)(referenceY - 0x0060 +
                CalculateSignedComponent(bus, unchecked((byte)(angle + 0x40)), distance))));

    private static MotherBrainNeckPoint CalculateUpperSegment(
        ISnesAddressSpace bus,
        MotherBrainNeckPoint segment2,
        byte angle,
        ushort distance) =>
        new(
            unchecked((ushort)(segment2.X + CalculateSignedComponent(bus, angle, distance))),
            unchecked((ushort)(segment2.Y +
                CalculateSignedComponent(bus, unchecked((byte)(angle + 0x40)), distance))));

    /// <summary>Exact signed multiply/high-word behavior of <c>$A9:C46C</c>.</summary>
    internal static short CalculateSignedComponent(
        ISnesAddressSpace bus,
        byte angle,
        ushort distance)
    {
        int address = 0xa0b443 + angle * 2;
        short sine = unchecked((short)(
            bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
        int product = sine * unchecked((sbyte)distance);
        return unchecked((short)(product >> 8));
    }
}
