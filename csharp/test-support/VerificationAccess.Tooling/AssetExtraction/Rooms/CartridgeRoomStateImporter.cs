using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Reads one native 26-byte bank-$8F room-state payload during import.</summary>
public static class CartridgeRoomStateImporter
{
    /// <summary>Decodes one 26-byte bank-$8F room-state payload, including its level (named by extraction's map from its compressed source) and native graphics, music (decoded by <see cref="RoomMusicCodec"/>), population, and setup fields.</summary>
    /// <param name="bus">Address space implementing the import-only cartridge-source capability.</param>
    /// <param name="statePointer">Sixteen-bit payload address within bank $8F, not the room header or state-selection list.</param>
    /// <returns>The decoded definition, named by the import map; no room selection or runtime setup is performed.</returns>
    public static RoomStateSettings Load(ISnesAddressSpace bus, ushort statePointer)
    {
        Ensure.NotNull(bus);
        int address = 0x8f0000 | statePointer;
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        byte Byte(int offset) => cartridge.ReadCartridgeByte(address + offset);
        ushort Word(int offset) => RomDataReader.ReadWordFixedBank(cartridge, address + offset);
        int Long(int offset) => RomDataReader.ReadLongFixedBank(cartridge, address + offset);
        int levelSource = Long(0);
        RoomLevelId level = RoomLevelSourceDefinitions.Addresses.Single(pair => pair.Value == levelSource).Key;
        return new RoomStateSettings(RoomStateSourcesConstants.State(statePointer).Name, level, Byte(3), RoomMusicCodec.Decode(Byte(4), Byte(5)),
            Word(6) == 0 ? null : SuperMetroid.Core.Fx.RoomFxDefinitions.Get(Word(6)), SuperMetroid.Core.Enemies.RoomEnemyPopulationDefinitions.Get(Word(8)),
            SuperMetroid.Core.Enemies.RoomEnemyGraphicsSetDefinitions.Get(Word(10)), SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes.FromHeader(Byte(12), Byte(13)),
            SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinitions.Get(Word(14)), Word(16) == 0 ? null : XrayRoomOverlaySources.Overlay(Word(16)),
            RoomCallbackDefinitions.ResolveMain(Word(18)), SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinitions.Get(Word(20)),
            Word(22) == 0 ? null : LibraryBackgroundSources.Program(Word(22)), RoomCallbackDefinitions.ResolveSetup(Word(24)));
    }

    /// <summary>The compressed level-data source the bank-$8F room-state payload at <paramref name="statePointer"/> names.</summary>
    public static int LevelSource(ISnesAddressSpace bus, ushort statePointer) =>
        RomDataReader.ReadLongFixedBank(CartridgeImportSource.Require(Ensure.NotNull(bus)), 0x8f0000 | statePointer);
}
