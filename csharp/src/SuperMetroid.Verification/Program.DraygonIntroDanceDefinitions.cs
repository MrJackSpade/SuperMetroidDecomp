using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDraygonIntroDanceDefinitions(SuperMetroidAddressSpace rom)
    {
        const int source = 0xa5a19f;
        for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
        {
            int address = source + (slotIndex - 28) * 2;
            short expected = unchecked((short)(
                rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
            AssertEqual(expected,
                DraygonIntroDanceDefinitions.MovementLatencyForSlot(slotIndex),
                $"Draygon intro Evir slot {slotIndex} latency");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDanceDefinitions.MovementLatencyForSlot(27),
            "Draygon intro Evir slot before native range");
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDanceDefinitions.MovementLatencyForSlot(32),
            "Draygon intro Evir slot after native range");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new DraygonIntroLatencyReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0));
        var run = typeof(RoomEnemySystem).GetMethod(
            "RunDraygonIntroDance",
            flags)!.CreateDelegate<Action<DraygonEnemyState, SamusState?, byte>>(enemies);
        var state = new DraygonEnemyState(enemies.Slots[0]);
        for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
        {
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[slotIndex];
            sprite.Kind = RoomSpriteObjectKind.DraygonIntroEvir;
            sprite.InstructionPointer = 1;
            sprite.XPosition = 0x0100;
            sprite.YPosition = 0x0180;
        }

        for (int frame = 0; frame < 128; frame++)
        {
            run(state, null, 1);
            for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
            {
                AssertEqual((ushort)0x0100, enemies.RoomSpriteObjects[slotIndex].XPosition,
                    $"Draygon intro inert frame {frame} slot {slotIndex} X");
                AssertEqual((ushort)0x0180, enemies.RoomSpriteObjects[slotIndex].YPosition,
                    $"Draygon intro inert frame {frame} slot {slotIndex} Y");
            }
        }
        AssertEqual((ushort)0x0200, state.FightIntroDanceIndex,
            "Draygon intro exact 128-frame latency");

        run(state, null, 1);
        for (int slotIndex = 28; slotIndex <= 30; slotIndex++)
            AssertEqual((ushort)0x0100, enemies.RoomSpriteObjects[slotIndex].XPosition,
                $"Draygon intro first active frame preserves slot {slotIndex}");
        AssertEqual((ushort)0x0103, enemies.RoomSpriteObjects[31].XPosition,
            "Draygon intro first active Evir uses stream X delta");
        AssertEqual((ushort)0x0180, enemies.RoomSpriteObjects[31].YPosition,
            "Draygon intro first active Evir uses stream Y delta");

        Console.WriteLine(
            "Draygon intro dance definitions: all four native latency words and the real " +
            "128-frame inert-to-first-movement handoff pass with latency reads forbidden.");
    }

    private sealed class DraygonIntroLatencyReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa5a19f and < 0xa5a1a7
            ? throw new InvalidOperationException(
                $"Draygon intro attempted migrated latency read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
