using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks exhaustive ownership and exported geometry for the twelve Mother Brain background rows against their native PLM records.</summary>
    /// <param name="rom">Retail address space supplying each row's native count, direction, and continuation offsets.</param>
    private static void VerifyMotherBrainBackgroundGeometry(SuperMetroidAddressSpace rom)
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
                AssertEqual(unchecked((sbyte)rom.ReadByte(0x840000 | (value + 28))), run.NextX, "Mother Brain background native X");
                AssertEqual(unchecked((sbyte)rom.ReadByte(0x840000 | (value + 29))), run.NextY, "Mother Brain background native Y");
            }
            foreach (int bad in new[] {int.MinValue,-1,13,int.MaxValue})
                AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(bad), "Mother Brain background column bounds");
        }
    }

    /// <summary>Verifies that every compiled Mother Brain background cell preserves the complete native level word, including collision and visual bits.</summary>
    /// <param name="rom">Retail address space containing the twelve thirteen-cell background rows.</param>
    private static void VerifyMotherBrainBackgroundCollision(SuperMetroidAddressSpace rom)
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
