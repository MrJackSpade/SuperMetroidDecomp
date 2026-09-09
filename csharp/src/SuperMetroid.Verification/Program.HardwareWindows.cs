using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHardwareWindows()
    {
        // Independent truth-table oracle for pinned ppu_getWindowState (ppu.c).
        // Bit index is first*2+second; do not call production's Boolean branches.
        int[] operations = [0b1110, 0b1000, 0b0110, 0b1001];
        (byte Left, byte Right)[] intervals = [(0,255), (32,96), (64,128), (80,80), (128,32), (255,255)];
        int cases = 0;
        foreach (var first in intervals)
        foreach (var second in intervals)
        for (int flags = 0; flags < 16; flags++)
        for (int operation = 0; operation < 4; operation++)
        for (int x = 0; x < 256; x++)
        {
            int a = x >= first.Left && x <= first.Right ? 1 : 0;
            int b = x >= second.Left && x <= second.Right ? 1 : 0;
            a ^= flags & 1;
            b ^= (flags >> 2) & 1;
            int enabled = ((flags >> 1) & 1) | ((flags >> 2) & 2);
            bool expected = enabled switch
            {
                0 => false,
                1 => a != 0,
                2 => b != 0,
                _ => ((operations[operation] >> (a * 2 + b)) & 1) != 0,
            };
            bool actual = SnesWindowMask.Contains((SnesWindowSelection)flags, (SnesWindowLogic)operation,
                (byte)x, first.Left, first.Right, second.Left, second.Right);
            if (actual != expected)
                throw new InvalidOperationException($"Window mismatch: flags={flags}, operation={operation}, x={x}, first={first}, second={second}.");
            cases++;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SnesWindowMask.Contains((SnesWindowSelection)16,
            SnesWindowLogic.Or, 0, 0, 0, 0, 0), "window evaluator rejects a packed byte masquerading as one nibble");
        Console.WriteLine($"Hardware windows: {cases} membership comparisons cover enable/invert, logic, inclusive edges and empty intervals.");
    }
}
