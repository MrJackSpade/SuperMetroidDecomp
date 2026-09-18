using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyElevatorInputDefinitions(SuperMetroidAddressSpace rom)
    {
        const int inputTable = 0xa394e2;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (ushort direction = 0; direction < 2; direction++)
        {
            ushort tableByteOffset = (ushort)(direction * 2);
            ushort native = (ushort)(rom.ReadByte(inputTable + tableByteOffset) |
                rom.ReadByte(inputTable + tableByteOffset + 1) << 8);
            AssertEqual(native, ElevatorActorDefinitions.RequiredDirectionInput(tableByteOffset),
                $"compiled elevator direction input {direction}");

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new ElevatorInputReadGuard(rom));
            typeof(RoomEnemySystem).GetField("_isAreaBossDefeated", flags)!.SetValue(
                enemies,
                (Func<bool>)(() => true));
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeElevator", flags)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState>>(enemies);
            var wait = typeof(RoomEnemySystem).GetMethod("WaitForElevatorDirectionInput", flags)!
                .CreateDelegate<Action<RoomEnemySlot, ElevatorEnemyState, SamusState, ushort,
                    SamusProjectileSystem?>>(enemies);

            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = direction;
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0180;
            var samus = new SamusState();
            initialize(slot, samus);
            AssertEqual(tableByteOffset, enemies.ElevatorStates[0]!.DirectionTableByteOffset,
                $"elevator doubled direction offset {direction}");

            enemies.PublishElevatorDoorContact();
            wait(slot, enemies.ElevatorStates[0]!, samus, native, null);
            AssertEqual(ElevatorActorStatus.Departing, enemies.ElevatorStatus,
                $"elevator departure input {direction}");
            AssertEqual(ElevatorFrameEvent.DepartureStarted, enemies.LastElevatorEvent,
                $"elevator departure event {direction}");
            AssertEqual(true, samus.InputLocked, $"elevator Samus input lock {direction}");
            AssertEqual(2, enemies.SoundRequests.Count, $"elevator departure sounds {direction}");
        }

        AssertThrows<InvalidDataException>(
            () => ElevatorActorDefinitions.RequiredDirectionInput(1),
            "elevator odd input-table offset");
        AssertThrows<InvalidDataException>(
            () => ElevatorActorDefinitions.RequiredDirectionInput(4),
            "elevator input-table offset beyond authored words");

        Console.WriteLine(
            "Elevator input definitions: both native masks and real departure paths pass with table reads forbidden.");
    }

    private sealed class ElevatorInputReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa394e2 and < 0xa394e6
            ? throw new InvalidOperationException(
                $"Elevator actor attempted migrated input-mask read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
