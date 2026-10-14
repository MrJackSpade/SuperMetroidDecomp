using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain fake-death background draw geometry and collision words.</summary>
internal static class LookupMotherBrainBackgroundVerification
{
    /// <summary>Checks the 12 native background rows, descriptor ownership, dimensions, offsets, and column bounds.</summary>
    internal static void VerifyMotherBrainBackgroundGeometry(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers = [0x9505,0x9523,0x9541,0x955f,0x957d,0x959b,0x95b9,0x95d7,0x95f5,0x9613,0x9631,0x964f];
        var exports = MotherBrainFakeDeathPlmDrawDefinitions.All.Where(draw => pointers.Contains(draw.Pointer)).ToArray();
        AssertEqual(12, exports.Length, "Mother Brain background export count");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            int row = Array.IndexOf(pointers, (ushort)value);
            AssertEqual(row >= 0, MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBackground((ushort)value, out var shape),
                "Mother Brain background ownership");
            if (row < 0)
            {
                AssertEqual(default(MotherBrainFakeDeathPlmDrawDefinitions.BackgroundDraw), shape, "Mother Brain missing background descriptor");
                continue;
            }
            AssertEqual(row, shape.Row, "Mother Brain native row order");
            AssertEqual((ushort)value, exports[row].Pointer, "Mother Brain background export order");
            MotherBrainFakeDeathPlmDrawDefinitions.TryGet((ushort)value, out var dto);
            foreach (var draw in new[] {dto, exports[row]})
            {
                AssertEqual(1, draw.Runs.Length, "Mother Brain single background row");
                var run = draw.Runs.Span[0];
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | value), run.DirectionAndCount, "Mother Brain native background count/direction");
                AssertEqual(13, run.LevelWords.Length, "Mother Brain background width");
                AssertEqual(unchecked((sbyte)MotherBrainBackgroundGeometryRomByte(rom, 0x840000 | (value + 28))), run.Next.X, "Mother Brain background native X");
                AssertEqual(unchecked((sbyte)MotherBrainBackgroundGeometryRomByte(rom, 0x840000 | (value + 29))), run.Next.Y, "Mother Brain background native Y");
            }
            foreach (int bad in new[] {int.MinValue,-1,13,int.MaxValue})
                AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(bad), "Mother Brain background column bounds");
        }
    }

    /// <summary>Reads one cartridge byte as the independent row-offset oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain background geometry")]
    private static byte MotherBrainBackgroundGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that every background cell keeps the ROM's collision and visual word intact.</summary>
    internal static void VerifyMotherBrainBackgroundCollision(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers = [0x9505,0x9523,0x9541,0x955f,0x957d,0x959b,0x95b9,0x95d7,0x95f5,0x9613,0x9631,0x964f];
        foreach (ushort pointer in pointers)
        {
            MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBackground(pointer, out var shape);
            MotherBrainFakeDeathPlmDrawDefinitions.TryGet(pointer, out var dto);
            for (int column = 0; column < 13; column++)
            {
                ushort original = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 2 + column * 2));
                AssertEqual(original, shape.WordAt(column), "Mother Brain original collision and preserved visual bits");
                AssertEqual(original, dto.Runs.Span[0].LevelWords.Span[column], "Mother Brain DTO complete native cell");
            }
        }
    }
}
