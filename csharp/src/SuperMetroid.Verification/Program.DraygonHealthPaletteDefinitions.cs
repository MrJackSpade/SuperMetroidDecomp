using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDraygonHealthPaletteDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
            bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

        ushort[] thresholds = new ushort[8];
        for (int index = 0; index < thresholds.Length; index++)
        {
            thresholds[index] = ReadWord(
                rom,
                DraygonHealthPaletteDefinitions.NativeThresholdAddress + index * 2);
        }
        AssertEqual((ushort)0xffff,
            ReadWord(rom, DraygonHealthPaletteDefinitions.NativeThresholdAddress + 16),
            "Draygon health threshold native terminator");

        for (ushort health = 0;
             health <= DraygonHealthPaletteDefinitions.MaximumAuthoredHealth;
             health++)
        {
            int expectedIndex = Array.FindIndex(
                thresholds,
                threshold => unchecked((short)(health - threshold)) >= 0);
            AssertTrue(expectedIndex >= 0, $"Draygon health {health} matches a native band");
            AssertEqual(unchecked((ushort)(expectedIndex * 2)),
                DraygonHealthPaletteDefinitions.ByteIndexForHealth(health),
                $"Draygon health {health} compiled palette band");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonHealthPaletteDefinitions.ByteIndexForHealth(6001),
            "Draygon rejects health above its authored enemy-header maximum");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new DraygonHealthThresholdReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        var update = typeof(RoomEnemySystem).GetMethod(
            "UpdateDraygonHealthPalette",
            flags)!.CreateDelegate<Action<DraygonEnemyState>>(enemies);
        var state = new DraygonEnemyState(enemies.Slots[0]);

        ushort[] productionHealth =
            [6000, 5250, 5249, 4500, 4499, 3750, 3749, 3000, 2999,
             2250, 2249, 1500, 1499, 750, 749, 1, 0];
        foreach (ushort health in productionHealth)
        {
            state.Body.Health = health;
            state.HealthPaletteTableByteIndex = ushort.MaxValue;
            update(state);
            AssertEqual(
                DraygonHealthPaletteDefinitions.ByteIndexForHealth(health),
                state.HealthPaletteTableByteIndex,
                $"production Draygon health {health} palette band");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Draygon palette selection performs no threshold-table reads");

        state.Body.Health = 6001;
        state.HealthPaletteTableByteIndex = ushort.MaxValue;
        AssertThrows<InvalidDataException>(
            () => update(state),
            "production Draygon rejects malformed restored health without a ROM fallback");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "malformed Draygon health performs no threshold-table read");

        Console.WriteLine(
            "Draygon health palette definitions: all eight reachable thresholds, 6,001 " +
            "authored health values and every band boundary pass; the production selector " +
            "runs with the threshold table and terminator forbidden.");
    }

    private sealed class DraygonHealthThresholdReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= DraygonHealthPaletteDefinitions.NativeThresholdAddress &&
                address < DraygonHealthPaletteDefinitions.NativeThresholdAddress + 18)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Draygon palette selection attempted threshold read ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
