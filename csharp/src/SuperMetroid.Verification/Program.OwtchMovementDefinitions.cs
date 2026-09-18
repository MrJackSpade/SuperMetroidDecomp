using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyOwtchMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int distanceTable = 0xa2a3dd;
        const int timerTable = 0xa2a3ed;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (byte index = 0; index < 8; index++)
        {
            int address = distanceTable + index * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, OwtchMovementDefinitions.TravelDistance(index),
                $"compiled Owtch travel distance {index}");
        }
        for (byte index = 0; index < 6; index++)
        {
            int address = timerTable + index * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, OwtchMovementDefinitions.UndergroundTimer(index),
                $"compiled Owtch underground timer {index}");
        }

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

        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.TravelDistance(8),
            "Owtch travel distance beyond authored table");
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.UndergroundTimer(6),
            "Owtch underground timer beyond authored table");
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.TravelDistance(byte.MaxValue),
            "Owtch restored travel selector does not read adjacent code");
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.UndergroundTimer(byte.MaxValue),
            "Owtch restored timer selector does not read adjacent code");

        Console.WriteLine(
            "Owtch movement definitions: fourteen native words and 144 wrapped production initializers pass with table reads forbidden.");
    }

    private sealed class OwtchMovementReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa2a3dd and < 0xa2a3f9
            ? throw new InvalidOperationException(
                $"Owtch movement attempted migrated definition read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
