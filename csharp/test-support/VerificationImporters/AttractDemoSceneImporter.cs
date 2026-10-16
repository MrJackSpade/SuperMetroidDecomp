using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Reads native title-demo scene records for import and reference diagnostics.</summary>
[CpuBusBoundary("codec: ROM attract-demo room, equipment and Samus-setup set tables → AttractDemoScene")]
internal static class AttractDemoSceneImporter
{
    /// <summary>Combines one demo scene's room, equipment, and Samus setup records, or returns null at the set terminator.</summary>
    public static AttractDemoScene? Read(ISnesAddressSpace bus, int set, int scene)
    {
        Ensure.NotNull(bus);
        Ensure.AtLeastZero(scene);
        if ((uint)set >= DemoSetUnlockRomData.SetCount)
            throw new ArgumentOutOfRangeException(nameof(set));
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        int roomList = RomDataReader.ReadWordFixedBank(cartridge, AttractDemoRomData.RoomSetPointers + set * 2);
        int roomAddress = RecordAddress(AttractDemoRomData.RoomBank, roomList, scene, AttractDemoRomData.RoomRecordBytes);
        ushort Room(int offset) => RomDataReader.ReadWordFixedBank(cartridge, roomAddress + offset);
        if (Room(AttractDemoRomDataRoomFields.Room) == AttractDemoRomData.EndOfSet)
            return null;
        int equipmentList = RomDataReader.ReadWordFixedBank(cartridge, AttractDemoRomData.EquipmentSetPointers + set * 2);
        int equipmentAddress = RecordAddress(AttractDemoRomData.EquipmentBank, equipmentList, scene, AttractDemoRomData.EquipmentRecordBytes);
        ushort Equipment(int offset) => RomDataReader.ReadWordFixedBank(cartridge, equipmentAddress + offset);
        int setupList = RomDataReader.ReadWordFixedBank(cartridge, AttractDemoRomData.SamusSetupSetPointers + set * 2);
        ushort setup = RomDataReader.ReadWordFixedBank(cartridge,
            RecordAddress(AttractDemoRomData.EquipmentBank, setupList, scene, sizeof(ushort)));
        return new(
            Room(AttractDemoRomDataRoomFields.Room), Room(AttractDemoRomDataRoomFields.Door),
            new(Room(AttractDemoRomDataRoomFields.CameraX), Room(AttractDemoRomDataRoomFields.CameraY)),
            new(unchecked((short)Room(AttractDemoRomDataRoomFields.SamusXFromCenter)),
                unchecked((short)Room(AttractDemoRomDataRoomFields.SamusYFromTop))),
            Room(AttractDemoRomDataRoomFields.Duration),
            ClosedNativeWords.Decode<AttractDemoRoomSetup>(Room(AttractDemoRomDataRoomFields.Setup), "demo room setup"),
            ClosedNativeWords.Decode<AttractDemoSamusSetup>(setup, "demo Samus setup"),
            (SamusEquipmentFlags)(Equipment(AttractDemoRomDataEquipmentFields.Items)), Equipment(AttractDemoRomDataEquipmentFields.Missiles),
            Equipment(AttractDemoRomDataEquipmentFields.SuperMissiles), Equipment(AttractDemoRomDataEquipmentFields.PowerBombs),
            Equipment(AttractDemoRomDataEquipmentFields.Health), (SamusBeamFlags)(Equipment(AttractDemoRomDataEquipmentFields.CollectedBeams)),
            (SamusBeamFlags)(Equipment(AttractDemoRomDataEquipmentFields.EquippedBeams)), Equipment(AttractDemoRomDataEquipmentFields.InputObject));
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
