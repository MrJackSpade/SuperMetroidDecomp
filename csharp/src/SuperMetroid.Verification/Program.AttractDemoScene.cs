using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyAttractDemoScene()
    {
        var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];
        WriteRomWord(rom, AttractDemoRomData.RoomSetPointers, 0x9000);
        WriteRomWord(rom, AttractDemoRomData.EquipmentSetPointers, 0xa000);
        WriteRomWord(rom, AttractDemoRomData.SamusSetupSetPointers, 0xb000);
        ushort[] room = [0x91f8, 0x8000, 1, 0x100, 0x200, 0x40, 0xffd2, 0x151, 0x8924];
        ushort[] equipment = [0x3105, 10, 5, 2, 399, 0x100f, 0x100b, 0x9000];
        for (int word = 0; word < room.Length; word++) WriteRomWord(rom, 0x829000 + word * 2, room[word]);
        for (int word = 0; word < equipment.Length; word++) WriteRomWord(rom, 0x91a000 + word * 2, equipment[word]);
        WriteRomWord(rom, 0x91b000, 0x8a53);
        WriteRomWord(rom, 0x829012, AttractDemoRomData.EndOfSet);
        var bus = new SuperMetroidAddressSpace(rom);
        var expected = new AttractDemoScene(0x91f8, 0x8000, 1, 0x100, 0x200, 0x40, -46,
            0x151, 0x8924, 0x8a53, 0x3105, 10, 5, 2, 399, 0x100f, 0x100b, 0x9000);
        if (AttractDemoScene.Read(bus, 0, 0) != expected)
            throw new InvalidDataException("Demo room/equipment/setup tables did not join at the same scene index.");
        if (expected.SamusX != 338 || expected.SamusY != 576)
            throw new InvalidDataException("Demo offsets lost their native X-center/Y-top interpretation.");
        if (AttractDemoScene.Read(bus, 0, 1) is not null)
            throw new InvalidDataException("Demo room sentinel did not terminate the set.");
        Console.WriteLine("  Attract demo data: joined fields, signed placement, and end-of-set sentinel agree.");
    }
}
