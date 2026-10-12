using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ridley claw offsets")]
    private static void VerifyRidleyClawOffsets(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        short[] x = Enumerable.Range(0, 3).Select(i => Word(EnemyRomTablePointers.Ridley.ClawXOffsetWords + i * 2)).ToArray();
        short[] y = Enumerable.Range(0, 9).Select(i => Word(EnemyRomTablePointers.Ridley.ClawYOffsetWords + i * 2)).ToArray();
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
            AssertEqual(x[(int)facing], RidleyClawOffsets.ReadX(facing), "All authored claw X words");
        foreach (RidleyFacing undefined in new[] { (RidleyFacing)3, (RidleyFacing)ushort.MaxValue })
            AssertThrows<InvalidOperationException>(() => RidleyClawOffsets.ReadX(undefined), "Undefined Ridley facing has no claw X word");
        for (ushort index = 0; index < 3; index++)
            AssertEqual(y[index], RidleyClawOffsets.ReadY((ushort)(index * 2)), "Only three claw Y words are authored geometry");
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            AssertEqual(y[Math.Min(word >> 1, 8)], RidleyClawOffsets.ReadY((ushort)word), "Existing foot-index read window preserved");
        }
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        var carry = typeof(RoomEnemySystem).GetMethod("UpdateNorfairRidleyGrabbedSamus", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState>>();
        var overlaps = typeof(RoomEnemySystem).GetMethod("RidleyClawOverlapsSamus", flags)!
            .CreateDelegate<Func<RoomEnemySlot, RidleyEnemyState, SamusState, Extent16<RoomPixels>, bool>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new RidleyEnemyState();
        var samus = new SamusState();
        static int Decay(short offset) => offset < 0 ? Math.Min(0, offset + 4) : Math.Max(0, offset - 4);
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
        for (ushort feet = 0; feet < 6; feet++)
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            slot.Position = slot.Position.WithWhole(new((ushort)raw, (ushort)(ushort.MaxValue - raw)));
            state.FacingDirection = facing;
            state.FeetDistanceIndex = feet;
            state.GrabOffset = new Offset16<RoomPixels>(unchecked((short)raw), unchecked((short)(ushort.MaxValue - raw)));
            int dx = Decay(unchecked((short)state.GrabOffset.X));
            int dy = Decay(unchecked((short)state.GrabOffset.Y));
            carry(slot, state, samus);
            AssertEqual(unchecked((ushort)(raw + x[(int)facing] + dx)), samus.Position.X.Whole, "Actual carry X includes claw, signed decay and word wrap");
            AssertEqual(unchecked((ushort)(ushort.MaxValue - raw + y[feet >> 1] + dy)), samus.Position.Y.Whole, "Actual carry Y includes animated claw, signed decay and word wrap");
            AssertEqual(unchecked((short)dx), state.GrabOffset.X, "Carry X offset decays four pixels");
            AssertEqual(unchecked((short)dy), state.GrabOffset.Y, "Carry Y offset decays four pixels");
        }
        foreach (ushort world in new ushort[] { 0, 128, 65535 })
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
        for (ushort feet = 0; feet < 6; feet++)
        for (int dx = -32; dx <= 32; dx++)
        for (int dy = -32; dy <= 32; dy++)
        {
            slot.Position = slot.Position.WithWhole(new(world, world));
            state.FacingDirection = facing;
            state.FeetDistanceIndex = feet;
            samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(world + x[(int)facing] + dx)), unchecked((ushort)(world + y[feet >> 1] + dy))));
            bool expected = Math.Abs(dx) < samus.Kinematics.Radius.X + 8 && Math.Abs(dy) < samus.Kinematics.Radius.Y + 12;
            AssertEqual(expected, overlaps(slot, state, samus, new Extent16<RoomPixels>(8, 12)), "Actual claw collision boundaries including world wrap");
        }
        Console.WriteLine("Ridley claws: exact native read windows, all index words, 1179648 carry placements and 228150 collision probes pass through bus-free consumers.");
    }
}
