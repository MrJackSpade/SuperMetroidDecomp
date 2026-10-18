using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Reads one native 26-byte bank-$8F room-state payload during import.</summary>
[CpuBusBoundary("codec: ROM bank-$8F 26-byte room-state payload → CartridgeRoomState")]
public static class CartridgeRoomStateImporter
{
    /// <summary>Decodes one 26-byte bank-$8F room-state payload, including its 24-bit level-data address and native graphics, audio, population, and setup fields.</summary>
    /// <param name="bus">Address space implementing the import-only cartridge-source capability.</param>
    /// <param name="statePointer">Sixteen-bit payload address within bank $8F, not the room header or state-selection list.</param>
    /// <returns>The decoded definition retaining its native state pointer; no room selection or runtime setup is performed.</returns>
    public static CartridgeRoomState Load(ISnesAddressSpace bus, ushort statePointer)
    {
        Ensure.NotNull(bus);
        int address = 0x8f0000 | statePointer;
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        byte Byte(int offset) => cartridge.ReadCartridgeByte(address + offset);
        ushort Word(int offset) => RomDataReader.ReadWordFixedBank(cartridge, address + offset);
        int Long(int offset) => RomDataReader.ReadLongFixedBank(cartridge, address + offset);
        return new CartridgeRoomState(statePointer, Long(0), Byte(3), Byte(4), Byte(5),
            Word(6) == 0 ? null : SuperMetroid.Core.Fx.RoomFxDefinitions.Get(Word(6)), SuperMetroid.Core.Enemies.RoomEnemyPopulationDefinitions.Get(Word(8)),
            SuperMetroid.Core.Enemies.RoomEnemyGraphicsSetDefinitions.Get(Word(10)), SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(Byte(12), Byte(13)),
            SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Get(Word(14)), Word(16) == 0 ? null : XrayRoomOverlaySources.Overlay(Word(16)),
            RoomCallbackDefinitions.ResolveMain(Word(18)), SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Get(Word(20)),
            Word(22) == 0 ? null : LibraryBackgroundSources.Program(Word(22)), RoomCallbackDefinitions.ResolveSetup(Word(24)));
    }
}
