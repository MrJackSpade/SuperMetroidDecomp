using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRidleyClawOffsets(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        short[] x = Enumerable.Range(0, 3).Select(i => Word(EnemyRomTablePointers.Ridley.ClawXOffsetWords + i * 2)).ToArray();
        short[] y = Enumerable.Range(0, 9).Select(i => Word(EnemyRomTablePointers.Ridley.ClawYOffsetWords + i * 2)).ToArray();
        AssertTrue(x.AsSpan().SequenceEqual(RidleyClawOffsets.X), "All authored claw X words");
        AssertTrue(y.AsSpan(0, 3).SequenceEqual(RidleyClawOffsets.Y), "Only three claw Y words are authored geometry");
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            AssertEqual(x[Math.Min(word, 2)], RidleyClawOffsets.ReadX((ushort)word), "Existing facing clamp preserved");
            AssertEqual(y[Math.Min(word >> 1, 8)], RidleyClawOffsets.ReadY((ushort)word), "Existing foot-index read window preserved");
        }
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        var carry = typeof(RoomEnemySystem).GetMethod("UpdateNorfairRidleyGrabbedSamus", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState>>();
        var overlaps = typeof(RoomEnemySystem).GetMethod("RidleyClawOverlapsSamus", flags)!
            .CreateDelegate<Func<RoomEnemySlot, RidleyEnemyState, SamusState, ushort, ushort, bool>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new RidleyEnemyState();
        var samus = new SamusState();
        static int Decay(short offset) => offset < 0 ? Math.Min(0, offset + 4) : Math.Max(0, offset - 4);
        for (ushort facing = 0; facing < 3; facing++)
        for (ushort feet = 0; feet < 6; feet++)
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            slot.XPosition = (ushort)raw;
            slot.YPosition = (ushort)(ushort.MaxValue - raw);
            state.FacingDirection = facing;
            state.FeetDistanceIndex = feet;
            state.GrabXOffset = (ushort)raw;
            state.GrabYOffset = (ushort)(ushort.MaxValue - raw);
            int dx = Decay(unchecked((short)state.GrabXOffset));
            int dy = Decay(unchecked((short)state.GrabYOffset));
            carry(slot, state, samus);
            AssertEqual(unchecked((ushort)(raw + x[facing] + dx)), samus.XPosition, "Actual carry X includes claw, signed decay and word wrap");
            AssertEqual(unchecked((ushort)(ushort.MaxValue - raw + y[feet >> 1] + dy)), samus.YPosition, "Actual carry Y includes animated claw, signed decay and word wrap");
            AssertEqual(unchecked((ushort)dx), state.GrabXOffset, "Carry X offset decays four pixels");
            AssertEqual(unchecked((ushort)dy), state.GrabYOffset, "Carry Y offset decays four pixels");
        }
        foreach (ushort world in new ushort[] { 0, 128, 65535 })
        for (ushort facing = 0; facing < 3; facing++)
        for (ushort feet = 0; feet < 6; feet++)
        for (int dx = -32; dx <= 32; dx++)
        for (int dy = -32; dy <= 32; dy++)
        {
            slot.XPosition = slot.YPosition = world;
            state.FacingDirection = facing;
            state.FeetDistanceIndex = feet;
            samus.XPosition = unchecked((ushort)(world + x[facing] + dx));
            samus.YPosition = unchecked((ushort)(world + y[feet >> 1] + dy));
            bool expected = Math.Abs(dx) < samus.Kinematics.XRadius + 8 && Math.Abs(dy) < samus.Kinematics.YRadius + 12;
            AssertEqual(expected, overlaps(slot, state, samus, 8, 12), "Actual claw collision boundaries including world wrap");
        }
        Console.WriteLine("Ridley claws: exact native read windows, all index words, 1179648 carry placements and 228150 collision probes pass through bus-free consumers.");
    }
}
