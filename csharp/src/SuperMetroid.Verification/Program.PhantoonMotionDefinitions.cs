using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledPhantoonMotion(SuperMetroidAddressSpace rom)
    {
        ushort[] definitions = [PhantoonMotionDefinitions.SlowFraction, PhantoonMotionDefinitions.SlowWhole,
            PhantoonMotionDefinitions.FastFraction, PhantoonMotionDefinitions.FastWhole,
            PhantoonMotionDefinitions.ForwardSlowCap, PhantoonMotionDefinitions.ForwardFastCap,
            PhantoonMotionDefinitions.ForwardMinimum, PhantoonMotionDefinitions.SlowFraction,
            PhantoonMotionDefinitions.SlowWhole, PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole, PhantoonMotionDefinitions.ReverseSlowCap,
            PhantoonMotionDefinitions.ReverseFastCap, PhantoonMotionDefinitions.ReverseMaximum];
        var native = new ushort[14];
        for (int i = 0; i < native.Length; i++)
        {
            int address = 0xa7cd73 + i * 2;
            native[i] = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native[i], definitions[i], "Phantoon compiled motion native word");
        }
        var forward = typeof(RoomEnemySystem).GetMethod("AdjustPhantoonForwardFigureEightSpeed", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot>>();
        var reverse = typeof(RoomEnemySystem).GetMethod("AdjustPhantoonReverseFigureEightSpeed", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot>>();
        var slot = new RoomEnemySystem().Slots[0];
        ushort[] phases = [0, 1, 2, 3, 0xffff];
        ushort[] fractions = [0, 1, 0x5ff, 0x600, 0xefff, 0xf000, 0xf9ff, 0xfa00, 0xffff];
        foreach (bool backwards in new[] { false, true })
        foreach (ushort phase in phases)
        foreach (ushort fraction in fractions)
        for (int whole = 0; whole <= ushort.MaxValue; whole++)
        {
            slot.VariableB = fraction;
            slot.VariableC = (ushort)whole;
            slot.VariableD = phase;
            (ushort low, ushort high, ushort next) = Expected(fraction, (ushort)whole, phase, backwards);
            (backwards ? reverse : forward)(slot);
            AssertEqual(low, slot.VariableB, "Phantoon real fractional speed after adjustment");
            AssertEqual(high, slot.VariableC, "Phantoon real signed whole speed after adjustment");
            AssertEqual(next, slot.VariableD, "Phantoon real acceleration phase transition");
        }
        Console.WriteLine("Phantoon motion: 14 native words and 5,898,240 real speed/phase adjustments match without a bus.");

        // Use the ROM records, wide arithmetic and explicit word extraction independently
        // of the production add/sub helpers. Signed comparisons intentionally wrap at 16 bits.
        (ushort Low, ushort High, ushort Phase) Expected(ushort low, ushort high, ushort phase, bool backwards)
        {
            int basis = backwards ? 7 : 0;
            bool slow = phase == 0;
            bool accelerating = slow || (phase & 1) != 0;
            int offset = basis + (slow ? 0 : 2);
            long delta = native[offset] + ((long)native[offset + 1] << 16);
            long value = low + ((long)high << 16);
            value += backwards == accelerating ? -delta : delta;
            low = unchecked((ushort)value);
            high = unchecked((ushort)(value >> 16));
            ushort cap = native[basis + (slow ? 4 : accelerating ? 5 : 6)];
            short difference = unchecked((short)(high - cap));
            bool reached = backwards == accelerating ? difference <= 0 : difference >= 0;
            if (reached)
            {
                low = 0;
                high = unchecked((ushort)(cap + (backwards ? slow ? 2 : accelerating ? 1 : 0 : slow ? -1 : accelerating ? 0 : 1)));
                phase = slow ? (ushort)1 : accelerating ? unchecked((ushort)(phase + 1)) : (ushort)0;
            }
            return (low, high, phase);
        }
    }
}
