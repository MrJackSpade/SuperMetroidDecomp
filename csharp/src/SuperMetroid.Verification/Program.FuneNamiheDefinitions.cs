using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks all Fune/Namihe species and facing selectors against real instruction-list installation behavior.</summary>
    /// <param name="rom">Cartridge address space containing the native selector table.</param>
    private static void VerifyFuneNamiheDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyFuneNamiheProgramSelection), () => VerifyFuneNamiheProgramSelection(rom));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
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

        Console.WriteLine(
            "Fune/Namihe definitions: eight native selectors and all eight real idle/active installs pass with table reads forbidden.");
    }

    /// <summary>Checks the eight supported Fune/Namihe instruction-list cursors and rejects invalid or adjacent cursors.</summary>
    /// <param name="rom">Cartridge address space used to read the native instruction-list pointers.</param>
    private static void VerifyFuneNamiheProgramSelection(SuperMetroidAddressSpace rom)
    {
        ushort[] cursors = [0x96d3, 0x96d5, 0x96d7, 0x96d9, 0x96db, 0x96dd, 0x96df, 0x96e1];
        foreach (ushort cursor in cursors)
        {
            ushort native = (ushort)(rom.ReadByte(0xa80000 | cursor) |
                rom.ReadByte(0xa80000 | (cursor + 1)) << 8);
            AssertEqual(native, FuneNamiheDefinitions.InstructionList(cursor),
                "Fune/Namihe named species/activity/facing case matches native pointer");
        }
        var valid = cursors.ToHashSet();
        for (int cursor = 0x96d1; cursor <= 0x96e4; cursor++)
            if (!valid.Contains((ushort)cursor))
                AssertThrows<InvalidDataException>(() => FuneNamiheDefinitions.InstructionList((ushort)cursor),
                    "Fune/Namihe selection rejects odd and adjacent cursors");
        foreach (ushort cursor in new ushort[] { 0, 1, 0x16d3, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => FuneNamiheDefinitions.InstructionList(cursor),
                "Fune/Namihe selection rejects distant and wrapped cursors");
    }
    /// <summary>Rejects runtime reads from the Fune/Namihe selector table while forwarding other bus access.</summary>
    /// <param name="source">Underlying address space for permitted reads and forwarded writes.</param>
    private sealed class FuneNamiheReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes a cartridge-byte request through the selector-table read guard.</summary>
        /// <param name="address">Cartridge address of the requested byte.</param>
        /// <returns>The underlying byte unless the address belongs to the protected table, in which case the read throws.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Reads a byte while rejecting access to the native Fune/Namihe selector table.</summary>
        /// <param name="address">Address of the requested byte.</param>
        /// <returns>The underlying byte when the address is outside the selector table.</returns>
        public byte ReadByte(int address) => address is >= 0xa896d3 and < 0xa896e3
            ? throw new InvalidOperationException(
                $"Fune/Namihe attempted migrated selector read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards a byte write to the underlying address space.</summary>
        /// <param name="address">Address where the byte is written.</param>
        /// <param name="value">Byte value to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
