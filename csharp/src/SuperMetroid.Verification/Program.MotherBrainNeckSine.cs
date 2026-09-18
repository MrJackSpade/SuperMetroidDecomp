using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainNeckSine(SuperMetroidAddressSpace rom)
    {
        for (int angle = byte.MinValue; angle <= byte.MaxValue; angle++)
        {
            for (int distance = byte.MinValue; distance <= byte.MaxValue; distance++)
            {
                short expected = ReferenceMotherBrainNeckComponent(
                    rom,
                    unchecked((byte)angle),
                    unchecked((ushort)distance));
                AssertEqual(
                    expected,
                    MotherBrainNeckKinematics.CalculateSignedComponent(
                        unchecked((byte)angle),
                        unchecked((ushort)distance)),
                    $"Mother Brain neck component angle {angle:X2}, distance {distance:X2}");
            }

            ushort lowerAngle = unchecked((ushort)(angle << 8));
            ushort upperAngle = unchecked((ushort)((byte)(255 - angle) << 8));
            MotherBrainNeckGeometry expectedGeometry = ReferenceMotherBrainNeckGeometry(
                rom,
                bodyX: 0x0038,
                bodyY: 0x00b6,
                lowerAngle,
                upperAngle);
            MotherBrainNeckGeometry actualGeometry = MotherBrainNeckKinematics.CalculateGeometry(
                bodyX: 0x0038,
                bodyY: 0x00b6,
                lowerAngle,
                upperAngle);
            AssertEqual(expectedGeometry, actualGeometry,
                $"Mother Brain complete neck geometry lower angle {angle:X2}");
        }

        Console.WriteLine(
            "Mother Brain neck sine: all 65,536 signed products and 256 complete five-joint geometries match an independent ROM-backed reference without a runtime bus.");
    }

    private static MotherBrainNeckGeometry ReferenceMotherBrainNeckGeometry(
        SuperMetroidAddressSpace rom,
        ushort bodyX,
        ushort bodyY,
        ushort lowerAngle,
        ushort upperAngle)
    {
        ushort referenceX = unchecked((ushort)(bodyX - 0x0050));
        ushort referenceY = unchecked((ushort)(bodyY + 0x002e));
        byte lower = unchecked((byte)(lowerAngle >> 8));
        byte upper = unchecked((byte)(upperAngle >> 8));

        MotherBrainNeckPoint Lower(ushort distance) => new(
            unchecked((ushort)(referenceX + 0x0070 +
                ReferenceMotherBrainNeckComponent(rom, lower, distance))),
            unchecked((ushort)(referenceY - 0x0060 +
                ReferenceMotherBrainNeckComponent(
                    rom,
                    unchecked((byte)(lower + 0x40)),
                    distance))));

        MotherBrainNeckPoint segment0 = Lower(2);
        MotherBrainNeckPoint segment1 = Lower(10);
        MotherBrainNeckPoint segment2 = Lower(20);
        MotherBrainNeckPoint Upper(ushort distance) => new(
            unchecked((ushort)(segment2.X +
                ReferenceMotherBrainNeckComponent(rom, upper, distance))),
            unchecked((ushort)(segment2.Y +
                ReferenceMotherBrainNeckComponent(
                    rom,
                    unchecked((byte)(upper + 0x40)),
                    distance))));

        return new(
            segment0,
            segment1,
            segment2,
            Upper(10),
            Upper(20));
    }

    private static short ReferenceMotherBrainNeckComponent(
        SuperMetroidAddressSpace rom,
        byte angle,
        ushort distance)
    {
        int address = EnemyMathReferenceData.SignedSine + angle * 2;
        short sine = unchecked((short)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        return unchecked((short)(sine * unchecked((sbyte)distance) >> 8));
    }
}
