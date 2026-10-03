using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyDownwardGateProgramControls(SuperMetroidAddressSpace rom) =>
        VerifyDownwardGateProgramField(rom, 0);
    private static void VerifyDownwardGateProgramDraws(SuperMetroidAddressSpace rom) =>
        VerifyDownwardGateProgramField(rom, 1);
    private static void VerifyDownwardGateProgramOperands(SuperMetroidAddressSpace rom) =>
        VerifyDownwardGateProgramField(rom, 2);

    private static void VerifyDownwardGateProgramField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] controls = [0xbc13,0xbc17,0xbc19,0xbc1d,0xbc1f,0xbc23,0xbc27,
            0xbc2a,0xbc2e,0xbc32,0xbc36,0xbc3a,0xbc3e,0xbc40,0xbc44,0xbc46,0xbc4a,
            0xbc4d,0xbc51,0xbc55,0xbc59,0xbc5d,
            0xbcaf,0xbcb3,0xbcb5,0xbcb9,0xbcbb,0xbcbf,0xbcc1,0xbcc5,
            0xbcc7,0xbccb,0xbccd,0xbcd1,0xbcd3,0xbcd7,0xbcd9,0xbcdd];
        ushort[] draws = [0xbc15,0xbc21,0xbc2c,0xbc30,0xbc34,0xbc38,0xbc3c,0xbc4f,0xbc53,0xbc57,0xbc5b,
            0xbcb1,0xbcb7,0xbcbd,0xbcc3,0xbcc9,0xbccf,0xbcd5,0xbcdb];
        ushort[] operands = [0xbc1b,0xbc25,0xbc42,0xbc48,0xbc5f];
        ushort[] all = controls.Concat(draws).Concat(operands).Order().ToArray();
        ushort[] selected = field == 0 ? controls : field == 1 ? draws : operands;
        var exported = DownwardGatePlmProgramDefinitions.MechanicsWords.ToArray();
        AssertEqual(62, exported.Length, "Gate complete exported word count");
        for (int index = 0; index < all.Length; index++)
        {
            AssertEqual(all[index], exported[index].Address, "Gate native word enumeration order");
            if (selected.Contains(all[index]))
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | all[index]), exported[index].Value,
                    "Gate original enumerated field");
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool owned = all.Contains(address);
            AssertEqual(owned, DownwardGatePlmProgramDefinitions.TryReadMechanicsWord(address, out ushort actual),
                "Gate complete word boundary domain");
            if (!owned) AssertEqual((ushort)0, actual, "Gate unowned word output");
            else if (selected.Contains(address))
            {
                ushort expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw);
                AssertEqual(expected, actual, "Gate original program field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Gate composed word reader");
                AssertEqual(expected, shared, "Gate composed original field");
            }
        }
    }

    private static void VerifyDownwardGateProgramSounds(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xbc29,0xbc4c];
        var exported = DownwardGatePlmProgramDefinitions.MechanicsBytes.ToArray();
        AssertEqual(2, exported.Length, "Gate sound enumeration count");
        for (int index = 0; index < addresses.Length; index++)
        {
            AssertEqual(addresses[index], exported[index].Address, "Gate sound enumeration order");
            AssertEqual(rom.ReadByte(0x840000 | addresses[index]), exported[index].Value, "Gate original enumerated sound");
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool owned = addresses.Contains(address);
            AssertEqual(owned, DownwardGatePlmProgramDefinitions.TryReadMechanicsByte(address, out byte value),
                "Gate complete byte domain");
            if (!owned) AssertEqual((byte)0, value, "Gate unowned byte output");
            else
            {
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, value, "Gate original sound operand");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Gate composed byte reader");
                AssertEqual(expected, shared, "Gate composed original sound");
            }
        }
    }
}
