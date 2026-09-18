using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyNorfairLavaJumpDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        for (ushort selector = 0; selector < 4; selector++)
        {
            ushort random = unchecked((ushort)(selector << 9));
            AssertEqual(Word(0xa2be86 + selector * 2),
                NorfairLavaJumpDefinitions.InitialVerticalVelocity(random),
                $"Norfair lava jumper velocity {selector}");
        }

        ushort nextRandom = 0;
        int advances = 0;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new NorfairLavaJumpReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies, (Func<ushort>)(() =>
            {
                advances++;
                return nextRandom;
            }));
        var runMain = typeof(RoomEnemySystem).GetMethod(
                "RunNorfairLavaJumpingEnemyMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, NorfairLavaJumpingEnemyState>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];
        var state = new NorfairLavaJumpingEnemyState(slot);

        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            nextRandom = unchecked((ushort)random);
            advances = 0;
            state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
            state.YVelocity = 0;
            slot.Properties = 0;
            runMain(slot, state);

            AssertEqual(Word(0xa2be86 + ((random >> 8) & 6)), state.YVelocity,
                $"Norfair lava jumper production velocity {random:X4}");
            AssertEqual(NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch,
                state.Function, $"Norfair lava jumper production handoff {random:X4}");
            AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                $"Norfair lava jumper off-screen processing {random:X4}");
            AssertEqual((ushort?)0x000d, enemies.LastNorfairLavaJumpingEnemySoundEffect,
                $"Norfair lava jumper sound {random:X4}");
            AssertEqual(1, advances, $"Norfair lava jumper RNG advance {random:X4}");
        }

        Console.WriteLine(
            "Norfair lava-jump definitions: four native velocities and all 65,536 real RNG selections pass with the source table forbidden.");
    }

    private sealed class NorfairLavaJumpReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa2be86 and < 0xa2be8e
                ? throw new InvalidOperationException(
                    $"Norfair lava jumper attempted migrated velocity read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
