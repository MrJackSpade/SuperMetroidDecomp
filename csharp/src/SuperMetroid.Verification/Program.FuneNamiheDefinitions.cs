using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFuneNamiheDefinitions(SuperMetroidAddressSpace rom)
    {
        const int table = 0xa896d3;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        for (int index = 0; index < 8; index++)
        {
            ushort cursor = (ushort)(0x96d3 + index * 2);
            ushort native = (ushort)(rom.ReadByte(table + index * 2) |
                rom.ReadByte(table + index * 2 + 1) << 8);
            AssertEqual(native, FuneNamiheDefinitions.InstructionList(cursor),
                $"Fune/Namihe instruction selector {index}");
        }

        for (int variant = 0; variant < 4; variant++)
        {
            bool namihe = (variant & 2) != 0;
            bool right = (variant & 1) != 0;
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies, new FuneNamiheReadGuard(new TestAddressSpace()));
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeFuneNamihe", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var runMain = typeof(RoomEnemySystem).GetMethod("RunFuneNamiheMain", flags)!
                .CreateDelegate<Action<RoomEnemySlot, FuneNamiheEnemyState, SamusState?>>();
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = (ushort)((namihe ? 1 : 0) | (right ? 0x10 : 0));
            slot.Parameter2 = 0x1000;
            slot.YPosition = 0x0200;
            initialize(slot);
            FuneNamiheEnemyState state = enemies.FuneNamiheStates[0]!;
            ushort idleCursor = (ushort)((namihe
                ? FuneNamiheDefinitions.NamiheIdleLeftCursor
                : FuneNamiheDefinitions.FuneIdleLeftCursor) +
                (right ? FuneNamiheDefinitions.FacingRightCursorDelta : 0));
            AssertEqual(FuneNamiheDefinitions.InstructionList(idleCursor), slot.CurrentInstruction,
                $"Fune/Namihe production idle list {variant}");

            runMain(slot, state, namihe ? new SamusState { YPosition = slot.YPosition } : null);
            ushort activeCursor = (ushort)(idleCursor - FuneNamiheDefinitions.ActiveCursorDelta);
            AssertEqual(FuneNamiheDefinitions.InstructionList(activeCursor), slot.CurrentInstruction,
                $"Fune/Namihe production active list {variant}");
        }

        AssertThrows<InvalidDataException>(() => FuneNamiheDefinitions.InstructionList(0x96d2),
            "Fune/Namihe selector below table");
        AssertThrows<InvalidDataException>(() => FuneNamiheDefinitions.InstructionList(0x96d4),
            "Fune/Namihe odd selector");
        AssertThrows<InvalidDataException>(() => FuneNamiheDefinitions.InstructionList(0x96e3),
            "Fune/Namihe selector beyond table");
        Console.WriteLine(
            "Fune/Namihe definitions: eight native selectors and all eight real idle/active installs pass with table reads forbidden.");
    }

    private sealed class FuneNamiheReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa896d3 and < 0xa896e3
            ? throw new InvalidOperationException(
                $"Fune/Namihe attempted migrated selector read ${address:X6}.")
            : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
