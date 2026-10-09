using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares Owtch movement tables with retail and verifies production initialization across wrapped spawn positions and parameter combinations.</summary>
    /// <param name="rom">Retail address space providing expected movement-table values.</param>
    private static void VerifyOwtchMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        Suite(nameof(VerifyOwtchTravelDistanceAlgorithm), () => VerifyOwtchTravelDistanceAlgorithm(rom));
        Suite(nameof(VerifyOwtchUndergroundTimerAlgorithm), () => VerifyOwtchUndergroundTimerAlgorithm(rom));

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new OwtchMovementReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeOwtch", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        foreach (ushort spawnX in new ushort[] { 0, 0x0100, ushort.MaxValue })
        {
            for (byte timerIndex = 0; timerIndex < 6; timerIndex++)
            {
                for (byte distanceIndex = 0; distanceIndex < 8; distanceIndex++)
                {
                    byte initialState = (byte)((timerIndex + distanceIndex) % 5);
                    slot.Parameter1 = (ushort)(timerIndex << 8 | initialState);
                    slot.Parameter2 = (ushort)(distanceIndex << 8);
                    slot.XPosition = spawnX;
                    slot.YPosition = 0x0100;
                    initialize(slot);

                    OwtchEnemyState state = enemies.OwtchStates[0]!;
                    ushort distance = OwtchMovementDefinitions.TravelDistance(distanceIndex);
                    AssertEqual(OwtchMovementDefinitions.UndergroundTimer(timerIndex),
                        state.UndergroundTimer,
                        $"Owtch production timer {spawnX:X4}/{timerIndex}/{distanceIndex}");
                    AssertEqual(unchecked((ushort)(spawnX - distance)), state.MinimumXPosition,
                        $"Owtch minimum X {spawnX:X4}/{timerIndex}/{distanceIndex}");
                    AssertEqual(unchecked((ushort)(spawnX + distance)), state.MaximumXPosition,
                        $"Owtch maximum X {spawnX:X4}/{timerIndex}/{distanceIndex}");
                }
            }
        }


        Console.WriteLine(
            "Owtch movement definitions: fourteen native words and 144 wrapped production initializers pass with table reads forbidden.");
    }

    /// <summary>Checks the eight compiled travel-distance entries against retail and confirms invalid indices are rejected.</summary>
    /// <param name="rom">Retail address space containing the native distance table.</param>
    private static void VerifyOwtchTravelDistanceAlgorithm(SuperMetroidAddressSpace rom)
    {
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.TravelDistance(8), "Owtch distance upper bound");
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.TravelDistance(byte.MaxValue), "Owtch distance maximum invalid input");
        for (byte index = 0; index < 8; index++)
        {
            int address = OwtchMovementDefinitions.DistanceReferenceAddress + index * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, OwtchMovementDefinitions.TravelDistance(index),
                $"compiled Owtch travel distance {index}");
        }
    }

    /// <summary>Checks the six compiled underground-timer entries against retail and confirms invalid indices are rejected.</summary>
    /// <param name="rom">Retail address space containing the native timer table.</param>
    private static void VerifyOwtchUndergroundTimerAlgorithm(SuperMetroidAddressSpace rom)
    {
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.UndergroundTimer(6), "Owtch timer upper bound");
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.UndergroundTimer(byte.MaxValue), "Owtch timer maximum invalid input");
        for (byte index = 0; index < 6; index++)
        {
            int address = OwtchMovementDefinitions.TimerReferenceAddress + index * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, OwtchMovementDefinitions.UndergroundTimer(index),
                $"compiled Owtch underground timer {index}");
        }

    }

    /// <summary>Wraps an address space and rejects runtime reads from the Owtch movement tables migrated into compiled definitions.</summary>
    /// <param name="source">Underlying address space for permitted reads and forwarded writes.</param>
    private sealed class OwtchMovementReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the migrated-table guard.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The source byte for an address outside the protected Owtch table range.</returns>
        /// <exception cref="InvalidOperationException">The importer attempted to read migrated Owtch movement data.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Reads a byte from the wrapped space unless it belongs to the compiled Owtch movement tables.</summary>
        /// <param name="address">Address requested by the production initializer.</param>
        /// <returns>The source byte when the address is not protected.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to migrated movement definitions.</exception>
        public byte ReadByte(int address) => address is >= 0xa2a3dd and < 0xa2a3f9
            ? throw new InvalidOperationException(
                $"Owtch movement attempted migrated definition read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards writes unchanged to the underlying address space.</summary>
        /// <param name="address">Address to write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
