using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Reads native title-demo scene records for import and reference diagnostics.</summary>
internal static class AttractDemoSceneImporter
{
    /// <summary>Combines one demo scene's room, equipment, and Samus setup records, or returns null at the set terminator.</summary>
    public static AttractDemoScene? Read(ISnesAddressSpace bus, int set, int scene)
    {
        Ensure.NotNull(bus);
        Ensure.AtLeastZero(scene);
        if ((uint)set >= DemoSetUnlockDefinitions.SetCount)
            throw new ArgumentOutOfRangeException(nameof(set));
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        int roomList = RomDataReader.ReadWordFixedBank(cartridge, AttractDemoSceneSources.RoomSetPointers + set * 2);
        int roomAddress = RecordAddress(AttractDemoSceneSources.RoomBank, roomList, scene, AttractDemoSceneSources.RoomRecordBytes);
        ushort Room(int offset) => RomDataReader.ReadWordFixedBank(cartridge, roomAddress + offset);
        if (Room(AttractDemoSceneRoomFields.Room) == AttractDemoSceneSources.EndOfSet)
            return null;
        int equipmentList = RomDataReader.ReadWordFixedBank(cartridge, AttractDemoSceneSources.EquipmentSetPointers + set * 2);
        int equipmentAddress = RecordAddress(AttractDemoSceneSources.EquipmentBank, equipmentList, scene, AttractDemoSceneSources.EquipmentRecordBytes);
        ushort Equipment(int offset) => RomDataReader.ReadWordFixedBank(cartridge, equipmentAddress + offset);
        int setupList = RomDataReader.ReadWordFixedBank(cartridge, AttractDemoSceneSources.SamusSetupSetPointers + set * 2);
        ushort setup = RomDataReader.ReadWordFixedBank(cartridge,
            RecordAddress(AttractDemoSceneSources.EquipmentBank, setupList, scene, sizeof(ushort)));
        return new(
            RoomHeaderSources.Room(Room(AttractDemoSceneRoomFields.Room)), DoorHeaderSources.Door(Room(AttractDemoSceneRoomFields.Door)),
            new(Room(AttractDemoSceneRoomFields.CameraX), Room(AttractDemoSceneRoomFields.CameraY)),
            new(unchecked((short)Room(AttractDemoSceneRoomFields.SamusXFromCenter)),
                unchecked((short)Room(AttractDemoSceneRoomFields.SamusYFromTop))),
            Room(AttractDemoSceneRoomFields.Duration),
            AttractDemoNativeWords.RoomSetups.Decode(Room(AttractDemoSceneRoomFields.Setup), "demo room setup"),
            AttractDemoNativeWords.SamusSetups.Decode(setup, "demo Samus setup"),
            SamusEquipmentWordCodec.FromEquipmentWord(Equipment(AttractDemoSceneEquipmentFields.Items)), Equipment(AttractDemoSceneEquipmentFields.Missiles),
            Equipment(AttractDemoSceneEquipmentFields.SuperMissiles), Equipment(AttractDemoSceneEquipmentFields.PowerBombs),
            Equipment(AttractDemoSceneEquipmentFields.Health), (SamusBeamFlags)(Equipment(AttractDemoSceneEquipmentFields.CollectedBeams)),
            (SamusBeamFlags)(Equipment(AttractDemoSceneEquipmentFields.EquippedBeams)),
            DemoInputObjectDefinitions.FromNativeHeader(Equipment(AttractDemoSceneEquipmentFields.InputObject)));
    }

    /// <summary>Bounds a fixed-stride scene record within its native cartridge data bank.</summary>
    private static int RecordAddress(int bank, int list, int scene, int stride)
    {
        long offset = list + (long)scene * stride;
        if (offset < 0x8000 || offset + stride - 1 > ushort.MaxValue)
            throw new InvalidDataException("Demo scene record exceeds its cartridge data bank.");
        return bank | (int)offset;
    }
}
