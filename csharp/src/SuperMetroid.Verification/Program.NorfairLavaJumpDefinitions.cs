using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the four native jump velocities and exhaustively verifies production RNG selection while forbidding reads from their source table.</summary>
    /// <param name="rom">Retail cartridge address space used for the expected velocity words and unrelated enemy reads.</param>
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

    /// <summary>Wraps an address space and fails if the production enemy path reads the migrated lava-jump velocity table.</summary>
    /// <param name="source">Address space that supplies all reads outside the guarded velocity-table range and receives writes.</param>
    private sealed class NorfairLavaJumpReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-source reads through the velocity-table guard.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The source byte when the address is outside the guarded velocity table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated velocity-table range and delegates other addresses to the source.</summary>
        /// <param name="address">Address of the byte to read.</param>
        /// <returns>The byte supplied by the wrapped address space outside the guarded range.</returns>
        public byte ReadByte(int address) =>
            address is >= 0xa2be86 and < 0xa2be8e
                ? throw new InvalidOperationException(
                    $"Norfair lava jumper attempted migrated velocity read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Address where the byte is written.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
