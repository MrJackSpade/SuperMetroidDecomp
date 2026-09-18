using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEscapeEtecoonDefinitions(SuperMetroidAddressSpace rom)
    {
        const int xTable = 0xb3e718;
        const int yTable = 0xb3e71e;
        const int preInstructionTable = 0xb3e724;
        const int instructionTable = 0xb3e72a;
        const int speedTable = 0xb3e730;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        for (int role = 0; role < 3; role++)
        {
            ushort parameter = unchecked((ushort)(role * 2));
            EscapeEtecoonInitialization initialization =
                EscapeEtecoonDefinitions.Initialization(rom, parameter);
            AssertEqual(ReadWord(rom, xTable + parameter), initialization.XPosition,
                $"escape Etecoon role {role} X");
            AssertEqual(ReadWord(rom, yTable + parameter), initialization.YPosition,
                $"escape Etecoon role {role} Y");
            AssertEqual((EscapeEtecoonPreInstruction)ReadWord(
                    rom,
                    preInstructionTable + parameter),
                initialization.PreInstruction,
                $"escape Etecoon role {role} pre-instruction");
            AssertEqual(ReadWord(rom, instructionTable + parameter),
                initialization.InstructionList,
                $"escape Etecoon role {role} instruction");
            AssertEqual(ReadWord(rom, speedTable + parameter),
                initialization.HorizontalSpeed,
                $"escape Etecoon role {role} speed");
        }

        // Parameter six is the first non-authored selector. Its five native reads overlap
        // subsequent tables and code differently, so compare the deliberate fallback with
        // the old unchecked address arithmetic rather than normalizing it to role zero.
        EscapeEtecoonInitialization adjacent =
            EscapeEtecoonDefinitions.Initialization(rom, 6);
        AssertEqual(ReadWord(rom, xTable + 6), adjacent.XPosition,
            "escape Etecoon adjacent X");
        AssertEqual(ReadWord(rom, yTable + 6), adjacent.YPosition,
            "escape Etecoon adjacent Y");
        AssertEqual((EscapeEtecoonPreInstruction)ReadWord(rom, preInstructionTable + 6),
            adjacent.PreInstruction,
            "escape Etecoon adjacent pre-instruction");
        AssertEqual(ReadWord(rom, instructionTable + 6), adjacent.InstructionList,
            "escape Etecoon adjacent instruction");
        AssertEqual(ReadWord(rom, speedTable + 6), adjacent.HorizontalSpeed,
            "escape Etecoon adjacent speed");

        var guarded = new EscapeEtecoonDefinitionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeEscapeEtecoon",
            instanceFlags)!;
        for (ushort parameter = 0; parameter < 6; parameter++)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guarded);
            typeof(RoomEnemySystem).GetField("_hasEvent", instanceFlags)!
                .SetValue(enemies, (Func<EventNumber, bool>)(_ => false));
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = parameter;
            slot.XPosition = 0xaaaa;
            slot.YPosition = 0xbbbb;
            initialize.Invoke(enemies, [slot]);

            int role = parameter >> 1;
            EscapeEtecoonInitialization expected =
                EscapeEtecoonDefinitions.Initialization(guarded, parameter);
            EscapeEtecoonEnemyState state = enemies.EscapeEtecoonStates[0] ??
                throw new InvalidDataException("Escape Etecoon initializer omitted typed state.");
            AssertEqual(expected.XPosition, slot.XPosition,
                $"production escape Etecoon selector {parameter} X");
            AssertEqual(expected.YPosition, slot.YPosition,
                $"production escape Etecoon selector {parameter} Y");
            AssertEqual(expected.PreInstruction, state.PreInstruction,
                $"production escape Etecoon selector {parameter} pre-instruction");
            AssertEqual(expected.InstructionList, slot.CurrentInstruction,
                $"production escape Etecoon selector {parameter} instruction");
            AssertEqual(expected.HorizontalSpeed, state.HorizontalSpeed,
                $"production escape Etecoon selector {parameter} speed");
            AssertEqual((EscapeEtecoonRole)(role * 2), (EscapeEtecoonRole)(parameter & 0xfffe),
                $"escape Etecoon selector {parameter} role mask");
        }

        Console.WriteLine(
            "Escape Etecoon definitions: fifteen native values, all six masked retail selectors and the first adjacent-data fallback pass with the authored tables forbidden during production initialization.");
    }

    private sealed class EscapeEtecoonDefinitionReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xb3e718 and < 0xb3e736
                ? throw new InvalidOperationException(
                    $"Escape Etecoon attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
