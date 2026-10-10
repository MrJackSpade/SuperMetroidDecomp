using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads native title-demo scene records for import and reference diagnostics.</summary>
internal static class AttractDemoSceneImporter
{
    /// <summary>Reads one native attract-demo scene and its equipment/setup records, returning null at the set terminator.</summary>
    /// <param name="bus">Cartridge address space containing the attract-demo tables.</param>
    /// <param name="set">Zero-based scene-set index within the compiled attract-demo set range.</param>
    /// <param name="scene">Zero-based record index within the selected set.</param>
    /// <returns>The combined scene and Samus setup data, or <see langword="null"/> when the room list reaches its end marker.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="set"/> is unsupported or <paramref name="scene"/> is negative.</exception>
    /// <exception cref="InvalidDataException">A selected room, equipment, or setup record exceeds its data bank.</exception>
    public static AttractDemoScene? Read(ISnesAddressSpace bus, int set, int scene)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentOutOfRangeException.ThrowIfNegative(scene);
        if ((uint)set >= AttractDemoRomData.SetCount)
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
            Room(AttractDemoRomDataRoomFields.Room), Room(AttractDemoRomDataRoomFields.Door), Room(AttractDemoRomDataRoomFields.CameraX),
            Room(AttractDemoRomDataRoomFields.CameraY), Room(AttractDemoRomDataRoomFields.SamusYFromTop),
            unchecked((short)Room(AttractDemoRomDataRoomFields.SamusXFromCenter)),
            Room(AttractDemoRomDataRoomFields.Duration), Room(AttractDemoRomDataRoomFields.Setup), setup,
            Equipment(AttractDemoRomDataEquipmentFields.Items), Equipment(AttractDemoRomDataEquipmentFields.Missiles),
            Equipment(AttractDemoRomDataEquipmentFields.SuperMissiles), Equipment(AttractDemoRomDataEquipmentFields.PowerBombs),
            Equipment(AttractDemoRomDataEquipmentFields.Health), Equipment(AttractDemoRomDataEquipmentFields.CollectedBeams),
            Equipment(AttractDemoRomDataEquipmentFields.EquippedBeams), Equipment(AttractDemoRomDataEquipmentFields.InputObject));
    }

    /// <summary>Calculates a strided record address and ensures the whole record remains inside its cartridge data bank.</summary>
    /// <param name="bank">Bank base combined with the resulting in-bank offset.</param>
    /// <param name="list">In-bank address of the first record in the list.</param>
    /// <param name="scene">Zero-based record index to locate.</param>
    /// <param name="stride">Record size in bytes.</param>
    /// <returns>The bank-qualified address of the selected record.</returns>
    /// <exception cref="InvalidDataException">The selected record begins before bank data or extends beyond the bank.</exception>
    private static int RecordAddress(int bank, int list, int scene, int stride)
    {
        long offset = list + (long)scene * stride;
        if (offset < 0x8000 || offset + stride - 1 > ushort.MaxValue)
            throw new InvalidDataException("Demo scene record exceeds its cartridge data bank.");
        return bank | (int)offset;
    }
}
