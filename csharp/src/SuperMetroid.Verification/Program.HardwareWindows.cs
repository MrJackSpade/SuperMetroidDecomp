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
        VerifyPackedWindowRegisters();
        Console.WriteLine($"Hardware windows: {cases} membership comparisons cover enable/invert, logic, inclusive edges and empty intervals.");
    }

    private static void VerifyPackedWindowRegisters()
    {
        for (int selected = 0; selected < 6; selected++)
        for (int flags = 0; flags < 16; flags++)
        for (int operation = 0; operation < 4; operation++)
        {
            byte selection = (byte)(flags << ((selected & 1) * 4));
            byte logic = (byte)(operation << ((selected < 4 ? selected : selected - 4) * 2));
            var registers = new SnesWindowRegisters(selected < 2 ? selection : (byte)0,
                selected is 2 or 3 ? selection : (byte)0, selected >= 4 ? selection : (byte)0,
                32, 96, 64, 128, selected < 4 ? logic : (byte)0, selected >= 4 ? logic : (byte)0);
            for (int x = 0; x < 256; x++)
            {
                bool expected = SnesWindowMask.Contains((SnesWindowSelection)flags, (SnesWindowLogic)operation,
                    (byte)x, 32, 96, 64, 128);
                for (int target = 0; target < 6; target++)
                    if (registers.Contains((SnesWindowTarget)target, (byte)x) != (target == selected && expected))
                        throw new InvalidOperationException($"Packed window target contamination: selected={selected}, target={target}, flags={flags}, operation={operation}, x={x}.");
                var expectedMask = selected < 5 && expected ? (SnesMainScreenLayers)(1 << selected) : SnesMainScreenLayers.None;
                AssertEqual(expectedMask, registers.MaskedLayers((byte)x, (SnesMainScreenLayers)31), "TMW/TSW layer identity");
                AssertEqual(SnesMainScreenLayers.None, registers.MaskedLayers((byte)x, SnesMainScreenLayers.None),
                    "disabled TMW/TSW leaves membership unconsumed");
            }
        }
    }
}
