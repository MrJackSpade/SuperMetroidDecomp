using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNailContourMappingsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks native Kraid body contour selection and nail reflection across wrapped positions.</summary>
internal static class KraidNailContourVerification
{
    /// <summary>Runs signed edge and velocity probes, including reads into adjacent contour words.</summary>
    internal static void VerifyKraidNailContour(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(SnesLittleEndian.Word(KraidNailContourRomByte(rom, a), KraidNailContourRomByte(rom, a + 1)));
        Suite(nameof(VerifyKraidNailLeftOffsets), () => VerifyKraidNailLeftOffsets(rom));
        Suite(nameof(VerifyKraidNailTopBoundaries), () => VerifyKraidNailTopBoundaries(rom));
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
        Func<RoomEnemySlot, bool> reflect = enemies.KraidBehavior.KraidBodyContourReflectsNail;
        var body = enemies.SlotTable.Slots[0];
        var nail = enemies.SlotTable.Slots[6];
        nail.Radius = nail.Radius.WithX(4);
        int overreadCases = 0;
        var histogram = new int[6];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            body.Position = body.Position.WithWhole(new((ushort)raw, 0x1234));
            nail.Position = nail.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + raw)));
            int record = 0;
            // Follow the native ROM walk independently, not the compiled span bound.
            while (unchecked((short)(Word(0xa7bf1f + record * 4) - raw)) >= 0)
            {
                record++;
                if (record >= 256) throw new InvalidOperationException("Native contour diagnostic did not terminate.");
            }
            AssertTrue(record < 6, "Every native relative-Y walk terminates within six records");
            histogram[record]++;
            if (record >= 4) overreadCases++;
            ushort nativeLeft = Word(0xa7bf1d + record * 4);

            ushort edge = unchecked((ushort)(body.Position.X.Whole + nativeLeft));
            foreach (ushort velocity in new ushort[] { 0, 1, 0x7fff, 0x8000, 0xffff })
            for (int delta = -1; delta <= 1; delta++)
            {
                nail.Position = nail.Position.WithWholeX(unchecked((ushort)(edge - nail.Radius.X + delta)));
                nail.VariableC = velocity;
                bool expected = unchecked((short)(nail.Position.X.Whole + nail.Radius.X - edge)) >= 0 && (short)velocity >= 0;
                AssertEqual(expected, reflect(nail), "Actual contour reflection retains signed edge and velocity gates");
            }
        }
        AssertTrue(overreadCases > 0, "Fixture exercises adjacent instruction words, not just authored geometry");
        int[] expectedHistogram = [32768, 56, 56, 32, 20928, 11696];
        for (int i = 0; i < histogram.Length; i++) AssertEqual(expectedHistogram[i], histogram[i], "Native contour selection distribution");
        Console.WriteLine($"Kraid nail contour: 983040 actual edge/direction probes pass bus-free; {overreadCases} relative-Y words select adjacent records.");
    }

    /// <summary>Reads the retail contour as an independent oracle for the compiled lookup.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid nail contour")]
    private static byte KraidNailContourRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
