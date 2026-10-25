using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Verification's native view of Samus's pose delay programs: the bank-$91 address of each.</summary>
internal static class SamusAnimationDelayNative
{
    /// <summary>$91:B20A: where the pose delay programs start in bank $91.</summary>
    private const ushort DelayStreamsStart = 0xb20a;

    /// <summary>The bank-$91 address of <paramref name="list"/>'s first byte.</summary>
    internal static ushort Address(SamusAnimationDelayList list) => list.Segment is { } segment
        ? (ushort)(DelayStreamsStart + SamusAnimationDelayPrograms.segments[segment].Offset)
        : checked((ushort)list.ResidueStart);

    /// <summary>The pose delay programs by their bank-$91 addresses.</summary>
    extension(SamusAnimationDelayDefinitions)
    {
        /// <summary>$91:B010: the table of 253 pose animation pointers.</summary>
        internal static int PointerTableAddress => 0x91B010;
        /// <summary>$91:B20A: the first delay-program byte, immediately after the pose pointer table.</summary>
        internal static int DelayStreamsAddress => 0x91B20A;
        /// <summary>$91:B5D1: the end of the pose delay programs, before the running cadences.</summary>
        internal static int DelayStreamsEndExclusive => 0x91B5D1;

        /// <summary>The bank-$91 address of the program <paramref name="pose"/> animates by.</summary>
        internal static ushort PointerForPose(SamusPoseId pose) => Address(SamusAnimationDelayDefinitions.ListForPose(pose));

        /// <summary>Reads a delay byte from the program starting at bank-$91 <paramref name="pointer"/>.</summary>
        internal static byte ReadAnimationByte(SuperMetroid.Core.Compatibility.IFloorUpdateResidue? floorUpdateResidue,
            ushort pointer, int byteIndex) =>
            SamusAnimationDelayDefinitions.ReadAnimationByte(floorUpdateResidue, List(pointer), byteIndex, InstalledFixtures.SamusOverreads.AnimationWindows);

        /// <summary>Read the bounded native table image for byte-for-byte verification.</summary>
        internal static byte ReadCompiledByte(int address)
        {
            if (address is >= 0x91B010 and < 0x91B20A)
            {
                int byteIndex = address - 0x91B010;
                ushort pointer = Address(SamusAnimationDelayDefinitions.ListForPose((SamusPoseId)(byteIndex / 2)));
                return unchecked((byte)(pointer >> ((byteIndex & 1) * 8)));
            }
            if (address is >= 0x91B20A and < 0x91B5D1)
                return SamusAnimationDelayPrograms.ByteAt(address - 0x91B20A);
            throw new ArgumentOutOfRangeException(nameof(address), address,
                "Address is outside the compiled Samus animation definition catalog.");
        }
    }

    /// <summary>The program starting at bank-$91 <paramref name="pointer"/>.</summary>
    private static SamusAnimationDelayList List(ushort pointer)
    {
        if (pointer < 0x8000)
            return new(null, pointer);
        for (int segment = 0; segment < SamusAnimationDelayPrograms.segments.Length; segment++)
            if (Address(new(segment, 0)) == pointer)
                return new(segment, 0);
        throw new InvalidDataException($"No pose delay program starts at $91:{pointer:X4}.");
    }
}
