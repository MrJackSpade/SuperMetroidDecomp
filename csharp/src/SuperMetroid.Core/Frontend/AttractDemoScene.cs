using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One title-demo scene joined across the native room, equipment, and Samus setup tables.
/// The room loader increments the global scene index after placement; these records all
/// refer to the same pre-increment index, not the following scene's equipment.
/// </summary>
public sealed record AttractDemoScene(
    ushort RoomPointer, ushort DoorPointer, ushort DoorSlot,
    ushort CameraX, ushort CameraY, ushort SamusYFromTop, short SamusXFromCenter,
    ushort Duration, ushort RoomSetupPointer, ushort SamusSetupPointer,
    ushort Items, ushort Missiles, ushort SuperMissiles, ushort PowerBombs,
    ushort Health, ushort CollectedBeams, ushort EquippedBeams, ushort InputObject)
{
    /// <summary>Native wrapping X placement: camera + half-screen + signed offset.</summary>
    public ushort SamusX => unchecked((ushort)(CameraX + 128 + SamusXFromCenter));
    /// <summary>Native wrapping Y placement: camera + offset from the screen top.</summary>
    public ushort SamusY => unchecked((ushort)(CameraY + SamusYFromTop));

    /// <summary>Reads a scene, or returns null for the cartridge's end-of-set sentinel.</summary>
    public static AttractDemoScene? Read(ISnesAddressSpace bus, int set, int scene)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentOutOfRangeException.ThrowIfNegative(scene);
        if ((uint)set >= AttractDemoRomData.SetCount)
            throw new ArgumentOutOfRangeException(nameof(set));
        int roomList = RomDataReader.ReadWordFixedBank(bus, AttractDemoRomData.RoomSetPointers + set * 2);
        int roomAddress = RecordAddress(AttractDemoRomData.RoomBank, roomList, scene, AttractDemoRomData.RoomRecordBytes);
        ushort Room(int offset) => RomDataReader.ReadWordFixedBank(bus, roomAddress + offset);
        if (Room(AttractDemoRomData.RoomFields.Room) == AttractDemoRomData.EndOfSet)
            return null;
        int equipmentList = RomDataReader.ReadWordFixedBank(bus, AttractDemoRomData.EquipmentSetPointers + set * 2);
        int equipmentAddress = RecordAddress(AttractDemoRomData.EquipmentBank, equipmentList, scene, AttractDemoRomData.EquipmentRecordBytes);
        ushort Equipment(int offset) => RomDataReader.ReadWordFixedBank(bus, equipmentAddress + offset);
        int setupList = RomDataReader.ReadWordFixedBank(bus, AttractDemoRomData.SamusSetupSetPointers + set * 2);
        ushort setup = RomDataReader.ReadWordFixedBank(bus, RecordAddress(AttractDemoRomData.EquipmentBank, setupList, scene, sizeof(ushort)));
        return new(
            Room(AttractDemoRomData.RoomFields.Room), Room(AttractDemoRomData.RoomFields.Door),
            Room(AttractDemoRomData.RoomFields.DoorSlot), Room(AttractDemoRomData.RoomFields.CameraX),
            Room(AttractDemoRomData.RoomFields.CameraY), Room(AttractDemoRomData.RoomFields.SamusYFromTop),
            unchecked((short)Room(AttractDemoRomData.RoomFields.SamusXFromCenter)),
            Room(AttractDemoRomData.RoomFields.Duration), Room(AttractDemoRomData.RoomFields.Setup), setup,
            Equipment(AttractDemoRomData.EquipmentFields.Items), Equipment(AttractDemoRomData.EquipmentFields.Missiles),
            Equipment(AttractDemoRomData.EquipmentFields.SuperMissiles), Equipment(AttractDemoRomData.EquipmentFields.PowerBombs),
            Equipment(AttractDemoRomData.EquipmentFields.Health), Equipment(AttractDemoRomData.EquipmentFields.CollectedBeams),
            Equipment(AttractDemoRomData.EquipmentFields.EquippedBeams), Equipment(AttractDemoRomData.EquipmentFields.InputObject));
    }

    private static int RecordAddress(int bank, int list, int scene, int stride)
    {
        long offset = list + (long)scene * stride;
        if (offset < 0x8000 || offset + stride - 1 > ushort.MaxValue)
            throw new InvalidDataException("Demo scene record exceeds its cartridge data bank.");
        return bank | (int)offset;
    }
}
