using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Compares both compiled family headers field-for-field with the pinned cartridge,
    /// then loads the real Mama Turtle room while every source-header byte is forbidden.
    /// </summary>
    private static void VerifyMamaTurtleEnemyDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var guard = new MamaTurtleDefinitionReadGuard(rom);
        foreach (ushort pointer in new ushort[]
                 {
                     MamaTurtleEnemyDefinitionCatalog.MamaPointer,
                     MamaTurtleEnemyDefinitionCatalog.BabyPointer,
                 })
        {
            RoomEnemyDefinition expected = ReadNativeEnemyDefinition(rom, pointer);
            RoomEnemyDefinition actual = RoomEnemySystem.ReadDefinition(guard, pointer);
            AssertEqual(expected, actual, $"compiled enemy header $A0:{pointer:X4}");
        }

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(rom, 0xd055);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(rom, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            guard,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            () => 0,
            level: assets.LevelData,
            samus: new SamusState());

        AssertEqual(5, enemies.EnemyCount, "Mama Turtle retail population count");
        AssertEqual(MamaTurtleEnemyDefinitionCatalog.MamaPointer,
            enemies.Slots[0].EnemyDefinitionPointer, "Mama Turtle parent header identity");
        AssertEqual(MamaTurtleEnemyDefinitionCatalog.BabyPointer,
            enemies.Slots[1].EnemyDefinitionPointer, "Baby Turtle header identity");
        AssertEqual(ReadNativeEnemyDefinition(rom, MamaTurtleEnemyDefinitionCatalog.MamaPointer),
            enemies.Slots[0].Definition, "Mama Turtle loaded definition");
        for (int slot = 1; slot < 5; slot++)
        {
            AssertEqual(ReadNativeEnemyDefinition(rom, MamaTurtleEnemyDefinitionCatalog.BabyPointer),
                enemies.Slots[slot].Definition, $"Baby Turtle loaded definition slot {slot}");
        }

        Console.WriteLine(
            "  Mama Turtle enemy definitions: two native headers and the five-actor " +
            "retail population load pass without reading 128 source bytes.");
    }

    private static RoomEnemyDefinition ReadNativeEnemyDefinition(
        SuperMetroidAddressSpace bus,
        ushort pointer)
    {
        int address = RoomEnemyRomLayout.DefinitionBank | pointer;
        ushort Word(int offset) => unchecked((ushort)(
            bus.ReadByte(address + offset) | bus.ReadByte(address + offset + 1) << 8));
        int Long(int offset) =>
            bus.ReadByte(address + offset) |
            bus.ReadByte(address + offset + 1) << 8 |
            bus.ReadByte(address + offset + 2) << 16;

        return new RoomEnemyDefinition(
            Word(0), Word(2), Word(4), Word(6), Word(8), Word(10),
            bus.ReadByte(address + 12), bus.ReadByte(address + 13),
            Word(14), Word(16), Word(18), Word(20), Word(22), Word(24),
            Word(26), Word(28), Word(30), Word(32), Word(34), Word(36),
            Word(38), Word(40), Word(42), Word(44), Word(46), Word(48),
            Word(50), Word(52), Long(54), bus.ReadByte(address + 57),
            Word(58), Word(60), Word(62));
    }

    private sealed class MamaTurtleDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= MamaTurtleEnemyDefinitionCatalog.SourceAddress and
                < MamaTurtleEnemyDefinitionCatalog.SourceAddress +
                    MamaTurtleEnemyDefinitionCatalog.SourceByteLength)
            {
                throw new InvalidOperationException(
                    $"Mama Turtle family still reads compiled header byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
