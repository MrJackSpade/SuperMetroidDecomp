using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Checks Mother Brain neck sine products and five-joint geometry against independent cartridge data.</summary>
internal static class MotherBrainNeckSineVerification
{
    /// <summary>Compares every byte angle and distance product, then a complete geometry for each lower-neck angle.</summary>
    internal static void VerifyMotherBrainNeckSine(SuperMetroidAddressSpace rom)
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

            ushort lowerAngle = SnesAngle.FromTableIndex(unchecked((byte)angle)).RawValue;
            ushort upperAngle = SnesAngle.FromTableIndex(unchecked((byte)(255 - angle))).RawValue;
            MotherBrainNeckGeometry expectedGeometry = ReferenceMotherBrainNeckGeometry(
                rom,
                body: new(0x0038, 0x00b6),
                lowerAngle,
                upperAngle);
            MotherBrainNeckGeometry actualGeometry = MotherBrainNeckKinematics.CalculateGeometry(body: new(0x0038, 0x00b6), lowerAngle, upperAngle, new MotherBrainNeckSegmentDistances());
            AssertEqual(expectedGeometry, actualGeometry,
                $"Mother Brain complete neck geometry lower angle {angle:X2}");
        }

        Console.WriteLine(
            "Mother Brain neck sine: all 65,536 signed products and 256 complete five-joint geometries match an independent ROM-backed reference without a runtime bus.");
    }

    /// <summary>Builds five neck joints using native body offsets and ROM-backed signed sine products.</summary>
    private static MotherBrainNeckGeometry ReferenceMotherBrainNeckGeometry(
        SuperMetroidAddressSpace rom,
        Position16<RoomPixels> body,
        ushort lowerAngle,
        ushort upperAngle)
    {
        ushort referenceX = unchecked((ushort)(body.X - 0x0050));
        ushort referenceY = unchecked((ushort)(body.Y + 0x002e));
        byte lower = SnesAngle.FromRaw(lowerAngle).TableIndex;
        byte upper = SnesAngle.FromRaw(upperAngle).TableIndex;

        Position16<RoomPixels> Lower(ushort distance) => new(
            unchecked((ushort)(referenceX + 0x0070 +
                ReferenceMotherBrainNeckComponent(rom, lower, distance))),
            unchecked((ushort)(referenceY - 0x0060 +
                ReferenceMotherBrainNeckComponent(
                    rom,
                    unchecked((byte)(lower + 0x40)),
                    distance))));

        Position16<RoomPixels> segment0 = Lower(2);
        Position16<RoomPixels> segment1 = Lower(10);
        Position16<RoomPixels> segment2 = Lower(20);
        Position16<RoomPixels> Upper(ushort distance) => new(
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

    /// <summary>Reads the native signed sine word and applies SNES 8.8 multiplication for the reference component.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain neck component")]
    private static short ReferenceMotherBrainNeckComponent(
        SuperMetroidAddressSpace rom,
        byte angle,
        ushort distance)
    {
        int address = EnemyMathReferenceData.SignedSine + angle * 2;
        short sine = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        return unchecked((short)(SnesEightEight.Multiply(sine, (sbyte)distance)));
    }
}
