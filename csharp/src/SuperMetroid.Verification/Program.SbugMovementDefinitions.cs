using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifySbugMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa3a111;
        const int activationTable = 0xa3a121;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new SbugMovementReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeSbug", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var runMain = typeof(RoomEnemySystem).GetMethod("RunSbugMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState, RoomLevelData?>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        for (ushort directionIndex = 0; directionIndex < 16; directionIndex++)
        {
            int address = instructionTable + (directionIndex >> 1) * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, SbugMovementDefinitions.FacingInstructionList(directionIndex),
                $"compiled Sbug facing selector {directionIndex}");
        }

        for (byte behavior = 0; behavior <= (byte)SbugActivationBehavior.MoveAwayFromSamus; behavior++)
        {
            int address = activationTable + behavior * 2;
            var native = (SbugEnemyFunction)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, SbugMovementDefinitions.ActivationFunction((SbugActivationBehavior)behavior),
                $"compiled Sbug activation selector {behavior}");
        }

        for (ushort selector = 0; selector < 8; selector++)
        {
            byte angle = unchecked((byte)(0x30 + selector * 0x20));
            slot.Parameter1 = (ushort)(angle << 8 | 1);
            slot.Parameter2 = 1;
            initialize(slot);

            ushort native = (ushort)(rom.ReadByte(instructionTable + selector * 2) |
                rom.ReadByte(instructionTable + selector * 2 + 1) << 8);
            SbugEnemyState state = enemies.SbugStates[0]!;
            AssertEqual(unchecked((ushort)(selector * 2)), state.ForwardInstructionIndex,
                $"Sbug production facing index {selector}");
            AssertEqual(native, slot.CurrentInstruction,
                $"Sbug production facing instruction {selector}");
        }

        var samus = new SamusState
        {
            XPosition = 0x1234,
            YPosition = 0x5678,
        };
        slot.XPosition = samus.XPosition;
        slot.YPosition = samus.YPosition;
        for (byte behavior = 0; behavior <= (byte)SbugActivationBehavior.MoveAwayFromSamus; behavior++)
        {
            SbugEnemyState state = enemies.SbugStates[0]!;
            state.Function = SbugEnemyFunction.WaitForSamus;
            slot.Parameter2 = (ushort)(behavior << 8 | 1);
            runMain(slot, samus, null);

            int address = activationTable + behavior * 2;
            var native = (SbugEnemyFunction)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, state.Function, $"Sbug production activation {behavior}");
        }

        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.FacingInstructionList(16),
            "Sbug facing selector beyond authored table");
        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.FacingInstructionList(ushort.MaxValue),
            "Sbug restored facing selector does not read adjacent code");
        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.ActivationFunction((SbugActivationBehavior)7),
            "Sbug activation selector beyond authored table");

        Console.WriteLine(
            "Sbug movement definitions: eight facing lists, seven activation callbacks and 15 production dispatches pass with table reads forbidden.");
    }

    private sealed class SbugMovementReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa3a111 and < 0xa3a12f
                ? throw new InvalidOperationException(
                    $"Sbug movement attempted migrated selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
