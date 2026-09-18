using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBotwoonHealthPaletteDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
            bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

        for (ushort phase = 0;
             phase < BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
             phase += 2)
        {
            ushort threshold = ReadWord(
                rom,
                BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase);
            for (int health = 0; health <= ushort.MaxValue; health++)
            {
                bool expected = unchecked((short)((ushort)health - threshold)) < 0;
                AssertEqual(expected,
                    BotwoonHealthPaletteDefinitions.ShouldAdvance(phase, (ushort)health),
                    $"Botwoon phase ${phase:X2} health {health} threshold decision");
            }
        }

        foreach (ushort invalid in new ushort[] { 1, 3, 15, 16, 18, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(
                () => BotwoonHealthPaletteDefinitions.ShouldAdvance(invalid, 0),
                $"Botwoon rejects restored palette phase ${invalid:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new BotwoonHealthThresholdReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        var update = typeof(RoomEnemySystem).GetMethod(
            "UpdateBotwoonHealthPalette",
            flags)!.CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState>>(enemies);
        RoomEnemySlot head = enemies.Slots[0];
        var state = new BotwoonEnemyState(head)
        {
            // Sprite palette seven starts at CGRAM color 240 / byte offset $01E0.
            PaletteDestinationByteOffset = 0x01e0,
        };

        for (ushort phase = 0;
             phase < BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
             phase += 2)
        {
            ushort threshold = ReadWord(
                rom,
                BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase);
            state.PalettePhaseByteOffset = phase;
            head.Health = threshold;
            update(head, state);
            AssertEqual(phase, state.PalettePhaseByteOffset,
                $"production Botwoon phase ${phase:X2} holds at threshold");

            state.PalettePhaseByteOffset = phase;
            head.Health = unchecked((ushort)(threshold - 1));
            update(head, state);
            AssertEqual(unchecked((ushort)(phase + 2)), state.PalettePhaseByteOffset,
                $"production Botwoon phase ${phase:X2} advances below threshold");
        }

        state.PalettePhaseByteOffset =
            BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
        update(head, state);
        AssertEqual(BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset,
            state.PalettePhaseByteOffset,
            "production Botwoon completed palette phase remains terminal");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Botwoon palette progression performs no threshold reads");

        foreach (ushort invalid in new ushort[] { 1, 18 })
        {
            state.PalettePhaseByteOffset = invalid;
            AssertThrows<InvalidDataException>(
                () => update(head, state),
                $"production Botwoon rejects restored palette phase ${invalid:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "malformed Botwoon palette phase performs no threshold reads");

        Console.WriteLine(
            "Botwoon health palette definitions: eight native thresholds and all " +
            "524,288 signed health/phase decisions match; real hold, one-band advance, " +
            "terminal, and malformed paths run with the threshold table forbidden.");
    }

    private sealed class BotwoonHealthThresholdReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= BotwoonHealthPaletteDefinitions.NativeThresholdAddress &&
                address < BotwoonHealthPaletteDefinitions.NativeThresholdAddress + 16)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Botwoon palette progression attempted threshold read ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
